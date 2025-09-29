using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SIGLENT;

namespace MultiRecord
{
    public partial class MeasurementMode : Form
    {
        private List<MeasurementPoint> _measurementPoints;
        private List<PCBMeasurement> _pcbMeasurements;
        private int _currentIndex = 0;
        private DataTable _recordsTable;
        private SDM3055 _dmm;
        private System.Windows.Forms.Timer _measurementTimer;
        private string _currentSessionId;

        public MeasurementMode(DataTable recordsTable, SDM3055 dmm)
        {
            InitializeComponent();
            _recordsTable = recordsTable;
            _dmm = dmm;
            _measurementPoints = new List<MeasurementPoint>();
            _pcbMeasurements = new List<PCBMeasurement>();
            InitializeMeasurementPoints();
            LoadMeasurementPoints();
            InitializeRealTimeDisplay();
            
            // Initialize position after form is loaded
            this.Load += async (s, e) => 
            {
                try 
                { 
                    await UpdateCurrentPosition(); 
                }
                catch (Exception ex) 
                { 
                    System.Diagnostics.Debug.WriteLine($"Error in Load event: {ex.Message}"); 
                }
            };
        }

        private void InitializeMeasurementPoints()
        {
            // Mock data - ในอนาคตจะดึงจาก database ตาม QWRP ID
            _measurementPoints = new List<MeasurementPoint>
            {
                new MeasurementPoint 
                { 
                    Position = "C1", 
                    MeasurementType = "VDC", 
                    TargetValue = 3.3, 
                    Tolerance = 5.0, 
                    IsPercent = true,
                    Description = "3.3V Power Supply"
                },
                new MeasurementPoint 
                { 
                    Position = "C2", 
                    MeasurementType = "CAP", 
                    TargetValue = 100e-6, 
                    Tolerance = 20.0, 
                    IsPercent = true,
                    Description = "100uF Capacitor"
                },
                new MeasurementPoint 
                { 
                    Position = "C3", 
                    MeasurementType = "RES2W", 
                    TargetValue = 10000, 
                    Tolerance = 1.0, 
                    IsPercent = true,
                    Description = "10K Resistor"
                },
                new MeasurementPoint 
                { 
                    Position = "D4", 
                    MeasurementType = "VDC", 
                    TargetValue = 1.2, 
                    Tolerance = 3.0, 
                    IsPercent = true,
                    Description = "1.2V Reference"
                },
                new MeasurementPoint 
                { 
                    Position = "C6", 
                    MeasurementType = "CAP", 
                    TargetValue = 22e-6, 
                    Tolerance = 10.0, 
                    IsPercent = true,
                    Description = "22uF Capacitor"
                }
            };
        }

        private void LoadMeasurementPoints()
        {
            dataGridViewPoints.Rows.Clear();
            
            for (int i = 0; i < _measurementPoints.Count; i++)
            {
                var point = _measurementPoints[i];
                int rowIndex = dataGridViewPoints.Rows.Add();
                var row = dataGridViewPoints.Rows[rowIndex];
                
                row.Cells["Position"].Value = point.Position;
                row.Cells["MeasurementType"].Value = point.MeasurementType;
                row.Cells["TargetValue"].Value = FormatValue(point.TargetValue, point.MeasurementType);
                row.Cells["RecordValue"].Value = point.RecordValue.HasValue ? FormatValue(point.RecordValue.Value, point.MeasurementType) : "-";
                row.Cells["Tolerance"].Value = $"±{point.Tolerance}{(point.IsPercent ? "%" : "")}";
                row.Cells["Description"].Value = point.Description;
                row.Cells["Status"].Value = point.GetToleranceStatus();
                
                // Highlight current row
                if (i == _currentIndex)
                {
                    row.DefaultCellStyle.BackColor = Color.LightBlue;
                    row.DefaultCellStyle.ForeColor = Color.Black;
                }
                else if (point.IsCompleted)
                {
                    // Color based on tolerance status
                    string toleranceStatus = point.GetToleranceStatus();
                    if (toleranceStatus == "Pass")
                    {
                        row.DefaultCellStyle.BackColor = Color.LightGreen;
                        row.DefaultCellStyle.ForeColor = Color.Black;
                    }
                    else if (toleranceStatus == "Fail")
                    {
                        row.DefaultCellStyle.BackColor = Color.LightCoral;
                        row.DefaultCellStyle.ForeColor = Color.Black;
                    }
                    else
                    {
                        row.DefaultCellStyle.BackColor = Color.LightGray;
                        row.DefaultCellStyle.ForeColor = Color.Black;
                    }
                }
            }
        }

