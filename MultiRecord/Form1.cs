using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media; // เพิ่ม namespace นี้
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SIGLENT;


namespace MultiRecord
{
    public partial class Form1 : Form
    {
        private SDM3055 _dmm;
        private MeasurementFunction _currentFunction;
        private Button _activeButton;
        private bool _isChangingFunction = false;
        private bool _isProgrammaticallyChangingParams = false;
        private bool _isRelativeEnabled = false;

        private DataTable _recordsTable;
        private double _lastReadingValue;
        private string _lastReadingUnit;
        private bool _isOverload = false;

        private readonly string _dataFilePath;
        private readonly string _appDataFolder;
        private readonly string _settingsFilePath;

        public Form1()
        {
            InitializeComponent();

            _appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MultiRecordApp");
            _dataFilePath = Path.Combine(_appDataFolder, "measurement_data.csv");
            _settingsFilePath = Path.Combine(_appDataFolder, "settings.ini");

            InitializeForm();
            InitializeDataTableAndLoadData();
            LoadLastSuccessfulConnection();
        }

        private void InitializeForm()
        {
            lblReading.Font = new Font("Consolas", 48F, FontStyle.Bold);
            lblUnit.Font = new Font("Consolas", 28F, FontStyle.Bold);

            UpdateConnectionStatus(false);
            panelParameters.Visible = false;
            LogActivity("แอปพลิเคชันเริ่มต้นแล้ว ยินดีต้อนรับ!");
        }

