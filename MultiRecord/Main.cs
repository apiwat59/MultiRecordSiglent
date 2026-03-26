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

        // Repair specific variables
        private DataTable _repairRecordsTable;
        private int _repairSerialId = -1;
        private string _repairSerialNumber = "";
        private string _repairQwid = "";
        private bool _isRepairTabActive = false;
        private int _previousTabIndex = 0;
        private bool _isChangingTab = false; // ป้องกัน event recursion
        
        // Repair Session variables
        private int _currentRepairSessionId = -1;
        private List<RepairSession> _repairSessions = new List<RepairSession>();
        private List<RepairMeasurement> _currentRepairMeasurements = new List<RepairMeasurement>();

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
            InitializeRepairDataTable();
            LoadLastSuccessfulConnection();
            InitializeToleranceControls();
            InitializeQWRecord();
            InitializeMySQL();
            InitializeRDTab();
            InitializeRepairTab();
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

            // Auto-scroll to latest record
            ScrollToLatestRecord(dataGridViewRecords);

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
                return $"{(value / 1000000).ToString("N3", CultureInfo.InvariantCulture)} M{unit}";
            }
            else if (absValue >= 1000) // Kilo
            {
                return $"{(value / 1000).ToString("N3", CultureInfo.InvariantCulture)} k{unit}";
            }
            else if (absValue >= 1) // Standard
            {
                return $"{value.ToString("N4", CultureInfo.InvariantCulture)} {unit}";
            }
            else if (absValue >= 0.001) // milli
            {
                return $"{(value * 1000).ToString("N3", CultureInfo.InvariantCulture)} m{unit}";
            }
            else if (absValue >= 0.000001) // micro
            {
                return $"{(value * 1000000).ToString("N3", CultureInfo.InvariantCulture)} µ{unit}";
            }
            else // nano or smaller
            {
                return $"{value.ToString("N6", CultureInfo.InvariantCulture)} {unit}";
            }
        }

        // Helper method to format values for R&D table (custom format like 100k, 1M)
        private string FormatRDTableValue(double value, string functionName)
        {
            // สำหรับโหมด RES2W, RES4W, Resistance2W, Resistance4W ให้ใช้ format แบบ 100k, 1M
            if (functionName != null && 
                (functionName.ToUpper().Contains("RES2W") || 
                 functionName.ToUpper().Contains("RES4W") ||
                 functionName.ToUpper().Contains("RESISTANCE2W") || 
                 functionName.ToUpper().Contains("RESISTANCE4W")))
            {
                double absValue = Math.Abs(value);
                
                if (absValue >= 1000000) // Mega (M)
                {
                    double megaValue = value / 1000000;
                    // แสดงทศนิยม 4 ตำแหน่งเสมอ เช่น 3.0211M, 1.0000M
                    return $"{megaValue:0.0000}M";
                }
                else if (absValue >= 1000) // Kilo (k)
                {
                    double kiloValue = value / 1000;
                    // แสดงทศนิยม 4 ตำแหน่งเสมอ เช่น 100.0000k, 4.6355k
                    return $"{kiloValue:0.0000}k";
                }
                else if (absValue >= 1) // Standard (Ω)
                {
                    // ค่าตั้งแต่ 1 ขึ้นไป แสดงไม่มีทศนิยม เช่น 100, 470
                    if (Math.Abs(value - Math.Round(value)) < 0.01)
                    {
                        return $"{Math.Round(value)}";
                    }
                    // ถ้ามีทศนิยม แสดง 1 ตำแหน่ง
                    return $"{value:0.#}";
                }
                else
                {
                    // ค่าน้อยกว่า 1 แสดง 2 ทศนิยม
                    return value.ToString("0.##", CultureInfo.InvariantCulture);
                }
            }
            else
            {
                // สำหรับโหมดอื่นๆ ใช้ format แบบปกติ (มี comma คั่นหลักพัน)
                return value.ToString("N4", CultureInfo.InvariantCulture);
            }
        }

        // Helper method to format tolerance values based on type
        private string FormatToleranceValue(double value, string toleranceType)
        {
            if (value == 0.0)
            {
                return toleranceType == "percent" ? "0%" : "0.00";
            }
            
            if (toleranceType == "percent")
            {
                // Remove trailing zeros but keep at least one decimal place
                string formatted = value.ToString("0.##########", CultureInfo.InvariantCulture);
                return formatted + "%";
            }
            else // abs
            {
                return value.ToString("0.00", CultureInfo.InvariantCulture);
            }
        }

        // Helper method to parse formatted tolerance value back to number
        private decimal? ParseFormattedToleranceValue(string formattedValue)
        {
            if (string.IsNullOrWhiteSpace(formattedValue))
            {
                return null;
            }
            
            // Remove % sign if present
            string cleanValue = formattedValue.Replace("%", "").Trim();
            
            if (decimal.TryParse(cleanValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }
            
            return null;
        }

        // Auto naming system for RD measurements
        private Dictionary<string, int> _functionCounters = new Dictionary<string, int>();
        
        private string GenerateAutoName(string function)
        {
            // Debug: แสดงค่า function ที่ได้รับ
            Console.WriteLine($"[AUTO NAME] GenerateAutoName called with function: '{function}'");
            
            // Map function types to prefixes
            string prefix = GetFunctionPrefix(function);
            Console.WriteLine($"[AUTO NAME] Mapped to prefix: '{prefix}'");
            
            // Get or initialize counter for this function type
            if (!_functionCounters.ContainsKey(prefix))
            {
                _functionCounters[prefix] = 0;
            }
            
            // Increment counter and generate name
            _functionCounters[prefix]++;
            string autoName = $"{prefix}{_functionCounters[prefix]}";
            Console.WriteLine($"[AUTO NAME] Generated name: '{autoName}'");
            
            return autoName;
        }
        
        private string GetFunctionPrefix(string function)
        {
            // Debug: แสดงค่า function ที่ได้รับ
            Console.WriteLine($"[AUTO NAME] GetFunctionPrefix received: '{function}'");
            
            // Map DMM functions to measurement prefixes
            switch (function?.ToUpper())
            {
                // Enum values from MeasurementFunction
                case "VOLTAGEDC":
                case "DCV":
                case "DCVOLT":
                case "DC_VOLT":
                    return "DC";
                    
                case "VOLTAGEAC":
                case "ACV":
                case "ACVOLT":
                case "AC_VOLT":
                    return "AC";
                    
                case "CURRENTDC":
                case "DCA":
                case "DCAMP":
                case "DC_AMP":
                case "DCCURRENT":
                case "DC_CURRENT":
                    return "CDC";
                    
                case "CURRENTAC":
                case "ACA":
                case "ACAMP":
                case "AC_AMP":
                case "ACCURRENT":
                case "AC_CURRENT":
                    return "CAC";
                    
                case "RESISTANCE2W":
                case "2W":
                case "2WIRE":
                case "2W_RES":
                case "2WIRE_RES":
                    return "2W";
                    
                case "RESISTANCE4W":
                case "4W":
                case "4WIRE":
                case "4W_RES":
                case "4WIRE_RES":
                    return "CON";
                    
                case "CAPACITANCE":
                case "CAP":
                    return "CAP";
                    
                case "DIODE":
                case "DIO":
                    return "DIO";
                    
                case "FREQUENCY":
                case "FREQ":
                    return "FREQ";
                    
                case "PERIOD":
                    return "PER";
                    
                case "TEMPERATURE":
                    return "TEMP";
                    
                case "CONTINUOUS":
                    return "CONT";
                    
                default:
                    return "MEAS"; // Default prefix for unknown functions
            }
        }
        
        // Load existing counters from database to ensure continuity
        private async Task LoadFunctionCountersFromDatabase()
        {
            if (_currentSerialId <= 0) return;
            
            try
            {
                string sql = @"
                    SELECT measurement_name, function_name 
                    FROM software_measurements 
                    WHERE serial_id = @serialId 
                    AND measurement_name IS NOT NULL 
                    AND measurement_name != ''
                    ORDER BY id";
                
                var result = await ExecuteSQLQuery(sql.Replace("@serialId", _currentSerialId.ToString()));
                
                if (result.Success && result.Data != null)
                {
                    _functionCounters.Clear();
                    
                    foreach (DataRow row in result.Data.Rows)
                    {
                        string measurementName = row["measurement_name"]?.ToString() ?? "";
                        string functionName = row["function_name"]?.ToString() ?? "";
                        
                        if (!string.IsNullOrEmpty(measurementName) && !string.IsNullOrEmpty(functionName))
                        {
                            string prefix = GetFunctionPrefix(functionName);
                            
                            // Extract number from measurement name (e.g., "DC5" -> 5)
                            if (measurementName.StartsWith(prefix))
                            {
                                string numberPart = measurementName.Substring(prefix.Length);
                                if (int.TryParse(numberPart, out int number))
                                {
                                    // Keep track of the highest number for each prefix
                                    if (!_functionCounters.ContainsKey(prefix) || _functionCounters[prefix] < number)
                                    {
                                        _functionCounters[prefix] = number;
                                    }
                                }
                            }
                        }
                    }
                    
                    LogActivity($"โหลดตัวนับชื่ออัตโนมัติสำเร็จ: {string.Join(", ", _functionCounters.Select(kv => $"{kv.Key}={kv.Value}"))}");
                }
            }
            catch (Exception ex)
            {
                LogActivity($"โหลดตัวนับชื่ออัตโนมัติล้มเหลว: {ex.Message}", true);
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
            _rdRecordsTable.Columns.Add("Select", typeof(bool)); // เพิ่ม checkbox column
            _rdRecordsTable.Columns.Add("No", typeof(int));
            _rdRecordsTable.Columns.Add("Name", typeof(string)); // เพิ่มชื่อจุดวัด
            _rdRecordsTable.Columns.Add("Function", typeof(string));
            _rdRecordsTable.Columns.Add("Measurement", typeof(string)); // สำหรับแสดงผล (formatted)
            _rdRecordsTable.Columns.Add("RawValue", typeof(double)); // เก็บค่าตัวเลขจริง (สำหรับ Export/Edit)
            _rdRecordsTable.Columns.Add("Upper", typeof(string));
            _rdRecordsTable.Columns.Add("Lower", typeof(string));
            _rdRecordsTable.Columns.Add("Type", typeof(string)); // Percent or Absolute
            _rdRecordsTable.Columns.Add("ToleranceEnable", typeof(bool)); // Tolerance Enable/Disable
            _rdRecordsTable.Columns.Add("Note", typeof(string)); // หมายเหตุ
            _rdRecordsTable.Columns.Add("ID", typeof(int)); // เก็บ measurement ID สำหรับอัพเดท

            dataGridViewRD.DataSource = _rdRecordsTable;

            // เพิ่มคอลัมน์ปุ่ม Action
            DataGridViewButtonColumn actionColumn = new DataGridViewButtonColumn();
            actionColumn.Name = "Action";
            actionColumn.HeaderText = "Action";
            actionColumn.Text = "แก้ไข";
            actionColumn.UseColumnTextForButtonValue = true;
            actionColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewRD.Columns.Add(actionColumn);

            // ตั้งค่า AutoSizeMode และ ReadOnly สำหรับแต่ละคอลัมน์
            dataGridViewRD.Columns["Select"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewRD.Columns["Select"].HeaderText = "☐";
            dataGridViewRD.Columns["Select"].ReadOnly = false;
            
            dataGridViewRD.Columns["No"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewRD.Columns["No"].ReadOnly = true;
            
            dataGridViewRD.Columns["Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewRD.Columns["Name"].ReadOnly = true;
            dataGridViewRD.Columns["Name"].HeaderText = "ชื่อจุดวัด";
            
            dataGridViewRD.Columns["Function"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewRD.Columns["Function"].ReadOnly = true;
            
            dataGridViewRD.Columns["Measurement"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewRD.Columns["Measurement"].ReadOnly = true;
            
            dataGridViewRD.Columns["Upper"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewRD.Columns["Upper"].ReadOnly = true;
            
            dataGridViewRD.Columns["Lower"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewRD.Columns["Lower"].ReadOnly = true;
            
            // ซ่อนคอลัมน์ RawValue, Type, Note และ ID (เก็บไว้ใน data แต่ไม่แสดง)
            dataGridViewRD.Columns["RawValue"].Visible = false;
            dataGridViewRD.Columns["Type"].Visible = false;
            dataGridViewRD.Columns["Note"].Visible = false;
            dataGridViewRD.Columns["ID"].Visible = false;

            // ตั้งค่า DataGridView
            dataGridViewRD.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewRD.MultiSelect = true;
            dataGridViewRD.AllowUserToDeleteRows = false;
            dataGridViewRD.AllowUserToAddRows = false;
            dataGridViewRD.ReadOnly = true; // ทำให้เป็น ReadOnly ทั้งหมด ยกเว้น checkbox

            // เพิ่ม Event Handler
            dataGridViewRD.CellClick += DataGridViewRD_CellClick;
            dataGridViewRD.CurrentCellDirtyStateChanged += DataGridViewRD_CurrentCellDirtyStateChanged;
            dataGridViewRD.CellFormatting += DataGridViewRD_CellFormatting;
        }

        private void InitializeRDTab()
        {
            // เพิ่ม Event Handlers สำหรับปุ่มต่างๆ
            buttonRecordRD.Click += ButtonRecordRD_Click;
            buttonDeleteRD.Click += ButtonDeleteRD_Click;
            buttonExportRD.Click += ButtonExportRD_Click;
            buttonSelectAll.Click += ButtonSelectAll_Click;
            buttonDeselectAll.Click += ButtonDeselectAll_Click;
            buttonClearRD.Click += ButtonClearRD_Click;
            buttonChangeModel.Click += ButtonChangeModel_Click;
            buttonBatchTolerance.Click += ButtonBatchTolerance_Click;
            
            // เปิดใช้งาน MultiSelect สำหรับ DataGridView
            dataGridViewRD.MultiSelect = true;
            
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
                                    await LoadFunctionCountersFromDatabase(); // โหลดตัวนับชื่ออัตโนมัติ
                                    
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
                            await LoadFunctionCountersFromDatabase(); // โหลดตัวนับชื่ออัตโนมัติ
                            
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

                string sql = $@"SELECT sm.id, sm.function_name, sm.measurement_name, sm.measurement_value, 
                                      sm.upper_limit, sm.lower_limit, sm.tolerance_type, sm.tolerance_enabled, sm.note, sm.measured_at
                               FROM software_measurements sm 
                               WHERE sm.serial_id = {_currentSerialId} 
                               ORDER BY sm.measured_at ASC";
                
                var result = await ExecuteSQLQuery(sql);
                
                if (result.Success && result.Data != null && result.Data.Rows.Count > 0)
                {
                    _rdRecordsTable.Clear();
                    int no = 1;
                    
                    foreach (DataRow row in result.Data.Rows)
                    {
                        var newRow = _rdRecordsTable.NewRow();
                        newRow["Select"] = false; // checkbox เริ่มต้นไม่ถูกเลือก
                        newRow["No"] = no++;
                        newRow["ID"] = row["id"] != DBNull.Value ? Convert.ToInt32(row["id"]) : 0;
                        newRow["Name"] = row["measurement_name"]?.ToString() ?? "";
                        newRow["Function"] = row["function_name"]?.ToString() ?? "";
                        
                        // จัดการกับค่า NULL และ DBNull
                        // ถ้าค่าเป็น -999999.99999999 แสดงว่าเป็น OVERLOAD
                        if (row["measurement_value"] != DBNull.Value)
                        {
                            double measurementValue = Convert.ToDouble(row["measurement_value"]);
                            // เก็บ raw value ไว้ใน RawValue column
                            newRow["RawValue"] = measurementValue;
                            
                            // ถ้าค่าเป็น -999999.99999999 (OVERLOAD constant) ให้แสดง "OVERLOAD"
                            if (Math.Abs(measurementValue - (-999999.99999999)) < 0.001 || measurementValue <= -999999.0)
                            {
                                newRow["Measurement"] = "OVERLOAD";
                            }
                            else
                            {
                                // ใช้ FormatRDTableValue() เพื่อ format ตามโหมดการวัด
                                string functionName = row["function_name"]?.ToString() ?? "";
                                newRow["Measurement"] = FormatRDTableValue(measurementValue, functionName);
                            }
                        }
                        else
                        {
                            newRow["RawValue"] = 0.0;
                            newRow["Measurement"] = "0.0000";
                        }
                        
                        string toleranceType = row["tolerance_type"]?.ToString() ?? "percent";
                        double upperValue = row["upper_limit"] != DBNull.Value 
                            ? Convert.ToDouble(row["upper_limit"]) 
                            : 0.0;
                        double lowerValue = row["lower_limit"] != DBNull.Value 
                            ? Convert.ToDouble(row["lower_limit"]) 
                            : 0.0;
                        
                        // Format Upper/Lower based on tolerance type
                        newRow["Upper"] = FormatToleranceValue(upperValue, toleranceType);
                        newRow["Lower"] = FormatToleranceValue(lowerValue, toleranceType);
                        newRow["Type"] = toleranceType;
                        newRow["ToleranceEnable"] = row["tolerance_enabled"] != DBNull.Value 
                            ? Convert.ToBoolean(row["tolerance_enabled"]) 
                            : false;
                        newRow["Note"] = row["note"]?.ToString() ?? "";
                        
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
            // ป้องกัน event recursion
            if (_isChangingTab)
                return;

            int currentTabIndex = tabControl1.SelectedIndex;
            _isRDTabActive = (tabControl1.SelectedTab == tab_rd);
            _isRepairTabActive = (tabControl1.SelectedTab == tab_repair);
            
            if (_isRDTabActive)
            {
                LogActivity("เข้าสู่ R&D Tab");
                // ตรวจสอบว่าต้องเลือก Model และ SN หรือไม่
                if (_currentModelId == -1 || _currentSerialId == -1)
                {
                    bool success = await ShowRDSelectionDialog();
                    if (!success)
                    {
                        // ถ้า Cancel ให้กลับไป Tab เดิม
                        LogActivity("ยกเลิก - กลับไป Tab เดิม");
                        await SwitchBackToPreviousTab();
                        return;
                    }
                }
                _previousTabIndex = currentTabIndex;
            }
            else if (_isRepairTabActive)
            {
                LogActivity("เข้าสู่ Repair Tab");
                // แสดง dialog เพื่อเลือก QWID ทุกครั้ง
                bool success = await ShowRepairQwidDialog();
                if (!success)
                {
                    // ถ้า Cancel ให้กลับไป Tab เดิม
                    LogActivity("ยกเลิก - กลับไป Tab เดิม");
                    await SwitchBackToPreviousTab();
                    return;
                }
                _previousTabIndex = currentTabIndex;
            }
            else
            {
                LogActivity("ออกจาก R&D/Repair Tab");
                _previousTabIndex = currentTabIndex;
            }
        }

        private async Task SwitchBackToPreviousTab()
        {
            _isChangingTab = true;
            
            // ตรวจสอบว่า _previousTabIndex ถูกต้อง
            if (_previousTabIndex >= 0 && _previousTabIndex < tabControl1.TabPages.Count)
            {
                // รอให้ UI อัพเดทก่อน
                await Task.Delay(10);
                
                // เปลี่ยน tab
                tabControl1.SelectedIndex = _previousTabIndex;
                
                // รอให้ UI แสดงผลเสร็จ
                await Task.Delay(10);
            }
            else
            {
                // ถ้า index ไม่ถูกต้อง ให้กลับไป tab แรก
                LogActivity("Tab index ไม่ถูกต้อง กลับไป tab แรก");
                tabControl1.SelectedIndex = 0;
                await Task.Delay(10);
            }
            
            _isChangingTab = false;
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
                // ตรวจสอบว่ามี checkbox ที่เลือกหรือไม่
                var selectedRows = GetSelectedRDRows();
                
                if (selectedRows.Count == 0)
                {
                    MessageBox.Show("กรุณาเลือกแถวที่ต้องการลบ\n\nคำแนะนำ:\n- ✓ ติ๊กถูกที่ช่อง checkbox หน้าแถวที่ต้องการลบ\n- ใช้ปุ่ม Select All เพื่อเลือกทั้งหมด\n- ใช้ปุ่ม Deselect All เพื่อยกเลิกการเลือกทั้งหมด", 
                                  "ไม่ได้เลือกแถว", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // สร้างข้อความยืนยันที่มีรายละเอียด
                string confirmMessage = $"ต้องการลบแถวที่เลือก ({selectedRows.Count} แถว) หรือไม่?\n\n";
                
                int displayCount = Math.Min(5, selectedRows.Count);
                confirmMessage += "แถวที่จะลบ:\n";
                for (int i = 0; i < displayCount; i++)
                {
                    var row = selectedRows[i];
                    int no = Convert.ToInt32(row["No"]);
                    string function = row["Function"]?.ToString() ?? "";
                    string measurement = row["Measurement"]?.ToString() ?? "";
                    confirmMessage += $"  #{no}: {function} = {measurement}\n";
                }
                
                if (selectedRows.Count > displayCount)
                {
                    confirmMessage += $"  ... และอีก {selectedRows.Count - displayCount} แถว\n";
                }

                // ยืนยันการลบ
                var result = MessageBox.Show(
                    confirmMessage,
                    "ยืนยันการลบ",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result != DialogResult.Yes)
                    return;

                // แสดง progress
                buttonDeleteRD.Enabled = false;
                buttonDeleteRD.Text = "กำลังลบ...";
                
                int deletedCount = 0;
                int failedCount = 0;
                
                // สร้าง list ของแถวที่ต้องลบ (ลบจากท้ายไปหน้าเพื่อไม่ให้ index เปลี่ยน)
                var rowsToDelete = selectedRows.OrderByDescending(r => Convert.ToInt32(r["No"])).ToList();
                
                foreach (var row in rowsToDelete)
                {
                    try
                    {
                        // ดึงข้อมูลจากแถว
                        int measurementNo = Convert.ToInt32(row["No"]);
                        
                        // ลบจาก database
                        await DeleteRDMeasurementFromDatabase(measurementNo);
                        
                        // ลบจาก DataTable
                        _rdRecordsTable.Rows.Remove(row);
                        
                        deletedCount++;
                    }
                    catch (Exception ex)
                    {
                        failedCount++;
                        LogActivity($"ลบแถว #{row["No"]} ล้มเหลว: {ex.Message}", true);
                    }
                }

                // อัปเดตหมายเลข No. ใหม่
                RenumberRDRecords();
                
                // แสดงผลลัพธ์
                string resultMessage = $"ลบข้อมูล R&D เสร็จสิ้น\n\nสำเร็จ: {deletedCount} แถว";
                if (failedCount > 0)
                {
                    resultMessage += $"\nล้มเหลว: {failedCount} แถว";
                }
                
                LogActivity($"ลบข้อมูล R&D: สำเร็จ {deletedCount} แถว, ล้มเหลว {failedCount} แถว");
                MessageBox.Show(resultMessage, "ผลการลบ", MessageBoxButtons.OK, 
                              failedCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogActivity($"ลบข้อมูล R&D ล้มเหลว: {ex.Message}", true);
                MessageBox.Show($"เกิดข้อผิดพลาดในการลบข้อมูล: {ex.Message}", 
                              "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                buttonDeleteRD.Enabled = true;
                buttonDeleteRD.Text = "ลบ";
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

        private void ButtonSelectAll_Click(object sender, EventArgs e)
        {
            foreach (DataRow row in _rdRecordsTable.Rows)
            {
                row["Select"] = true;
            }
        }

        private void ButtonDeselectAll_Click(object sender, EventArgs e)
        {
            foreach (DataRow row in _rdRecordsTable.Rows)
            {
                row["Select"] = false;
            }
        }

        private List<DataRow> GetSelectedRDRows()
        {
            return _rdRecordsTable.Rows.Cast<DataRow>()
                .Where(row => row["Select"] != DBNull.Value && Convert.ToBoolean(row["Select"]))
                .ToList();
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
                
                // สำหรับ OVERLOAD ใช้ค่าพิเศษ -999999.99999999 (ภายในขีดจำกัด DECIMAL(15,8))
                double valueToSave = _isOverload ? -999999.99999999 : _lastReadingValue;
                
                // สร้าง formatted value สำหรับแสดงผล (ใช้ FormatRDTableValue ถ้าไม่ OVERLOAD)
                string measurementFormatted;
                if (_isOverload)
                {
                    measurementFormatted = "OVERLOAD";
                }
                else
                {
                    measurementFormatted = FormatRDTableValue(_lastReadingValue, function);
                }
                
                // ดึงข้อมูล system_info จาก textBoxSystemInfo
                string systemInfo = textBoxSystemInfo.Text ?? "";
                
                // สร้างชื่ออัตโนมัติตาม function type
                string autoName = GenerateAutoName(function);
                
                // เพิ่มข้อมูลใน DataTable (อัพเดทให้ตรงกับ columns ใหม่)
                var newRow = _rdRecordsTable.NewRow();
                newRow["Select"] = false;
                newRow["No"] = newNo;
                newRow["Name"] = autoName; // ใช้ชื่ออัตโนมัติ
                newRow["Function"] = function;
                newRow["Measurement"] = measurementFormatted; // แสดงผลแบบ formatted
                newRow["RawValue"] = valueToSave; // เก็บค่าตัวเลขจริง
                newRow["Upper"] = "0%"; // ค่าเริ่มต้นแบบ formatted
                newRow["Lower"] = "0%"; // ค่าเริ่มต้นแบบ formatted
                newRow["Type"] = "percent"; // ค่าเริ่มต้น
                newRow["ToleranceEnable"] = false; // ค่าเริ่มต้น
                newRow["Note"] = "";
                newRow["ID"] = 0; // จะอัพเดทหลังจากบันทึกลง database
                _rdRecordsTable.Rows.Add(newRow);
                
                // บันทึกลง database พร้อม system_info และชื่ออัตโนมัติ (ส่งค่าพิเศษสำหรับ OVERLOAD)
                int newId = await SaveRDMeasurementToDatabase(newNo, function, valueToSave, systemInfo, autoName);
                
                // อัพเดท ID ใน DataTable
                if (newId > 0)
                {
                    newRow["ID"] = newId;
                }
                
                // Auto-scroll to latest record
                _mysqlManager.ScrollRDToLatest(this);
                
                LogActivity($"บันทึกค่า R&D No. {newNo}: {function}, {measurementFormatted}");
                SoundUtil.Beep();
            }
            catch (Exception ex)
            {
                LogActivity($"บันทึก R&D ล้มเหลว: {ex.Message}", true);
            }
        }

        private async Task<int> SaveRDMeasurementToDatabase(int measurementNo, string function, double value, string systemInfo = "", string measurementName = "")
        {
            try
            {
                // Escape strings สำหรับ system_info และ measurement_name
                string escapedSystemInfo = MySqlHelper.EscapeString(systemInfo);
                string escapedMeasurementName = MySqlHelper.EscapeString(measurementName);
                
                string insertSQL = $@"
                    INSERT INTO software_measurements 
                    (serial_id, measurement_no, function_name, measurement_value, tolerance_enabled, system_info, measurement_name, measured_at)
                    VALUES ({_currentSerialId}, {measurementNo}, '{function}', {value}, false, '{escapedSystemInfo}', '{escapedMeasurementName}', NOW())";
                
                Console.WriteLine($"[RD SAVE] SQL Query: {insertSQL}");
                Console.WriteLine($"[RD SAVE] Parameters - SerialId: {_currentSerialId}, MeasurementNo: {measurementNo}, Function: {function}, Value: {value}");
                
                var result = await ExecuteSQLQuery(insertSQL);
                
                Console.WriteLine($"[RD SAVE] Query Result - Success: {result.Success}, Message: {result.Message}");
                
                if (!result.Success)
                {
                    LogActivity($"บันทึก measurement ล้มเหลว: {result.Message}", true);
                    return 0;
                }
                else
                {
                    LogActivity($"บันทึก measurement สำเร็จ - No: {measurementNo}, Function: {function}, Value: {value}");
                    
                    // ดึง ID ของ record ที่เพิ่งบันทึก
                    string getIdSQL = "SELECT LAST_INSERT_ID() as id";
                    var idResult = await ExecuteSQLQuery(getIdSQL);
                    if (idResult.Success && idResult.Data != null && idResult.Data.Rows.Count > 0)
                    {
                        return Convert.ToInt32(idResult.Data.Rows[0]["id"]);
                    }
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RD SAVE] Exception: {ex.Message}");
                Console.WriteLine($"[RD SAVE] Stack Trace: {ex.StackTrace}");
                LogActivity($"บันทึก measurement ลง database ล้มเหลว: {ex.Message}", true);
                return 0;
            }
        }

        private void DataGridViewRD_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                string columnName = dataGridViewRD.Columns[e.ColumnIndex].Name;
                
                // Handle checkbox click
                if (columnName == "Select")
                {
                    // Toggle checkbox value
                    var currentValue = dataGridViewRD.Rows[e.RowIndex].Cells["Select"].Value;
                    dataGridViewRD.Rows[e.RowIndex].Cells["Select"].Value = !(bool)(currentValue ?? false);
                    dataGridViewRD.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
                // Handle Action button click
                else if (columnName == "Action")
                {
                    OpenEditDialog(e.RowIndex);
                }
            }
        }

        private void DataGridViewRD_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Format Measurement column with k/M suffixes for Resistance
            if (dataGridViewRD.Columns[e.ColumnIndex].Name == "Measurement")
            {
                if (e.RowIndex >= 0 && e.RowIndex < dataGridViewRD.Rows.Count)
                {
                    var row = dataGridViewRD.Rows[e.RowIndex];
                    string measurement = e.Value?.ToString() ?? "";
                    string functionName = row.Cells["Function"].Value?.ToString() ?? "";

                    // ถ้าเป็น OVERLOAD ไม่ต้อง format
                    if (measurement == "OVERLOAD")
                    {
                        e.Value = "OVERLOAD";
                        e.FormattingApplied = true;
                        return;
                    }

                    // Parse และ format ใหม่สำหรับ Resistance
                    if (double.TryParse(measurement, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
                    {
                        string formattedValue = FormatRDTableValue(value, functionName);
                        e.Value = formattedValue;
                        e.FormattingApplied = true;
                    }
                }
            }
        }

        /// <summary>
        /// Auto-scroll DataGridView to the latest (last) record
        /// </summary>
        private void ScrollToLatestRecord(DataGridView dataGridView)
        {
            try
            {
                if (dataGridView == null || dataGridView.Rows.Count == 0)
                {
                    return;
                }

                // Check if we need to invoke on UI thread
                if (dataGridView.InvokeRequired)
                {
                    dataGridView.Invoke(new Action(() => ScrollToLatestRecord(dataGridView)));
                    return;
                }

                int lastRowIndex = dataGridView.Rows.Count - 1;
                
                // Clear current selection
                dataGridView.ClearSelection();
                
                // Select the last row
                dataGridView.Rows[lastRowIndex].Selected = true;
                
                // Scroll to make the last row visible
                dataGridView.FirstDisplayedScrollingRowIndex = Math.Max(0, lastRowIndex);
                
                // Ensure the row is fully visible
                dataGridView.CurrentCell = dataGridView.Rows[lastRowIndex].Cells[0];
            }
            catch (Exception ex)
            {
                LogActivity($"Error scrolling to latest record: {ex.Message}");
            }
        }

        private async void OpenEditDialog(int rowIndex)
        {
            try
            {
                var row = dataGridViewRD.Rows[rowIndex];
                int measurementId = Convert.ToInt32(row.Cells["ID"].Value);
                string currentName = row.Cells["Name"].Value?.ToString() ?? "";
                string currentFunction = row.Cells["Function"].Value?.ToString() ?? "";
                
                // ใช้ RawValue แทน Measurement เพื่อให้ได้ค่าตัวเลขจริง
                string currentMeasurementValue;
                if (row.Cells["RawValue"].Value != null && row.Cells["RawValue"].Value != DBNull.Value)
                {
                    double rawValue = Convert.ToDouble(row.Cells["RawValue"].Value);
                    // ถ้าเป็น OVERLOAD ให้แสดง "OVERLOAD"
                    if (Math.Abs(rawValue - (-999999.99999999)) < 0.001 || rawValue <= -999999.0)
                    {
                        currentMeasurementValue = "OVERLOAD";
                    }
                    else
                    {
                        currentMeasurementValue = rawValue.ToString("F8", System.Globalization.CultureInfo.InvariantCulture);
                    }
                }
                else
                {
                    currentMeasurementValue = "0.00000000";
                }
                
                bool currentToleranceEnable = row.Cells["ToleranceEnable"].Value != null && 
                                            row.Cells["ToleranceEnable"].Value != DBNull.Value && 
                                            Convert.ToBoolean(row.Cells["ToleranceEnable"].Value);
                string currentType = row.Cells["Type"].Value?.ToString() ?? "percent";
                string currentNote = row.Cells["Note"].Value?.ToString() ?? "";
                
                // ดึงค่า Upper/Lower ปัจจุบัน (parse from formatted string)
                decimal? currentUpper = null;
                decimal? currentLower = null;
                
                if (row.Cells["Upper"].Value != null && row.Cells["Upper"].Value != DBNull.Value)
                {
                    currentUpper = ParseFormattedToleranceValue(row.Cells["Upper"].Value.ToString());
                }
                
                if (row.Cells["Lower"].Value != null && row.Cells["Lower"].Value != DBNull.Value)
                {
                    currentLower = ParseFormattedToleranceValue(row.Cells["Lower"].Value.ToString());
                }

                using (RDEditDialog dialog = new RDEditDialog(currentName, currentFunction, currentMeasurementValue, currentToleranceEnable, currentType, currentUpper, currentLower, currentNote))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        // อัพเดทข้อมูลใน DataGridView with formatted values
                        row.Cells["Name"].Value = dialog.MeasurementName;
                        row.Cells["Function"].Value = dialog.Function;
                        
                        // แปลงค่า measurement - handle OVERLOAD case
                        double rawValueToSave;
                        string displayMeasurementValue;
                        
                        if (dialog.MeasurementValue.ToUpper() == "OVERLOAD")
                        {
                            rawValueToSave = -999999.99999999;
                            displayMeasurementValue = "OVERLOAD";
                        }
                        else if (double.TryParse(dialog.MeasurementValue, out double parsedValue))
                        {
                            rawValueToSave = parsedValue;
                            // Format ตามโหมดการวัด
                            displayMeasurementValue = FormatRDTableValue(parsedValue, dialog.Function);
                        }
                        else
                        {
                            rawValueToSave = 0.0;
                            displayMeasurementValue = "0.0000";
                        }
                        
                        row.Cells["RawValue"].Value = rawValueToSave;
                        row.Cells["Measurement"].Value = displayMeasurementValue;
                        
                        row.Cells["ToleranceEnable"].Value = dialog.ToleranceEnable;
                        row.Cells["Type"].Value = dialog.ToleranceType;
                        
                        // Format Upper/Lower based on tolerance type
                        if (dialog.UpperLimit.HasValue)
                        {
                            row.Cells["Upper"].Value = FormatToleranceValue((double)dialog.UpperLimit.Value, dialog.ToleranceType);
                        }
                        else
                        {
                            row.Cells["Upper"].Value = dialog.ToleranceType == "percent" ? "0%" : "0.00";
                        }
                        
                        if (dialog.LowerLimit.HasValue)
                        {
                            row.Cells["Lower"].Value = FormatToleranceValue((double)dialog.LowerLimit.Value, dialog.ToleranceType);
                        }
                        else
                        {
                            row.Cells["Lower"].Value = dialog.ToleranceType == "percent" ? "0%" : "0.00";
                        }
                        
                        row.Cells["Note"].Value = dialog.Note;

                        // อัพเดทข้อมูลใน database
                        await UpdateMeasurementDetails(measurementId, dialog.MeasurementName, dialog.Function, 
                            dialog.MeasurementValue, dialog.ToleranceEnable, dialog.ToleranceType, 
                            dialog.UpperLimit, dialog.LowerLimit, dialog.Note);
                        
                        LogActivity($"อัพเดทข้อมูลจุดวัด ID: {measurementId} สำเร็จ");
                    }
                }
            }
            catch (Exception ex)
            {
                LogActivity($"เกิดข้อผิดพลาดในการแก้ไขข้อมูล: {ex.Message}", true);
                MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task UpdateMeasurementDetails(int measurementId, string name, string function, string measurementValue, bool toleranceEnable, string type, decimal? upperLimit, decimal? lowerLimit, string note)
        {
            try
            {
                string upperValue = upperLimit.HasValue ? upperLimit.Value.ToString() : "NULL";
                string lowerValue = lowerLimit.HasValue ? lowerLimit.Value.ToString() : "NULL";
                
                // Parse measurement value - handle OVERLOAD case
                string measurementValueSql = "NULL";
                if (!string.IsNullOrWhiteSpace(measurementValue))
                {
                    if (measurementValue.ToUpper() == "OVERLOAD")
                    {
                        measurementValueSql = "-999999.99999999"; // Use our OVERLOAD constant
                    }
                    else if (decimal.TryParse(measurementValue, out decimal parsedValue))
                    {
                        measurementValueSql = parsedValue.ToString();
                    }
                }
                
                string sql = $@"UPDATE software_measurements 
                               SET measurement_name = '{MySqlHelper.EscapeString(name)}',
                                   function_name = '{MySqlHelper.EscapeString(function)}',
                                   measurement_value = {measurementValueSql},
                                   tolerance_enabled = {(toleranceEnable ? 1 : 0)},
                                   tolerance_type = '{type}',
                                   upper_limit = {upperValue},
                                   lower_limit = {lowerValue},
                                   note = '{MySqlHelper.EscapeString(note)}'
                               WHERE id = {measurementId}";

                var result = await ExecuteSQLQuery(sql);
                
                if (!result.Success)
                {
                    throw new Exception(result.Message);
                }
            }
            catch (Exception ex)
            {
                LogActivity($"อัพเดทข้อมูลจุดวัดล้มเหลว: {ex.Message}", true);
                throw;
            }
        }

        private void DataGridViewRD_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            // สำหรับ checkbox columns ให้ commit การเปลี่ยนแปลงทันที
            if (dataGridViewRD.IsCurrentCellDirty)
            {
                if (dataGridViewRD.CurrentCell.OwningColumn.Name == "Select")
                {
                    dataGridViewRD.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            }
        }

        private async Task UpdateToleranceInDatabase(int measurementNo, DataGridViewRow row)
        {
            try
            {
                double upperLimit = 0;
                double lowerLimit = 0;
                bool toleranceEnabled = Convert.ToBoolean(row.Cells["ToleranceEnable"].Value ?? false);
                
                // แปลงค่า Upper และ Lower
                if (row.Cells["Upper"].Value != null && !string.IsNullOrEmpty(row.Cells["Upper"].Value.ToString()))
                {
                    double.TryParse(row.Cells["Upper"].Value.ToString(), out upperLimit);
                }
                
                if (row.Cells["Lower"].Value != null && !string.IsNullOrEmpty(row.Cells["Lower"].Value.ToString()))
                {
                    double.TryParse(row.Cells["Lower"].Value.ToString(), out lowerLimit);
                }
                
                // เรียกใช้ฟังก์ชันใหม่
                await UpdateToleranceInDatabase(measurementNo, upperLimit, lowerLimit, toleranceEnabled);
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
                        lines.Add("No,Name,Function,Measurement,Upper,Lower,Type,ToleranceEnable,Note");
                        
                        foreach (DataRow row in _rdRecordsTable.Rows)
                        {
                            // ใช้ RawValue แทน Measurement เพื่อให้ได้ค่าตัวเลขจริง
                            string measurementValue;
                            if (row["RawValue"] != DBNull.Value)
                            {
                                double rawValue = Convert.ToDouble(row["RawValue"]);
                                // ถ้าเป็น OVERLOAD (-999999.99999999) ให้แสดงเป็น OVERLOAD
                                if (Math.Abs(rawValue - (-999999.99999999)) < 0.001 || rawValue <= -999999.0)
                                {
                                    measurementValue = "OVERLOAD";
                                }
                                else
                                {
                                    measurementValue = rawValue.ToString("F8", CultureInfo.InvariantCulture);
                                }
                            }
                            else
                            {
                                measurementValue = "0.00000000";
                            }
                            
                            string csvLine = string.Join(",",
                                $"\"{row["No"]}\"",
                                $"\"{row["Name"]}\"",
                                $"\"{row["Function"]}\"",
                                $"\"{measurementValue}\"",
                                $"\"{row["Upper"]}\"",
                                $"\"{row["Lower"]}\"",
                                $"\"{row["Type"]}\"",
                                $"\"{row["ToleranceEnable"]}\"",
                                $"\"{row["Note"]}\""
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

        private async void ButtonBatchTolerance_Click(object sender, EventArgs e)
        {
            try
            {
                if (_rdRecordsTable.Rows.Count == 0)
                {
                    MessageBox.Show("ไม่มีข้อมูลในตาราง", "ไม่มีข้อมูล", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // ตรวจสอบจำนวนแถวที่เลือกผ่าน checkbox
                var selectedRows = GetSelectedRDRows();
                int selectedCount = selectedRows.Count;
                int totalCount = _rdRecordsTable.Rows.Count;

                if (selectedCount == 0)
                {
                    MessageBox.Show("กรุณาเลือกแถวที่ต้องการตั้งค่า tolerance\n\nคำแนะนำ:\n- ✓ ติ๊กถูกที่ช่อง checkbox หน้าแถวที่ต้องการ\n- ใช้ปุ่ม Select All เพื่อเลือกทั้งหมด", 
                                  "ไม่ได้เลือกแถว", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (var dialog = new BatchToleranceDialog(selectedCount, totalCount))
                {
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        await ApplyBatchTolerance(
                            dialog.UpperLimit, 
                            dialog.LowerLimit, 
                            dialog.ToleranceEnabled, 
                            dialog.ApplyToSelected,
                            dialog.ToleranceType
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                LogActivity($"Batch Tolerance ล้มเหลว: {ex.Message}", true);
                MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}", "ข้อผิดพลาด", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task ApplyBatchTolerance(double upperLimit, double lowerLimit, bool toleranceEnabled, bool applyToSelected, string toleranceType)
        {
            try
            {
                List<DataRow> rowsToUpdate = new List<DataRow>();

                if (applyToSelected)
                {
                    // ใช้กับแถวที่เลือก (ผ่าน checkbox)
                    rowsToUpdate = GetSelectedRDRows();
                }
                else
                {
                    // ใช้กับทุกแถว
                    rowsToUpdate = _rdRecordsTable.Rows.Cast<DataRow>().ToList();
                }

                int updateCount = 0;
                foreach (var dataRow in rowsToUpdate)
                {
                    int measurementNo = Convert.ToInt32(dataRow["No"]);
                    
                    // อัปเดตค่าใน DataTable
                    dataRow["Upper"] = upperLimit;
                    dataRow["Lower"] = lowerLimit;
                    dataRow["ToleranceEnable"] = toleranceEnabled;

                    // อัปเดตใน database (รวมทั้ง tolerance_type)
                    await UpdateToleranceInDatabase(measurementNo, upperLimit, lowerLimit, toleranceEnabled, toleranceType);
                    updateCount++;
                }

                LogActivity($"อัปเดต Tolerance แบบ Batch สำเร็จ ({updateCount} แถว, Type: {toleranceType})");
                MessageBox.Show($"อัปเดต Tolerance สำเร็จ\nจำนวน: {updateCount} แถว\nType: {toleranceType}", 
                              "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogActivity($"อัปเดต Batch Tolerance ล้มเหลว: {ex.Message}", true);
                throw;
            }
        }

        private async Task UpdateToleranceInDatabase(int measurementNo, double upperLimit, double lowerLimit, bool toleranceEnabled)
        {
            // Overload: เรียกใช้ฟังก์ชันหลักโดยไม่อัพเดท tolerance_type
            await UpdateToleranceInDatabase(measurementNo, upperLimit, lowerLimit, toleranceEnabled, null);
        }

        private async Task UpdateToleranceInDatabase(int measurementNo, double upperLimit, double lowerLimit, bool toleranceEnabled, string toleranceType)
        {
            try
            {
                string updateSQL;
                
                if (!string.IsNullOrEmpty(toleranceType))
                {
                    // อัพเดตทั้ง limits, enable และ type
                    updateSQL = $@"
                        UPDATE software_measurements 
                        SET upper_limit = {upperLimit},
                            lower_limit = {lowerLimit},
                            tolerance_enabled = {toleranceEnabled},
                            tolerance_type = '{toleranceType}'
                        WHERE serial_id = {_currentSerialId} AND measurement_no = {measurementNo}";
                }
                else
                {
                    // อัพเดตเฉพาะ limits และ enable (ไม่แก้ type)
                    updateSQL = $@"
                        UPDATE software_measurements 
                        SET upper_limit = {upperLimit},
                            lower_limit = {lowerLimit},
                            tolerance_enabled = {toleranceEnabled}
                        WHERE serial_id = {_currentSerialId} AND measurement_no = {measurementNo}";
                }
                
                var updateResult = await ExecuteSQLQuery(updateSQL);
                
                if (!updateResult.Success)
                {
                    LogActivity($"อัพเดต tolerance ใน database ล้มเหลว: {updateResult.Message}", true);
                }
            }
            catch (Exception ex)
            {
                LogActivity($"อัพเดต tolerance ใน database ล้มเหลว: {ex.Message}", true);
                throw;
            }
        }

        #endregion

        #region --- Repair Tab Methods ---

        private void InitializeRepairDataTable()
        {
            _repairRecordsTable = new DataTable("RepairMeasurementRecords");
            _repairRecordsTable.Columns.Add("Select", typeof(bool)); // เพิ่ม checkbox column
            _repairRecordsTable.Columns.Add("No", typeof(int));
            _repairRecordsTable.Columns.Add("Name", typeof(string)); // ชื่อจุดวัด
            _repairRecordsTable.Columns.Add("Function", typeof(string));
            _repairRecordsTable.Columns.Add("RefValue", typeof(string)); // ค่าอ้างอิง (จาก template) - เปลี่ยนเป็น string เพื่อรองรับ "-" และ "<->"
            _repairRecordsTable.Columns.Add("RawValue", typeof(decimal)); // ค่าดิบที่วัดได้จริง (สำหรับ export และคำนวณ) - รองรับ -1000000.0 สำหรับ OVERLOAD
            _repairRecordsTable.Columns.Add("Value", typeof(string)); // ค่าที่วัดได้ (ต้องวัดใหม่) - เปลี่ยนเป็น string เพื่อรองรับ "-" และ "<->"
            _repairRecordsTable.Columns.Add("Status", typeof(string)); // PASS/FAIL status
            _repairRecordsTable.Columns.Add("TolEnabled", typeof(bool)); // Tolerance Enable/Disable
            _repairRecordsTable.Columns.Add("TolerancePercentage", typeof(decimal)); // เก็บ tolerance % ไว้คำนวณ
            _repairRecordsTable.Columns.Add("SystemInfo", typeof(string)); // System Info
            _repairRecordsTable.Columns.Add("MeasurementId", typeof(int)); // เก็บ measurement ID

            dataGridViewRepair.DataSource = _repairRecordsTable;

            // เพิ่มคอลัมน์ปุ่ม Action
            DataGridViewButtonColumn actionColumn = new DataGridViewButtonColumn();
            actionColumn.Name = "Action";
            actionColumn.HeaderText = "Action";
            actionColumn.Text = "แก้ไข";
            actionColumn.UseColumnTextForButtonValue = true;
            actionColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewRepair.Columns.Add(actionColumn);

            // ตั้งค่า AutoSizeMode และ ReadOnly สำหรับแต่ละคอลัมน์
            dataGridViewRepair.Columns["Select"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewRepair.Columns["Select"].HeaderText = "☐";
            dataGridViewRepair.Columns["Select"].ReadOnly = false;
            
            dataGridViewRepair.Columns["No"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewRepair.Columns["No"].ReadOnly = true;
            
            dataGridViewRepair.Columns["Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewRepair.Columns["Name"].HeaderText = "ชื่อจุดวัด";
            dataGridViewRepair.Columns["Name"].ReadOnly = true;
            
            dataGridViewRepair.Columns["Function"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewRepair.Columns["Function"].ReadOnly = true;
            
            dataGridViewRepair.Columns["RefValue"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewRepair.Columns["RefValue"].HeaderText = "ค่าอ้างอิง";
            dataGridViewRepair.Columns["RefValue"].ReadOnly = true;
            
            dataGridViewRepair.Columns["Value"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewRepair.Columns["Value"].HeaderText = "ค่าวัดได้";
            dataGridViewRepair.Columns["Value"].ReadOnly = true;
            
            
            dataGridViewRepair.Columns["Status"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewRepair.Columns["Status"].HeaderText = "สถานะ";
            dataGridViewRepair.Columns["Status"].ReadOnly = true;
            dataGridViewRepair.Columns["Status"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            
            
            // ซ่อนคอลัมน์ที่ไม่ต้องแสดง
            dataGridViewRepair.Columns["Select"].Visible = false; // ใช้ row selection แทน checkbox
            dataGridViewRepair.Columns["RawValue"].Visible = false; // ซ่อนค่าดิบ (ใช้สำหรับ export และคำนวณเท่านั้น)
            dataGridViewRepair.Columns["TolEnabled"].Visible = false;
            dataGridViewRepair.Columns["TolerancePercentage"].Visible = false;
            dataGridViewRepair.Columns["SystemInfo"].Visible = false;
            dataGridViewRepair.Columns["MeasurementId"].Visible = false;

            // ตั้งค่า DataGridView
            dataGridViewRepair.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewRepair.MultiSelect = true; // เลือกได้หลายแถว (สำหรับ Delete)
            dataGridViewRepair.AllowUserToDeleteRows = false;
            dataGridViewRepair.AllowUserToAddRows = false;
            dataGridViewRepair.ReadOnly = false; // เปลี่ยนเป็น false เพื่อให้ checkbox แก้ไขได้

            // เพิ่ม Event Handler
            dataGridViewRepair.CellClick += DataGridViewRepair_CellClick;
            dataGridViewRepair.CurrentCellDirtyStateChanged += DataGridViewRepair_CurrentCellDirtyStateChanged;
            dataGridViewRepair.CellFormatting += DataGridViewRepair_CellFormatting;
        }

        private void InitializeRepairTab()
        {
            // เพิ่ม Event Handlers สำหรับปุ่มต่างๆ
            buttonRecordRepair.Click += ButtonRecordRepair_Click;
            buttonDeleteRepair.Click += ButtonDeleteRepair_Click;
            buttonExportRepair.Click += ButtonExportRepair_Click;
            buttonSelectAllRepair.Click += ButtonSelectAllRepair_Click;
            buttonDeselectAllRepair.Click += ButtonDeselectAllRepair_Click;
            buttonClearRepair.Click += ButtonClearRepair_Click;
            buttonChangeModelRepair.Click += ButtonChangeModelRepair_Click;
            
            // เพิ่ม Event Handlers สำหรับ Repair Sessions
            buttonNewRepairSession.Click += ButtonNewRepairSession_Click;
            buttonRepairHistory.Click += ButtonRepairHistory_Click;
            comboBoxRepairSession.SelectedIndexChanged += ComboBoxRepairSession_SelectedIndexChanged;
            
            // ซ่อน session controls เริ่มต้น
            labelRepairSession.Visible = false;
            comboBoxRepairSession.Visible = false;
            buttonNewRepairSession.Visible = false;
            buttonRepairHistory.Visible = false;
            
            LogActivity("เริ่มต้น Repair Tab สำเร็จ");
        }

        private async Task<bool> ShowRepairQwidDialog()
        {
            try
            {
                using (var dialog = new RepairQwidDialog(_mysqlManager))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        _repairSerialId = dialog.SelectedSerialId;
                        _repairSerialNumber = dialog.SelectedSerialNumber;
                        _repairQwid = dialog.SelectedQwid;
                        
                        LogActivity($"เลือก QWID: {_repairQwid}, Serial: {_repairSerialNumber}");
                        
                        // โหลดข้อมูล measurements
                        await LoadRepairRecordsFromDatabase();
                        
                        return true;
                    }
                    else
                    {
                        LogActivity("ยกเลิกการเลือก QWID");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                LogActivity($"เกิดข้อผิดพลาดในการแสดง Repair QWID Dialog: {ex.Message}", true);
                MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private async Task LoadRepairRecordsFromDatabase()
        {
            try
            {
                if (_repairSerialId <= 0)
                {
                    LogActivity("ไม่มี Serial ID สำหรับโหลดข้อมูล Repair", true);
                    return;
                }

                // โหลด repair sessions แทนที่จะโหลด measurements โดยตรง
                await LoadRepairSessions();

                // ถ้าไม่มี session ให้แจ้งเตือน
                if (_repairSessions == null || _repairSessions.Count == 0)
                {
                    var confirmResult = MessageBox.Show(
                        $"QWID: {_repairQwid}\nชุดทดสอบ: {_repairSerialNumber}\n\n" +
                        "นี่เป็นครั้งแรกที่ทำการวัดด้วยชุดทดสอบนี้\n\n" +
                        "ต้องการสร้างรอบการซ่อมครั้งแรกหรือไม่?",
                        "ครั้งแรกสำหรับชุดทดสอบนี้",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (confirmResult == DialogResult.Yes)
                    {
                        // สร้าง session แรกโดยอัตโนมัติ
                        await CreateFirstRepairSession();
                    }
                }
                else
                {
                    // แจ้งเตือนว่ามีการวัดอยู่แล้ว
                    int sessionCount = _repairSessions.Count;
                    MessageBox.Show(
                        $"QWID: {_repairQwid}\nชุดทดสอบ: {_repairSerialNumber}\n\n" +
                        $"พบรอบการวัดที่มีอยู่แล้ว: {sessionCount} รอบ\n\n" +
                        "สามารถสร้างรอบการซ่อมใหม่ได้จากปุ่ม 'สร้างรอบซ่อมใหม่'",
                        $"การวัดครั้งที่ {sessionCount}",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                LogActivity($"เกิดข้อผิดพลาดในการโหลดข้อมูล Repair: {ex.Message}", true);
            }
        }

        private void DataGridViewRepair_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                string columnName = dataGridViewRepair.Columns[e.ColumnIndex].Name;
                
                // Handle Action button click
                if (columnName == "Action")
                {
                    ShowRepairEditDialog(e.RowIndex);
                }
            }
        }

        private void ShowRepairEditDialog(int rowIndex)
        {
            try
            {
                if (rowIndex < 0 || rowIndex >= _repairRecordsTable.Rows.Count)
                    return;

                var dataRow = _repairRecordsTable.Rows[rowIndex];
                
                // ดึงข้อมูลปัจจุบัน
                string name = dataRow["Name"]?.ToString() ?? "";
                string function = dataRow["Function"]?.ToString() ?? "";
                string refValue = dataRow["RefValue"]?.ToString() ?? "";
                string currentValue = dataRow["Value"]?.ToString() ?? "-";
                decimal currentRawValue = dataRow["RawValue"] != DBNull.Value ? Convert.ToDecimal(dataRow["RawValue"]) : 0;
                
                // สร้าง Edit Dialog
                using (var editForm = new Form())
                {
                    editForm.Text = $"แก้ไขค่าวัด - {name}";
                    editForm.Size = new Size(450, 250);
                    editForm.StartPosition = FormStartPosition.CenterParent;
                    editForm.BackColor = Color.FromArgb(45, 45, 48);
                    editForm.ForeColor = Color.White;
                    editForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                    editForm.MaximizeBox = false;
                    editForm.MinimizeBox = false;

                    // Labels
                    var lblName = new Label 
                    { 
                        Text = $"จุดวัด: {name}", 
                        Location = new Point(20, 20), 
                        Size = new Size(400, 25),
                        ForeColor = Color.LightGray
                    };
                    
                    var lblFunction = new Label 
                    { 
                        Text = $"Function: {function}", 
                        Location = new Point(20, 45), 
                        Size = new Size(400, 25),
                        ForeColor = Color.LightGray
                    };
                    
                    var lblRefValue = new Label 
                    { 
                        Text = $"ค่าอ้างอิง: {refValue}", 
                        Location = new Point(20, 70), 
                        Size = new Size(400, 25),
                        ForeColor = Color.LightGray
                    };
                    
                    var lblValue = new Label 
                    { 
                        Text = "ค่าใหม่:", 
                        Location = new Point(20, 100), 
                        Size = new Size(80, 25),
                        ForeColor = Color.White
                    };

                    // TextBox สำหรับกรอกค่าใหม่
                    var txtValue = new TextBox 
                    { 
                        Location = new Point(110, 98), 
                        Size = new Size(300, 25),
                        BackColor = Color.FromArgb(60, 60, 60),
                        ForeColor = Color.White,
                        BorderStyle = BorderStyle.FixedSingle
                    };
                    
                    // ใส่ค่าเดิมลงไป (แปลงจาก display format เป็นค่าดิบ)
                    if (currentRawValue == -1000000.0m)
                    {
                        txtValue.Text = "OVERLOAD";
                    }
                    else if (currentValue != "-")
                    {
                        txtValue.Text = GetRawValueFromRepairDisplay(currentValue);
                    }

                    // Buttons
                    var btnOK = new Button 
                    { 
                        Text = "บันทึก", 
                        Location = new Point(200, 150), 
                        Size = new Size(100, 35),
                        BackColor = Color.FromArgb(0, 122, 204),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat
                    };
                    
                    var btnCancel = new Button 
                    { 
                        Text = "ยกเลิก", 
                        Location = new Point(310, 150), 
                        Size = new Size(100, 35),
                        BackColor = Color.FromArgb(80, 80, 80),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat
                    };
                    
                    btnOK.Click += (s, args) => 
                    {
                        string newValue = txtValue.Text.Trim();
                        
                        // Validate input
                        if (string.IsNullOrEmpty(newValue))
                        {
                            MessageBox.Show("กรุณากรอกค่า", "ข้อผิดพลาด", 
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        
                        // Update the repair record
                        UpdateRepairRecord(rowIndex, newValue, function);
                        editForm.DialogResult = DialogResult.OK;
                        editForm.Close();
                    };
                    
                    btnCancel.Click += (s, args) => 
                    {
                        editForm.DialogResult = DialogResult.Cancel;
                        editForm.Close();
                    };
                    
                    // Add controls
                    editForm.Controls.AddRange(new Control[] { 
                        lblName, lblFunction, lblRefValue, lblValue, txtValue, btnOK, btnCancel 
                    });
                    
                    // Set focus and show dialog
                    txtValue.Focus();
                    txtValue.SelectAll();
                    editForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                LogActivity($"เกิดข้อผิดพลาดในการแก้ไขค่าวัด: {ex.Message}", true);
                MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetRawValueFromRepairDisplay(string displayValue)
        {
            try
            {
                // ถ้าเป็น OVERLOAD
                if (displayValue == "OVERLOAD")
                {
                    return "-1000000.0";
                }
                
                // ถ้าเป็น "-" (ยังไม่ได้วัด)
                if (displayValue == "-" || displayValue == "<->")
                {
                    return displayValue;
                }
                
                // แปลงจาก format "100.0000k" → "100000"
                displayValue = displayValue.Trim();
                
                // ตรวจสอบ M (Mega)
                if (displayValue.EndsWith("M", StringComparison.OrdinalIgnoreCase))
                {
                    string numStr = displayValue.Substring(0, displayValue.Length - 1);
                    if (double.TryParse(numStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double num))
                    {
                        return (num * 1000000).ToString(CultureInfo.InvariantCulture);
                    }
                }
                
                // ตรวจสอบ k (Kilo)
                if (displayValue.EndsWith("k", StringComparison.OrdinalIgnoreCase))
                {
                    string numStr = displayValue.Substring(0, displayValue.Length - 1);
                    if (double.TryParse(numStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double num))
                    {
                        return (num * 1000).ToString(CultureInfo.InvariantCulture);
                    }
                }
                
                // ถ้าไม่มี suffix ให้คืนค่าเดิม
                return displayValue;
            }
            catch
            {
                return displayValue;
            }
        }

        private async void UpdateRepairRecord(int rowIndex, string newValueStr, string function)
        {
            try
            {
                if (rowIndex < 0 || rowIndex >= _repairRecordsTable.Rows.Count)
                    return;

                var dataRow = _repairRecordsTable.Rows[rowIndex];
                
                decimal newRawValue;
                string newDisplayValue;
                bool? isPass = null;
                
                // จัดการค่าพิเศษ
                if (newValueStr.ToUpper() == "OVERLOAD")
                {
                    newRawValue = -1000000.0m;
                    newDisplayValue = "OVERLOAD";
                }
                else if (newValueStr == "-" || newValueStr == "<->")
                {
                    newRawValue = 0;
                    newDisplayValue = "-";
                }
                else
                {
                    // แปลงค่าจาก string เป็น decimal
                    // รองรับการพิมพ์แบบ "100k", "1M" หรือค่าตัวเลขปกติ
                    double parsedValue;
                    
                    if (newValueStr.EndsWith("M", StringComparison.OrdinalIgnoreCase))
                    {
                        string numStr = newValueStr.Substring(0, newValueStr.Length - 1);
                        parsedValue = double.Parse(numStr, CultureInfo.InvariantCulture) * 1000000;
                    }
                    else if (newValueStr.EndsWith("k", StringComparison.OrdinalIgnoreCase))
                    {
                        string numStr = newValueStr.Substring(0, newValueStr.Length - 1);
                        parsedValue = double.Parse(numStr, CultureInfo.InvariantCulture) * 1000;
                    }
                    else
                    {
                        parsedValue = double.Parse(newValueStr, NumberStyles.Any, CultureInfo.InvariantCulture);
                    }
                    
                    newRawValue = (decimal)parsedValue;
                    
                    // Format ค่าสำหรับการแสดงผล (จะถูก format อีกครั้งโดย CellFormatting)
                    if (function.ToUpper().Contains("RES2W") || 
                        function.ToUpper().Contains("RES4W") ||
                        function.ToUpper().Contains("RESISTANCE2W") || 
                        function.ToUpper().Contains("RESISTANCE4W"))
                    {
                        newDisplayValue = FormatRDTableValue(parsedValue, function);
                    }
                    else
                    {
                        newDisplayValue = parsedValue.ToString("N4", CultureInfo.InvariantCulture);
                    }
                }
                
                // อัพเดทค่าใน DataTable
                dataRow["RawValue"] = newRawValue;
                dataRow["Value"] = newDisplayValue;
                
                // คำนวณ Status ใหม่
                RecalculateRepairStatus(dataRow);
                
                // ดึง Status สำหรับบันทึกลง database
                string status = dataRow["Status"]?.ToString() ?? "";
                if (status == "PASS")
                {
                    isPass = true;
                }
                else if (status == "FAIL")
                {
                    isPass = false;
                }
                // else isPass = null (สำหรับ N/A หรือ "-")
                
                // บันทึกลง database
                int measurementId = dataRow["MeasurementId"] != DBNull.Value ? Convert.ToInt32(dataRow["MeasurementId"]) : 0;
                if (measurementId > 0 && _mysqlManager != null)
                {
                    bool saveSuccess = await _mysqlManager.UpdateRepairMeasurementAsync(measurementId, newRawValue, isPass);
                    if (saveSuccess)
                    {
                        LogActivity($"✓ บันทึกค่าวัดลง database สำเร็จ - ID: {measurementId}, Value: {newDisplayValue}, Status: {status}");
                    }
                    else
                    {
                        LogActivity($"✗ บันทึกค่าวัดลง database ล้มเหลว - ID: {measurementId}", true);
                        MessageBox.Show("แก้ไขค่าสำเร็จ แต่บันทึกลง database ล้มเหลว", "คำเตือน", 
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else if (measurementId <= 0)
                {
                    LogActivity($"⚠ ไม่พบ MeasurementId สำหรับแถวที่ {rowIndex + 1} - ข้อมูลจะไม่ถูกบันทึกลง database", true);
                }
                else if (_mysqlManager == null)
                {
                    LogActivity($"⚠ MySQL ไม่ได้เชื่อมต่อ - ข้อมูลจะไม่ถูกบันทึกลง database", true);
                }
                
                // Refresh DataGridView
                dataGridViewRepair.Refresh();
                
                LogActivity($"แก้ไขค่าวัดแถวที่ {rowIndex + 1}: {newDisplayValue}");
            }
            catch (Exception ex)
            {
                LogActivity($"เกิดข้อผิดพลาดในการอัพเดทค่าวัด: {ex.Message}", true);
                MessageBox.Show($"ไม่สามารถอัพเดทค่าได้: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RecalculateRepairStatus(DataRow dataRow)
        {
            try
            {
                string refValueStr = dataRow["RefValue"]?.ToString() ?? "";
                decimal rawValue = dataRow["RawValue"] != DBNull.Value ? Convert.ToDecimal(dataRow["RawValue"]) : 0;
                bool tolEnabled = dataRow["TolEnabled"] != DBNull.Value ? Convert.ToBoolean(dataRow["TolEnabled"]) : false;
                decimal tolerancePercentage = dataRow["TolerancePercentage"] != DBNull.Value ? Convert.ToDecimal(dataRow["TolerancePercentage"]) : 0;
                
                // ถ้ายังไม่ได้วัด
                if (dataRow["Value"]?.ToString() == "-")
                {
                    dataRow["Status"] = "-";
                    return;
                }
                
                // ถ้าเป็น OVERLOAD
                if (rawValue == -1000000.0m)
                {
                    dataRow["Status"] = "FAIL";
                    return;
                }
                
                // ถ้าไม่มีค่าอ้างอิง หรือเป็น "-" หรือ "<->"
                if (string.IsNullOrEmpty(refValueStr) || refValueStr == "-" || refValueStr == "<->")
                {
                    dataRow["Status"] = "N/A";
                    return;
                }
                
                // Parse ค่าอ้างอิง
                if (!double.TryParse(refValueStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double refValue))
                {
                    dataRow["Status"] = "N/A";
                    return;
                }
                
                // คำนวณ tolerance
                if (tolEnabled && tolerancePercentage > 0)
                {
                    double tolerance = Math.Abs(refValue * (double)tolerancePercentage / 100.0);
                    double measuredValue = (double)rawValue;
                    double difference = Math.Abs(measuredValue - refValue);
                    
                    if (difference <= tolerance)
                    {
                        dataRow["Status"] = "PASS";
                    }
                    else
                    {
                        dataRow["Status"] = "FAIL";
                    }
                }
                else
                {
                    // ไม่มี tolerance - เปรียบเทียบค่าตรงๆ
                    if (Math.Abs((double)rawValue - refValue) < 0.0001)
                    {
                        dataRow["Status"] = "PASS";
                    }
                    else
                    {
                        dataRow["Status"] = "FAIL";
                    }
                }
            }
            catch (Exception ex)
            {
                LogActivity($"เกิดข้อผิดพลาดในการคำนวณ Status: {ex.Message}", true);
                dataRow["Status"] = "ERROR";
            }
        }

        private void DataGridViewRepair_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridViewRepair.IsCurrentCellDirty)
            {
                // Commit the changes immediately for checkbox columns
                if (dataGridViewRepair.CurrentCell.ColumnIndex == dataGridViewRepair.Columns["Select"].Index)
                {
                    dataGridViewRepair.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            }
        }

        private void DataGridViewRepair_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // จัดการ Value column - แปลง RawValue = -1000000.0 เป็น "OVERLOAD" และ format ค่า 2W
            if (dataGridViewRepair.Columns[e.ColumnIndex].Name == "Value")
            {
                if (e.RowIndex >= 0 && e.RowIndex < dataGridViewRepair.Rows.Count)
                {
                    var row = dataGridViewRepair.Rows[e.RowIndex];
                    if (row.DataBoundItem is DataRowView rowView)
                    {
                        var dataRow = rowView.Row;
                        
                        // ถ้ายังไม่ได้วัด (Value = "-") ไม่ต้อง format
                        string valueStr = e.Value?.ToString() ?? "";
                        if (valueStr == "-")
                        {
                            return;
                        }
                        
                        // ตรวจสอบ OVERLOAD
                        if (!dataRow.IsNull("RawValue"))
                        {
                            decimal rawValue = Convert.ToDecimal(dataRow["RawValue"]);
                            if (rawValue == -1000000.0m)
                            {
                                e.Value = "OVERLOAD";
                                e.FormattingApplied = true;
                                return;
                            }
                            
                            // Format สำหรับ Resistance 2W/4W
                            string functionName = dataRow["Function"]?.ToString() ?? "";
                            if (functionName.ToUpper().Contains("RES2W") || 
                                functionName.ToUpper().Contains("RES4W") ||
                                functionName.ToUpper().Contains("RESISTANCE2W") || 
                                functionName.ToUpper().Contains("RESISTANCE4W"))
                            {
                                double value = (double)rawValue;
                                string formattedValue = FormatRDTableValue(value, functionName);
                                e.Value = formattedValue;
                                e.FormattingApplied = true;
                            }
                        }
                    }
                }
            }
            
            // จัดการสีสำหรับ Status column
            if (dataGridViewRepair.Columns[e.ColumnIndex].Name == "Status")
            {
                if (e.Value != null)
                {
                    string status = e.Value.ToString();
                    
                    if (status == "PASS")
                    {
                        e.CellStyle.BackColor = Color.LightGreen;
                        e.CellStyle.ForeColor = Color.DarkGreen;
                        e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                    }
                    else if (status == "FAIL")
                    {
                        e.CellStyle.BackColor = Color.LightCoral;
                        e.CellStyle.ForeColor = Color.DarkRed;
                        e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                    }
                    else // "-" or empty
                    {
                        e.CellStyle.BackColor = Color.LightGray;
                        e.CellStyle.ForeColor = Color.Gray;
                    }
                }
            }
        }

        private async void ButtonRecordRepair_Click(object sender, EventArgs e)
        {
            if (!_isRepairTabActive)
            {
                MessageBox.Show("กรุณาเลือก QWID และ Serial Number ก่อน", "ข้อมูลไม่ครบ",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_currentRepairSessionId == -1)
            {
                MessageBox.Show("กรุณาเลือก Repair Session ก่อน", "ข้อมูลไม่ครบ",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ตรวจสอบว่าได้อ่านค่าจาก DMM หรือไม่
            if (double.IsNaN(_lastReadingValue))
            {
                MessageBox.Show("กรุณาอ่านค่าจาก DMM ก่อนบันทึก", "ไม่มีค่าที่วัดได้",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ตรวจสอบว่ามีการเลือกแถวใน DataGridView หรือไม่
            // ใช้ CurrentRow แทน SelectedRows เพื่อให้แน่ใจว่ามีแถวที่ active อยู่
            if (dataGridViewRepair.CurrentRow == null)
            {
                MessageBox.Show("กรุณาเลือกอย่างน้อย 1 แถวก่อนบันทึก", "ไม่มีแถวที่เลือก",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ตรวจสอบว่าเลือกมากกว่า 1 แถวหรือไม่
            if (dataGridViewRepair.SelectedRows.Count > 1)
            {
                MessageBox.Show("กรุณาเลือกเพียง 1 แถวต่อการบันทึก", "เลือกมากเกินไป",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // ตรวจสอบว่าเป็น OVERLOAD หรือไม่
                decimal measuredValue;
                if (_isOverload)
                {
                    measuredValue = -1000000.0m; // บันทึกค่าพิเศษสำหรับ OVERLOAD
                }
                else
                {
                    measuredValue = (decimal)_lastReadingValue;
                }

                // หาแถวที่เลือก - ใช้ CurrentRow
                DataGridViewRow selectedGridRow = dataGridViewRepair.CurrentRow;
                DataRowView rowView = selectedGridRow.DataBoundItem as DataRowView;
                if (rowView == null)
                {
                    MessageBox.Show("ไม่สามารถดึงข้อมูลแถวได้", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                DataRow selectedRow = rowView.Row;

                int measurementId = Convert.ToInt32(selectedRow["MeasurementId"]);
                decimal refValue = Convert.ToDecimal(selectedRow["RefValue"]);
                bool tolEnabled = Convert.ToBoolean(selectedRow["TolEnabled"]);
                decimal tolPercentage = Convert.ToDecimal(selectedRow["TolerancePercentage"]);
                string expectedFunction = selectedRow["Function"].ToString();

                // ตรวจสอบว่า function ที่เลือกตรงกับ row ที่เลือกหรือไม่
                if (_currentFunction.ToString() != expectedFunction)
                {
                    MessageBox.Show($"Function ไม่ตรงกัน!\n\n" +
                        $"Function ที่เลือก: {_currentFunction}\n" +
                        $"Function ที่คาดหวัง: {expectedFunction}\n\n" +
                        "กรุณาเลือก Function ที่ถูกต้องก่อนบันทึก", 
                        "Function ไม่ตรงกัน", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // คำนวณ Status (OVERLOAD จะถือว่า FAIL เสมอ)
                string status;
                bool isPass;
                if (_isOverload)
                {
                    status = "FAIL";
                    isPass = false;
                }
                else
                {
                    status = CalculateRepairStatus(measuredValue, refValue, tolEnabled, tolPercentage);
                    isPass = status == "PASS";
                }

                // บันทึกลง Database
                bool success = await _mysqlManager.UpdateRepairMeasurementAsync(measurementId, measuredValue, isPass);

                if (success)
                {
                    // อัพเดต UI
                    selectedRow["RawValue"] = measuredValue; // เก็บค่าดิบ
                    if (_isOverload)
                    {
                        selectedRow["Value"] = "OVERLOAD"; // แสดงเป็น OVERLOAD
                    }
                    else
                    {
                        selectedRow["Value"] = measuredValue.ToString("N4", CultureInfo.InvariantCulture);
                    }
                    selectedRow["Status"] = status;

                    string displayValue = _isOverload ? "OVERLOAD" : measuredValue.ToString("N4", CultureInfo.InvariantCulture);
                    LogActivity($"บันทึกค่า Repair Measurement สำเร็จ: {displayValue} ({status})");

                    // เล่นเสียงตาม Status
                    if (isPass)
                    {
                        SoundUtil.Beep();
                    }
                    else
                    {
                        SoundUtil.Over();
                    }

                    // เลื่อนไป row ถัดไป
                    MoveToNextRepairRow();
                }
                else
                {
                    MessageBox.Show("บันทึกค่าล้มเหลว", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                LogActivity($"เกิดข้อผิดพลาดในการบันทึก Repair: {ex.Message}", true);
                MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ButtonDeleteRepair_Click(object sender, EventArgs e)
        {
            // ตรวจสอบว่ามีการเลือกแถวหรือไม่
            if (dataGridViewRepair.SelectedRows.Count == 0)
            {
                MessageBox.Show("กรุณาเลือกรายการที่ต้องการลบ", "ไม่มีรายการที่เลือก", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmResult = MessageBox.Show(
                $"ยืนยันการลบ {dataGridViewRepair.SelectedRows.Count} รายการที่เลือก?", 
                "ยืนยันการลบ", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Question);

            if (confirmResult == DialogResult.Yes)
            {
                try
                {
                    SoundUtil.Delete();

                    // เก็บ ID ของแถวที่จะลบ
                    List<int> idsToDelete = new List<int>();
                    List<DataRow> rowsToDelete = new List<DataRow>();

                    foreach (DataGridViewRow gridRow in dataGridViewRepair.SelectedRows)
                    {
                        DataRowView rowView = gridRow.DataBoundItem as DataRowView;
                        if (rowView != null)
                        {
                            DataRow row = rowView.Row;
                            int id = row["ID"] != DBNull.Value ? Convert.ToInt32(row["ID"]) : 0;
                            if (id > 0)
                            {
                                idsToDelete.Add(id);
                            }
                            rowsToDelete.Add(row);
                        }
                    }

                    // ลบออกจาก DataTable
                    foreach (DataRow row in rowsToDelete)
                    {
                        row.Delete();
                    }
                    _repairRecordsTable.AcceptChanges();

                    // จัดเรียงหมายเลขใหม่
                    RenumberRepairRows();

                    // ลบจาก database (ถ้ามี)
                    if (idsToDelete.Count > 0)
                    {
                        _ = DeleteRepairRecordsFromDatabase(idsToDelete);
                    }

                    LogActivity($"ลบข้อมูล Repair จำนวน {rowsToDelete.Count} รายการ");
                }
                catch (Exception ex)
                {
                    LogActivity($"เกิดข้อผิดพลาดในการลบข้อมูล: {ex.Message}", true);
                    MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void RenumberRepairRows()
        {
            int counter = 1;
            foreach (DataRow row in _repairRecordsTable.Rows)
            {
                row["No"] = counter++;
            }
        }

        private async Task DeleteRepairRecordsFromDatabase(List<int> ids)
        {
            try
            {
                string idsList = string.Join(",", ids);
                string sql = $"DELETE FROM spaze.measurements WHERE id IN ({idsList})";
                
                var result = await _mysqlManager.ExecuteQuery(sql);
                
                if (result.Success)
                {
                    LogActivity($"ลบข้อมูลจาก database สำเร็จ: {ids.Count} รายการ");
                }
                else
                {
                    LogActivity($"ลบข้อมูลจาก database ล้มเหลว: {result.ErrorMessage}", true);
                }
            }
            catch (Exception ex)
            {
                LogActivity($"เกิดข้อผิดพลาดในการลบข้อมูลจาก database: {ex.Message}", true);
            }
        }

        private void ButtonExportRepair_Click(object sender, EventArgs e)
        {
            // Export Repair data to CSV
            if (_repairRecordsTable.Rows.Count == 0)
            {
                MessageBox.Show("ไม่มีข้อมูล Repair ให้ส่งออก", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "CSV File (*.csv)|*.csv";
                saveFileDialog.FileName = $"Repair_Data_{_repairQwid}_{_repairSerialNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var lines = new List<string>();
                        
                        // Header
                        lines.Add("No,Name,Function,RefValue,MeasuredValue,Status");
                        
                        foreach (DataRow row in _repairRecordsTable.Rows)
                        {
                            string no = row["No"].ToString();
                            string name = row["Name"].ToString();
                            string function = row["Function"].ToString();
                            string refValue = row["RefValue"].ToString();
                            
                            // ใช้ RawValue สำหรับ export (ค่าดิบที่แท้จริง)
                            string measuredValue;
                            if (row.IsNull("RawValue"))
                            {
                                measuredValue = "-"; // ยังไม่ได้วัด
                            }
                            else
                            {
                                decimal rawValue = Convert.ToDecimal(row["RawValue"]);
                                if (rawValue == -1000000.0m)
                                {
                                    measuredValue = "OVERLOAD";
                                }
                                else
                                {
                                    measuredValue = rawValue.ToString("F8", CultureInfo.InvariantCulture); // ส่งออกเป็นทศนิยม 8 ตำแหน่ง
                                }
                            }
                            
                            string status = row["Status"].ToString();
                            
                            string csvLine = string.Join(",",
                                $"\"{no}\"",
                                $"\"{name}\"",
                                $"\"{function}\"",
                                $"\"{refValue}\"",
                                $"\"{measuredValue}\"",
                                $"\"{status}\""
                            );
                            lines.Add(csvLine);
                        }
                        
                        File.WriteAllLines(saveFileDialog.FileName, lines, Encoding.UTF8);
                        LogActivity($"ส่งออกข้อมูล Repair ไปยัง {saveFileDialog.FileName} สำเร็จ");
                        MessageBox.Show("ส่งออกข้อมูลสำเร็จ!", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        LogActivity($"เกิดข้อผิดพลาดในการส่งออกข้อมูล: {ex.Message}", true);
                        MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ButtonSelectAllRepair_Click(object sender, EventArgs e)
        {
            dataGridViewRepair.SelectAll();
            LogActivity("เลือกทั้งหมดใน Repair Tab");
        }

        private void ButtonDeselectAllRepair_Click(object sender, EventArgs e)
        {
            dataGridViewRepair.ClearSelection();
            LogActivity("ยกเลิกการเลือกทั้งหมดใน Repair Tab");
        }

        private void ButtonClearRepair_Click(object sender, EventArgs e)
        {
            // TODO: Implement clear functionality
            MessageBox.Show("Clear functionality coming soon", "Info", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void ButtonChangeModelRepair_Click(object sender, EventArgs e)
        {
            // ล้าง datatable
            _repairRecordsTable.Clear();
            
            // รีเซ็ตค่า Repair Serial และ QWID
            _repairSerialId = -1;
            _repairSerialNumber = "";
            _repairQwid = "";
            _currentRepairSessionId = -1;
            _repairSessions.Clear();
            
            // ซ่อน session controls
            labelRepairSession.Visible = false;
            comboBoxRepairSession.Visible = false;
            buttonNewRepairSession.Visible = false;
            buttonRepairHistory.Visible = false;
            
            LogActivity("เปลี่ยนโมเดล: ล้างข้อมูลและเลือกใหม่");
            
            // เปิด RepairQwidDialog เพื่อเลือกใหม่
            await ShowRepairQwidDialog();
        }

        #region Repair Session Methods

        private async Task CreateFirstRepairSession()
        {
            try
            {
                // ตรวจสอบว่ามี template (software_measurements) หรือไม่
                var templateMeasurements = await _mysqlManager.GetSoftwareMeasurementsBySerialIdAsync(_repairSerialId);
                
                if (templateMeasurements == null || templateMeasurements.Count == 0)
                {
                    MessageBox.Show(
                        "ไม่พบ Template สำหรับ Serial Number นี้\n\nกรุณาสร้างข้อมูลใน R&D Tab ก่อน",
                        "ไม่พบ Template",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // ตรวจสอบว่ามี session อยู่แล้วหรือไม่
                var existingSessions = await _mysqlManager.GetRepairSessionsBySerialIdAsync(_repairSerialId, _repairQwid);
                if (existingSessions != null && existingSessions.Count > 0)
                {
                    MessageBox.Show(
                        "มีรอบการซ่อมอยู่แล้ว\n\nกรุณาใช้ปุ่ม 'สร้างรอบซ่อมใหม่' แทน",
                        "แจ้งเตือน",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // สร้าง session ครั้งแรก
                int sessionId = await _mysqlManager.CreateRepairSessionAsync(
                    _repairQwid,
                    _repairSerialId,
                    _repairSerialNumber,
                    1,
                    "รอบการซ่อมครั้งแรก",
                    "before_repair",
                    AuthManager.CurrentUser?.Username);

                // Clone measurements
                int clonedCount = await _mysqlManager.CloneMeasurementsToRepairSessionAsync(sessionId, _repairSerialId);

                LogActivity($"สร้างรอบการซ่อมครั้งแรกสำเร็จ (Clone {clonedCount} measurements)");

                // โหลด sessions ใหม่
                await LoadRepairSessions();

                MessageBox.Show("สร้างรอบการซ่อมครั้งแรกสำเร็จ", "สำเร็จ", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogActivity($"เกิดข้อผิดพลาดในการสร้างรอบการซ่อมครั้งแรก: {ex.Message}", true);
                MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void ButtonNewRepairSession_Click(object sender, EventArgs e)
        {
            try
            {
                if (_repairSerialId <= 0)
                {
                    MessageBox.Show("กรุณาเลือก Serial Number ก่อน", "แจ้งเตือน", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // ดึง session number ถัดไป (กรองตาม QWID ด้วย)
                int nextSessionNumber = await _mysqlManager.GetNextRepairSessionNumberAsync(_repairSerialId, _repairQwid);

                // แสดง dialog สร้าง session ใหม่
                using (var dialog = new NewRepairSessionDialog(_repairQwid, _repairSerialNumber, nextSessionNumber))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        // สร้าง session ใหม่
                        int sessionId = await _mysqlManager.CreateRepairSessionAsync(
                            _repairQwid, 
                            _repairSerialId, 
                            _repairSerialNumber, 
                            nextSessionNumber, 
                            dialog.Description,
                            dialog.RepairType,
                            AuthManager.CurrentUser?.Username);

                        // Clone measurements จาก template
                        int clonedCount = await _mysqlManager.CloneMeasurementsToRepairSessionAsync(sessionId, _repairSerialId);

                        LogActivity($"สร้างรอบการซ่อมครั้งที่ {nextSessionNumber} สำเร็จ (Clone {clonedCount} measurements)");
                        
                        // โหลด sessions ใหม่
                        await LoadRepairSessions();
                        
                        // เลือก session ที่สร้างใหม่
                        SelectRepairSession(sessionId);
                        
                        MessageBox.Show($"สร้างรอบการซ่อมครั้งที่ {nextSessionNumber} สำเร็จ", "สำเร็จ", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                LogActivity($"เกิดข้อผิดพลาดในการสร้างรอบการซ่อม: {ex.Message}", true);
                MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void ButtonRepairHistory_Click(object sender, EventArgs e)
        {
            try
            {
                if (_repairSessions == null || _repairSessions.Count == 0)
                {
                    MessageBox.Show("ไม่มีประวัติการซ่อม", "แจ้งเตือน", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (var dialog = new RepairHistoryDialog(_repairSessions, _mysqlManager, _repairQwid, _repairSerialNumber))
                {
                    var result = dialog.ShowDialog();
                    
                    // ถ้ามีการลบหรือเปลี่ยนประเภท ให้โหลดใหม่เสมอ
                    if (dialog.SessionDeleted || dialog.SessionTypeChanged)
                    {
                        await LoadRepairSessions();
                    }
                    
                    // ถ้ากด OK และเลือก session ให้ไปที่ session นั้น
                    if (result == DialogResult.OK && dialog.SelectedSession != null)
                    {
                        SelectRepairSession(dialog.SelectedSession.Id);
                        LogActivity($"เลือกรอบการซ่อมครั้งที่ {dialog.SelectedSession.SessionNumber}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogActivity($"เกิดข้อผิดพลาดในการแสดงประวัติการซ่อม: {ex.Message}", true);
                MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void ComboBoxRepairSession_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (comboBoxRepairSession.SelectedItem != null)
                {
                    var session = (RepairSession)comboBoxRepairSession.SelectedItem;
                    if (session.Id != _currentRepairSessionId)
                    {
                        await LoadRepairMeasurementsBySession(session.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                LogActivity($"เกิดข้อผิดพลาดในการเปลี่ยน session: {ex.Message}", true);
            }
        }

        private async Task LoadRepairSessions()
        {
            try
            {
                if (_repairSerialId <= 0)
                    return;

                _repairSessions = await _mysqlManager.GetRepairSessionsBySerialIdAsync(_repairSerialId, _repairQwid);
                
                comboBoxRepairSession.DataSource = null;
                comboBoxRepairSession.DataSource = _repairSessions;
                comboBoxRepairSession.DisplayMember = "DisplayText";
                comboBoxRepairSession.ValueMember = "Id";

                if (_repairSessions.Count > 0)
                {
                    // แสดง session controls
                    labelRepairSession.Visible = true;
                    comboBoxRepairSession.Visible = true;
                    buttonNewRepairSession.Visible = true;
                    buttonRepairHistory.Visible = true;
                    
                    // เลือก session แรก (ล่าสุด)
                    comboBoxRepairSession.SelectedIndex = 0;
                    
                    // แจ้ง user ว่านี่คือการวัดครั้งที่เท่าไหร่
                    int sessionCount = _repairSessions.Count;
                    LogActivity($"พบรอบการซ่อม {sessionCount} รอบสำหรับ QWID: {_repairQwid}, Serial: {_repairSerialNumber}");
                }
                else
                {
                    // ถ้าไม่มี session ให้ซ่อนทุกปุ่ม (จะให้สร้างรอบแรกผ่าน LoadRepairRecordsFromDatabase)
                    labelRepairSession.Visible = false;
                    comboBoxRepairSession.Visible = false;
                    buttonNewRepairSession.Visible = false;
                    buttonRepairHistory.Visible = false;
                }
                
                LogActivity($"โหลด Repair Sessions สำเร็จ ({_repairSessions.Count} sessions)");
            }
            catch (Exception ex)
            {
                LogActivity($"เกิดข้อผิดพลาดในการโหลด Repair Sessions: {ex.Message}", true);
            }
        }

        private void SelectRepairSession(int sessionId)
        {
            var session = _repairSessions.FirstOrDefault(s => s.Id == sessionId);
            if (session != null)
            {
                comboBoxRepairSession.SelectedItem = session;
            }
        }

        private async Task LoadRepairMeasurementsBySession(int sessionId)
        {
            try
            {
                _currentRepairSessionId = sessionId;
                _currentRepairMeasurements = await _mysqlManager.GetRepairMeasurementsBySessionIdAsync(sessionId);

                // อัพเดต DataTable
                _repairRecordsTable.Clear();
                
                foreach (var measurement in _currentRepairMeasurements)
                {
                    var row = _repairRecordsTable.NewRow();
                    row["Select"] = false;
                    row["No"] = measurement.MeasurementNo;
                    row["Name"] = measurement.MeasurementName ?? "";
                    row["Function"] = measurement.FunctionName;
                    row["RefValue"] = measurement.MeasurementValue.ToString("N4", CultureInfo.InvariantCulture); // ค่าอ้างอิงจาก template (format เป็น string)
                    
                    // จัดการ RawValue และ Value
                    if (measurement.ActualMeasuredValue.HasValue)
                    {
                        row["RawValue"] = measurement.ActualMeasuredValue.Value; // เก็บค่าดิบ
                        
                        // ถ้าเป็น -1000000.0 แสดงเป็น OVERLOAD, ถ้าไม่ใช่แสดงเป็นตัวเลข
                        if (measurement.ActualMeasuredValue.Value == -1000000.0m)
                        {
                            row["Value"] = "OVERLOAD";
                        }
                        else
                        {
                            row["Value"] = measurement.ActualMeasuredValue.Value.ToString("N4", CultureInfo.InvariantCulture);
                        }
                    }
                    else
                    {
                        row["RawValue"] = DBNull.Value; // ยังไม่มีค่า
                        row["Value"] = "-"; // ยังไม่ได้วัด
                    }
                    
                    row["TolEnabled"] = measurement.ToleranceEnabled;
                    row["TolerancePercentage"] = measurement.TolerancePercentage ?? 0m;
                    row["SystemInfo"] = measurement.SystemInfo ?? "";
                    row["MeasurementId"] = measurement.Id;
                    
                    // คำนวณ Status โดยใช้ ActualMeasuredValue ถ้ามี
                    if (measurement.ActualMeasuredValue.HasValue)
                    {
                        // ถ้าเป็น OVERLOAD จะถือว่า FAIL เสมอ
                        if (measurement.ActualMeasuredValue.Value == -1000000.0m)
                        {
                            row["Status"] = "FAIL";
                        }
                        else
                        {
                            row["Status"] = CalculateRepairStatus(
                                measurement.ActualMeasuredValue.Value,
                                measurement.MeasurementValue,
                                measurement.ToleranceEnabled,
                                measurement.TolerancePercentage ?? 0m
                            );
                        }
                    }
                    else
                    {
                        row["Status"] = "-"; // ยังไม่ได้วัด
                    }
                    
                    _repairRecordsTable.Rows.Add(row);
                }

                LogActivity($"โหลด Repair Measurements สำเร็จ ({_currentRepairMeasurements.Count} measurements)");
            }
            catch (Exception ex)
            {
                LogActivity($"เกิดข้อผิดพลาดในการโหลด Repair Measurements: {ex.Message}", true);
            }
        }

        /// <summary>
        /// เลื่อนไป row ถัดไปที่ยังไม่ได้วัด
        /// </summary>
        private void MoveToNextRepairRow()
        {
            try
            {
                // หา row ถัดไปที่ยังไม่ได้วัด (Value = "-")
                for (int i = 0; i < dataGridViewRepair.Rows.Count; i++)
                {
                    var row = dataGridViewRepair.Rows[i];
                    string value = row.Cells["Value"].Value?.ToString() ?? "";
                    
                    if (value == "-" || value == "<->") // ยังไม่ได้วัด
                    {
                        // เลือก row นี้
                        dataGridViewRepair.ClearSelection();
                        row.Selected = true;
                        
                        // เลื่อน view ไปที่ row นี้
                        dataGridViewRepair.FirstDisplayedScrollingRowIndex = i;
                        dataGridViewRepair.CurrentCell = row.Cells["Name"];
                        
                        LogActivity($"เลื่อนไป row ถัดไป: {row.Cells["No"].Value} - {row.Cells["Name"].Value}");
                        return;
                    }
                }
                
                // ถ้าไม่เจอ row ที่ยังไม่ได้วัด
                LogActivity("ไม่พบ row ที่ยังไม่ได้วัด");
            }
            catch (Exception ex)
            {
                LogActivity($"เกิดข้อผิดพลาดในการเลื่อนไป row ถัดไป: {ex.Message}", true);
            }
        }

        /// <summary>
        /// คำนวณ Status (PASS/FAIL) สำหรับ Repair Measurement
        /// </summary>
        private string CalculateRepairStatus(decimal actualValue, decimal refValue, 
            bool tolEnabled, decimal tolPercentage)
        {
            // ถ้าไม่ได้เปิด tolerance ให้ผ่านทันที
            if (!tolEnabled)
            {
                return "PASS";
            }

            // คำนวณ tolerance range จาก reference value
            decimal toleranceRange = Math.Abs(refValue * tolPercentage / 100m);
            decimal calculatedUpper = refValue + toleranceRange;
            decimal calculatedLower = refValue - toleranceRange;

            // เช็คว่าอยู่ใน tolerance range หรือไม่
            if (actualValue >= calculatedLower && actualValue <= calculatedUpper)
            {
                return "PASS";
            }
            else
            {
                return "FAIL";
            }
        }

        #endregion

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
            if (e.Control && e.KeyCode == Keys.Q)
            {
                // ตรวจสอบว่าอยู่หน้าไหน
                bool isMainTab = (tabControl1.SelectedTab == tab_datarecording);
                bool isRDTab = (tabControl1.SelectedTab == tab_rd);
                bool isRepairTab = (tabControl1.SelectedTab == tab_repair);

                // ตรวจสอบว่าปุ่ม Record ของแท็บนั้นเปิดใช้งานอยู่หรือไม่
                bool canRecord = (isMainTab && buttonRecord.Enabled) ||
                                (isRDTab && buttonRecordRD.Enabled) ||
                                (isRepairTab && buttonRecordRepair.Enabled);

                if (!canRecord)
                {
                    return; // ไม่สามารถบันทึกได้ในแท็บนี้
                }

                if (isWaitingForSecondCtrlQ)
                {
                    // Ctrl+Q ครั้งที่ 2 = Double Ctrl+Q = ลบข้อมูลล่าสุด
                    isWaitingForSecondCtrlQ = false;
                    
                    if (isMainTab)
                    {
                        deleteLatestRecord(); // ลบข้อมูลหน้าหลัก
                    }
                    else if (isRDTab)
                    {
                        // TODO: เพิ่ม function ลบข้อมูล R&D ถ้ามี
                        LogActivity("ฟีเจอร์ลบข้อมูล R&D ยังไม่รองรับ", true);
                    }
                    else if (isRepairTab)
                    {
                        // TODO: เพิ่ม function ลบข้อมูล Repair ถ้ามี
                        LogActivity("ฟีเจอร์ลบข้อมูล Repair ยังไม่รองรับ", true);
                    }
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
                        
                        if (isMainTab)
                        {
                            await saveRecord(); // บันทึกข้อมูลหน้าหลัก
                        }
                        else if (isRDTab)
                        {
                            // เรียก ButtonRecordRD_Click
                            ButtonRecordRD_Click(this, EventArgs.Empty);
                        }
                        else if (isRepairTab)
                        {
                            // เรียก ButtonRecordRepair_Click
                            ButtonRecordRepair_Click(this, EventArgs.Empty);
                        }
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

        private void ConfigureTabAccessBasedOnRole()
        {
            if (!AuthManager.IsLoggedIn() || AuthManager.CurrentUser == null)
            {
                // ถ้าไม่ได้ login ให้ซ่อนทุก tabs ยกเว้นหน้าหลัก
                HideAllTabsExceptMain();
                return;
            }

            int roleId = AuthManager.CurrentUser.RoleId;
            string roleName = AuthManager.CurrentUser.RoleDisplayName ?? AuthManager.CurrentUser.Role ?? "Unknown";
            
            LogActivity($"กำหนดสิทธิ์การเข้าถึงสำหรับ Role: {roleName} (ID: {roleId})");

            // กำหนดสิทธิ์ตาม role ID
            if (roleId == AuthManager.ROLE_ADMIN || roleId == AuthManager.ROLE_MANAGER)
            {
                // admin (1) และ manager (2): ดูได้ทุก tabs
                ShowAllTabs();
                LogActivity("สิทธิ์: เข้าถึงได้ทุก tabs");
            }
            else if (roleId == AuthManager.ROLE_RD)
            {
                // rd (3): ดูได้ทุก tabs ยกเว้นตั้งค่า
                ShowAllTabs();
                HideTab(tab_settings);
                LogActivity("สิทธิ์: เข้าถึงได้ทุก tabs ยกเว้นตั้งค่า");
            }
            else if (roleId == AuthManager.ROLE_STAFF)
            {
                // staff (4): ดูได้แค่หน้าหลักและ Repair
                HideAllTabsExceptMain();
                ShowTab(tab_repair);
                LogActivity("สิทธิ์: เข้าถึงได้เฉพาะหน้าหลักและ Repair");
            }
            else
            {
                // role อื่นๆ: ดูได้แค่หน้าหลัก
                HideAllTabsExceptMain();
                LogActivity("สิทธิ์: เข้าถึงได้เฉพาะหน้าหลัก");
            }
        }

        private void ShowAllTabs()
        {
            if (!tabControl1.TabPages.Contains(tab_datarecording))
                tabControl1.TabPages.Add(tab_datarecording);
            if (!tabControl1.TabPages.Contains(tab_rd))
                tabControl1.TabPages.Add(tab_rd);
            if (!tabControl1.TabPages.Contains(tab_repair))
                tabControl1.TabPages.Add(tab_repair);
            if (!tabControl1.TabPages.Contains(tab_settings))
                tabControl1.TabPages.Add(tab_settings);

            // เรียงลำดับ tabs ให้ถูกต้อง
            tabControl1.TabPages.Clear();
            tabControl1.TabPages.Add(tab_datarecording);
            tabControl1.TabPages.Add(tab_rd);
            tabControl1.TabPages.Add(tab_repair);
            tabControl1.TabPages.Add(tab_settings);
        }

        private void HideAllTabsExceptMain()
        {
            tabControl1.TabPages.Clear();
            tabControl1.TabPages.Add(tab_datarecording);
        }

        private void ShowTab(TabPage tab)
        {
            if (!tabControl1.TabPages.Contains(tab))
            {
                // เพิ่ม tab ในตำแหน่งที่เหมาะสม
                if (tab == tab_rd && !tabControl1.TabPages.Contains(tab_rd))
                {
                    int index = tabControl1.TabPages.IndexOf(tab_datarecording) + 1;
                    tabControl1.TabPages.Insert(index, tab_rd);
                }
                else if (tab == tab_repair && !tabControl1.TabPages.Contains(tab_repair))
                {
                    int index = tabControl1.TabPages.Contains(tab_rd) ? 
                               tabControl1.TabPages.IndexOf(tab_rd) + 1 : 
                               tabControl1.TabPages.IndexOf(tab_datarecording) + 1;
                    tabControl1.TabPages.Insert(index, tab_repair);
                }
                else if (tab == tab_settings && !tabControl1.TabPages.Contains(tab_settings))
                {
                    tabControl1.TabPages.Add(tab_settings);
                }
            }
        }

        private void HideTab(TabPage tab)
        {
            if (tabControl1.TabPages.Contains(tab))
            {
                tabControl1.TabPages.Remove(tab);
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
            
            // Initialize previous tab index to current tab
            _previousTabIndex = tabControl1.SelectedIndex;
            
            InitializeMySQLSettings();
            UpdateUserDisplay();
            ConfigureTabAccessBasedOnRole();
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
            
            // Initialize other settings
            InitializeSoundSettings();
            InitializeInstrumentSettings();
            InitializeQWRecordSettings();
        }
        
        private void InitializeSoundSettings()
        {
            // Load sound settings from SettingsManager
            textBoxBeepSound.Text = SettingsManager.GetSoundPath("Beep");
            textBoxDeleteSound.Text = SettingsManager.GetSoundPath("Delete");
            textBoxOverSound.Text = SettingsManager.GetSoundPath("Over");
            
            // Add event handlers for sound settings changes
            textBoxBeepSound.TextChanged += (s, e) => { SettingsManager.SetSoundPath("Beep", textBoxBeepSound.Text); SettingsManager.SaveSettings(); };
            textBoxDeleteSound.TextChanged += (s, e) => { SettingsManager.SetSoundPath("Delete", textBoxDeleteSound.Text); SettingsManager.SaveSettings(); };
            textBoxOverSound.TextChanged += (s, e) => { SettingsManager.SetSoundPath("Over", textBoxOverSound.Text); SettingsManager.SaveSettings(); };
            
            // Add event handlers for browse buttons
            buttonBrowseBeep.Click += ButtonBrowseBeep_Click;
            buttonBrowseDelete.Click += ButtonBrowseDelete_Click;
            buttonBrowseOver.Click += ButtonBrowseOver_Click;
            
            // Add event handlers for test buttons
            buttonTestBeep.Click += (s, e) => SoundUtil.Beep();
            buttonTestDelete.Click += (s, e) => SoundUtil.Delete();
            buttonTestOver.Click += (s, e) => SoundUtil.Over();
        }
        
        private void ButtonBrowseBeep_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Audio Files|*.mp3;*.wav;*.m4a;*.aac|All Files|*.*";
                dialog.Title = "เลือกไฟล์เสียงสำหรับบันทึกข้อมูล";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxBeepSound.Text = dialog.FileName;
                }
            }
        }
        
        private void ButtonBrowseDelete_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Audio Files|*.mp3;*.wav;*.m4a;*.aac|All Files|*.*";
                dialog.Title = "เลือกไฟล์เสียงสำหรับลบข้อมูล";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxDeleteSound.Text = dialog.FileName;
                }
            }
        }
        
        private void ButtonBrowseOver_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Audio Files|*.mp3;*.wav;*.m4a;*.aac|All Files|*.*";
                dialog.Title = "เลือกไฟล์เสียงสำหรับค่าวัดเกิน";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxOverSound.Text = dialog.FileName;
                }
            }
        }
        
        private void InitializeInstrumentSettings()
        {
            // Load instrument settings from SettingsManager
            textBoxInstrumentSerial.Text = SettingsManager.InstrumentSerial;
            textBoxOperatorID.Text = SettingsManager.OperatorID.ToString();
            
            // Add event handlers for instrument settings changes
            textBoxInstrumentSerial.TextChanged += (s, e) => { SettingsManager.InstrumentSerial = textBoxInstrumentSerial.Text; SettingsManager.SaveSettings(); };
            textBoxOperatorID.TextChanged += (s, e) => 
            { 
                if (int.TryParse(textBoxOperatorID.Text, out int operatorId)) 
                { 
                    SettingsManager.OperatorID = operatorId; 
                    SettingsManager.SaveSettings(); 
                } 
            };
        }
        
        private void InitializeQWRecordSettings()
        {
            // Load QW record settings from SettingsManager
            textBoxCurrentQWIDSettings.Text = SettingsManager.CurrentQWID;
            textBoxCurrentSectionSettings.Text = SettingsManager.CurrentSection;
            
            // Add event handlers for QW record settings changes
            textBoxCurrentQWIDSettings.TextChanged += (s, e) => { SettingsManager.CurrentQWID = textBoxCurrentQWIDSettings.Text; SettingsManager.SaveSettings(); };
            textBoxCurrentSectionSettings.TextChanged += (s, e) => { SettingsManager.CurrentSection = textBoxCurrentSectionSettings.Text; SettingsManager.SaveSettings(); };
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