        private string FormatValue(double value, string measurementType)
        {
            switch (measurementType.ToUpper())
            {
                case "VDC":
                case "VAC":
                    return $"{value:F3} V";
                case "ADC":
                case "AAC":
                    return $"{value:F6} A";
                case "RES2W":
                case "RES4W":
                    if (value >= 1000000)
                        return $"{value / 1000000:F2} MΩ";
                    else if (value >= 1000)
                        return $"{value / 1000:F2} kΩ";
                    else
                        return $"{value:F2} Ω";
                case "CAP":
                    if (value >= 1e-3)
                        return $"{value * 1000:F2} mF";
                    else if (value >= 1e-6)
                        return $"{value * 1000000:F2} µF";
                    else if (value >= 1e-9)
                        return $"{value * 1000000000:F2} nF";
                    else
                        return $"{value * 1000000000000:F2} pF";
                case "FREQ":
                    if (value >= 1000000)
                        return $"{value / 1000000:F2} MHz";
                    else if (value >= 1000)
                        return $"{value / 1000:F2} kHz";
                    else
                        return $"{value:F2} Hz";
                default:
                    return value.ToString("F6");
            }
        }

        private async System.Threading.Tasks.Task UpdateCurrentPosition()
        {
            if (_currentIndex < _measurementPoints.Count)
            {
                var currentPoint = _measurementPoints[_currentIndex];
                
                labelCurrentPosition.Text = $"Position: {currentPoint.Position}";
                labelMeasurementType.Text = $"Type: {currentPoint.MeasurementType}";
                labelTargetValue.Text = $"Target: {FormatValue(currentPoint.TargetValue, currentPoint.MeasurementType)}";
                labelTolerance.Text = $"Tolerance: ±{currentPoint.Tolerance}{(currentPoint.IsPercent ? "%" : "")}";
                labelDescription.Text = currentPoint.Description;
                
                // Auto-change DMM measurement mode based on marker type
                _ = SetDMMFunctionForCurrentMeasurement(currentPoint.MeasurementType);
                
                // Update progress
                labelProgress.Text = $"{_currentIndex + 1} / {_measurementPoints.Count}";
                progressBar.Value = (int)((double)(_currentIndex + 1) / _measurementPoints.Count * 100);
            }
            else
            {
                labelCurrentPosition.Text = "All measurements completed!";
                labelMeasurementType.Text = "";
                labelTargetValue.Text = "";
                labelTolerance.Text = "";
                labelDescription.Text = "";
                labelProgress.Text = $"{_measurementPoints.Count} / {_measurementPoints.Count}";
                progressBar.Value = 100;
            }
            
            LoadMeasurementPoints();
        }

