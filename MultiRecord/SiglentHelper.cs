using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SIGLENT
{
    // Enum to define measurement functions clearly
    public enum MeasurementFunction
    {
        VoltageDC,
        VoltageAC,
        CurrentDC,
        CurrentAC,
        Resistance2W,
        Resistance4W,
        Capacitance,
        Frequency,
        Temperature,
        Diode
    }

    // Class to hold measurement results
    public class MeasurementResult
    {
        public double Value { get; }
        public string Unit { get; }
        public MeasurementFunction Function { get; }
        public bool IsValid { get; }

        public MeasurementResult(double value, string unit, MeasurementFunction function)
        {
            Value = value;
            Unit = unit;
            Function = function;
            IsValid = !double.IsNaN(value) && !double.IsInfinity(value);
        }
    }

    public class SDM3055 : IDisposable
    {
        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private readonly string _ipAddress;
        private readonly int _port;
        private readonly int _connectTimeout;
        private readonly int _readTimeout;
        private readonly SemaphoreSlim _commandSemaphore = new SemaphoreSlim(1, 1);
        private CancellationTokenSource _ctsContinuousRead;
        private Task _continuousReadTask;
        private volatile bool _isReading = false;
        private readonly object _lockObject = new object();

        public bool IsConnected
        {
            get
            {
                lock (_lockObject)
                {
                    return _tcpClient != null && _tcpClient.Connected && _stream != null;
                }
            }
        }

        public event EventHandler<MeasurementResult> ReadingReceived;
        public event EventHandler ConnectionLost;

        public SDM3055(string ipAddress, int port = 5025, int connectTimeout = 3000, int readTimeout = 5000)
        {
            _ipAddress = ipAddress;
            _port = port;
            _connectTimeout = connectTimeout;
            _readTimeout = readTimeout;
        }

        public async Task ConnectAsync()
        {
            if (IsConnected) return;

            TcpClient tempClient = null;
            try
            {
                tempClient = new TcpClient();
                
                // Set connection timeout using Task.WhenAny with better error handling
                var connectTask = tempClient.ConnectAsync(_ipAddress, _port);
                var timeoutTask = Task.Delay(_connectTimeout);
                
                var completedTask = await Task.WhenAny(connectTask, timeoutTask);
                
                if (completedTask == timeoutTask)
                {
                    // Connection timed out
                    tempClient?.Close();
                    tempClient?.Dispose();
                    throw new TimeoutException($"Connection to {_ipAddress}:{_port} timed out after {_connectTimeout}ms.");
                }
                
                // Wait for the connection task to complete and check for exceptions
                try
                {
                    await connectTask;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Connection failed: {ex.Message}", ex);
                }

                // Check if connection was successful
                if (!tempClient.Connected)
                {
                    tempClient?.Close();
                    throw new Exception("Failed to establish connection to device");
                }

                // Connection successful, assign to class member
                _tcpClient = tempClient;
                tempClient = null; // Prevent disposal in catch block

                _stream = _tcpClient.GetStream();
                _stream.ReadTimeout = _readTimeout;
                _stream.WriteTimeout = _readTimeout;

                if (_stream.DataAvailable)
                {
                    byte[] buffer = new byte[1024];
                    while (_stream.DataAvailable) await _stream.ReadAsync(buffer, 0, buffer.Length);
                }

                await SendCommandAsync("*RST");
                await Task.Delay(500);
                await SendCommandAsync("*CLS");
                await Task.Delay(100);

                string idn = await QueryCommandAsync("*IDN?");
                if (string.IsNullOrEmpty(idn))
                {
                    throw new Exception("Device did not respond to identification query");
                }
            }
            catch (Exception)
            {
                // Clean up temp client if connection failed
                tempClient?.Close();
                tempClient?.Dispose();
                
                // Clean up class members if already assigned
                Disconnect();
                throw;
            }
        }

        private async Task<bool> SendDataAsync(byte[] data, CancellationToken cancellationToken)
        {
            if (!IsConnected) return false;
            
            try
            {
                await _stream.WriteAsync(data, 0, data.Length, cancellationToken);
                await _stream.FlushAsync(cancellationToken);
                return true;
            }
            catch (ObjectDisposedException ex)
            {
                Debug.WriteLine($"SendDataAsync - Connection disposed: {ex.Message}");
                HandleConnectionLost();
                return false;
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"SendDataAsync - Invalid operation: {ex.Message}");
                HandleConnectionLost();
                return false;
            }
            catch (System.IO.IOException ex)
            {
                Debug.WriteLine($"SendDataAsync - IO error: {ex.Message}");
                HandleConnectionLost();
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SendDataAsync - Unexpected error: {ex.GetType().Name}: {ex.Message}");
                HandleConnectionLost();
                return false;
            }
        }

        private async Task<string> ReadDataAsync(CancellationToken cancellationToken)
        {
            if (!IsConnected) return string.Empty;
            
            try
            {
                byte[] buffer = new byte[1024];
                var readTask = _stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                var timeoutTask = Task.Delay(_readTimeout, cancellationToken);
                
                var completedTask = await Task.WhenAny(readTask, timeoutTask);
                
                if (completedTask == timeoutTask)
                {
                    throw new TimeoutException($"Read operation timed out after {_readTimeout}ms");
                }

                int bytesRead = await readTask;
                if (bytesRead == 0) 
                {
                    throw new Exception("Connection closed by device - no data received");
                }

                return Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
            }
            catch (ObjectDisposedException ex)
            {
                Debug.WriteLine($"ReadDataAsync - Connection disposed: {ex.Message}");
                HandleConnectionLost();
                throw new Exception("Connection was closed", ex);
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"ReadDataAsync - Invalid operation: {ex.Message}");
                HandleConnectionLost();
                throw new Exception("Invalid read operation", ex);
            }
            catch (System.IO.IOException ex)
            {
                Debug.WriteLine($"ReadDataAsync - IO error: {ex.Message}");
                HandleConnectionLost();
                throw new Exception("Network communication error", ex);
            }
            catch (TimeoutException)
            {
                Debug.WriteLine("ReadDataAsync - Timeout");
                // Don't call HandleConnectionLost for timeout - device might just be slow
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ReadDataAsync - Unexpected error: {ex.GetType().Name}: {ex.Message}");
                HandleConnectionLost();
                throw;
            }
        }

        public async Task SendCommandAsync(string command)
        {
            if (!IsConnected) throw new InvalidOperationException("Not connected to device");

            await _commandSemaphore.WaitAsync();
            try
            {
                Debug.WriteLine($"Sending: {command}");
                byte[] data = Encoding.ASCII.GetBytes(command + "\n");
                if (!await SendDataAsync(data, CancellationToken.None))
                {
                    throw new Exception("Failed to send command");
                }
                await Task.Delay(50);
            }
            finally
            {
                _commandSemaphore.Release();
            }
        }

        public async Task<string> QueryCommandAsync(string command)
        {
            if (!IsConnected) throw new InvalidOperationException("Not connected to device");

            await _commandSemaphore.WaitAsync();
            try
            {
                Debug.WriteLine($"Querying: {command}");
                byte[] data = Encoding.ASCII.GetBytes(command + "\n");
                if (!await SendDataAsync(data, CancellationToken.None))
                {
                    return string.Empty;
                }

                await Task.Delay(100);
                return await ReadDataAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"QueryCommandAsync error: {ex.Message}");
                return string.Empty;
            }
            finally
            {
                _commandSemaphore.Release();
            }
        }

        // --- ฟีเจอร์ใหม่: เมธอดสำหรับตั้งค่าพารามิเตอร์ ---
        public Task SetMeasurementRangeAsync(MeasurementFunction func, string range)
        {
            string cmd = GetCommandPrefix(func) + $":RANGE {range}";
            return SendCommandAsync(cmd);
        }

        public Task SetMeasurementSpeedAsync(MeasurementFunction func, string nplc)
        {
            string cmd = GetCommandPrefix(func) + $":NPLC {nplc}";
            return SendCommandAsync(cmd);
        }

        public Task SetRelativeStateAsync(MeasurementFunction func, bool enabled)
        {
            string cmd = GetCommandPrefix(func) + $":NULL:STATE {(enabled ? "ON" : "OFF")}";
            return SendCommandAsync(cmd);
        }

        public Task SetRelativeValueAsync(MeasurementFunction func, double value)
        {
            string cmd = GetCommandPrefix(func) + $":NULL:VALUE {value.ToString(CultureInfo.InvariantCulture)}";
            return SendCommandAsync(cmd);
        }

        public Task SetRelativeValueAutoAsync(MeasurementFunction func)
        {
            string cmd = GetCommandPrefix(func) + ":NULL:VALUE:AUTO ON";
            return SendCommandAsync(cmd);
        }

        // --- เมธอดช่วย: ดึงคำสั่งตั้งต้นของแต่ละฟังก์ชัน ---
        private string GetCommandPrefix(MeasurementFunction func)
        {
            switch (func)
            {
                case MeasurementFunction.VoltageDC: return "SENS:VOLT:DC";
                case MeasurementFunction.VoltageAC: return "SENS:VOLT:AC";
                case MeasurementFunction.CurrentDC: return "SENS:CURR:DC";
                case MeasurementFunction.CurrentAC: return "SENS:CURR:AC";
                case MeasurementFunction.Resistance2W: return "SENS:RES";
                case MeasurementFunction.Resistance4W: return "SENS:FRES";
                case MeasurementFunction.Capacitance: return "SENS:CAP";
                case MeasurementFunction.Frequency: return "SENS:FREQ";
                case MeasurementFunction.Diode: return "SENS:DIOD"; // Diode might not have all these settings
                case MeasurementFunction.Temperature: return "SENS:TEMP";
                default: throw new ArgumentException("Unsupported function for parameter setting");
            }
        }


        public async Task StartContinuousReadingAsync(MeasurementFunction function)
        {
            await StopContinuousReadingAsync();
            if (!IsConnected) throw new InvalidOperationException("Not connected to device");

            _isReading = true;
            _ctsContinuousRead = new CancellationTokenSource();
            var token = _ctsContinuousRead.Token;

            string configCommand = GetConfigCommand(function);
            string unit = GetUnit(function);

            try
            {
                Debug.WriteLine($"Starting continuous reading for {function}");
                await SendCommandAsync(configCommand);
                await Task.Delay(200);

                _continuousReadTask = Task.Run(async () =>
                {
                    while (!token.IsCancellationRequested && _isReading && IsConnected)
                    {
                        try
                        {
                            string response = await QueryCommandAsync("READ?");
                            if (!string.IsNullOrEmpty(response))
                            {
                                Debug.WriteLine($"READ? response: {response}");
                                if (double.TryParse(response, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
                                {
                                    ReadingReceived?.Invoke(this, new MeasurementResult(value, unit, function));
                                }
                            }
                            await Task.Delay(200, token); // Delay between readings
                        }
                        catch (OperationCanceledException) { break; }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Reading error: {ex.GetType().Name} - {ex.Message}");
                            await Task.Delay(500, token);
                        }
                    }
                    Debug.WriteLine("Continuous reading loop ended");
                }, token);
            }
            catch (Exception ex)
            {
                _isReading = false;
                _ctsContinuousRead?.Cancel();
                throw new Exception($"Failed to start continuous reading: {ex.Message}", ex);
            }
        }

        public async Task StopContinuousReadingAsync()
        {
            _isReading = false;
            if (_ctsContinuousRead != null)
            {
                _ctsContinuousRead.Cancel();
                if (_continuousReadTask != null)
                {
                    try
                    {
                        await _continuousReadTask;
                    }
                    catch (OperationCanceledException) { /* Expected */ }
                }
                _ctsContinuousRead.Dispose();
                _ctsContinuousRead = null;
                _continuousReadTask = null;
            }

            if (IsConnected)
            {
                try
                {
                    await SendCommandAsync("ABOR");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error aborting measurement: {ex.Message}");
                }
            }
        }

        private void HandleConnectionLost()
        {
            if (IsConnected)
            {
                _isReading = false;
                _ctsContinuousRead?.Cancel();
                Task.Run(() => ConnectionLost?.Invoke(this, EventArgs.Empty));
            }
        }

        public async Task<bool> ReconnectAsync()
        {
            Disconnect();
            await Task.Delay(500);
            try
            {
                await ConnectAsync();
                return IsConnected;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Reconnect failed: {ex.Message}");
                return false;
            }
        }

        private string GetConfigCommand(MeasurementFunction func)
        {
            switch (func)
            {
                case MeasurementFunction.VoltageDC: return "CONF:VOLT:DC";
                case MeasurementFunction.VoltageAC: return "CONF:VOLT:AC";
                case MeasurementFunction.CurrentDC: return "CONF:CURR:DC";
                case MeasurementFunction.CurrentAC: return "CONF:CURR:AC";
                case MeasurementFunction.Resistance2W: return "CONF:RES";
                case MeasurementFunction.Resistance4W: return "CONF:FRES";
                case MeasurementFunction.Capacitance: return "CONF:CAP";
                case MeasurementFunction.Frequency: return "CONF:FREQ";
                case MeasurementFunction.Temperature: return "CONF:TEMP";
                case MeasurementFunction.Diode: return "CONF:DIOD";
                default: throw new ArgumentException("Invalid measurement function");
            }
        }

        private string GetUnit(MeasurementFunction func)
        {
            switch (func)
            {
                case MeasurementFunction.VoltageDC:
                case MeasurementFunction.VoltageAC:
                case MeasurementFunction.Diode:
                    return "V";
                case MeasurementFunction.CurrentDC:
                case MeasurementFunction.CurrentAC:
                    return "A";
                case MeasurementFunction.Resistance2W:
                case MeasurementFunction.Resistance4W:
                    return "Ω";
                case MeasurementFunction.Capacitance: return "F";
                case MeasurementFunction.Frequency: return "Hz";
                case MeasurementFunction.Temperature: return "°C";
                default: return "";
            }
        }

        public void Disconnect()
        {
            _isReading = false;
            _ctsContinuousRead?.Cancel();
            _ctsContinuousRead?.Dispose();
            _ctsContinuousRead = null;

            lock (_lockObject)
            {
                try
                {
                    _stream?.Close();
                    _stream?.Dispose();
                    _stream = null;
                    _tcpClient?.Close();
                    _tcpClient?.Dispose();
                    _tcpClient = null;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Disconnect error: {ex.Message}");
                }
            }
        }

        public void Dispose()
        {
            Disconnect();
            _commandSemaphore?.Dispose();
        }
    }
}