        // *** ปรับปรุง InitializeDataTableAndLoadData() ***
        private void InitializeDataTableAndLoadData()
        {
            _recordsTable = new DataTable("MeasurementRecords");
            _recordsTable.Columns.Add("No", typeof(int));
            _recordsTable.Columns.Add("Function", typeof(string));
            _recordsTable.Columns.Add("Measurement", typeof(string));
            _recordsTable.Columns.Add("Unit", typeof(string));
            _recordsTable.Columns.Add("Timestamp", typeof(string));

            dataGridViewRecords.DataSource = _recordsTable;

            // ตั้งค่า AutoSizeMode
            dataGridViewRecords.Columns["No"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewRecords.Columns["Function"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewRecords.Columns["Measurement"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewRecords.Columns["Unit"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewRecords.Columns["Timestamp"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            // *** เพิ่มการตั้งค่า Selection Mode ***
            dataGridViewRecords.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewRecords.MultiSelect = true; // เปิดให้เลือกหลายแถวได้ (สำหรับการลบ)

            // ปรับแต่งการแสดงผลให้แสดง measurement + unit รวมกัน
            dataGridViewRecords.CellFormatting += DataGridViewRecords_CellFormatting;

            LoadDataFromFile();

            // *** SelectLatestRow() จะถูกเรียกใน LoadDataFromFile() แล้ว ***
        }

        // ========= Connection and Reconnection =========
        private async void buttonConnect_Click(object sender, EventArgs e)
        {
            // Validate input first
            if (!ValidateConnectionInput())
                return;
                
            buttonConnect.Enabled = false;
            buttonConnect.Text = "Connecting...";
            LogActivity($"พยายามเชื่อมต่อ {textBoxIP.Text}:{textBoxPort.Text}");

            try
            {
                _dmm = new SDM3055(textBoxIP.Text, int.Parse(textBoxPort.Text));
                _dmm.ConnectionLost += (s, args) => Dmm_ConnectionLost();
                _dmm.ReadingReceived += Dmm_ReadingReceived;

                await _dmm.ConnectAsync();

                string idn = await _dmm.QueryCommandAsync("*IDN?");
                LogActivity($"เชื่อมต่อสำเร็จ: {idn}");
                textBoxSystemInfo.Text = idn;
                UpdateConnectionStatus(true);

                // บันทึกการตั้งค่าการเชื่อมต่อที่สำเร็จ
                SaveLastSuccessfulConnection(textBoxIP.Text, textBoxPort.Text);

                await SetActiveMeasurementAsync(MeasurementFunction.VoltageDC, buttonMeasureVDC);
            }
            catch (Exception ex)
            {
                LogActivity($"เชื่อมต่อล้มเหลว: {ex.Message}", true);
                
                // Show detailed error message with suggestions
                string errorMessage = $"ไม่สามารถเชื่อมต่อได้:\n{ex.Message}\n\n";
                errorMessage += "กรุณาตรวจสอบ:\n";
                errorMessage += "1. IP Address และ Port ถูกต้อง\n";
                errorMessage += "2. เครื่องมือเปิดอยู่และเชื่อมต่อเครือข่าย\n";
                errorMessage += "3. ไฟร์วอลล์ไม่ได้บล็อกการเชื่อมต่อ\n";
                errorMessage += "4. เครื่องมือไม่ได้ถูกใช้งานโดยโปรแกรมอื่น";
                
                MessageBox.Show(errorMessage, "ข้อผิดพลาดการเชื่อมต่อ", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                UpdateConnectionStatus(false);
                _dmm?.Dispose();
                _dmm = null;
            }
            finally
            {
                buttonConnect.Enabled = true;
                buttonConnect.Text = "Connect";
            }
        }

        private async void buttonDisconnect_Click(object sender, EventArgs e)
        {
            // Disable button during disconnection
            buttonDisconnect.Enabled = false;
            buttonDisconnect.Text = "Disconnecting...";
            
            try
            {
                LogActivity("กำลังตัดการเชื่อมต่อ...");
                
                if (_dmm != null)
                {
                    // Stop continuous reading first
                    await _dmm.StopContinuousReadingAsync();
                    
                    // Disconnect and dispose
                    _dmm.Disconnect();
                    _dmm.Dispose();
                    _dmm = null;
                }
                
                UpdateConnectionStatus(false);
                LogActivity("ตัดการเชื่อมต่อแล้ว");
                
                // Show success message
                MessageBox.Show("ตัดการเชื่อมต่อเรียบร้อยแล้ว", "สำเร็จ", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogActivity($"เกิดข้อผิดพลาดขณะตัดการเชื่อมต่อ: {ex.Message}", true);
                MessageBox.Show($"เกิดข้อผิดพลาดขณะตัดการเชื่อมต่อ:\n{ex.Message}", 
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                
                // Force cleanup anyway
                if (_dmm != null)
                {
                    try { _dmm.Dispose(); } catch { }
                    _dmm = null;
                }
                UpdateConnectionStatus(false);
            }
            finally
            {
                // Re-enable the button (but it should be disabled by UpdateConnectionStatus)
                buttonDisconnect.Text = "Disconnect";
            }
        }

        private void Dmm_ConnectionLost()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(Dmm_ConnectionLost));
                return;
            }
            
            LogActivity("การเชื่อมต่อขาดหาย!", true);
            UpdateConnectionStatus(false);
            
            // Show connection lost notification
            MessageBox.Show("การเชื่อมต่อกับเครื่องมือขาดหาย!\nโปรดตรวจสอบการเชื่อมต่อและลองเชื่อมต่อใหม่", 
                "การเชื่อมต่อขาดหาย", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            
            // Clean up the connection object
            if (_dmm != null)
            {
                try
                {
                    _dmm.Dispose();
                }
                catch (Exception ex)
                {
                    LogActivity($"ข้อผิดพลาดขณะล้างข้อมูลการเชื่อมต่อ: {ex.Message}", true);
                }
                finally
                {
                    _dmm = null;
                }
            }
        }

        // ========= Measurement and Parameter Control =========
        private async Task SetActiveMeasurementAsync(MeasurementFunction function, Button clickedButton)
        {
            if (_dmm == null || !_dmm.IsConnected)
            {
                LogActivity("ไม่สามารถเปลี่ยนฟังก์ชันได้: ไม่ได้เชื่อมต่อ", true);
                return;
            }
            if (_isChangingFunction) return;

            _isChangingFunction = true;
            panelParameters.Visible = false;

            try
            {
                SetFunctionButtonsEnabled(false);
                LogActivity($"กำลังเปลี่ยนฟังก์ชันเป็น: {function}");

                await _dmm.StopContinuousReadingAsync();
                await Task.Delay(100);

                _currentFunction = function;
                _activeButton = clickedButton;

                UpdateButtonStyles(clickedButton);
                UpdateParameterControls(function);

                lblMeasurementType.Text = function.ToString().ToUpper();
                lblReading.Text = "CONFIG...";
                lblUnit.Text = "";

                await _dmm.StartContinuousReadingAsync(function);
                LogActivity($"เปลี่ยนฟังก์ชันเป็น {function} สำเร็จ");
                panelParameters.Visible = true;
            }
            catch (Exception ex)
            {
                LogActivity($"เกิดข้อผิดพลาดในการเปลี่ยนฟังก์ชัน: {ex.Message}", true);
                if (_activeButton != null)
                {
                    _activeButton.BackColor = Color.FromArgb(63, 63, 70);
                    _activeButton.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
                }
                lblMeasurementType.Text = "ERROR";
            }
            finally
            {
                _isChangingFunction = false;
                SetFunctionButtonsEnabled(true);
            }
        }

        private async void comboBoxRange_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isProgrammaticallyChangingParams || _dmm == null || !_dmm.IsConnected) return;

            string selectedRange = ((KeyValuePair<string, string>)comboBoxRange.SelectedItem).Key;
            LogActivity($"กำลังเปลี่ยน Range เป็น: {selectedRange}");
            try
            {
                await _dmm.SetMeasurementRangeAsync(_currentFunction, selectedRange);
                LogActivity("เปลี่ยน Range สำเร็จ");
            }
            catch (Exception ex)
            {
                LogActivity($"เปลี่ยน Range ล้มเหลว: {ex.Message}", true);
            }
        }

        private async void comboBoxSpeed_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isProgrammaticallyChangingParams || _dmm == null || !_dmm.IsConnected) return;

            string selectedSpeed = ((KeyValuePair<string, string>)comboBoxSpeed.SelectedItem).Key;
            LogActivity($"กำลังเปลี่ยน Speed (NPLC) เป็น: {selectedSpeed}");
            try
            {
                await _dmm.SetMeasurementSpeedAsync(_currentFunction, selectedSpeed);
                LogActivity("เปลี่ยน Speed สำเร็จ");
            }
            catch (Exception ex)
            {
                LogActivity($"เปลี่ยน Speed ล้มเหลว: {ex.Message}", true);
            }
        }

        private async void buttonSetRelative_Click(object sender, EventArgs e)
        {
            if (_dmm == null || !_dmm.IsConnected) return;

            _isRelativeEnabled = !_isRelativeEnabled;
            LogActivity($"กำลังตั้งค่า Relative เป็น: {(_isRelativeEnabled ? "ON" : "OFF")}");

            try
            {
                await _dmm.SetRelativeStateAsync(_currentFunction, _isRelativeEnabled);

                if (_isRelativeEnabled)
                {
                    await _dmm.SetRelativeValueAutoAsync(_currentFunction);
                    LogActivity("เปิดใช้งาน Relative และใช้ค่าปัจจุบันเป็นค่าอ้างอิง");
                }
                else
                {
                    LogActivity("ปิดใช้งาน Relative");
                }
                UpdateRelativeUI();
            }
            catch (Exception ex)
            {
                _isRelativeEnabled = !_isRelativeEnabled;
                LogActivity($"ตั้งค่า Relative ล้มเหลว: {ex.Message}", true);
            }
        }

        // ========= UI Update Helpers =========
        private void UpdateParameterControls(MeasurementFunction function)
        {
            _isProgrammaticallyChangingParams = true;

            var rangeSource = GetRangeOptions(function);
            var speedSource = GetSpeedOptions(function);

            labelRange.Visible = rangeSource.Any();
            comboBoxRange.Visible = rangeSource.Any();
            if (rangeSource.Any())
            {
                comboBoxRange.DataSource = new BindingSource(rangeSource, null);
                comboBoxRange.DisplayMember = "Value";
                comboBoxRange.ValueMember = "Key";
                comboBoxRange.SelectedIndex = 0;
            }

            labelSpeed.Visible = speedSource.Any();
            comboBoxSpeed.Visible = speedSource.Any();
            if (speedSource.Any())
            {
                comboBoxSpeed.DataSource = new BindingSource(speedSource, null);
                comboBoxSpeed.DisplayMember = "Value";
                comboBoxSpeed.ValueMember = "Key";
                comboBoxSpeed.SelectedIndex = speedSource.Count - 1;
            }

            bool supportsRelative = SupportsRelative(function);
            buttonSetRelative.Visible = supportsRelative;
            labelRelativeState.Visible = supportsRelative;
            if (supportsRelative)
            {
                _isRelativeEnabled = false;
                UpdateRelativeUI();
            }

            _isProgrammaticallyChangingParams = false;
        }

        private void UpdateRelativeUI()
        {
            if (_isRelativeEnabled)
            {
                labelRelativeState.Text = "REL: ON";
                labelRelativeState.ForeColor = Color.FromArgb(115, 255, 127);
            }
            else
            {
                labelRelativeState.Text = "REL: OFF";
                labelRelativeState.ForeColor = Color.Gainsboro;
            }
        }

        private Dictionary<string, string> GetRangeOptions(MeasurementFunction func)
        {
            switch (func)
            {
                case MeasurementFunction.VoltageDC:
                case MeasurementFunction.VoltageAC:
                    return new Dictionary<string, string>
                    {
                        { "AUTO", "Auto" }, { "0.2", "200 mV" }, { "2", "2 V" },
                        { "20", "20 V" }, { "200", "200 V" }, { "1000", "1000 V" }
                    };
                case MeasurementFunction.Resistance2W:
                case MeasurementFunction.Resistance4W:
                    return new Dictionary<string, string>
                    {
                        { "AUTO", "Auto" }, { "200", "200 Ω" }, { "2E3", "2 kΩ" },
                        { "20E3", "20 kΩ" }, { "200E3", "200 kΩ" }, { "2E6", "2 MΩ" },
                        { "10E6", "10 MΩ" }, { "100E6", "100 MΩ" }
                    };
                case MeasurementFunction.CurrentDC:
                case MeasurementFunction.CurrentAC:
                    return new Dictionary<string, string>
                     {
                        { "AUTO", "Auto" }, { "200E-6", "200 µA" }, { "2E-3", "2 mA" },
                        { "20E-3", "20 mA" }, { "200E-3", "200 mA" }, { "2", "2 A" }, { "10", "10 A" }
                     };
                default:
                    return new Dictionary<string, string>();
            }
        }
        private void SelectLatestRow()
        {
            if (dataGridViewRecords.Rows.Count > 0)
            {
                // Clear selection ก่อน
                dataGridViewRecords.ClearSelection();

                int lastIndex = dataGridViewRecords.Rows.Count - 1;

                // Select แถวล่าสุด
                dataGridViewRecords.Rows[lastIndex].Selected = true;

                // Scroll ไปที่แถวล่าสุด
                dataGridViewRecords.FirstDisplayedScrollingRowIndex = lastIndex;

                // Set current cell ให้เป็นแถวล่าสุด
                dataGridViewRecords.CurrentCell = dataGridViewRecords.Rows[lastIndex].Cells[0];
            }
        }
        private Dictionary<string, string> GetSpeedOptions(MeasurementFunction func)
        {
            switch (func)
            {
                case MeasurementFunction.VoltageDC:
                case MeasurementFunction.CurrentDC:
                case MeasurementFunction.Resistance2W:
                case MeasurementFunction.Resistance4W:
                    return new Dictionary<string, string>
                    {
                        { "0.02", "Fast" }, { "0.2", "Medium" }, { "1", "Slow (1 PLC)" },
                        { "10", "Very Slow (10 PLC)" }
                    };
                default:
                    return new Dictionary<string, string>();
            }
        }

        private bool SupportsRelative(MeasurementFunction func)
        {
            switch (func)
            {
                case MeasurementFunction.VoltageDC:
                case MeasurementFunction.VoltageAC:
                case MeasurementFunction.CurrentDC:
                case MeasurementFunction.CurrentAC:
                case MeasurementFunction.Resistance2W:
                case MeasurementFunction.Resistance4W:
                case MeasurementFunction.Capacitance:
                case MeasurementFunction.Temperature:
                    return true;
                default:
                    return false;
            }
        }

        private void Dmm_ReadingReceived(object sender, MeasurementResult e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => Dmm_ReadingReceived(sender, e)));
                return;
            }