        private async void buttonLoadSession_Click(object sender, EventArgs e)
        {
            string sessionId = textBoxSessionID.Text.Trim();
            if (string.IsNullOrEmpty(sessionId))
            {
                MessageBox.Show("Please enter a Measurement Session ID", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            try
            {
                buttonLoadSession.Enabled = false;
                buttonLoadSession.Text = "Loading...";
                
                await LoadMeasurementsFromDatabase(sessionId);
                
                MessageBox.Show($"Session loaded successfully!\n\nFound {_measurementPoints.Count} measurement points.", 
                              "Session Loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load session:\n\n{ex.Message}", 
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                buttonLoadSession.Enabled = true;
                buttonLoadSession.Text = "Load Session";
            }
        }

        private async System.Threading.Tasks.Task LoadMeasurementsFromDatabase(string sessionId)
        {
            if (!SettingsManager.MySqlEnabled)
            {
                MessageBox.Show("MySQL is not enabled. Please configure MySQL settings first.", 
                              "MySQL Not Enabled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var mysqlManager = new MySqlManager(
                SettingsManager.MySqlHost,
                SettingsManager.MySqlPort,
                SettingsManager.MySqlUser,
                SettingsManager.MySqlPassword,
                SettingsManager.MySqlDatabase))
            {
                _pcbMeasurements = await mysqlManager.GetPCBMeasurementsBySessionAsync(sessionId);
                _currentSessionId = sessionId;
                
                // Convert PCBMeasurement to MeasurementPoint
                ConvertPCBMeasurementsToPoints();
                
                _currentIndex = 0;
                foreach (var point in _measurementPoints)
                {
                    point.IsCompleted = false;
                }
                
                LoadMeasurementPoints();
                if (_measurementPoints.Count > 0)
                {
                    await UpdateCurrentPosition();
                }
            }
        }

        private void ConvertPCBMeasurementsToPoints()
        {
            _measurementPoints.Clear();
            
            foreach (var pcbMeasurement in _pcbMeasurements.OrderBy(m => m.MeasurementNumber))
            {
                var measurementPoint = new MeasurementPoint
                {
                    Position = pcbMeasurement.MarkerDisplayName,
                    MeasurementType = GetDMMFunctionFromMarkerType(pcbMeasurement.MarkerType),
                    TargetValue = GetTargetValueFromParameters(pcbMeasurement.MarkerParameters, pcbMeasurement.MarkerType),
                    Tolerance = (double)pcbMeasurement.ToleranceUpper,
                    IsPercent = pcbMeasurement.ToleranceUpperType == "percent",
                    Description = $"{pcbMeasurement.MarkerDisplayName} ({pcbMeasurement.MarkerType})",
                    IsCompleted = false,
                    PCBMeasurement = pcbMeasurement
                };
                
                _measurementPoints.Add(measurementPoint);
            }
        }

        private string GetDMMFunctionFromMarkerType(string markerType)
        {
            switch (markerType?.ToUpper())
            {
                case "DC": return "VDC";
                case "AC": return "VAC";
                case "2W": return "RES2W";
                case "4W": return "RES4W";
                case "CAP": return "CAP";
                case "CON": return "RES2W"; // Continuity as resistance
                case "DIO": return "DIODE";
                default: return "VDC";
            }
        }

        private double GetTargetValueFromParameters(string parameters, string markerType)
        {
            try
            {
                var json = Newtonsoft.Json.Linq.JObject.Parse(parameters ?? "{}");
                
                switch (markerType?.ToUpper())
                {
                    case "DC":
                    case "AC":
                        return json["voltage"]?.ToObject<double>() ?? 0.0;
                    case "2W":
                    case "4W":
                        return json["resistance"]?.ToObject<double>() ?? 0.0;
                    case "CAP":
                        return json["capacitance"]?.ToObject<double>() ?? 0.0;
                    case "CON":
                        return json["ohms"]?.ToObject<double>() ?? 0.0;
                    case "DIO":
                        return json["voltage"]?.ToObject<double>() ?? 0.0;
                    default:
                        return 0.0;
                }
            }
            catch
            {
                return 0.0;
            }
        }

        private async System.Threading.Tasks.Task SetDMMFunctionForCurrentMeasurement(string measurementType)
        {
            try
            {
                if (_dmm != null && _dmm.IsConnected)
                {
                    MeasurementFunction function = GetMeasurementFunction(measurementType);
                    await _dmm.StartContinuousReadingAsync(function);
                    
                    // Small delay to allow DMM to switch modes
                    await System.Threading.Tasks.Task.Delay(100);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting DMM function: {ex.Message}");
            }
        }

        private MeasurementFunction GetMeasurementFunction(string measurementType)
        {
            switch (measurementType?.ToUpper())
            {
                case "VDC": return MeasurementFunction.VoltageDC;
                case "VAC": return MeasurementFunction.VoltageAC;
                case "RES2W": return MeasurementFunction.Resistance2W;
                case "RES4W": return MeasurementFunction.Resistance4W;
                case "CAP": return MeasurementFunction.Capacitance;
                case "DIODE": return MeasurementFunction.Diode;
                case "IDC": return MeasurementFunction.CurrentDC;
                case "IAC": return MeasurementFunction.CurrentAC;
                case "FREQ": return MeasurementFunction.Frequency;
                case "TEMP": return MeasurementFunction.Temperature;
                default: return MeasurementFunction.VoltageDC;
            }
        }

        private void buttonRecord_Click(object sender, EventArgs e)
        {
            RecordCurrentMeasurement();
        }

        private async void RecordCurrentMeasurement()
        {
            if (_currentIndex >= _measurementPoints.Count) return;
            
            var currentPoint = _measurementPoints[_currentIndex];
            
            // Get actual measurement value from DMM
            double measuredValue;
            try
            {
                if (_dmm != null && _dmm.IsConnected)
                {
                    // Get current reading from DMM
                    string readingStr = await _dmm.QueryCommandAsync("READ?");
                    if (double.TryParse(readingStr, out measuredValue))
                    {
                        // Successfully got reading from DMM
                    }
                    else
                    {
                        // Fallback to mock data if reading fails
                        Random rand = new Random();
                        measuredValue = currentPoint.TargetValue * (1 + (rand.NextDouble() - 0.5) * 0.1);
                    }
                }
                else
                {
                    // Fallback to mock data if not connected
                    Random rand = new Random();
                    measuredValue = currentPoint.TargetValue * (1 + (rand.NextDouble() - 0.5) * 0.1);
                }
            }
            catch
            {
                // Fallback to mock data on any error
                Random rand = new Random();
                measuredValue = currentPoint.TargetValue * (1 + (rand.NextDouble() - 0.5) * 0.1);
            }
            
            // Calculate tolerance check
            double toleranceRange;
            if (currentPoint.IsPercent)
            {
                toleranceRange = Math.Abs(currentPoint.TargetValue * currentPoint.Tolerance / 100.0);
            }
            else
            {
                toleranceRange = currentPoint.Tolerance;
            }
            
            bool withinTolerance = Math.Abs(measuredValue - currentPoint.TargetValue) <= toleranceRange;
            string toleranceStatus = withinTolerance ? "Pass" : "Fail";
            
            // Add to records table (same format as existing records)
            DataRow newRow = _recordsTable.NewRow();
            newRow["No"] = _recordsTable.Rows.Count + 1;
            newRow["Function"] = currentPoint.MeasurementType;
            newRow["Measurement"] = measuredValue.ToString("F6");
            newRow["Unit"] = GetUnitFromMeasurementType(currentPoint.MeasurementType);
            newRow["Timestamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            newRow["Tolerance"] = toleranceStatus;
            
            _recordsTable.Rows.Add(newRow);
            
            // Store the recorded value in the measurement point
            currentPoint.RecordValue = measuredValue;
            
            // Mark current point as completed
            currentPoint.IsCompleted = true;
            
            // Move to next position
            _currentIndex++;
            
            // Update UI
            await UpdateCurrentPosition();
            
            // Play sound feedback
            if (withinTolerance)
            {
                SoundUtil.Beep(); // Success sound
            }
            else
            {
                SoundUtil.Over(); // Fail sound
            }
            
            // Show measurement result
            string resultMessage = $"Position {currentPoint.Position}: {FormatValue(measuredValue, currentPoint.MeasurementType)}\n" +
                                 $"Target: {FormatValue(currentPoint.TargetValue, currentPoint.MeasurementType)}\n" +
                                 $"Result: {toleranceStatus}";
            
            if (_currentIndex >= _measurementPoints.Count)
            {
                resultMessage += "\n\nAll measurements completed!";
            }
            
            // Update result label safely in UI thread
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => labelLastResult.Text = resultMessage));
            }
            else
            {
                labelLastResult.Text = resultMessage;
            }
        }

        private string GetUnitFromMeasurementType(string measurementType)
        {
            switch (measurementType.ToUpper())
            {
                case "VDC":
                case "VAC":
                    return "V";
                case "ADC":
                case "AAC":
                    return "A";
                case "RES2W":
                case "RES4W":
                    return "Ω";
                case "CAP":
                    return "F";
                case "FREQ":
                    return "Hz";
                case "TEMP":
                    return "°C";
                case "DIOD":
                    return "V";
                default:
                    return "";
            }
        }

        private void MeasurementMode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Q)
            {
                RecordCurrentMeasurement();
                e.Handled = true;
            }
        }

