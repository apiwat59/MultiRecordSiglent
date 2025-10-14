using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using SIGLENT;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media; // เพิ่ม namespace นี้
using System.Runtime.InteropServices; // สำหรับจัดการ COM exceptions
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace MultiRecord
{
    public partial class Main : Form
    {
        private SDM3055 _dmm;
        private MeasurementFunction _currentFunction;
        private Button _activeButton;
        private bool _isChangingFunction = false;
        private bool _isProgrammaticallyChangingParams = false;
        private bool _isRelativeEnabled = false;

        private DataTable _recordsTable;
        private DataTable _rdRecordsTable;
        private double _lastReadingValue;
        private string _lastReadingUnit;
        private bool _isOverload = false;
        
        // R&D specific variables
        private int _currentModelId = -1;
        private int _currentSerialId = -1;
        private string _currentSerialNumber = "";
        private bool _isRDTabActive = false;

        // Tolerance settings
        private bool _toleranceEnabled = false;
        private double _toleranceValueDC = 0.5;
        private double _toleranceValueAC = 1.0;
        private double _toleranceValue2W = 0.1;
        private bool _toleranceIsPercent = true;
        private double _toleranceTargetValue = 5.0;

        private readonly string _dataFilePath;
        private readonly string _appDataFolder;
        private readonly string _settingsFilePath;

        public Main()
        {
            InitializeComponent();

            _appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MultiRecordApp");
            _dataFilePath = Path.Combine(_appDataFolder, "measurement_data.csv");
            _settingsFilePath = Path.Combine(_appDataFolder, "settings.ini");

            InitializeForm();
            InitializeDataTableAndLoadData();
            InitializeRDDataTable();
            LoadLastSuccessfulConnection();
            InitializeToleranceControls();
            InitializeQWRecord();
            InitializeMySQL();
            InitializeRDTab();
        }

        private void InitializeForm()
        {
            lblReading.Font = new Font("Consolas", 72F, FontStyle.Bold);
            lblUnit.Font = new Font("Consolas", 28F, FontStyle.Bold);

            UpdateConnectionStatus(false);
            panelParameters.Visible = false;
            LogActivity("แอปพลิเคชันเริ่มต้นแล้ว ยินดีต้อนรับ!");
            
            // Add logout button programmatically
            InitializeLogoutButton();
        }
        
        private void InitializeLogoutButton()
        {
            var logoutButton = new Button
            {
                Text = "Logout",
                Name = "buttonLogout",
                Size = new Size(80, 30),
                Location = new Point(this.Width - 100, 10),
                BackColor = Color.FromArgb(186, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            
            logoutButton.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
            logoutButton.Click += async (s, e) => await LogoutUser_Click();
            
            this.Controls.Add(logoutButton);
            logoutButton.BringToFront();
        }

        private async Task LogoutUser_Click()
        {
            var result = MessageBox.Show("คุณต้องการออกจากระบบหรือไม่?", 
                                       "Logout Confirmation", 
                                       MessageBoxButtons.YesNo, 
                                       MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                await LogoutUser();
            }
        }

        private void InitializeToleranceControls()
        {
            // Wire up event handlers for Designer-created controls
            buttonSetTarget.Click += ButtonSetTarget_Click;
            checkBoxEnableTolerance.CheckedChanged += CheckBoxEnableTolerance_CheckedChanged;
            textBoxTargetValue.TextChanged += TextBoxTargetValue_TextChanged;
            textBoxDCTolerance.TextChanged += TextBoxDCTolerance_TextChanged;
            textBoxACTolerance.TextChanged += TextBoxACTolerance_TextChanged;
            textBox2WTolerance.TextChanged += TextBox2WTolerance_TextChanged;
            checkBoxIsPercent.CheckedChanged += CheckBoxIsPercent_CheckedChanged;

            // Load tolerance settings
            LoadToleranceSettings();
            UpdateToleranceButtonAndLimits();
            LogActivity("เชื่อมต่อ Tolerance Controls สำเร็จ");
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
            _recordsTable.Columns.Add("Tolerance", typeof(string));

            dataGridViewRecords.DataSource = _recordsTable;

            // ตั้งค่า AutoSizeMode
            dataGridViewRecords.Columns["No"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewRecords.Columns["Function"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewRecords.Columns["Measurement"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewRecords.Columns["Unit"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewRecords.Columns["Timestamp"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewRecords.Columns["Tolerance"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            // *** เพิ่มการตั้งค่า Selection Mode ***
            dataGridViewRecords.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewRecords.MultiSelect = true; // เปิดให้เลือกหลายแถวได้ (สำหรับการลบ)
            
            // *** คืนค่าเป็น ReadOnly เนื่องจากจะใช้ Edit Panel แทน ***
            dataGridViewRecords.ReadOnly = true;
            dataGridViewRecords.AllowUserToDeleteRows = false;
            dataGridViewRecords.AllowUserToAddRows = false;

            // ปรับแต่งการแสดงผลให้แสดง measurement + unit รวมกัน
            dataGridViewRecords.CellFormatting += DataGridViewRecords_CellFormatting;
            
            // *** เพิ่ม Event Handler สำหรับ Double Click เพื่อแก้ไข ***
            dataGridViewRecords.CellDoubleClick += DataGridViewRecords_CellDoubleClick;

            LoadDataFromFile();

            // *** SelectLatestRow() จะถูกเรียกใน LoadDataFromFile() แล้ว ***
        }

        // ========= Connection and Reconnection =========
        private async void buttonConnect_Click(object sender, EventArgs e)
        {
            // Validate input first
            if (!ValidateConnectionInput())
                return;

            // Clean up existing connection first
            if (_dmm != null)
            {
                try
                {
                    _dmm.Dispose();
                }
                catch { }
                finally
                {
                    _dmm = null;
                }
            }
                
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

        private static DateTime lastDisconnectClick = DateTime.MinValue;
        
        private async void buttonDisconnect_Click(object sender, EventArgs e)
        {
            // Check for double-click (within 500ms) for force disconnect
            if ((DateTime.Now - lastDisconnectClick).TotalMilliseconds < 500)
            {
                ForceDisconnect();
                return;
            }
            lastDisconnectClick = DateTime.Now;
            
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
                
                var result = MessageBox.Show($"เกิดข้อผิดพลาดขณะตัดการเชื่อมต่อ:\n{ex.Message}\n\nต้องการบังคับตัดการเชื่อมต่อหรือไม่?", 
                    "ข้อผิดพลาด", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    
                if (result == DialogResult.Yes)
                {
                    ForceDisconnect();
                    return;
                }
                
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
                // Force re-enable and reset button state
                buttonDisconnect.Enabled = (_dmm != null);
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
            
            // Clean up the connection object first
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
            
            // Update UI after cleanup
            UpdateConnectionStatus(false);
            
            // Show connection lost notification
            MessageBox.Show("การเชื่อมต่อกับเครื่องมือขาดหาย!\nโปรดตรวจสอบการเชื่อมต่อและลองเชื่อมต่อใหม่", 
                "การเชื่อมต่อขาดหาย", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                Console.WriteLine(lblMeasurementType.Text);
                if (lblMeasurementType.Text == "Continuous".ToUpper())
                {
                    lblReading.Text = "OPEN";
                    lblUnit.Text = "";
                }
                else
                {
                    lblReading.Text = "OVERLOAD";
                    lblUnit.Text = "";
                }

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
            // Always enable disconnect if DMM exists, regardless of connection status
            buttonDisconnect.Enabled = (_dmm != null);
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
        private async void buttonContinus_Click(object sender, EventArgs e) => await SetActiveMeasurementAsync(MeasurementFunction.Continuous, (Button)sender);
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

        #region --- Tolerance Control Event Handlers ---

        private void ButtonSetTarget_Click(object sender, EventArgs e)
        {
            if (!_toleranceEnabled)
            {
                LogActivity("ไม่สามารถตั้งค่า Target ได้: Tolerance Check ไม่ได้เปิดใช้งาน", true);
                return;
            }

            if (_dmm == null || !_dmm.IsConnected || _isOverload)
            {
                LogActivity("ไม่สามารถตั้งค่า Target ได้: ไม่มีค่าที่วัดได้", true);
                return;
            }

            // Set current reading as target value
            _toleranceTargetValue = _lastReadingValue;
            textBoxTargetValue.Text = _toleranceTargetValue.ToString("F4", CultureInfo.InvariantCulture);
            
            LogActivity($"ตั้งค่า Target เป็น {_toleranceTargetValue:F4} {_lastReadingUnit}");
            UpdateToleranceButtonAndLimits();
            SaveToleranceSettings();
            
            // Visual feedback
            SoundUtil.Beep();
        }

        private void UpdateToleranceButtonAndLimits()
        {
            // Update checkbox state
            checkBoxEnableTolerance.Checked = _toleranceEnabled;
            
            if (_toleranceEnabled && _toleranceTargetValue != 0.0)
            {
                buttonSetTarget.BackColor = Color.FromArgb(0, 122, 204);
                buttonSetTarget.ForeColor = Color.White;
                buttonSetTarget.Text = $"Target: {FormatValueForDisplay(_toleranceTargetValue, _lastReadingUnit ?? "V")}";
                
                // Calculate and display limits
                UpdateToleranceLimits();
            }
            else
            {
                buttonSetTarget.BackColor = Color.FromArgb(63, 63, 70);
                buttonSetTarget.ForeColor = Color.Gainsboro;
                buttonSetTarget.Text = "Set as Target";
                
                // Clear limits
                labelUpperLimit.Text = "Upper Limit: ---";
                labelLowerLimit.Text = "Lower Limit: ---";
            }
            
            // Enable/disable tolerance controls based on checkbox
            buttonSetTarget.Enabled = _toleranceEnabled;
            textBoxTargetValue.Enabled = _toleranceEnabled;
            textBoxDCTolerance.Enabled = _toleranceEnabled;
            textBoxACTolerance.Enabled = _toleranceEnabled;
            textBox2WTolerance.Enabled = _toleranceEnabled;
            checkBoxIsPercent.Enabled = _toleranceEnabled;
        }

        private void UpdateToleranceLimits()
        {
            if (!_toleranceEnabled || _toleranceTargetValue == 0.0)
            {
                labelUpperLimit.Text = "Upper Limit: ---";
                labelLowerLimit.Text = "Lower Limit: ---";
                return;
            }

            double toleranceValue = GetCurrentToleranceValue();
            double allowedDeviation;
            
            if (_toleranceIsPercent)
            {
                allowedDeviation = Math.Abs(_toleranceTargetValue * toleranceValue / 100.0);
            }
            else
            {
                allowedDeviation = toleranceValue;
            }

            double upperLimit = _toleranceTargetValue + allowedDeviation;
            double lowerLimit = _toleranceTargetValue - allowedDeviation;

            string unit = _lastReadingUnit ?? "V";
            labelUpperLimit.Text = $"Upper Limit: {FormatValueForDisplay(upperLimit, unit)}";
            labelLowerLimit.Text = $"Lower Limit: {FormatValueForDisplay(lowerLimit, unit)}";
        }

        private double GetCurrentToleranceValue()
        {
            // Get tolerance value based on current function
            switch (_currentFunction)
            {
                case MeasurementFunction.VoltageDC:
                case MeasurementFunction.CurrentDC:
                    return _toleranceValueDC;
                case MeasurementFunction.VoltageAC:
                case MeasurementFunction.CurrentAC:
                    return _toleranceValueAC;
                case MeasurementFunction.Resistance2W:
                    return _toleranceValue2W;
                default:
                    return _toleranceValueDC;
            }
        }

        private void CheckBoxEnableTolerance_CheckedChanged(object sender, EventArgs e)
        {
            var checkBox = sender as CheckBox;
            bool newState = checkBox.Checked;
            
            // Only change if there's an actual change
            if (newState != _toleranceEnabled)
            {
                _toleranceEnabled = newState;
                LogActivity($"Tolerance Check: {(_toleranceEnabled ? "เปิด" : "ปิด")}");
                UpdateToleranceButtonAndLimits();
                SaveToleranceSettings();
            }
        }

        private void TextBoxTargetValue_TextChanged(object sender, EventArgs e)
        {
            var textBox = sender as TextBox;
            if (double.TryParse(textBox.Text, out double value))
            {
                _toleranceTargetValue = value;
                UpdateToleranceLimits();
                SaveToleranceSettings();
            }
        }

        private void TextBoxDCTolerance_TextChanged(object sender, EventArgs e)
        {
            var textBox = sender as TextBox;
            if (double.TryParse(textBox.Text, out double value))
            {
                _toleranceValueDC = value;
                UpdateToleranceLimits();
                SaveToleranceSettings();
            }
        }

        private void TextBoxACTolerance_TextChanged(object sender, EventArgs e)
        {
            var textBox = sender as TextBox;
            if (double.TryParse(textBox.Text, out double value))
            {
                _toleranceValueAC = value;
                UpdateToleranceLimits();
                SaveToleranceSettings();
            }
        }

        private void TextBox2WTolerance_TextChanged(object sender, EventArgs e)
        {
            var textBox = sender as TextBox;
            if (double.TryParse(textBox.Text, out double value))
            {
                _toleranceValue2W = value;
                UpdateToleranceLimits();
                SaveToleranceSettings();
            }
        }

        private void CheckBoxIsPercent_CheckedChanged(object sender, EventArgs e)
        {
            var checkBox = sender as CheckBox;
            _toleranceIsPercent = checkBox.Checked;
            LogActivity($"Tolerance Mode: {(_toleranceIsPercent ? "Percentage" : "Absolute")}");
            UpdateToleranceLimits();
            SaveToleranceSettings();
        }

        private bool CheckTolerance(double currentValue, MeasurementFunction function)
        {
            if (!_toleranceEnabled || _toleranceTargetValue == 0.0) return true;

            double toleranceValue = 0.0;
            switch (function)
            {
                case MeasurementFunction.VoltageDC:
                case MeasurementFunction.CurrentDC:
                    toleranceValue = _toleranceValueDC;
                    break;
                case MeasurementFunction.VoltageAC:
                case MeasurementFunction.CurrentAC:
                    toleranceValue = _toleranceValueAC;
                    break;
                case MeasurementFunction.Resistance2W:
                    toleranceValue = _toleranceValue2W;
                    break;
                default:
                    return true; // No tolerance check for other functions
            }

            double allowedDeviation;
            if (_toleranceIsPercent)
            {
                allowedDeviation = Math.Abs(_toleranceTargetValue * toleranceValue / 100.0);
            }
            else
            {
                allowedDeviation = toleranceValue;
            }

            double deviation = Math.Abs(currentValue - _toleranceTargetValue);
            bool withinTolerance = deviation <= allowedDeviation;

            if (!withinTolerance)
            {
                string deviationStr = _toleranceIsPercent 
                    ? $"{(deviation / Math.Abs(_toleranceTargetValue)) * 100:F2}%" 
                    : $"{deviation:F4}";
                
                LogActivity($"ค่าเกิน Tolerance! ค่าปัจจุบัน: {currentValue:F4}, เป้าหมาย: {_toleranceTargetValue:F4}, เบี่ยงเบน: {deviationStr}", true);
                
                // Play over sound
                SoundUtil.Over();
            }

            return withinTolerance;
        }

        #endregion

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

        // Force disconnect method for emergency situations
        private void ForceDisconnect()
        {
            LogActivity("กำลังบังคับตัดการเชื่อมต่อ...");
            
            if (_dmm != null)
            {
                try
                {
                    // Try graceful disconnect first
                    _dmm.Disconnect();
            }
            catch (Exception ex)
            {
                    LogActivity($"Graceful disconnect ล้มเหลว: {ex.Message}");
                }
                
                try
                {
                    // Force dispose
                    _dmm.Dispose();
                }
                catch (Exception ex)
                {
                    LogActivity($"Force dispose ล้มเหลว: {ex.Message}");
            }
            finally
            {
                    _dmm = null;
                }
            }
            
            // Force UI update
            UpdateConnectionStatus(false);
            ClearDisplayReadings();
            LogActivity("บังคับตัดการเชื่อมต่อเสร็จสิ้น");
        }

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

        private void SaveToleranceSettings()
        {
            try
            {
                Directory.CreateDirectory(_appDataFolder);
                var toleranceFilePath = Path.Combine(_appDataFolder, "tolerance.ini");
                var settings = new List<string>
                {
                    $"ToleranceEnabled={_toleranceEnabled}",
                    $"ToleranceTargetValue={_toleranceTargetValue.ToString(CultureInfo.InvariantCulture)}",
                    $"ToleranceValueDC={_toleranceValueDC.ToString(CultureInfo.InvariantCulture)}",
                    $"ToleranceValueAC={_toleranceValueAC.ToString(CultureInfo.InvariantCulture)}",
                    $"ToleranceValue2W={_toleranceValue2W.ToString(CultureInfo.InvariantCulture)}",
                    $"ToleranceIsPercent={_toleranceIsPercent}"
                };
                File.WriteAllLines(toleranceFilePath, settings, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                LogActivity($"บันทึก Tolerance Settings ล้มเหลว: {ex.Message}", true);
            }
        }

        private void LoadToleranceSettings()
        {
            try
            {
                var toleranceFilePath = Path.Combine(_appDataFolder, "tolerance.ini");
                if (File.Exists(toleranceFilePath))
                {
                    var lines = File.ReadAllLines(toleranceFilePath);
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("ToleranceEnabled="))
                        {
                            bool.TryParse(line.Substring("ToleranceEnabled=".Length), out _toleranceEnabled);
                        }
                        else if (line.StartsWith("ToleranceTargetValue="))
                        {
                            double.TryParse(line.Substring("ToleranceTargetValue=".Length), NumberStyles.Any, CultureInfo.InvariantCulture, out _toleranceTargetValue);
                        }
                        else if (line.StartsWith("ToleranceValueDC="))
                        {
                            double.TryParse(line.Substring("ToleranceValueDC=".Length), NumberStyles.Any, CultureInfo.InvariantCulture, out _toleranceValueDC);
                        }
                        else if (line.StartsWith("ToleranceValueAC="))
                        {
                            double.TryParse(line.Substring("ToleranceValueAC=".Length), NumberStyles.Any, CultureInfo.InvariantCulture, out _toleranceValueAC);
                        }
                        else if (line.StartsWith("ToleranceValue2W="))
                        {
                            double.TryParse(line.Substring("ToleranceValue2W=".Length), NumberStyles.Any, CultureInfo.InvariantCulture, out _toleranceValue2W);
                        }
                        else if (line.StartsWith("ToleranceIsPercent="))
                        {
                            bool.TryParse(line.Substring("ToleranceIsPercent=".Length), out _toleranceIsPercent);
                        }
                    }

                    // Update UI controls with loaded values
                    UpdateToleranceControls();
                    LogActivity("โหลด Tolerance Settings สำเร็จ");
                }
            }
            catch (Exception ex)
            {
                LogActivity($"โหลด Tolerance Settings ล้มเหลว: {ex.Message}", true);
            }
        }

        private void UpdateToleranceControls()
        {
            // Update text controls with loaded values
            textBoxTargetValue.Text = _toleranceTargetValue.ToString(CultureInfo.InvariantCulture);
            textBoxDCTolerance.Text = _toleranceValueDC.ToString(CultureInfo.InvariantCulture);
            textBoxACTolerance.Text = _toleranceValueAC.ToString(CultureInfo.InvariantCulture);
            textBox2WTolerance.Text = _toleranceValue2W.ToString(CultureInfo.InvariantCulture);
            checkBoxIsPercent.Checked = _toleranceIsPercent;
            checkBoxEnableTolerance.Checked = _toleranceEnabled;
            
            // Update button and limits
            UpdateToleranceButtonAndLimits();
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
                    if (values.Length == 6)
                    {
                        _recordsTable.Rows.Add(
                            int.Parse(values[0].Trim('"')), values[1].Trim('"'),
                            values[2].Trim('"'), values[3].Trim('"'), values[4].Trim('"'),
                            values[5].Trim('"')
                        );
                    }
                    else if (values.Length == 5)
                    {
                        // Support old format without Tolerance column
                        _recordsTable.Rows.Add(
                            int.Parse(values[0].Trim('"')), values[1].Trim('"'),
                            values[2].Trim('"'), values[3].Trim('"'), values[4].Trim('"'),
                            "N/A"
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

        private async void buttonRecord_Click(object sender, EventArgs e)
        {
            await saveRecord();
        }

        // *** ปรับปรุง saveRecord() ***
        private async Task saveRecord()
        {
            if (_dmm == null || !_dmm.IsConnected || lblReading.Text == "CONFIG..." || lblReading.Text == "ERROR")
            {
                LogActivity("ไม่สามารถบันทึกได้: ไม่มีค่าที่วัดได้", true);
                return;
            }

            int newId = _recordsTable.Rows.Count + 1;
            string function = _currentFunction.ToString();

            // เปลี่ยนเป็นทศนิยม 4 หลัก แทน E6
            string measurement = _isOverload ? "OVERLOAD" : _lastReadingValue.ToString("F4", CultureInfo.InvariantCulture);
            string unit = _isOverload ? "" : _lastReadingUnit;
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Check tolerance และเพิ่ม marker ถ้าเกิน
            bool withinTolerance = true;
            if (!_isOverload && _toleranceEnabled && _toleranceTargetValue != 0.0)
            {
                withinTolerance = CheckToleranceForRecord(_lastReadingValue, _currentFunction);
                if (!withinTolerance)
                {
                    measurement += " [OVER]"; // เพิ่ม marker ถ้าเกิน tolerance
                }
            }

            // เล่นเสียงตามสถานะ tolerance
            if (!withinTolerance)
            {
                SoundUtil.Over(); // เล่น over.mp3 เมื่อเกิน tolerance
            }
            else
            {
                SoundUtil.Beep(); // เล่นเสียงปกติเมื่อบันทึก
            }

            string toleranceStatus = withinTolerance ? "Pass" : "Fail";
            _recordsTable.Rows.Add(newId, function, measurement, unit, timestamp, toleranceStatus);

            try
            {
                string formattedValue = FormatValueForDisplay(Convert.ToDouble(measurement), unit);
                lb_lates.Text = $"{formattedValue}";
            }
            catch
            {
                if (lblMeasurementType.Text == "Continuous".ToUpper())
                {
                    lb_lates.Text = $"Open";
                }
                else
                {
                    lb_lates.Text = $"Overload";
                }
            }
    

            string logMessage = $"บันทึกค่า No. {newId}: {function}, {measurement} {unit}";
            if (!withinTolerance)
            {
                logMessage += " (เกิน Tolerance!)";
            }
            LogActivity(logMessage, !withinTolerance);
            
            SaveDataToFile();

            // Save to MySQL if enabled
            if (_mysqlManager != null)
            {
                await SaveToMySQLAsync(newId, function, measurement, unit, DateTime.Now, toleranceStatus);
            }

            // *** เพิ่มการ select แถวล่าสุด ***
            SelectLatestRow();
        }

        private bool CheckToleranceForRecord(double currentValue, MeasurementFunction function)
        {
            if (!_toleranceEnabled || _toleranceTargetValue == 0.0) return true;

            double toleranceValue = 0.0;
            switch (function)
            {
                case MeasurementFunction.VoltageDC:
                case MeasurementFunction.CurrentDC:
                    toleranceValue = _toleranceValueDC;
                    break;
                case MeasurementFunction.VoltageAC:
                case MeasurementFunction.CurrentAC:
                    toleranceValue = _toleranceValueAC;
                    break;
                case MeasurementFunction.Resistance2W:
                    toleranceValue = _toleranceValue2W;
                    break;
                default:
                    return true; // No tolerance check for other functions
            }

            double allowedDeviation;
            if (_toleranceIsPercent)
            {
                allowedDeviation = Math.Abs(_toleranceTargetValue * toleranceValue / 100.0);
            }
            else
            {
                allowedDeviation = toleranceValue;
            }

            double deviation = Math.Abs(currentValue - _toleranceTargetValue);
            return deviation <= allowedDeviation;
        }



        // Format measurement display for better readability while keeping original data
        private void DataGridViewRecords_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Format Tolerance column with colors
            if (dataGridViewRecords.Columns[e.ColumnIndex].Name == "Tolerance")
            {
                if (e.RowIndex >= 0 && e.RowIndex < dataGridViewRecords.Rows.Count)
                {
                    var row = dataGridViewRecords.Rows[e.RowIndex];
                    string toleranceValue = row.Cells["Tolerance"].Value?.ToString() ?? "";
                    
                    if (toleranceValue == "Pass")
                    {
                        row.Cells["Tolerance"].Style.BackColor = Color.LightGreen;
                        row.Cells["Tolerance"].Style.ForeColor = Color.DarkGreen;
                    }
                    else if (toleranceValue == "Fail")
                    {
                        row.Cells["Tolerance"].Style.BackColor = Color.LightPink;
                        row.Cells["Tolerance"].Style.ForeColor = Color.DarkRed;
                    }
                }
            }
            else if (dataGridViewRecords.Columns[e.ColumnIndex].Name == "Measurement")
            {
                if (e.RowIndex >= 0 && e.RowIndex < dataGridViewRecords.Rows.Count)
                {
                    var row = dataGridViewRecords.Rows[e.RowIndex];
                    string measurement = row.Cells["Measurement"].Value?.ToString() ?? "";
                    string unit = row.Cells["Unit"].Value?.ToString() ?? "";

                    if (measurement == "OVERLOAD")
                    {
                        e.Value = "OVERLOAD";
                        e.FormattingApplied = true;
                        return;
                    }

                    // Check for tolerance marker
                    bool hasToleranceMarker = measurement.Contains("[OVER]");
                    string cleanMeasurement = measurement.Replace(" [OVER]", "");
                    
                    // เอาส่วน inline editing ออกเนื่องจากใช้ Edit Panel แทน
                    
                    if (double.TryParse(cleanMeasurement, NumberStyles.Any, CultureInfo.InvariantCulture, out double value) && !string.IsNullOrEmpty(unit))
                    {
                        // Format for display readability
                        string formattedValue = FormatValueForDisplay(value, unit);
                        
                        // Add tolerance marker back if present
                        if (hasToleranceMarker)
                        {
                            formattedValue += " [OVER]";
                        }
                        
                        e.Value = formattedValue;
                        e.FormattingApplied = true;
                    }
                    else
                    {
                        // Fallback to original display
                        e.Value = !string.IsNullOrEmpty(unit) ? $"{measurement} {unit}" : measurement;
                        e.FormattingApplied = true;
                    }
                }
            }
        }

        // Helper method to format values for display (with k/M prefixes)
        private string FormatValueForDisplay(double value, string unit)
        {
            double absValue = Math.Abs(value);
            
            if (absValue >= 1000000) // Mega
            {
                return $"{(value / 1000000):F3} M{unit}";
            }
            else if (absValue >= 1000) // Kilo
            {
                return $"{(value / 1000):F3} k{unit}";
            }
            else if (absValue >= 1) // Standard
            {
                return $"{value:F4} {unit}";
            }
            else if (absValue >= 0.001) // milli
            {
                return $"{(value * 1000):F3} m{unit}";
            }
            else if (absValue >= 0.000001) // micro
            {
                return $"{(value * 1000000):F3} µ{unit}";
            }
            else // nano or smaller
            {
                return $"{value:F6} {unit}";
            }
        }

        #region --- DataGridView Edit Panel Event Handlers ---

        private void DataGridViewRecords_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // เรียก Edit Panel เมื่อ double click
            if (e.RowIndex >= 0)
            {
                buttonEditRecord_Click(sender, e);
            }
        }

        private void buttonEditRecord_Click(object sender, EventArgs e)
        {
            if (dataGridViewRecords.SelectedRows.Count == 0)
            {
                MessageBox.Show("กรุณาเลือกแถวที่ต้องการแก้ไข", "ไม่ได้เลือกแถว", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // เปิด Edit Panel Dialog
            ShowEditDialog();
        }

        private void ShowEditDialog()
        {
            var selectedRow = dataGridViewRecords.SelectedRows[0];
            int rowIndex = selectedRow.Index;
            
            // ดึงข้อมูลปัจจุบัน
            string currentMeasurement = selectedRow.Cells["Measurement"].Value?.ToString() ?? "";
            string function = selectedRow.Cells["Function"].Value?.ToString() ?? "";
            string unit = selectedRow.Cells["Unit"].Value?.ToString() ?? "";
            
            // แปลงค่าจากการแสดงผลกลับเป็นค่าดิบ
            string rawValue = GetRawValueFromDisplay(currentMeasurement);
            
            // สร้าง Edit Dialog
            using (var editForm = new Form())
            {
                editForm.Text = $"แก้ไขค่า - Row {rowIndex + 1}";
                editForm.Size = new Size(400, 200);
                editForm.StartPosition = FormStartPosition.CenterParent;
                editForm.BackColor = Color.FromArgb(45, 45, 48);
                editForm.ForeColor = Color.White;
                editForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                editForm.MaximizeBox = false;
                editForm.MinimizeBox = false;

                // Labels
                var lblFunction = new Label 
                { 
                    Text = $"Function: {function}", 
                    Location = new Point(20, 20), 
                    Size = new Size(350, 25),
                    ForeColor = Color.LightGray
                };
                
                var lblUnit = new Label 
                { 
                    Text = $"Unit: {unit}", 
                    Location = new Point(20, 45), 
                    Size = new Size(350, 25),
                    ForeColor = Color.LightGray
                };
                
                var lblValue = new Label 
                { 
                    Text = "ค่าใหม่:", 
                    Location = new Point(20, 75), 
                    Size = new Size(100, 25),
                    ForeColor = Color.White
                };
                
                // TextBox for editing
                var txtValue = new TextBox 
                { 
                    Text = rawValue,
                    Location = new Point(130, 72), 
                    Size = new Size(240, 25),
                    BackColor = Color.FromArgb(30, 30, 30),
                    ForeColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle
                };
                txtValue.SelectAll();
                
                // Buttons
                var btnOK = new Button 
                { 
                    Text = "ตกลง", 
                    Location = new Point(215, 115), 
                    Size = new Size(75, 30),
                    BackColor = Color.FromArgb(0, 122, 204),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                
                var btnCancel = new Button 
                { 
                    Text = "ยกเลิก", 
                    Location = new Point(295, 115), 
                    Size = new Size(75, 30),
                    BackColor = Color.FromArgb(63, 63, 70),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                
                btnOK.Click += (s, args) => 
                {
                    string newValue = txtValue.Text.Trim();
                    
                    // Validate input
                    if (!ValidateEditInput(newValue))
                    {
                        return; // ถ้า validate ไม่ผ่านให้คงอยู่ใน dialog
                    }
                    
                    // Update the record
                    UpdateRecord(rowIndex, newValue);
                    editForm.DialogResult = DialogResult.OK;
                    editForm.Close();
                };
                
                btnCancel.Click += (s, args) => 
                {
                    editForm.DialogResult = DialogResult.Cancel;
                    editForm.Close();
                };
                
                // Add controls
                editForm.Controls.AddRange(new Control[] { lblFunction, lblUnit, lblValue, txtValue, btnOK, btnCancel });
                
                // Set focus and show dialog
                txtValue.Focus();
                editForm.ShowDialog(this);
            }
        }

        private string GetRawValueFromDisplay(string displayValue)
        {
            // ถ้าเป็น OVERLOAD หรือมี [OVER] ให้คืนค่าเดิม
            if (displayValue.Equals("OVERLOAD", StringComparison.OrdinalIgnoreCase) || 
                displayValue.Contains("[OVER]"))
            {
                return displayValue;
            }
            
            // แยกค่าและหน่วยจากการแสดงผล
            if (displayValue.Contains(" "))
            {
                string[] parts = displayValue.Split(' ');
                if (parts.Length >= 2)
                {
                    string valuepart = parts[0];
                    string unitpart = string.Join(" ", parts.Skip(1));
                    
                    if (double.TryParse(valuepart, out double displayNum))
                    {
                        double actualValue = displayNum;
                        
                        // แปลงค่าจากหน่วยที่แสดงกลับเป็นค่าดิบ
                        if (unitpart.StartsWith("M"))
                        {
                            actualValue = displayNum * 1000000;
                        }
                        else if (unitpart.StartsWith("k"))
                        {
                            actualValue = displayNum * 1000;
                        }
                        else if (unitpart.StartsWith("m"))
                        {
                            actualValue = displayNum / 1000;
                        }
                        else if (unitpart.StartsWith("µ"))
                        {
                            actualValue = displayNum / 1000000;
                        }
                        
                        return actualValue.ToString("F6", CultureInfo.InvariantCulture);
                    }
                }
            }
            
            return displayValue;
        }

        private bool ValidateEditInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                MessageBox.Show("กรุณาใส่ค่า", "ค่าว่างเปล่า", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            
            // อนุญาต OVERLOAD และ [OVER]
            if (input.Equals("OVERLOAD", StringComparison.OrdinalIgnoreCase) || 
                input.Contains("[OVER]"))
            {
                return true;
            }
            
            // ตรวจสอบว่าเป็นตัวเลข
            if (!double.TryParse(input.Replace(" [OVER]", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            {
                MessageBox.Show("กรุณาใส่ค่าตัวเลขที่ถูกต้อง หรือ 'OVERLOAD'", "ค่าไม่ถูกต้อง", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            
            return true;
        }

        private void UpdateRecord(int rowIndex, string newValue)
        {
            try
            {
                // อัพเดตค่าใน DataTable
                _recordsTable.Rows[rowIndex]["Measurement"] = newValue;
                _recordsTable.AcceptChanges();
                
                // บันทึกลงไฟล์
                SaveDataToFile();
                
                // Refresh การแสดงผล
                dataGridViewRecords.InvalidateRow(rowIndex);
                
                // Log และเล่นเสียง
                LogActivity($"แก้ไขค่าเสร็จสิ้น Row {rowIndex + 1}: {newValue}");
                SoundUtil.Beep();
                
                // เลือกแถวที่แก้ไขไว้
                dataGridViewRecords.Rows[rowIndex].Selected = true;
            }
            catch (Exception ex)
            {
                LogActivity($"เกิดข้อผิดพลาดในการแก้ไข: {ex.Message}", true);
                MessageBox.Show($"เกิดข้อผิดพลาดในการแก้ไข:\n{ex.Message}", 
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        // *** เพิ่มการ select แถวล่าสุดหลังจากลบข้อมูล ***
        private void buttonDeleteRecord_Click(object sender, EventArgs e)
        {
            if (dataGridViewRecords.SelectedRows.Count == 0) return;
            var confirmResult = MessageBox.Show("ยืนยันการลบแถวที่เลือก?", "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmResult == DialogResult.Yes)
            {
                SoundUtil.Delete();

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

        private void buttonCopyTable_Click(object sender, EventArgs e)
        {
            if (_recordsTable.Rows.Count == 0)
            {
                MessageBox.Show("ไม่มีข้อมูลให้คัดลอก", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var measurementValues = new List<string>();

                // เฉพาะค่า Measurement
                foreach (DataRow row in _recordsTable.Rows)
                {
                    measurementValues.Add(row["Measurement"].ToString());
                }

                // Copy to clipboard with multiple formats for better compatibility
                string clipboardText = string.Join(Environment.NewLine, measurementValues);
                
                var dataObject = new DataObject();
                dataObject.SetText(clipboardText, TextDataFormat.Text);
                dataObject.SetText(clipboardText, TextDataFormat.UnicodeText);
                
                // For Excel compatibility, also set as CSV format
                string csvData = string.Join("\r\n", measurementValues);
                dataObject.SetData(DataFormats.CommaSeparatedValue, csvData);
                
                Clipboard.SetDataObject(dataObject, true);

                LogActivity($"คัดลอกข้อมูล {_recordsTable.Rows.Count} แถวไปยังคลิปบอร์ดแล้ว");
                MessageBox.Show("คัดลอกข้อมูลไปยังคลิปบอร์ดสำเร็จ!", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogActivity($"คัดลอกข้อมูลล้มเหลว: {ex.Message}", true);
                MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonCopyTableHorizontal_Click(object sender, EventArgs e)
        {
            if (_recordsTable.Rows.Count == 0)
            {
                MessageBox.Show("ไม่มีข้อมูลให้คัดลอก", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var measurementValues = new List<string>();

                // เฉพาะค่า Measurement
                foreach (DataRow row in _recordsTable.Rows)
                {
                    measurementValues.Add(row["Measurement"].ToString());
                }

                // Copy to clipboard horizontally with tab separation
                string clipboardText = string.Join("\t", measurementValues);
                
                var dataObject = new DataObject();
                dataObject.SetText(clipboardText, TextDataFormat.Text);
                dataObject.SetText(clipboardText, TextDataFormat.UnicodeText);
                
                // For Excel compatibility, also set as CSV format with tab delimiter
                dataObject.SetData(DataFormats.CommaSeparatedValue, clipboardText);
                
                Clipboard.SetDataObject(dataObject, true);

                LogActivity($"คัดลอกข้อมูลแนวนอน {_recordsTable.Rows.Count} แถวไปยังคลิปบอร์ดแล้ว");
            }
            catch (Exception ex)
            {
                LogActivity($"คัดลอกข้อมูลแนวนอนล้มเหลว: {ex.Message}", true);
                MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void buttonGetIDN_Click(object sender, EventArgs e)
        {
            if (_dmm == null || !_dmm.IsConnected) return;
            textBoxSystemInfo.Text = await _dmm.QueryCommandAsync("*IDN?");
        }

        private MySqlManager _mysqlManager = null;

        private void InitializeQWRecord()
        {
            textBoxQWID.Text = SettingsManager.CurrentQWID;
            textBoxSection.Text = SettingsManager.CurrentSection;
            
            // Wire up event handlers
            textBoxQWID.TextChanged += TextBoxQWID_TextChanged;
            textBoxSection.TextChanged += TextBoxSection_TextChanged;
        }

        private async void InitializeMySQL()
        {
            if (SettingsManager.MySqlEnabled)
            {
                try
                {
                    _mysqlManager = DatabaseConfig.CreateMySqlManager();

                    bool connected = await _mysqlManager.TestConnectionAsync();
                    if (connected)
                    {
                        await _mysqlManager.InitializeDatabaseAsync();
                        LogActivity("เชื่อมต่อ MySQL สำเร็จ");
                    }
                    else
                    {
                        LogActivity("เชื่อมต่อ MySQL ล้มเหลว", true);
                        _mysqlManager?.Dispose();
                        _mysqlManager = null;
                    }
                }
                catch (Exception ex)
                {
                    LogActivity($"MySQL error: {ex.Message}", true);
                    _mysqlManager?.Dispose();
                    _mysqlManager = null;
                }
            }
        }

        private void TextBoxQWID_TextChanged(object sender, EventArgs e)
        {
            SettingsManager.CurrentQWID = textBoxQWID.Text;
            SettingsManager.SaveSettings();
        }

        private void TextBoxSection_TextChanged(object sender, EventArgs e)
        {
            SettingsManager.CurrentSection = textBoxSection.Text;
            SettingsManager.SaveSettings();
        }

        private void buttonNewQW_Click(object sender, EventArgs e)
        {
            string newQWID = $"QW{DateTime.Now:yyyyMMdd}{new Random().Next(10, 99)}";
            textBoxQWID.Text = newQWID;
            LogActivity($"สร้าง QW ID ใหม่: {newQWID}");
        }

        private void buttonSettings_Click(object sender, EventArgs e)
        {
            using (SettingsForm settingsForm = new SettingsForm())
            {
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    LogActivity("บันทึกการตั้งค่าเรียบร้อยแล้ว");
                    
                    // Reinitialize MySQL if settings changed
                    _mysqlManager?.Dispose();
                    _mysqlManager = null;
                    InitializeMySQL();
                }
            }
        }

        private async Task SaveToMySQLAsync(int measurementIndex, string function, string measurement, string unit, DateTime timestamp, string toleranceStatus)
        {
            try
            {
                // Create or update QW Record
                int qwRecordId = await _mysqlManager.CreateOrUpdateQWRecordAsync(
                    SettingsManager.CurrentQWID,
                    SettingsManager.CurrentSection,
                    SettingsManager.InstrumentSerial,
                    SettingsManager.OperatorID.ToString()
                );

                // Parse measurement value
                if (decimal.TryParse(measurement, out decimal measurementValue))
                {
                    // Create tolerance data
                    ToleranceData toleranceData = null;
                    if (_toleranceEnabled)
                    {
                        toleranceData = new ToleranceData
                        {
                            UpperLimit = _toleranceIsPercent ? null : (decimal?)_toleranceValueDC,
                            LowerLimit = _toleranceIsPercent ? null : (decimal?)_toleranceValueDC,
                            UpperPercent = _toleranceIsPercent ? (decimal?)_toleranceValueDC : null,
                            LowerPercent = _toleranceIsPercent ? (decimal?)_toleranceValueDC : null,
                            UpperAbs = !_toleranceIsPercent ? (decimal?)_toleranceValueDC : null,
                            LowerAbs = !_toleranceIsPercent ? (decimal?)_toleranceValueDC : null,
                            Enabled = true
                        };
                    }

                    // Insert measurement
                    int measurementId = await _mysqlManager.InsertMeasurementAsync(
                        qwRecordId,
                        measurementIndex,
                        function,
                        measurementValue,
                        unit,
                        timestamp,
                        toleranceStatus,
                        toleranceData
                    );

                    LogActivity($"บันทึกใน MySQL สำเร็จ: QW {SettingsManager.CurrentQWID}, Measurement #{measurementIndex}");
                }
                else
                {
                    LogActivity($"ไม่สามารถแปลงค่า measurement: {measurement}", true);
                }
            }
            catch (Exception ex)
            {
                LogActivity($"MySQL บันทึกล้มเหลว: {ex.Message}", true);
            }
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
            _mysqlManager?.Dispose();
        }

        #endregion

        #region --- R&D Tab Methods ---

        private void InitializeRDDataTable()
        {
            _rdRecordsTable = new DataTable("RDMeasurementRecords");
            _rdRecordsTable.Columns.Add("No", typeof(int));
            _rdRecordsTable.Columns.Add("Function", typeof(string));
            _rdRecordsTable.Columns.Add("Measurement", typeof(string));
            _rdRecordsTable.Columns.Add("Upper", typeof(string));
            _rdRecordsTable.Columns.Add("Lower", typeof(string));
            _rdRecordsTable.Columns.Add("ToleranceEnable", typeof(bool));

            dataGridViewRD.DataSource = _rdRecordsTable;

            // ตั้งค่า AutoSizeMode
            dataGridViewRD.Columns["No"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewRD.Columns["Function"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewRD.Columns["Measurement"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewRD.Columns["Upper"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewRD.Columns["Lower"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewRD.Columns["ToleranceEnable"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            // ตั้งค่า Selection Mode
            dataGridViewRD.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewRD.MultiSelect = false;
            dataGridViewRD.ReadOnly = false; // อนุญาตให้แก้ไขได้
            dataGridViewRD.AllowUserToDeleteRows = false;
            dataGridViewRD.AllowUserToAddRows = false;

            // เพิ่ม Event Handler สำหรับ Cell Click เพื่อแก้ไข tolerance
            dataGridViewRD.CellClick += DataGridViewRD_CellClick;
            dataGridViewRD.CellValueChanged += DataGridViewRD_CellValueChanged;
        }

        private void InitializeRDTab()
        {
            // เพิ่ม Event Handlers สำหรับปุ่มต่างๆ
            buttonRecordRD.Click += ButtonRecordRD_Click;
            buttonDeleteRD.Click += ButtonDeleteRD_Click;
            buttonExportRD.Click += ButtonExportRD_Click;
            buttonClearRD.Click += ButtonClearRD_Click;
            buttonChangeModel.Click += ButtonChangeModel_Click;
            
            // เพิ่ม Tab Changed Event
            tabControl1.SelectedIndexChanged += TabControl1_SelectedIndexChanged;
            
            LogActivity("เริ่มต้น R&D Tab สำเร็จ");
        }

        private async Task<bool> ShowRDSelectionDialog()
        {
            try
            {
                using (var dialog = new RDSelectionDialog())
                {
                    DialogResult result;
                    try
                    {
                        result = dialog.ShowDialog(this);
                    }
                    catch (System.Runtime.InteropServices.COMException comEx)
                    {
                        LogActivity($"COM Interop Warning (ไม่กระทบการทำงาน): {comEx.Message}");
                        // ลองใหม่อีกครั้ง
                        result = dialog.ShowDialog(this);
                    }
                    catch (System.InvalidOperationException invEx) when (invEx.Message.Contains("ComboBox"))
                    {
                        LogActivity($"ComboBox COM Warning (ไม่กระทบการทำงาน): {invEx.Message}");
                        // ลองใหม่อีกครั้ง
                        result = dialog.ShowDialog(this);
                    }
                    
                    if (result == DialogResult.OK)
                    {
                        _currentModelId = dialog.SelectedModelId;
                        _currentSerialNumber = dialog.SelectedSerialNumber;
                        
                        if (dialog.IsCreateNew)
                        {
                            // สร้าง Serial Number ใหม่
                            using (var createDialog = new CreateSerialDialog(_currentModelId, dialog.SelectedModelName))
                            {
                                DialogResult createResult;
                                try
                                {
                                    createResult = createDialog.ShowDialog(this);
                                }
                                catch (System.Runtime.InteropServices.COMException comEx)
                                {
                                    LogActivity($"COM Interop Warning ใน Create Dialog (ไม่กระทบการทำงาน): {comEx.Message}");
                                    createResult = createDialog.ShowDialog(this);
                                }
                                catch (System.InvalidOperationException invEx) when (invEx.Message.Contains("ComboBox") || invEx.Message.Contains("TextBox"))
                                {
                                    LogActivity($"Control COM Warning ใน Create Dialog (ไม่กระทบการทำงาน): {invEx.Message}");
                                    createResult = createDialog.ShowDialog(this);
                                }
                                
                                if (createResult == DialogResult.OK)
                                {
                                    _currentSerialNumber = createDialog.SerialNumber;
                                    _currentSerialId = await GetSerialIdByNumber(_currentSerialNumber, _currentModelId);
                                    
                                    LogActivity($"สร้าง Serial Number ใหม่: {_currentSerialNumber} สำหรับ Model: {dialog.SelectedModelName}");
                                    
                                    // อัปเดต UI
                                    await UpdateRDTabUI();
                                    await LoadRDRecordsFromDatabase();
                                    
                                    return true;
                                }
                                else
                                {
                                    // ยกเลิกการสร้าง SN ใหม่ - กลับไป tab เดิม
                                    tabControl1.SelectedIndex = 0; // กลับไป Data Recording tab
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            // โหลด Serial Number ที่มีอยู่
                            _currentSerialId = dialog.SelectedSerialId;
                            
                            LogActivity($"โหลด Serial Number: {_currentSerialNumber} สำหรับ Model: {dialog.SelectedModelName}");
                            
                            // อัปเดต UI
                            await UpdateRDTabUI();
                            await LoadRDRecordsFromDatabase();
                            
                            return true;
                        }
                    }
                    else
                    {
                        // ยกเลิก - กลับไป tab เดิม
                        tabControl1.SelectedIndex = 0; // กลับไป Data Recording tab
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                LogActivity($"เกิดข้อผิดพลาดใน R&D Selection Dialog: {ex.Message}", true);
                MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}", "ข้อผิดพลาด", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                tabControl1.SelectedIndex = 0; // กลับไป Data Recording tab
                return false;
            }
        }

        private async Task<int> GetSerialIdByNumber(string serialNumber, int modelId)
        {
            try
            {
                // ใช้ฟังก์ชันใหม่ที่ไม่สนใจ model_id เพราะ Serial Number ไม่ซ้ำกันอยู่แล้ว
                var serialId = await _mysqlManager.GetSoftwareSerialNumberIdBySerialAsync(serialNumber);
                return serialId ?? -1;
            }
            catch (Exception ex)
            {
                LogActivity($"เกิดข้อผิดพลาดในการค้นหา Serial ID: {ex.Message}", true);
            }
            
            return -1;
        }

        private async Task UpdateRDTabUI()
        {
            try
            {
                // อัปเดต UI elements ใน R&D Tab
                if (_currentModelId > 0 && _currentSerialId > 0)
                {
                    // แสดงข้อมูลที่เลือกไว้
                    string modelName = await GetModelNameById(_currentModelId);
                    LogActivity($"R&D Tab พร้อมใช้งาน - Model: {modelName}, SN: {_currentSerialNumber}");
                }
            }
            catch (Exception ex)
            {
                LogActivity($"เกิดข้อผิดพลาดในการอัปเดต R&D Tab UI: {ex.Message}", true);
            }
        }

        private async Task<string> GetModelNameById(int modelId)
        {
            try
            {
                // Query from Spaze database
                string sql = $"SELECT name FROM spaze.models WHERE id = {modelId}";
                var result = await ExecuteSQLQuery(sql);
                
                if (result.Success && result.Data != null && result.Data.Rows.Count > 0)
                {
                    return result.Data.Rows[0]["name"].ToString();
                }
            }
            catch (Exception ex)
            {
                LogActivity($"เกิดข้อผิดพลาดในการค้นหาชื่อ Model: {ex.Message}", true);
            }
            
            return "Unknown Model";
        }

        private async Task LoadRDRecordsFromDatabase()
        {
            try
            {
                if (_currentSerialId <= 0)
                {
                    LogActivity("ไม่มี Serial ID สำหรับโหลดข้อมูล R&D", true);
                    return;
                }

                string sql = $@"SELECT sm.id, sm.function_name, sm.measurement_value, sm.upper_limit, sm.lower_limit, sm.tolerance_enabled, sm.measured_at
                               FROM software_measurements sm 
                               WHERE sm.serial_id = {_currentSerialId} 
                               ORDER BY sm.measured_at DESC";
                
                var result = await ExecuteSQLQuery(sql);
                
                if (result.Success && result.Data != null && result.Data.Rows.Count > 0)
                {
                    _rdRecordsTable.Clear();
                    int no = 1;
                    
                    foreach (DataRow row in result.Data.Rows)
                    {
                        var newRow = _rdRecordsTable.NewRow();
                        newRow["No"] = no++;
                        newRow["Function"] = row["function_name"]?.ToString() ?? "";
                        
                        // จัดการกับค่า NULL และ DBNull
                        newRow["Measurement"] = row["measurement_value"] != DBNull.Value 
                            ? Convert.ToDouble(row["measurement_value"]) 
                            : 0.0;
                        newRow["Upper"] = row["upper_limit"] != DBNull.Value 
                            ? Convert.ToDouble(row["upper_limit"]) 
                            : 0.0;
                        newRow["Lower"] = row["lower_limit"] != DBNull.Value 
                            ? Convert.ToDouble(row["lower_limit"]) 
                            : 0.0;
                        newRow["ToleranceEnable"] = row["tolerance_enabled"] != DBNull.Value 
                            ? Convert.ToBoolean(row["tolerance_enabled"]) 
                            : false;
                        
                        _rdRecordsTable.Rows.Add(newRow);
                    }
                    
                    LogActivity($"โหลดข้อมูล R&D จำนวน {_rdRecordsTable.Rows.Count} รายการ");
                }
                else
                {
                    LogActivity("ไม่พบข้อมูล R&D ในฐานข้อมูล");
                }
            }
            catch (Exception ex)
            {
                LogActivity($"เกิดข้อผิดพลาดในการโหลดข้อมูล R&D: {ex.Message}", true);
            }
        }


        private async Task<(bool Success, DataTable Data, string Message)> ExecuteSQLQuery(string sql)
        {
            try
            {
                Console.WriteLine($"[ExecuteSQLQuery] SQL: {sql}");
                
                // ตรวจสอบว่ามี MySqlManager หรือไม่
                if (_mysqlManager == null)
                {
                    Console.WriteLine("[ExecuteSQLQuery] MySqlManager is null, initializing...");
                    InitializeMySQL();
                    
                    if (_mysqlManager == null)
                    {
                        string errorMsg = "MySqlManager ยังไม่ได้เชื่อมต่อ";
                        Console.WriteLine($"[ExecuteSQLQuery] Error: {errorMsg}");
                        return (false, null, errorMsg);
                    }
                }
                
                // ใช้ MySqlManager จริง
                var result = await _mysqlManager.ExecuteQuery(sql);
                
                Console.WriteLine($"[ExecuteSQLQuery] Result - Success: {result.Success}");
                
                if (result.Success)
                {
                    Console.WriteLine($"[ExecuteSQLQuery] Data rows: {result.Data?.Rows?.Count ?? 0}");
                    return (result.Success, result.Data, result.ErrorMessage ?? "Success");
                }
                else
                {
                    string errorMsg = result.ErrorMessage ?? "Query execution failed";
                    Console.WriteLine($"[ExecuteSQLQuery] Error: {errorMsg}");
                    return (result.Success, result.Data, errorMsg);
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"SQL Query Error: {ex.Message}";
                Console.WriteLine($"[ExecuteSQLQuery] Exception: {errorMsg}");
                Console.WriteLine($"[ExecuteSQLQuery] Stack Trace: {ex.StackTrace}");
                LogActivity(errorMsg, true);
                return (false, null, errorMsg);
            }
        }

        private async void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            _isRDTabActive = (tabControl1.SelectedTab == tab_rd);
            
            if (_isRDTabActive)
            {
                LogActivity("เข้าสู่ R&D Tab");
                // ตรวจสอบว่าต้องเลือก Model และ SN หรือไม่
                if (_currentModelId == -1 || _currentSerialId == -1)
                {
                    await ShowRDSelectionDialog();
                }
            }
            else
            {
                LogActivity("ออกจาก R&D Tab");
            }
        }

        private async void ButtonRecordRD_Click(object sender, EventArgs e)
        {
            if (!_isRDTabActive)
            {
                await saveRecord(); // ใช้ฟังก์ชันเดิมถ้าไม่ได้อยู่ใน R&D Tab
                return;
            }

            await SaveRDRecord();
        }

        private async void ButtonDeleteRD_Click(object sender, EventArgs e)
        {
            try
            {
                // ตรวจสอบว่ามีแถวที่เลือกหรือไม่
                if (dataGridViewRD.SelectedRows.Count == 0)
                {
                    MessageBox.Show("กรุณาเลือกแถวที่ต้องการลบ", "ไม่ได้เลือกแถว", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // ยืนยันการลบ
                var result = MessageBox.Show(
                    $"ต้องการลบแถวที่เลือก ({dataGridViewRD.SelectedRows.Count} แถว) หรือไม่?",
                    "ยืนยันการลบ",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                    return;

                // ลบแถวที่เลือก
                var selectedRows = dataGridViewRD.SelectedRows.Cast<DataGridViewRow>().ToList();
                
                foreach (var row in selectedRows)
                {
                    if (row.IsNewRow) continue;
                    
                    // ดึงข้อมูลจากแถว
                    int measurementNo = Convert.ToInt32(row.Cells["No"].Value);
                    
                    // ลบจาก database
                    await DeleteRDMeasurementFromDatabase(measurementNo);
                    
                    // ลบจาก DataTable
                    _rdRecordsTable.Rows.Remove(((DataRowView)row.DataBoundItem).Row);
                }

                // อัปเดตหมายเลข No. ใหม่
                RenumberRDRecords();
                
                LogActivity($"ลบข้อมูล R&D สำเร็จ ({selectedRows.Count} แถว)");
            }
            catch (Exception ex)
            {
                LogActivity($"ลบข้อมูล R&D ล้มเหลว: {ex.Message}", true);
                MessageBox.Show($"เกิดข้อผิดพลาดในการลบข้อมูล: {ex.Message}", 
                              "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task DeleteRDMeasurementFromDatabase(int measurementNo)
        {
            try
            {
                string deleteSQL = $@"
                    DELETE FROM software_measurements 
                    WHERE serial_id = {_currentSerialId} AND measurement_no = {measurementNo}";
                
                Console.WriteLine($"[RD DELETE] SQL Query: {deleteSQL}");
                
                var result = await ExecuteSQLQuery(deleteSQL);
                
                if (!result.Success)
                {
                    LogActivity($"ลบ measurement จาก database ล้มเหลว: {result.Message}", true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RD DELETE] Exception: {ex.Message}");
                LogActivity($"ลบ measurement จาก database ล้มเหลว: {ex.Message}", true);
            }
        }

        private void RenumberRDRecords()
        {
            // อัปเดตหมายเลข No. ให้เรียงลำดับใหม่
            for (int i = 0; i < _rdRecordsTable.Rows.Count; i++)
            {
                _rdRecordsTable.Rows[i]["No"] = i + 1;
            }
            
            // อัปเดต measurement_no ใน database
            Task.Run(async () =>
            {
                try
                {
                    for (int i = 0; i < _rdRecordsTable.Rows.Count; i++)
                    {
                        int newNo = i + 1;
                        string function = _rdRecordsTable.Rows[i]["Function"].ToString();
                        
                        string updateSQL = $@"
                            UPDATE software_measurements 
                            SET measurement_no = {newNo}
                            WHERE serial_id = {_currentSerialId} 
                            AND function_name = '{function}'
                            AND measurement_no != {newNo}
                            LIMIT 1";
                        
                        await ExecuteSQLQuery(updateSQL);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RD RENUMBER] Exception: {ex.Message}");
                }
            });
        }

        private async Task SaveRDRecord()
        {
            if (_dmm == null || !_dmm.IsConnected || lblReading.Text == "CONFIG..." || lblReading.Text == "ERROR")
            {
                LogActivity("ไม่สามารถบันทึกได้: ไม่มีค่าที่วัดได้", true);
                return;
            }

            if (_currentModelId <= 0 || _currentSerialId <= 0)
            {
                MessageBox.Show("กรุณาเลือก Model และสร้าง/โหลด Serial Number ก่อน", 
                              "ไม่ได้เลือก Model/SN", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int newNo = _rdRecordsTable.Rows.Count + 1;
                string function = _currentFunction.ToString();
                string measurement = _isOverload ? "OVERLOAD" : _lastReadingValue.ToString("F4", CultureInfo.InvariantCulture);
                
                // เพิ่มข้อมูลใน DataTable
                _rdRecordsTable.Rows.Add(newNo, function, measurement, "", "", false);
                
                // บันทึกลง database
                await SaveRDMeasurementToDatabase(newNo, function, _lastReadingValue);
                
                LogActivity($"บันทึกค่า R&D No. {newNo}: {function}, {measurement}");
                SoundUtil.Beep();
            }
            catch (Exception ex)
            {
                LogActivity($"บันทึก R&D ล้มเหลว: {ex.Message}", true);
            }
        }

        private async Task SaveRDMeasurementToDatabase(int measurementNo, string function, double value)
        {
            try
            {
                string insertSQL = $@"
                    INSERT INTO software_measurements 
                    (serial_id, measurement_no, function_name, measurement_value, tolerance_enabled, measured_at)
                    VALUES ({_currentSerialId}, {measurementNo}, '{function}', {value}, false, NOW())";
                
                Console.WriteLine($"[RD SAVE] SQL Query: {insertSQL}");
                Console.WriteLine($"[RD SAVE] Parameters - SerialId: {_currentSerialId}, MeasurementNo: {measurementNo}, Function: {function}, Value: {value}");
                
                var result = await ExecuteSQLQuery(insertSQL);
                
                Console.WriteLine($"[RD SAVE] Query Result - Success: {result.Success}, Message: {result.Message}");
                
                if (!result.Success)
                {
                    LogActivity($"บันทึก measurement ล้มเหลว: {result.Message}", true);
                }
                else
                {
                    LogActivity($"บันทึก measurement สำเร็จ - No: {measurementNo}, Function: {function}, Value: {value}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RD SAVE] Exception: {ex.Message}");
                Console.WriteLine($"[RD SAVE] Stack Trace: {ex.StackTrace}");
                LogActivity($"บันทึก measurement ลง database ล้มเหลว: {ex.Message}", true);
            }
        }

        private void DataGridViewRD_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                string columnName = dataGridViewRD.Columns[e.ColumnIndex].Name;
                
                // อนุญาตให้แก้ไขได้เฉพาะคอลัมน์ Upper, Lower, ToleranceEnable
                if (columnName == "Upper" || columnName == "Lower" || columnName == "ToleranceEnable")
                {
                    dataGridViewRD.ReadOnly = false;
                    dataGridViewRD.BeginEdit(true);
                }
                else
                {
                    dataGridViewRD.ReadOnly = true;
                }
            }
        }

        private void DataGridViewRD_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                try
                {
                    var row = dataGridViewRD.Rows[e.RowIndex];
                    int measurementNo = Convert.ToInt32(row.Cells["No"].Value);
                    string columnName = dataGridViewRD.Columns[e.ColumnIndex].Name;
                    
                    // อัพเดต database เมื่อมีการเปลี่ยนแปลง tolerance settings
                    if (columnName == "Upper" || columnName == "Lower" || columnName == "ToleranceEnable")
                    {
                        _ = UpdateToleranceInDatabase(measurementNo, row);
                        LogActivity($"อัพเดต tolerance สำหรับ measurement #{measurementNo}");
                    }
                }
                catch (Exception ex)
                {
                    LogActivity($"อัพเดต tolerance ล้มเหลว: {ex.Message}", true);
                }
            }
        }

        private async Task UpdateToleranceInDatabase(int measurementNo, DataGridViewRow row)
        {
            try
            {
                string upperLimit = row.Cells["Upper"].Value?.ToString() ?? "";
                string lowerLimit = row.Cells["Lower"].Value?.ToString() ?? "";
                bool toleranceEnabled = Convert.ToBoolean(row.Cells["ToleranceEnable"].Value ?? false);
                
                string updateSQL = $@"
                    UPDATE software_measurements 
                    SET upper_limit = {(string.IsNullOrEmpty(upperLimit) ? "NULL" : upperLimit)},
                        lower_limit = {(string.IsNullOrEmpty(lowerLimit) ? "NULL" : lowerLimit)},
                        tolerance_enabled = {toleranceEnabled}
                    WHERE serial_id = {_currentSerialId} AND measurement_no = {measurementNo}";
                
                var updateResult = await ExecuteSQLQuery(updateSQL);
                Console.WriteLine($"[UPDATE MEASUREMENT] Success: {updateResult.Success}, Message: {updateResult.Message}");
            }
            catch (Exception ex)
            {
                LogActivity($"อัพเดต tolerance ใน database ล้มเหลว: {ex.Message}", true);
            }
        }

        private void ButtonExportRD_Click(object sender, EventArgs e)
        {
            // Export R&D data to CSV
            if (_rdRecordsTable.Rows.Count == 0)
            {
                MessageBox.Show("ไม่มีข้อมูล R&D ให้ส่งออก", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "CSV File (*.csv)|*.csv";
                saveFileDialog.FileName = $"RD_Data_{_currentSerialNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var lines = new List<string>();
                        lines.Add("No,Function,Measurement,Upper,Lower,ToleranceEnable");
                        
                        foreach (DataRow row in _rdRecordsTable.Rows)
                        {
                            string csvLine = string.Join(",",
                                $"\"{row["No"]}\"",
                                $"\"{row["Function"]}\"",
                                $"\"{row["Measurement"]}\"",
                                $"\"{row["Upper"]}\"",
                                $"\"{row["Lower"]}\"",
                                $"\"{row["ToleranceEnable"]}\""
                            );
                            lines.Add(csvLine);
                        }

                        File.WriteAllLines(saveFileDialog.FileName, lines, Encoding.UTF8);
                        LogActivity($"ส่งออกข้อมูล R&D ไปยัง {saveFileDialog.FileName} สำเร็จ");
                        MessageBox.Show("ส่งออกข้อมูล R&D สำเร็จ!", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        LogActivity($"ส่งออก R&D CSV ล้มเหลว: {ex.Message}", true);
                        MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ButtonClearRD_Click(object sender, EventArgs e)
        {
            if (_rdRecordsTable.Rows.Count == 0) return;
            
            var confirmResult = MessageBox.Show("ยืนยันการล้างข้อมูล R&D ทั้งหมด?", 
                                              "ยืนยันการล้างข้อมูล", 
                                              MessageBoxButtons.YesNo, 
                                              MessageBoxIcon.Warning);

            if (confirmResult == DialogResult.Yes)
            {
                _rdRecordsTable.Clear();
                LogActivity("ล้างข้อมูล R&D ทั้งหมดแล้ว");
            }
        }

        private async void ButtonChangeModel_Click(object sender, EventArgs e)
        {
            // ล้าง datatable
            _rdRecordsTable.Clear();
            
            // รีเซ็ตค่า Model และ Serial Number
            _currentModelId = -1;
            _currentSerialId = -1;
            _currentSerialNumber = "";
            
            LogActivity("เปลี่ยนโมเดล: ล้างข้อมูลและเลือกใหม่");
            
            // เปิด RDSelectionDialog เพื่อเลือกใหม่
            await ShowRDSelectionDialog();
        }

        #endregion

        private bool isWaitingForSecondCtrlQ = false;
        private int doubleKeyInterval = 500;

        private void deleteLatestRecord()
        {
            if (_recordsTable.Rows.Count == 0)
            {
                LogActivity("ไม่มีข้อมูลให้ลบ", true);
                return;
            }

            try
            {
                SoundUtil.Delete();

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
                        await saveRecord();
                    }
                }

                e.Handled = true;
            }
        }

        private Task<bool> PerformAuthentication()
        {
            try
            {
                using (var loginForm = new LoginForm())
                {
                    var result = loginForm.ShowDialog(this);
                    
                    if (result == DialogResult.OK && loginForm.LoginResult.Success)
                    {
                        LogActivity($"ผู้ใช้ {AuthManager.CurrentUser.FullName} เข้าสู่ระบบสำเร็จ");
                        return Task.FromResult(true);
                    }
                    else
                    {
                        LogActivity("การเข้าสู่ระบบถูกยกเลิก");
                        return Task.FromResult(false);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการเข้าสู่ระบบ:\n{ex.Message}", 
                              "Authentication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return Task.FromResult(false);
            }
        }

        private void UpdateUserDisplay()
        {
            if (AuthManager.IsLoggedIn() && AuthManager.CurrentUser != null)
            {
                // Update status bar or add user info to title
                this.Text = $"MultiRecord - {AuthManager.CurrentUser.FullName} ({AuthManager.CurrentUser.RoleDisplayName})";
                LogActivity($"ยินดีต้อนรับ {AuthManager.CurrentUser.FullName} สู่ระบบ MultiRecord");
            }
        }

        private async Task LogoutUser()
        {
            try
            {
                await AuthManager.LogoutAsync();
                LogActivity("ออกจากระบบเรียบร้อยแล้ว");
                
                // Close the application or show login again
                Application.Restart();
            }
            catch (Exception ex)
            {
                LogActivity($"เกิดข้อผิดพลาดในการออกจากระบบ: {ex.Message}");
            }
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            this.KeyPreview = true; // ให้ Form รับ KeyDown ก่อน Control อื่น
            
            // Show login form before initializing the application
            if (!await PerformAuthentication())
            {
                this.Close();
                return;
            }
            
            InitializeMySQLSettings();
            UpdateUserDisplay();
        }

        private void InitializeMySQLSettings()
        {
            // Load MySQL settings from SettingsManager
            textBoxMySqlHost.Text = SettingsManager.MySqlHost;
            textBoxMySqlPort.Text = SettingsManager.MySqlPort.ToString();
            textBoxUser.Text = SettingsManager.MySqlUser;
            textBoxPassword.Text = SettingsManager.MySqlPassword;
            textBoxDatabase.Text = SettingsManager.MySqlDatabase;
            checkBoxMySQLEnabled.Checked = SettingsManager.MySqlEnabled;
            
            // Add event handlers for settings changes
            textBoxMySqlHost.TextChanged += (s, e) => { SettingsManager.MySqlHost = textBoxMySqlHost.Text; SettingsManager.SaveSettings(); };
            textBoxMySqlPort.TextChanged += (s, e) => { if (int.TryParse(textBoxMySqlPort.Text, out int port)) { SettingsManager.MySqlPort = port; SettingsManager.SaveSettings(); } };
            textBoxUser.TextChanged += (s, e) => { SettingsManager.MySqlUser = textBoxUser.Text; SettingsManager.SaveSettings(); };
            textBoxPassword.TextChanged += (s, e) => { SettingsManager.MySqlPassword = textBoxPassword.Text; SettingsManager.SaveSettings(); };
            textBoxDatabase.TextChanged += (s, e) => { SettingsManager.MySqlDatabase = textBoxDatabase.Text; SettingsManager.SaveSettings(); };
            checkBoxMySQLEnabled.CheckedChanged += (s, e) => { SettingsManager.MySqlEnabled = checkBoxMySQLEnabled.Checked; SettingsManager.SaveSettings(); };
            
            // Add event handler for test connection button
            buttonTestConnection.Click += buttonTestConnection_Click;
            
            labelConnectionStatus.Text = "Ready to test connection";
        }

        private async void buttonTestConnection_Click(object sender, EventArgs e)
        {
            try
            {
                labelConnectionStatus.Text = "Testing connection...";
                labelConnectionStatus.ForeColor = Color.Yellow;
                
                // Log connection details for debugging
                string connectionDetails = $"Host: {SettingsManager.MySqlHost}:{SettingsManager.MySqlPort}, " +
                                         $"User: {SettingsManager.MySqlUser}, " +
                                         $"Database: {SettingsManager.MySqlDatabase}";
                
                richTextBoxLog.AppendText($"[{DateTime.Now:HH:mm:ss}] Attempting MySQL connection - {connectionDetails}\n");
                richTextBoxLog.ScrollToCaret();
                
                labelConnectionStatus.Text = "Testing network connectivity...";
                richTextBoxLog.AppendText($"[{DateTime.Now:HH:mm:ss}] Step 1: Testing network ping to {SettingsManager.MySqlHost}\n");
                richTextBoxLog.ScrollToCaret();
                
                using (var mysqlManager = DatabaseConfig.CreateMySqlManager())
                {
                    labelConnectionStatus.Text = "Testing MySQL connection...";
                    richTextBoxLog.AppendText($"[{DateTime.Now:HH:mm:ss}] Step 2: Testing MySQL authentication and database access\n");
                    richTextBoxLog.ScrollToCaret();
                    
                    // Test basic connection first
                    bool success = await mysqlManager.TestConnectionAsync();
                    
                    if (success)
                    {
                        labelConnectionStatus.Text = "Connection successful!";
                        labelConnectionStatus.ForeColor = Color.LightGreen;
                        richTextBoxLog.AppendText($"[{DateTime.Now:HH:mm:ss}] MySQL connection successful!\n");
                        richTextBoxLog.ScrollToCaret();
                        
                        // Ask user if they want to initialize database schema
                        var result = MessageBox.Show("Connection successful!\n\nWould you like to initialize the database schema now?", 
                                                    "Connection Successful", 
                                                    MessageBoxButtons.YesNo, 
                                                    MessageBoxIcon.Question);
                        
                        if (result == DialogResult.Yes)
                        {
                            try
                            {
                                labelConnectionStatus.Text = "Initializing schema...";
                                labelConnectionStatus.ForeColor = Color.Orange;
                                
                                await mysqlManager.InitializeDatabaseAsync();
                                
                                labelConnectionStatus.Text = "Schema initialized!";
                                labelConnectionStatus.ForeColor = Color.LightGreen;
                                richTextBoxLog.AppendText($"[{DateTime.Now:HH:mm:ss}] Database schema initialized successfully!\n");
                                richTextBoxLog.ScrollToCaret();
                                
                                MessageBox.Show("Database schema initialized successfully!", 
                                              "Schema Initialization", 
                                              MessageBoxButtons.OK, 
                                              MessageBoxIcon.Information);
                            }
                            catch (Exception initEx)
                            {
                                labelConnectionStatus.Text = "Schema init failed!";
                                labelConnectionStatus.ForeColor = Color.Red;
                                richTextBoxLog.AppendText($"[{DateTime.Now:HH:mm:ss}] Database schema initialization failed: {initEx.Message}\n");
                                richTextBoxLog.ScrollToCaret();
                                MessageBox.Show($"Schema initialization failed:\n\n{initEx.Message}", 
                                              "Schema Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    // Note: If TestConnectionAsync throws an exception, it will be caught by the outer catch block
                }
            }
            catch (Exception ex)
            {
                labelConnectionStatus.Text = "Connection failed";
                labelConnectionStatus.ForeColor = Color.Red;
                richTextBoxLog.AppendText($"[{DateTime.Now:HH:mm:ss}] MySQL connection failed: {ex.Message}\n");
                richTextBoxLog.ScrollToCaret();
                
                // Provide specific guidance based on error type
                string guidance = "";
                if (ex.Message.Contains("ping") || ex.Message.Contains("unreachable"))
                {
                    guidance = "Network Issues:\n" +
                              "• Check if server IP 100.84.90.72 is correct\n" +
                              "• Verify network connectivity\n" +
                              "• Check firewall settings\n" +
                              "• Ensure VPN connection if required";
                }
                else if (ex.Message.Contains("timeout") || ex.Message.Contains("Timeout"))
                {
                    guidance = "Connection Timeout:\n" +
                              "• Server may be overloaded\n" +
                              "• Network latency issues\n" +
                              "• Try again in a few moments\n" +
                              "• Contact system administrator";
                }
                else if (ex.Message.Contains("1045") || ex.Message.Contains("Access denied"))
                {
                    guidance = "Authentication Error:\n" +
                              "• Check username: orbitz_portal\n" +
                              "• Verify password is correct\n" +
                              "• Contact database administrator";
                }
                else if (ex.Message.Contains("1049") || ex.Message.Contains("Unknown database"))
                {
                    guidance = "Database Error:\n" +
                              "• Database 'Orbitz' may not exist\n" +
                              "• Contact database administrator\n" +
                              "• Check database name spelling";
                }
                else
                {
                    guidance = "General Troubleshooting:\n" +
                              "• Check all connection settings\n" +
                              "• Verify server is running\n" +
                              "• Contact system administrator";
                }
                
                // Show detailed error message with guidance
                MessageBox.Show($"MySQL Connection Failed\n\n" +
                              $"Error Details:\n{ex.Message}\n\n" +
                              $"{guidance}", 
                              "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void buttonMeasurementMode_Click(object sender, EventArgs e)
        {
            // ตรวจสอบการเชื่อมต่อ DMM ก่อน
            if (_dmm == null || !_dmm.IsConnected)
            {
                MessageBox.Show("Please connect to DMM before opening Measurement Mode", 
                              "DMM Not Connected", 
                              MessageBoxButtons.OK, 
                              MessageBoxIcon.Warning);
                return;
            }
            
            var measurementMode = new MeasurementMode(_recordsTable, _dmm);
            measurementMode.Show();
        }
    }
}