            if (e.Function == _currentFunction)
            {
                UpdateDisplay(e.Value, e.Unit);
            }
        }

        // ปรับปรุง UpdateDisplay ให้แสดงค่าเหมือนมิเตอร์ แต่เพิ่มการแปลงหน่วย k และ M
        private void UpdateDisplay(double value, string unit)
        {
            if (!IsHandleCreated || IsDisposed) return;

            _lastReadingValue = value;
            _lastReadingUnit = unit;

            if (double.IsNaN(value) || value >= 9.9E37)
            {
                lblReading.Text = "OVERLOAD";
                lblUnit.Text = "";
                _isOverload = true;
                return;
            }
            _isOverload = false;

            double absValue = Math.Abs(value);
            double displayValue = value;
            string displayUnit = unit;

            // จัดการหน่วยและค่าที่แสดง
            if (absValue >= 1000000) // หลักล้าน -> M
            {
                displayValue = value / 1000000;
                displayUnit = "M" + unit;
                lblReading.Text = displayValue.ToString("0.000");
            }
            else if (absValue >= 1000) // หลักพัน -> k
            {
                displayValue = value / 1000;
                displayUnit = "k" + unit;
                lblReading.Text = displayValue.ToString("0.000");
            }
            else if (absValue >= 1) // ค่า 1-999
            {
                lblReading.Text = value.ToString("0.0000");
            }
            else if (absValue >= 0.001) // ค่า 0.001-0.999
            {
                lblReading.Text = value.ToString("0.000000");
            }
            else // ค่าน้อยกว่า 0.001
            {
                lblReading.Text = value.ToString("0.000000000");
            }

            lblUnit.Text = displayUnit;
        }