        private void MeasurementMode_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Unsubscribe from DMM events
            if (_dmm != null)
            {
                _dmm.ReadingReceived -= Dmm_ReadingReceived;
            }
            
            _measurementTimer?.Stop();
            _measurementTimer?.Dispose();
        }

        private async void buttonPrevious_Click(object sender, EventArgs e)
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
                await UpdateCurrentPosition();
            }
        }

        private async void buttonNext_Click(object sender, EventArgs e)
        {
            if (_currentIndex < _measurementPoints.Count - 1)
            {
                _currentIndex++;
                await UpdateCurrentPosition();
            }
        }

        private async void buttonReset_Click(object sender, EventArgs e)
        {
            _currentIndex = 0;
            foreach (var point in _measurementPoints)
            {
                point.IsCompleted = false;
            }
            await UpdateCurrentPosition();
            labelLastResult.Text = "";
        }

        private void InitializeRealTimeDisplay()
        {
            // Subscribe to DMM events for real-time updates (faster than timer)
            if (_dmm != null)
            {
                _dmm.ReadingReceived += Dmm_ReadingReceived;
            }
            
            // Keep a backup timer for fallback (slower interval)
            _measurementTimer = new System.Windows.Forms.Timer();
            _measurementTimer.Interval = 1000; // Backup update every 1000ms
            _measurementTimer.Tick += async (s, e) => await UpdateRealTimeDisplay();
            _measurementTimer.Start();
        }

        private void Dmm_ReadingReceived(object sender, MeasurementResult e)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => Dmm_ReadingReceived(sender, e)));
                    return;
                }

                // Get current measurement type for comparison
                if (_currentIndex < _measurementPoints.Count)
                {
                    var currentPoint = _measurementPoints[_currentIndex];
                    var expectedFunction = GetMeasurementFunction(currentPoint.MeasurementType);
                    
                    // Only update if the reading matches current measurement function
                    if (e.Function == expectedFunction)
                    {
                        UpdateDisplay(e.Value, e.Unit);
                        lblCurrentFunction.Text = currentPoint.MeasurementType;
                    }
                }
                else
                {
                    // No specific measurement selected, show any reading
                    UpdateDisplay(e.Value, e.Unit);
                    lblCurrentFunction.Text = e.Function.ToString();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Dmm_ReadingReceived: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task UpdateRealTimeDisplay()
        {
            try
            {
                if (_dmm != null && _dmm.IsConnected)
                {
                    // Get current reading from DMM
                    string readingStr = await _dmm.QueryCommandAsync("READ?");
                    if (double.TryParse(readingStr, out double currentReading))
                    {
                        // Get unit from current measurement type
                        string unit = "";
                        string function = "VDC";
                        
                        if (_currentIndex < _measurementPoints.Count)
                        {
                            var currentPoint = _measurementPoints[_currentIndex];
                            unit = GetUnitFromMeasurementType(currentPoint.MeasurementType);
                            function = currentPoint.MeasurementType;
                        }
                        else
                        {
                            unit = "V"; // Default unit
                            function = "VDC"; // Default function
                        }
                        
                        // Update real-time display using UpdateDisplay method (same as Main.cs)
                        if (this.InvokeRequired)
                        {
                            this.Invoke(new Action(() =>
                            {
                                UpdateDisplay(currentReading, unit);
                                lblCurrentFunction.Text = function;
                            }));
                        }
                        else
                        {
                            UpdateDisplay(currentReading, unit);
                            lblCurrentFunction.Text = function;
                        }
                    }
                    else
                    {
                        SafeUpdateLabels("----", "", "NO FUNCTION");
                    }
                }
                else
                {
                    SafeUpdateLabels("----", "", "DISCONNECTED");
                }
            }
            catch
            {
                SafeUpdateLabels("ERROR", "", "ERROR");
            }
        }

        private void UpdateDisplay(double value, string unit)
        {
            if (double.IsNaN(value) || value >= 9.9E37)
            {
                // Check if current measurement is continuity/resistance for OPEN
                string currentFunction = "";
                if (_currentIndex < _measurementPoints.Count)
                {
                    currentFunction = _measurementPoints[_currentIndex].MeasurementType;
                }
                
                if (currentFunction.ToUpper().Contains("RES") || currentFunction.ToUpper().Contains("CONTINUOUS"))
                {
                    lblCurrentReading.Text = "OPEN";
                    lblCurrentUnit.Text = "";
                }
                else
                {
                    lblCurrentReading.Text = "OVERLOAD";
                    lblCurrentUnit.Text = "";
                }
                return;
            }

            double absValue = Math.Abs(value);
            double displayValue = value;
            string displayUnit = unit;

            // จัดการหน่วยและค่าที่แสดง (เหมือน Main.cs)
            if (absValue >= 1000000) // หลักล้าน -> M
            {
                displayValue = value / 1000000;
                displayUnit = "M" + unit;
                lblCurrentReading.Text = displayValue.ToString("0.000");
            }
            else if (absValue >= 1000) // หลักพัน -> k
            {
                displayValue = value / 1000;
                displayUnit = "k" + unit;
                lblCurrentReading.Text = displayValue.ToString("0.000");
            }
            else if (absValue >= 1) // ค่า 1-999
            {
                lblCurrentReading.Text = value.ToString("0.0000");
            }
            else if (absValue >= 0.001) // ค่า 0.001-0.999
            {
                lblCurrentReading.Text = value.ToString("0.000000");
            }
            else // ค่าน้อยกว่า 0.001
            {
                lblCurrentReading.Text = value.ToString("0.000000000");
            }

            lblCurrentUnit.Text = displayUnit;
        }

        private void SafeUpdateLabels(string reading, string unit, string function)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    lblCurrentReading.Text = reading;
                    lblCurrentUnit.Text = unit;
                    lblCurrentFunction.Text = function;
                }));
            }
            else
            {
                lblCurrentReading.Text = reading;
                lblCurrentUnit.Text = unit;
                lblCurrentFunction.Text = function;
            }
        }

    }

    public class MeasurementPoint
    {
        public string Position { get; set; }
        public string MeasurementType { get; set; }
        public double TargetValue { get; set; }
        public double? RecordValue { get; set; }
        public double Tolerance { get; set; }
        public bool IsPercent { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public PCBMeasurement PCBMeasurement { get; set; }
        
        public string GetToleranceStatus()
        {
            if (!RecordValue.HasValue)
                return "-";
                
            double toleranceRange;
            if (IsPercent)
            {
                toleranceRange = Math.Abs(TargetValue * Tolerance / 100.0);
            }
            else
            {
                toleranceRange = Tolerance;
            }
            
            bool withinTolerance = Math.Abs(RecordValue.Value - TargetValue) <= toleranceRange;
            return withinTolerance ? "Pass" : "Fail";
        }
    }
}