        private void UpdateConnectionStatus(bool isConnected)
        {
            groupBoxConnection.Enabled = !isConnected;
            buttonDisconnect.Enabled = isConnected;
            groupBoxFunctions.Enabled = isConnected;
            groupBoxSystem.Enabled = isConnected;
            groupBoxRecord.Enabled = isConnected;
            panelParameters.Visible = isConnected;

            if (isConnected)
            {
                labelStatus.Text = "สถานะ: เชื่อมต่อแล้ว";
                labelStatus.ForeColor = Color.FromArgb(115, 255, 127);
            }
            else
            {
                labelStatus.Text = "สถานะ: ไม่ได้เชื่อมต่อ";
                labelStatus.ForeColor = Color.OrangeRed;
                panelParameters.Visible = false;
                if (_activeButton != null)
                {
                    _activeButton.BackColor = Color.FromArgb(63, 63, 70);
                    _activeButton.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
                    _activeButton = null;
                }
                ClearDisplayReadings();
            }
        }

        private async void buttonMeasureVDC_Click(object sender, EventArgs e) => await SetActiveMeasurementAsync(MeasurementFunction.VoltageDC, (Button)sender);
        private async void buttonMeasureVAC_Click(object sender, EventArgs e) => await SetActiveMeasurementAsync(MeasurementFunction.VoltageAC, (Button)sender);
        private async void buttonMeasureIDC_Click(object sender, EventArgs e) => await SetActiveMeasurementAsync(MeasurementFunction.CurrentDC, (Button)sender);
        private async void buttonMeasureIAC_Click(object sender, EventArgs e) => await SetActiveMeasurementAsync(MeasurementFunction.CurrentAC, (Button)sender);
        private async void buttonMeasureRes2W_Click(object sender, EventArgs e) => await SetActiveMeasurementAsync(MeasurementFunction.Resistance2W, (Button)sender);
        private async void buttonMeasureRes4W_Click(object sender, EventArgs e) => await SetActiveMeasurementAsync(MeasurementFunction.Resistance4W, (Button)sender);
        private async void buttonMeasureCap_Click(object sender, EventArgs e) => await SetActiveMeasurementAsync(MeasurementFunction.Capacitance, (Button)sender);
        private async void buttonMeasureFreq_Click(object sender, EventArgs e) => await SetActiveMeasurementAsync(MeasurementFunction.Frequency, (Button)sender);
        private async void buttonMeasureTemp_Click(object sender, EventArgs e) => await SetActiveMeasurementAsync(MeasurementFunction.Temperature, (Button)sender);
        private async void buttonMeasureDiode_Click(object sender, EventArgs e) => await SetActiveMeasurementAsync(MeasurementFunction.Diode, (Button)sender);

        private void SetFunctionButtonsEnabled(bool enabled)
        {
            foreach (Control c in groupBoxFunctions.Controls)
            {
                if (c is Button btn) btn.Enabled = enabled;
            }
        }

        private void UpdateButtonStyles(Button activeButton)
        {
            foreach (Control c in groupBoxFunctions.Controls)
            {
                if (c is Button btn)
                {
                    btn.BackColor = (btn == activeButton) ? Color.FromArgb(0, 122, 204) : Color.FromArgb(63, 63, 70);
                    btn.Font = new Font("Segoe UI", 9F, (btn == activeButton) ? FontStyle.Bold : FontStyle.Regular);
                }
            }
        }

        private void ClearDisplayReadings()
        {
            lblMeasurementType.Text = "NO FUNCTION";
            lblReading.Text = "0.000000";
            lblUnit.Text = "";
            textBoxSystemInfo.Clear();
        }

        private void LogActivity(string message, bool isError = false)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => LogActivity(message, isError)));
                return;
            }
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string logEntry = $"[{timestamp}] {message}{Environment.NewLine}";
            richTextBoxLog.SelectionStart = richTextBoxLog.TextLength;
            richTextBoxLog.SelectionColor = isError ? Color.OrangeRed : Color.Gainsboro;
            richTextBoxLog.AppendText(logEntry);
            richTextBoxLog.ScrollToCaret();
        }

        #region --- Input Validation ---
        
        private bool ValidateConnectionInput()
        {
            // Validate IP address
            if (string.IsNullOrWhiteSpace(textBoxIP.Text))
            {
                MessageBox.Show("กรุณาใส่ IP Address", "ข้อมูลไม่ครบ", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxIP.Focus();
                return false;
            }

            if (!System.Net.IPAddress.TryParse(textBoxIP.Text.Trim(), out _))
            {
                MessageBox.Show("รูปแบบ IP Address ไม่ถูกต้อง\nตัวอย่าง: 192.168.1.100", 
                    "ข้อมูลไม่ถูกต้อง", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxIP.Focus();
                textBoxIP.SelectAll();
                return false;
            }

            // Validate port
            if (string.IsNullOrWhiteSpace(textBoxPort.Text))
            {
                MessageBox.Show("กรุณาใส่หมายเลข Port", "ข้อมูลไม่ครบ", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxPort.Focus();
                return false;
            }

            if (!int.TryParse(textBoxPort.Text.Trim(), out int port) || port < 1 || port > 65535)
            {
                MessageBox.Show("หมายเลข Port ไม่ถูกต้อง\nต้องเป็นตัวเลขระหว่าง 1-65535", 
                    "ข้อมูลไม่ถูกต้อง", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxPort.Focus();
                textBoxPort.SelectAll();
                return false;
            }

            return true;
        }

        #endregion

        #region --- Settings Management ---
        
        private void LoadLastSuccessfulConnection()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var lines = File.ReadAllLines(_settingsFilePath);
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("LastSuccessfulIP="))
                        {
                            string savedIP = line.Substring("LastSuccessfulIP=".Length);
                            if (!string.IsNullOrEmpty(savedIP))
                            {
                                textBoxIP.Text = savedIP;
                                LogActivity($"โหลด IP ล่าสุดที่เชื่อมต่อสำเร็จ: {savedIP}");
                            }
                        }
                        else if (line.StartsWith("LastSuccessfulPort="))
                        {
                            string savedPort = line.Substring("LastSuccessfulPort=".Length);
                            if (!string.IsNullOrEmpty(savedPort))
                            {
                                textBoxPort.Text = savedPort;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogActivity($"โหลดการตั้งค่าล้มเหลว: {ex.Message}", true);
            }
        }

        private void SaveLastSuccessfulConnection(string ip, string port)
        {
            try
            {
                Directory.CreateDirectory(_appDataFolder);
                var settings = new List<string>
                {
                    $"LastSuccessfulIP={ip}",
                    $"LastSuccessfulPort={port}",
                    $"LastConnectionTime={DateTime.Now:yyyy-MM-dd HH:mm:ss}"
                };
                File.WriteAllLines(_settingsFilePath, settings, Encoding.UTF8);
                LogActivity($"บันทึกการตั้งค่าการเชื่อมต่อ: {ip}:{port}");
            }
            catch (Exception ex)
            {
                LogActivity($"บันทึกการตั้งค่าล้มเหลว: {ex.Message}", true);
            }
        }

        #endregion

        #region --- Data, System, and Closing Methods ---

        // *** ปรับปรุง LoadDataFromFile() ***
        private void LoadDataFromFile()
        {
            if (!File.Exists(_dataFilePath)) return;
            try
            {
                var lines = File.ReadAllLines(_dataFilePath).Skip(1);
                foreach (var line in lines)
                {
                    var values = line.Split(',');
                    if (values.Length == 5)
                    {
                        _recordsTable.Rows.Add(
                            int.Parse(values[0].Trim('"')), values[1].Trim('"'),
                            values[2].Trim('"'), values[3].Trim('"'), values[4].Trim('"')
                        );
                    }
                }
                LogActivity($"โหลดข้อมูล {_recordsTable.Rows.Count} รายการสำเร็จ");

                // *** เพิ่มการ select แถวล่าสุดหลังโหลดข้อมูล ***
                SelectLatestRow();
            }
            catch (Exception ex)
            {
                LogActivity($"โหลดข้อมูลล้มเหลว: {ex.Message}", true);
            }
        }

        private void SaveDataToFile()
        {
            try
            {
                Directory.CreateDirectory(_appDataFolder);
                var lines = new List<string> { string.Join(",", _recordsTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName)) };
                lines.AddRange(_recordsTable.AsEnumerable().Select(row => string.Join(",", row.ItemArray.Select(field => $"\"{field}\""))));
                File.WriteAllLines(_dataFilePath, lines, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                LogActivity($"บันทึกข้อมูลล้มเหลว: {ex.Message}", true);
            }
        }

        private void RenumberRows()
        {
            for (int i = 0; i < _recordsTable.Rows.Count; i++)
            {
                _recordsTable.Rows[i]["No"] = i + 1;
            }
        }

        private void buttonRecord_Click(object sender, EventArgs e)
        {
            saveRecord();
        }

        // *** ปรับปรุง saveRecord() ***
        private void saveRecord()
        {
            if (_dmm == null || !_dmm.IsConnected || lblReading.Text == "CONFIG..." || lblReading.Text == "ERROR")
            {
                LogActivity("ไม่สามารถบันทึกได้: ไม่มีค่าที่วัดได้", true);
                return;
            }

            SoundUtil.Beep(); // เล่นเสียงเตือนเมื่อบันทึก

            int newId = _recordsTable.Rows.Count + 1;
            string function = _currentFunction.ToString();

            // เปลี่ยนเป็นทศนิยม 4 หลัก แทน E6
            string measurement = _isOverload ? "OVERLOAD" : _lastReadingValue.ToString("F4", CultureInfo.InvariantCulture);
            string unit = _isOverload ? "" : _lastReadingUnit;
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            _recordsTable.Rows.Add(newId, function, measurement, unit, timestamp);
            LogActivity($"บันทึกค่า No. {newId}: {function}, {measurement} {unit}");
            SaveDataToFile();

            // *** เพิ่มการ select แถวล่าสุด ***
            SelectLatestRow();
        }



        // *** เพิ่ม Event Handler ใหม่สำหรับการแสดงผล measurement + unit ***
        // *** เพิ่ม Event Handler ใหม่สำหรับการแสดงผล measurement + unit ***
        private void DataGridViewRecords_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridViewRecords.Columns[e.ColumnIndex].Name == "Measurement")
            {
                if (e.RowIndex >= 0 && e.RowIndex < dataGridViewRecords.Rows.Count)
                {
                    var row = dataGridViewRecords.Rows[e.RowIndex];
                    string measurement = row.Cells["Measurement"].Value?.ToString() ?? "";
                    string unit = row.Cells["Unit"].Value?.ToString() ?? "";

                    // แสดง measurement + unit รวมกัน (ถ้าไม่ใช่ OVERLOAD)
                    if (measurement != "OVERLOAD" && !string.IsNullOrEmpty(unit))
                    {
                        e.Value = $"{measurement} {unit}";
                    }
                    else
                    {
                        e.Value = measurement;
                    }
                    e.FormattingApplied = true;
                }
            }
        }
        // *** เพิ่มการ select แถวล่าสุดหลังจากลบข้อมูล ***
        private async void buttonDeleteRecord_Click(object sender, EventArgs e)
        {
            if (dataGridViewRecords.SelectedRows.Count == 0) return;
            var confirmResult = MessageBox.Show("ยืนยันการลบแถวที่เลือก?", "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmResult == DialogResult.Yes)
            {
                // ฟีเจอร์ใหม่: เล่นเสียง 2 ครั้ง
                SoundUtil.Beep();
                await Task.Delay(500);
                SoundUtil.Beep();

                foreach (DataGridViewRow row in dataGridViewRecords.SelectedRows.Cast<DataGridViewRow>().ToList())
                {
                    (row.DataBoundItem as DataRowView)?.Row.Delete();
                }
                _recordsTable.AcceptChanges();
                RenumberRows();
                SaveDataToFile();
                LogActivity("ลบข้อมูลที่เลือกแล้ว");

                // *** เพิ่มการ select แถวล่าสุดหลังจากลบ ***
                SelectLatestRow();
            }
        }

        private void buttonClearTable_Click(object sender, EventArgs e)
        {
            if (_recordsTable.Rows.Count == 0) return;
            var confirmResult = MessageBox.Show("ยืนยันการล้างข้อมูลทั้งหมด?", "ยืนยันการล้างข้อมูล", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirmResult == DialogResult.Yes)
            {
                _recordsTable.Clear();
                SaveDataToFile();
                LogActivity("ล้างข้อมูลทั้งหมดแล้ว");
            }
        }

        // *** ปรับปรุง Export function ให้แยก measurement กับ unit ***
        private void buttonExportCsv_Click(object sender, EventArgs e)
        {
            if (_recordsTable.Rows.Count == 0)
            {
                MessageBox.Show("ไม่มีข้อมูลให้ส่งออก", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "CSV File (*.csv)|*.csv";
                saveFileDialog.FileName = $"MeasurementLog_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // สร้าง CSV โดยแยก measurement และ unit เป็นคอลัมน์ต่างหาก
                        var lines = new List<string>();

                        // Header
                        lines.Add("No,Function,Measurement,Unit,Timestamp");

                        // Data rows - ใช้ข้อมูลต้นฉบับที่แยก measurement กับ unit
                        foreach (DataRow row in _recordsTable.Rows)
                        {
                            string csvLine = string.Join(",",
                                $"\"{row["No"]}\"",
                                $"\"{row["Function"]}\"",
                                $"\"{row["Measurement"]}\"",  // measurement อย่างเดียว ไม่รวม unit
                                $"\"{row["Unit"]}\"",
                                $"\"{row["Timestamp"]}\""
                            );
                            lines.Add(csvLine);
                        }

                        File.WriteAllLines(saveFileDialog.FileName, lines, Encoding.UTF8);
                        LogActivity($"ส่งออกข้อมูลไปยัง {saveFileDialog.FileName} สำเร็จ");
                        MessageBox.Show("ส่งออกข้อมูลสำเร็จ!", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        LogActivity($"ส่งออก CSV ล้มเหลว: {ex.Message}", true);
                        MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async void buttonGetIDN_Click(object sender, EventArgs e)
        {
            if (_dmm == null || !_dmm.IsConnected) return;
            textBoxSystemInfo.Text = await _dmm.QueryCommandAsync("*IDN?");
        }

        private async void buttonReset_Click(object sender, EventArgs e)
        {
            if (_dmm == null || !_dmm.IsConnected) return;
            var result = MessageBox.Show("คุณต้องการรีเซ็ตเครื่องมือหรือไม่?", "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;

            LogActivity("กำลัง Reset เครื่องมือ...");
            await _dmm.SendCommandAsync("*RST");
            await Task.Delay(1000);
            await SetActiveMeasurementAsync(MeasurementFunction.VoltageDC, buttonMeasureVDC);
        }

        private async void buttonGetError_Click(object sender, EventArgs e)
        {
            if (_dmm == null || !_dmm.IsConnected) return;
            string error = await _dmm.QueryCommandAsync("SYST:ERR?");
            textBoxSystemInfo.Text = $"System Error: {error}";
            LogActivity($"System Error: {error}");
        }

        private void buttonClearLog_Click(object sender, EventArgs e)
        {
            richTextBoxLog.Clear();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _dmm?.Dispose();
        }

        #endregion

        private bool isWaitingForSecondCtrlQ = false;
        private int doubleKeyInterval = 500;

        private async void deleteLatestRecord()
        {
            if (_recordsTable.Rows.Count == 0)
            {
                LogActivity("ไม่มีข้อมูลให้ลบ", true);
                return;
            }

            try
            {
                // เล่นเสียงเตือน 2 ครั้ง (เหมือนกับ buttonDeleteRecord_Click)
                SoundUtil.Beep();
                await Task.Delay(200); // หน่วงเวลาเล็กน้อยระหว่างเสียง
                SoundUtil.Beep();

                // เก็บข้อมูลแถวล่าสุดก่อนลบ เพื่อแสดงใน Log
                DataRow lastRow = _recordsTable.Rows[_recordsTable.Rows.Count - 1];
                int deletedNo = (int)lastRow["No"];
                string deletedFunction = lastRow["Function"].ToString();
                string deletedMeasurement = lastRow["Measurement"].ToString();
                string deletedUnit = lastRow["Unit"].ToString();

                // สร้างข้อความแสดงผลเหมือนใน GridView (measurement + unit)
                string displayMeasurement = (deletedMeasurement != "OVERLOAD" && !string.IsNullOrEmpty(deletedUnit))
                    ? $"{deletedMeasurement} {deletedUnit}"
                    : deletedMeasurement;

                // ลบแถวล่าสุด
                _recordsTable.Rows.RemoveAt(_recordsTable.Rows.Count - 1);
                _recordsTable.AcceptChanges();

                // จัดเรียงหมายเลขใหม่
                RenumberRows();

                // บันทึกลงไฟล์
                SaveDataToFile();

                // Scroll ไปยังแถวล่าสุดใหม่ (ถ้ายังมีข้อมูล)
                if (dataGridViewRecords.Rows.Count > 0)
                {
                    int lastIndex = dataGridViewRecords.Rows.Count - 1;
                    dataGridViewRecords.FirstDisplayedScrollingRowIndex = lastIndex;
                    dataGridViewRecords.Rows[lastIndex].Selected = true;
                }

                LogActivity($"ลบข้อมูลล่าสุดแล้ว - No. {deletedNo}: {deletedFunction}, {displayMeasurement}");
            }
            catch (Exception ex)
            {
                LogActivity($"เกิดข้อผิดพลาดในการลบข้อมูล: {ex.Message}", true);
            }
        }

        // เปลี่ยนจาก LogActivity($"ลบจ้า"); เป็นการเรียก function ใหม่
        private async void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (buttonRecord.Enabled && e.Control && e.KeyCode == Keys.Q)
            {
                if (isWaitingForSecondCtrlQ)
                {
                    // Ctrl+Q ครั้งที่ 2 = Double Ctrl+Q = ลบข้อมูลล่าสุด
                    isWaitingForSecondCtrlQ = false;
                    deleteLatestRecord(); // เรียก function ใหม่
                }
                else
                {
                    // Ctrl+Q ครั้งแรก = รอ 500ms
                    isWaitingForSecondCtrlQ = true;

                    await Task.Delay(doubleKeyInterval);

                    if (isWaitingForSecondCtrlQ)
                    {
                        // หมดเวลารอ = Single Ctrl+Q = บันทึกข้อมูล
                        isWaitingForSecondCtrlQ = false;
                        saveRecord();
                    }
                }

                e.Handled = true;
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            this.KeyPreview = true; // ให้ Form รับ KeyDown ก่อน Control อื่น
        }

       
    }
}