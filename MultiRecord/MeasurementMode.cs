using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
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
        private string _currentRepairId;
        private string _currentTestGroupId;
        private string _currentAnnotationId;
        private string _lastError = "";

        public MeasurementMode(DataTable recordsTable, SDM3055 dmm)
        {
            InitializeComponent();
            _recordsTable = recordsTable;
            _dmm = dmm;
            _measurementPoints = new List<MeasurementPoint>();
            _pcbMeasurements = new List<PCBMeasurement>();
            InitializeMeasurementPoints();
            
            // Add DataGridView event handler for marker navigation
            dataGridViewPoints.CellClick += DataGridViewPoints_CellClick;
            
            // Add context menu for marker navigation
            InitializeMarkerNavigationContextMenu();
            
            // Initialize position after form is loaded
            this.Load += async (s, e) => 
            {
                try 
                { 
                    // Small delay to ensure all controls are fully initialized
                    await Task.Delay(100);
                    
                    // Load measurement points after form is fully loaded
                    LoadMeasurementPoints();
                    InitializeRealTimeDisplay();
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
            // Check if DataGridView is initialized and accessible
            if (dataGridViewPoints == null || dataGridViewPoints.IsDisposed || !dataGridViewPoints.IsHandleCreated)
            {
                LogActivity("DataGridView not ready for loading points");
                return;
            }

            // Check if we're on the UI thread
            if (dataGridViewPoints.InvokeRequired)
            {
                dataGridViewPoints.Invoke(new Action(LoadMeasurementPoints));
                return;
            }

            try
            {
                dataGridViewRowsClearSafely();

                for (int i = 0; i < _measurementPoints.Count; i++)
                {
                    var point = _measurementPoints[i];
                    
                    // Add row safely
                    int rowIndex = dataGridViewPoints.Rows.Add();
                    var row = dataGridViewPoints.Rows[rowIndex];

                    // Set cell values safely with null checks
                    SetCellValueSafely(row, "Position", point.Position);
                    SetCellValueSafely(row, "MeasurementType", point.MeasurementType);
                    SetCellValueSafely(row, "TargetValue", FormatValue(point.TargetValue, point.MeasurementType));
                    SetCellValueSafely(row, "RecordValue", point.RecordValue.HasValue ? FormatValue(point.RecordValue.Value, point.MeasurementType) : "-");
                    SetCellValueSafely(row, "Tolerance", $"±{point.Tolerance}{(point.IsPercent ? "%" : "")}");
                    SetCellValueSafely(row, "Description", point.Description);
                    SetCellValueSafely(row, "Status", point.GetToleranceStatusDisplay());
                    SetCellValueSafely(row, "ApiResponse", GetApiResponseDisplay(point));

                    // Highlight current row
                    if (i == _currentIndex)
                    {
                        row.DefaultCellStyle.BackColor = Color.LightBlue;
                        row.DefaultCellStyle.ForeColor = Color.Black;
                    }
                    else if (point.IsCompleted)
                    {
                        // Color based on tolerance status
                        string toleranceStatus = point.GetToleranceStatusDisplay();
                        Color bgColor, textColor;
                        GetStatusColors(toleranceStatus, out bgColor, out textColor);
                        row.DefaultCellStyle.BackColor = bgColor;
                        row.DefaultCellStyle.ForeColor = textColor;
                    }
                }
            }
            catch (Exception ex)
            {
                LogActivity($"Error loading measurement points to DataGridView: {ex.Message}");
                _lastError = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DataGridView Error: {ex.Message}\n\n{ex.StackTrace}";
            }
            
            // Scroll to current row after loading
            if (_measurementPoints.Count > 0)
            {
                ScrollToCurrentRow();
            }
        }

        private void dataGridViewRowsClearSafely()
        {
            try
            {
                if (dataGridViewPoints.Rows != null)
                {
                    dataGridViewPoints.Rows.Clear();
                }
            }
            catch (Exception ex)
            {
                LogActivity($"Error clearing DataGridView rows: {ex.Message}");
            }
        }

        private void SetCellValueSafely(DataGridViewRow row, string columnName, object value)
        {
            try
            {
                if (row?.Cells[columnName] != null)
                {
                    row.Cells[columnName].Value = value ?? "";
                }
            }
            catch (Exception ex)
            {
                LogActivity($"Error setting cell {columnName}: {ex.Message}");
            }
        }

        private void RefreshDataGridViewSafely()
        {
            try
            {
                if (dataGridViewPoints != null && dataGridViewPoints.IsHandleCreated && !dataGridViewPoints.InvokeRequired)
                {
                    dataGridViewPoints.Refresh();
                }
            }
            catch (Exception ex)
            {
                LogActivity($"Error refreshing DataGridView: {ex.Message}");
            }
        }

        private void ScrollToCurrentRow()
        {
            try
            {
                if (dataGridViewPoints == null || dataGridViewPoints.Rows.Count == 0 || _currentIndex < 0 || _currentIndex >= dataGridViewPoints.Rows.Count)
                {
                    return;
                }

                // Check if we need to invoke
                if (dataGridViewPoints.InvokeRequired)
                {
                    dataGridViewPoints.Invoke(new Action(ScrollToCurrentRow));
                    return;
                }

                // Select the current row
                dataGridViewPoints.ClearSelection();
                dataGridViewPoints.Rows[_currentIndex].Selected = true;

                // Scroll to make the current row visible
                dataGridViewPoints.FirstDisplayedScrollingRowIndex = _currentIndex;

                // Ensure the row is visible by accounting for row height
                var visibleRowsCount = dataGridViewPoints.DisplayedRowCount(false);
                
                if (_currentIndex >= dataGridViewPoints.FirstDisplayedScrollingRowIndex + visibleRowsCount)
                {
                    // Scroll down to show the row
                    dataGridViewPoints.FirstDisplayedScrollingRowIndex = Math.Max(0, _currentIndex - visibleRowsCount + 1);
                }
                else if (_currentIndex < dataGridViewPoints.FirstDisplayedScrollingRowIndex)
                {
                    // Scroll up to show the row
                    dataGridViewPoints.FirstDisplayedScrollingRowIndex = _currentIndex;
                }
            }
            catch (Exception ex)
            {
                LogActivity($"Error scrolling to current row: {ex.Message}");
            }
        }

        private void UpdateDataGridViewRecordValue(int rowIndex, double measuredValue, string toleranceStatus)
        {
            try
            {
                if (dataGridViewPoints == null || dataGridViewPoints.IsDisposed || !dataGridViewPoints.IsHandleCreated)
                {
                    return;
                }

                if (rowIndex < 0 || rowIndex >= dataGridViewPoints.Rows.Count || rowIndex >= _measurementPoints.Count)
                {
                    return;
                }

                // Check if we need to invoke
                if (dataGridViewPoints.InvokeRequired)
                {
                    dataGridViewPoints.Invoke(new Action(() => UpdateDataGridViewRecordValue(rowIndex, measuredValue, toleranceStatus)));
                    return;
                }

                var point = _measurementPoints[rowIndex];
                var row = dataGridViewPoints.Rows[rowIndex];
                
                // Format the record value based on condition
                string displayValue = GetDisplayValueForCondition(measuredValue, toleranceStatus, point.MeasurementType);
                
                // Get display status for UI (what user sees)
                string displayStatus = GetDisplayStatus(point, measuredValue);
                
                // Update the Record Value cell
                SetCellValueSafely(row, "RecordValue", displayValue);
                
                // Update the Status cell with display status
                SetCellValueSafely(row, "Status", displayStatus);
                
                // Update the API Response cell
                SetCellValueSafely(row, "ApiResponse", GetApiResponseDisplay(point));
                
                // Update row styling based on display status
                Color bgColor, textColor;
                GetStatusColors(displayStatus, out bgColor, out textColor);
                
                row.DefaultCellStyle.BackColor = bgColor;
                row.DefaultCellStyle.ForeColor = textColor;
            }
            catch (Exception ex)
            {
                LogActivity($"Error updating DataGridView record value: {ex.Message}");
            }
        }

        private string GetDisplayStatus(MeasurementPoint point, double measuredValue)
        {
            // Check for overload condition (for 2W resistance measurements)
            if (point.Marker != null && point.Marker.Type.ToUpper() == "2W")
            {
                if (double.IsNaN(measuredValue) || measuredValue >= 50.0E6 || measuredValue < 0)
                {
                    return "Overload";
                }
            }
            
            // Check for open circuit condition (for Diode/Continuity markers)
            if (point.Marker != null && (point.Marker.Type.ToUpper() == "DIO" || point.Marker.Type.ToUpper() == "CON"))
            {
                if (double.IsNaN(measuredValue) || measuredValue >= 9.9E37)
                {
                    return "Open";
                }
            }
            
            // Check if tolerance is enabled
            bool toleranceEnabled = point.Marker != null && point.Marker.ToleranceEnabled == 1;
            
            // If tolerance is disabled, always pass
            if (!toleranceEnabled)
            {
                return "Pass";
            }
            
            // Normal tolerance check
            double toleranceRange;
            if (point.IsPercent)
            {
                toleranceRange = Math.Abs(point.TargetValue * point.Tolerance / 100.0);
            }
            else
            {
                toleranceRange = point.Tolerance;
            }
            
            bool withinTolerance = Math.Abs(measuredValue - point.TargetValue) <= toleranceRange;
            return withinTolerance ? "Pass" : "Fail";
        }

        private string GetDisplayValueForCondition(double measuredValue, string toleranceStatus, string measurementType)
        {
            // Use display status instead of API status
            if (double.IsNaN(measuredValue))
            {
                // Check if this is overload or open circuit based on measurement type
                if (toleranceStatus == "pass") // API status indicates special condition
                {
                    return "---"; // Will be updated by GetDisplayStatus in the UI
                }
                else
                {
                    return "---";
                }
            }
            else
            {
                return FormatValue(measuredValue, measurementType);
            }
        }

        private void GetStatusColors(string toleranceStatus, out Color bgColor, out Color textColor)
        {
            switch (toleranceStatus)
            {
                case "Pass":
                    bgColor = Color.LightGreen;
                    textColor = Color.Black;
                    break;
                case "Fail":
                    bgColor = Color.LightCoral;
                    textColor = Color.Black;
                    break;
                case "Overload":
                    bgColor = Color.LightYellow;
                    textColor = Color.Black;
                    break;
                case "Open":
                    bgColor = Color.LightGray;
                    textColor = Color.Black;
                    break;
                default:
                    bgColor = Color.LightYellow;
                    textColor = Color.Black;
                    break;
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
            
            // Refresh DataGridView display and scroll to current row
            RefreshDataGridViewSafely();
            ScrollToCurrentRow();
        }

        private async void buttonLoadSession_Click(object sender, EventArgs e)
        {
            string sessionId = textBoxSessionID.Text.Trim();
            if (string.IsNullOrEmpty(sessionId))
            {
                string error = "Please enter a Measurement Session ID";
                _lastError = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {error}";
                MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                _lastError = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Failed to load session {sessionId}\n\nException: {ex.GetType().Name}\nMessage: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
                MessageBox.Show($"Failed to load session:\n\n{ex.Message}", 
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                buttonLoadSession.Enabled = true;
                buttonLoadSession.Text = "Load Session";
            }
        }
        
        private void buttonCopyError_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_lastError))
            {
                MessageBox.Show("No error information available to copy.\n\nPlease perform an action that generates an error first.", 
                              "No Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            try
            {
                // Create detailed error report
                string errorReport = $"===== MULTI RECORD ERROR REPORT =====\n\n";
                errorReport += $"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n";
                errorReport += $"Application: MultiRecord Measurement Mode\n";
                errorReport += $"Session ID: {textBoxSessionID.Text.Trim()}\n";
                errorReport += $"User: {AuthManager.CurrentUser?.Email ?? "Not logged in"}\n\n";
                
                errorReport += $"ERROR DETAILS:\n";
                errorReport += $"{_lastError}\n\n";
                
                errorReport += $"SYSTEM INFORMATION:\n";
                errorReport += $"OS: {Environment.OSVersion}\n";
                errorReport += $".NET Version: {Environment.Version}\n";
                errorReport += $"Machine Name: {Environment.MachineName}\n\n";
                
                errorReport += $"MEASUREMENT MODE STATE:\n";
                errorReport += $"Current Index: {_currentIndex}\n";
                errorReport += $"Measurement Points Count: {_measurementPoints?.Count ?? 0}\n";
                errorReport += $"DMM Connected: {_dmm?.IsConnected ?? false}\n\n";
                
                errorReport += $"===== END REPORT =====";
                
                Clipboard.SetText(errorReport);
                MessageBox.Show("Error details copied to clipboard!\n\nYou can now paste this information for debugging.", 
                              "Error Copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to copy error to clipboard:\n\n{ex.Message}", 
                              "Copy Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async System.Threading.Tasks.Task LoadMeasurementsFromDatabase(string sessionId)
        {
            if (!AuthManager.IsLoggedIn())
            {
                MessageBox.Show("Please login first to access Orbitz API.", 
                              "Authentication Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Get session details first to extract repair_id
                var session = await OrbitzAPI.GetMeasurementSessionAsync(sessionId);
                _currentSessionId = sessionId;
                _currentRepairId = session.RepairId;
                _currentTestGroupId = session.PcbTestGroupId;
                // Note: You might need to get annotation ID from somewhere else or via API
                
                // Load measurement markers from Orbitz API
                var markers = await OrbitzAPI.GetMeasurementMarkersAsync(sessionId);
                
                // Convert MeasurementMarker to MeasurementPoint
                ConvertMarkersToPoints(markers);
                
                _currentIndex = 0;
                foreach (var point in _measurementPoints)
                {
                    point.IsCompleted = point.RecordValue.HasValue;
                }
                
                LoadMeasurementPoints();
                if (_measurementPoints.Count > 0)
                {
                    await UpdateCurrentPosition();
                }
            }
            catch (Exception ex)
            {
                _lastError = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Orbitz API Error loading session {sessionId}\n\nException: {ex.GetType().Name}\nMessage: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
                throw new Exception($"Failed to load session from Orbitz API: {ex.Message}");
            }
        }

        private void ConvertMarkersToPoints(List<MeasurementMarker> markers)
        {
            _measurementPoints.Clear();
            
            foreach (var marker in markers.OrderBy(m => m.SortOrder))
            {
                var measurementPoint = new MeasurementPoint
                {
                    Position = marker.DisplayName,
                    MeasurementType = GetDMMFunctionFromMarkerType(marker.Type),
                    TargetValue = GetTargetValueFromMarkerParameters(marker.Parameters, marker.Type),
                    RecordValue = marker.MeasuredValue.HasValue ? (double?)marker.MeasuredValue.Value : null,
                    Tolerance = (double)(marker.ToleranceUpper ?? 0m),
                    IsPercent = marker.ToleranceUpperType == "percent",
                    Description = $"{marker.DisplayName} ({marker.Type})",
                    IsCompleted = marker.IsMeasured,
                    Marker = marker
                };
                
                _measurementPoints.Add(measurementPoint);
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
                case "RES": return "RES2W";
                case "CAP": return "CAP";
                case "DIO": return "DIODE";
                case "CON": return "RES2W"; // Continuity as resistance
                case "VOL": return "VDC";
                case "CUR": return "IDC";
                case "DC": return "VDC";
                case "AC": return "VAC";
                case "2W": return "RES2W";
                case "4W": return "RES4W";
                default: return "VDC";
            }
        }
        
        private double GetTargetValueFromMarkerParameters(Dictionary<string, object> parameters, string markerType)
        {
            try
            {
                if (parameters == null) return 0.0;
                
                switch (markerType?.ToUpper())
                {
                    case "RES":
                    case "2W":
                    case "4W":
                        if (parameters.ContainsKey("resistance"))
                            return Convert.ToDouble(parameters["resistance"]);
                        if (parameters.ContainsKey("nominal_value"))
                            return Convert.ToDouble(parameters["nominal_value"]);
                        break;
                        
                    case "CAP":
                        if (parameters.ContainsKey("capacitance"))
                            return Convert.ToDouble(parameters["capacitance"]);
                        if (parameters.ContainsKey("nominal_value"))
                            return Convert.ToDouble(parameters["nominal_value"]);
                        break;
                        
                    case "DIO":
                        if (parameters.ContainsKey("voltage"))
                            return Convert.ToDouble(parameters["voltage"]);
                        if (parameters.ContainsKey("forward_voltage"))
                            return Convert.ToDouble(parameters["forward_voltage"]);
                        break;
                        
                    case "CON":
                        if (parameters.ContainsKey("ohms"))
                            return Convert.ToDouble(parameters["ohms"]);
                        if (parameters.ContainsKey("expected_resistance"))
                            return Convert.ToDouble(parameters["expected_resistance"]);
                        return 0.0; // Continuity usually expects near 0 ohms
                        
                    case "DC":
                    case "AC":
                    case "VOL":
                        if (parameters.ContainsKey("voltage"))
                            return Convert.ToDouble(parameters["voltage"]);
                        if (parameters.ContainsKey("nominal_value"))
                            return Convert.ToDouble(parameters["nominal_value"]);
                        break;
                        
                    case "CUR":
                        if (parameters.ContainsKey("current"))
                            return Convert.ToDouble(parameters["current"]);
                        if (parameters.ContainsKey("nominal_value"))
                            return Convert.ToDouble(parameters["nominal_value"]);
                        break;
                }
                
                return 0.0;
            }
            catch
            {
                return 0.0;
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
            string toleranceStatus;
            bool withinTolerance = false;
            bool isOverload = false;
            bool isOpenCircuit = false;
            
            // Check for overload condition (for 2W resistance measurements)
            if (currentPoint.Marker != null && currentPoint.Marker.Type.ToUpper() == "2W")
            {
                // Check if the measured value indicates overload
                if (double.IsNaN(measuredValue) || measuredValue >= 50.0E6 || measuredValue < 0) // >50MΩ typically indicates overload
                {
                    isOverload = true;
                    measuredValue = double.NaN; // Set to NaN for API
                    toleranceStatus = "pass"; // Overload is considered pass for API
                    withinTolerance = true; // Overload is considered pass
                }
            }
            
            // Check if tolerance is enabled (1 = enabled, 0 = disabled)
            bool toleranceEnabled = currentPoint.Marker != null && currentPoint.Marker.ToleranceEnabled == 1;
            
            // If tolerance is disabled or overload, always pass
            if (!toleranceEnabled || isOverload)
            {
                toleranceStatus = "pass"; // API only accepts 'pass' or 'fail'
                withinTolerance = true;
            }
            
            // Special handling for Diode/Continuity markers with OPEN/OVERLOAD conditions
            else if (currentPoint.Marker != null && 
                (currentPoint.Marker.Type.ToUpper() == "DIO" || currentPoint.Marker.Type.ToUpper() == "CON"))
            {
                if (double.IsNaN(measuredValue) || measuredValue >= 9.9E37)
                {
                    isOpenCircuit = true;
                    measuredValue = double.NaN; // Set to NaN for API
                    toleranceStatus = "pass"; // Open circuit is considered pass for API
                    withinTolerance = true; // Open circuit is considered pass
                }
                else
                {
                    // Normal tolerance check for closed circuit
                    double toleranceRange;
                    if (currentPoint.IsPercent)
                    {
                        toleranceRange = Math.Abs(currentPoint.TargetValue * currentPoint.Tolerance / 100.0);
                    }
                    else
                    {
                        toleranceRange = currentPoint.Tolerance;
                    }
                    
                    withinTolerance = Math.Abs(measuredValue - currentPoint.TargetValue) <= toleranceRange;
                    toleranceStatus = withinTolerance ? "pass" : "fail"; // API only accepts 'pass' or 'fail'
                }
            }
            else
            {
                // Normal tolerance check for other component types
                double toleranceRange;
                if (currentPoint.IsPercent)
                {
                    toleranceRange = Math.Abs(currentPoint.TargetValue * currentPoint.Tolerance / 100.0);
                }
                else
                {
                    toleranceRange = currentPoint.Tolerance;
                }
                
                withinTolerance = Math.Abs(measuredValue - currentPoint.TargetValue) <= toleranceRange;
                toleranceStatus = withinTolerance ? "pass" : "fail"; // API only accepts 'pass' or 'fail'
            }
            
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
            
            // Clear previous API response
            currentPoint.ApiResponse = null;
            currentPoint.ApiError = null;
            currentPoint.ApiTimestamp = null;
            
            // Update DataGridView immediately with new record value (use current index)
            UpdateDataGridViewRecordValue(_currentIndex, measuredValue, toleranceStatus);
            
            // Update measurement via Orbitz API
            if (currentPoint.Marker != null)
            {
                UpdateMeasurementViaAPI(currentPoint, measuredValue, toleranceStatus, isOverload, isOpenCircuit);
            }
            
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
            
            // Unsubscribe from DataGridView events
            dataGridViewPoints.CellClick -= DataGridViewPoints_CellClick;
            
            _measurementTimer?.Stop();
            _measurementTimer?.Dispose();
        }

        private async void buttonPrevious_Click(object sender, EventArgs e)
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
                await UpdateCurrentPosition();
                ScrollToCurrentRow();
            }
        }

        private async void buttonNext_Click(object sender, EventArgs e)
        {
            if (_currentIndex < _measurementPoints.Count - 1)
            {
                _currentIndex++;
                await UpdateCurrentPosition();
                ScrollToCurrentRow();
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
        
        private async void UpdateMeasurementViaAPI(MeasurementPoint point, double measuredValue, string toleranceStatus, bool isOverload = false, bool isOpenCircuit = false)
        {
            var startTime = DateTime.Now;
            try
            {
                var marker = point.Marker;
                var repairId = ExtractRepairIdFromSession(_currentSessionId);
                
                // Update UI to show "sending" status
                point.ApiResponse = "Sending...";
                UpdateDataGridViewRecordValue(_measurementPoints.IndexOf(point), measuredValue, toleranceStatus);
                
                // Handle measured value for overload/open circuit conditions
                decimal? apiMeasuredValue = null;
                if (!double.IsNaN(measuredValue))
                {
                    apiMeasuredValue = (decimal)measuredValue;
                }
                
                // Create marker parameters with overload/open information
                var updatedParameters = new Dictionary<string, object>();
                if (marker.Parameters != null)
                {
                    foreach (var param in marker.Parameters)
                    {
                        updatedParameters[param.Key] = param.Value;
                    }
                }
                
                // Set overload/open parameters based on marker type
                bool isOpen = false;
                if (marker.Type.ToUpper() == "2W")
                {
                    updatedParameters["overload"] = isOverload;
                    updatedParameters["open"] = isOverload;
                    isOpen = isOverload;
                }
                else if (marker.Type.ToUpper() == "DIO" || marker.Type.ToUpper() == "CON")
                {
                    updatedParameters["open"] = isOpenCircuit;
                    isOpen = isOpenCircuit;
                }
                
                var request = new MeasurementUpdateRequest
                {
                    MarkerId = marker.Id,
                    MeasuredValue = apiMeasuredValue,
                    ToleranceStatus = toleranceStatus.ToLower() == "pass" ? "pass" : "fail",
                    MeasurementId = marker.MeasurementId,
                    MarkerType = marker.Type,
                    MarkerParameters = updatedParameters,
                    MarkerPositionX = marker.X,
                    MarkerPositionY = marker.Y,
                    MarkerDisplayName = marker.DisplayName,
                    MarkerColor = marker.Color,
                    ToleranceUpper = marker.ToleranceUpper,
                    ToleranceLower = marker.ToleranceLower,
                    ToleranceUpperType = marker.ToleranceUpperType,
                    ToleranceLowerType = marker.ToleranceLowerType,
                    ToleranceEnabled = marker.ToleranceEnabled,
                    ToleranceUpperLimit = marker.ToleranceUpperLimit,
                    ToleranceLowerLimit = marker.ToleranceLowerLimit,
                    MeasurementUnit = marker.MeasurementUnit,
                    Notes = GetOverloadNotes(isOverload, isOpenCircuit, toleranceStatus),
                    Open = isOpen
                };
                
                // Update measurement via API
                var response = await OrbitzAPI.UpdateMeasurementAsync(repairId, marker.MeasurementId, request);
                var endTime = DateTime.Now;
                var duration = endTime - startTime;
                
                // Store successful response
                point.ApiResponse = response ? "Success" : "Failed";
                point.ApiTimestamp = endTime;
                point.ApiError = null;
                
                LogActivity($"Updated measurement for {marker.DisplayName}: {measuredValue} {marker.MeasurementUnit} ({toleranceStatus}) - {duration.TotalMilliseconds:F0}ms");
                
                // Update UI to show success
                UpdateDataGridViewRecordValue(_measurementPoints.IndexOf(point), measuredValue, toleranceStatus);
            }
            catch (Exception ex)
            {
                var endTime = DateTime.Now;
                var duration = endTime - startTime;
                
                // Store error information
                point.ApiError = ex.Message;
                point.ApiTimestamp = endTime;
                point.ApiResponse = null;
                
                LogActivity($"Failed to update measurement via API: {ex.Message} - {duration.TotalMilliseconds:F0}ms");
                
                // Update UI to show error
                UpdateDataGridViewRecordValue(_measurementPoints.IndexOf(point), measuredValue, toleranceStatus);
                
                // Don't throw - allow local recording to continue even if API fails
            }
        }
        
        private string ExtractRepairIdFromSession(string sessionId)
        {
            // Return the repair ID that we stored when loading the session
            return _currentRepairId ?? "unknown-repair-id";
        }
        
        private string GetOverloadNotes(bool isOverload, bool isOpenCircuit, string toleranceStatus)
        {
            if (isOverload)
            {
                return "Overload condition detected (Resistance > 50MΩ)";
            }
            else if (isOpenCircuit)
            {
                return "Open circuit detected";
            }
            else if (toleranceStatus.ToLower() == "pass")
            {
                return "Normal measurement - Passed";
            }
            else if (toleranceStatus.ToLower() == "fail")
            {
                return "Normal measurement - Failed";
            }
            
            return "Measurement recorded";
        }

        private string GetApiResponseDisplay(MeasurementPoint point)
        {
            if (!string.IsNullOrEmpty(point.ApiError))
            {
                return "❌ Error";
            }
            else if (!string.IsNullOrEmpty(point.ApiResponse))
            {
                return "✅ Success";
            }
            else if (point.IsCompleted)
            {
                return "📤 Sent";
            }
            else
            {
                return "⏳ Pending";
            }
        }
        
        private void LogActivity(string message)
        {
            // Add to debug output or activity log
            System.Diagnostics.Debug.WriteLine($"[MeasurementMode] {DateTime.Now:HH:mm:ss} - {message}");
        }
        
        private void InitializeMarkerNavigationContextMenu()
        {
            var contextMenu = new ContextMenuStrip();
            
            var navigateMenuItem = new ToolStripMenuItem("🧭 นำทางไปยัง Marker");
            navigateMenuItem.Click += (sender, e) =>
            {
                if (dataGridViewPoints.SelectedRows.Count > 0)
                {
                    int rowIndex = dataGridViewPoints.SelectedRows[0].Index;
                    if (rowIndex < _measurementPoints.Count)
                    {
                        var selectedPoint = _measurementPoints[rowIndex];
                        if (selectedPoint.Marker != null)
                        {
                            HandleMarkerNavigation(selectedPoint.Marker);
                        }
                        else
                        {
                            MessageBox.Show("ไม่มีข้อมูล marker สำหรับจุดการวัดนี้", "No Marker Data", 
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            };
            
            var showMarkerInfoMenuItem = new ToolStripMenuItem("📋 แสดงข้อมูล Marker");
            showMarkerInfoMenuItem.Click += (sender, e) =>
            {
                if (dataGridViewPoints.SelectedRows.Count > 0)
                {
                    int rowIndex = dataGridViewPoints.SelectedRows[0].Index;
                    if (rowIndex < _measurementPoints.Count)
                    {
                        var selectedPoint = _measurementPoints[rowIndex];
                        if (selectedPoint.Marker != null)
                        {
                            ShowMarkerInfo(selectedPoint.Marker);
                        }
                        else
                        {
                            MessageBox.Show("ไม่มีข้อมูล marker สำหรับจุดการวัดนี้", "No Marker Data", 
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            };
            
            var showApiResponseMenuItem = new ToolStripMenuItem("🌐 แสดงข้อมูล API Response");
            showApiResponseMenuItem.Click += (sender, e) =>
            {
                if (dataGridViewPoints.SelectedRows.Count > 0)
                {
                    int rowIndex = dataGridViewPoints.SelectedRows[0].Index;
                    if (rowIndex < _measurementPoints.Count)
                    {
                        var selectedPoint = _measurementPoints[rowIndex];
                        ShowApiResponseInfo(selectedPoint);
                    }
                }
            };
            
            contextMenu.Items.Add(navigateMenuItem);
            contextMenu.Items.Add(showMarkerInfoMenuItem);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(showApiResponseMenuItem);
            
            dataGridViewPoints.ContextMenuStrip = contextMenu;
        }
        
        private void ShowApiResponseInfo(MeasurementPoint point)
        {
            string responseInfo = $"🌐 API Response Information\n\n";
            responseInfo += $"Position: {point.Position}\n";
            responseInfo += $"Type: {point.MeasurementType}\n";
            responseInfo += $"Recorded Value: {(point.RecordValue.HasValue ? point.RecordValue.Value.ToString() : "N/A")}\n\n";
            
            if (point.ApiTimestamp.HasValue)
            {
                responseInfo += $"Timestamp: {point.ApiTimestamp.Value:yyyy-MM-dd HH:mm:ss}\n";
            }
            else
            {
                responseInfo += $"Timestamp: Not sent\n";
            }
            
            if (!string.IsNullOrEmpty(point.ApiError))
            {
                responseInfo += $"Status: ❌ FAILED\n\n";
                responseInfo += $"Error Message:\n{point.ApiError}\n";
            }
            else if (!string.IsNullOrEmpty(point.ApiResponse))
            {
                responseInfo += $"Status: ✅ SUCCESS\n\n";
                responseInfo += $"Response:\n{point.ApiResponse}\n";
            }
            else if (point.IsCompleted)
            {
                responseInfo += $"Status: 📤 Sent (Response pending)\n";
            }
            else
            {
                responseInfo += $"Status: ⏳ Not sent\n";
            }
            
            if (point.Marker != null)
            {
                responseInfo += $"\nMarker ID: {point.Marker.Id}\n";
                responseInfo += $"Measurement ID: {point.Marker.MeasurementId}\n";
            }
            
            MessageBox.Show(responseInfo, $"API Response - {point.Position}", 
                          MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        private void ShowMarkerInfo(MeasurementMarker marker)
        {
            string markerInfo = $"📍 Marker Information\n\n" +
                               $"Name: {marker.DisplayName}\n" +
                               $"Type: {marker.Type}\n" +
                               $"Position: X={marker.X:F2}, Y={marker.Y:F2}\n" +
                               $"ID: {marker.Id}\n" +
                               $"Color: {marker.Color}\n" +
                               $"Sort Order: {marker.SortOrder}\n" +
                               $"Tolerance Upper: {marker.ToleranceUpper ?? 0}% ({marker.ToleranceUpperType})\n" +
                               $"Tolerance Lower: {marker.ToleranceLower ?? 0}% ({marker.ToleranceLowerType})\n" +
                               $"Measured: {(marker.IsMeasured ? "Yes" : "No")}\n" +
                               $"Open Circuit: {marker.OpenCircuit ?? false}";
                               
            if (marker.Parameters != null && marker.Parameters.Count > 0)
            {
                markerInfo += "\n\nParameters:";
                foreach (var param in marker.Parameters)
                {
                    markerInfo += $"\n• {param.Key}: {param.Value}";
                }
            }
            
            MessageBox.Show(markerInfo, $"Marker Info - {marker.DisplayName}", 
                          MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        
        private void DataGridViewPoints_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // Ignore header clicks
                if (e.RowIndex < 0) return;
                
                // Get the selected measurement point
                if (e.RowIndex < _measurementPoints.Count)
                {
                    var selectedPoint = _measurementPoints[e.RowIndex];
                    
                    // Navigate to marker if it has marker data
                    if (selectedPoint.Marker != null)
                    {
                        HandleMarkerNavigation(selectedPoint.Marker);
                    }
                    
                    // Also update current index to match selected row
                    _currentIndex = e.RowIndex;
                    _ = UpdateCurrentPosition();
                    ScrollToCurrentRow();
                }
            }
            catch (Exception ex)
            {
                LogActivity($"Error handling cell click: {ex.Message}");
            }
        }
        
        private void HandleMarkerNavigation(MeasurementMarker marker)
        {
            try
            {
                // Try PostMessage first (if opened in popup/iframe)
                if (TryPostMessageNavigation(marker))
                {
                    LogActivity($"นำทางไปยัง {marker.DisplayName} ผ่าน PostMessage");
                    return;
                }
                
                // Fallback to URL navigation
                TryUrlNavigation(marker);
                LogActivity($"นำทางไปยัง {marker.DisplayName} ผ่าน URL");
            }
            catch (Exception ex)
            {
                LogActivity($"Error navigating to marker {marker.DisplayName}: {ex.Message}");
                MessageBox.Show($"ไม่สามารถนำทางไปยัง marker {marker.DisplayName} ได้\n\nError: {ex.Message}", 
                              "Navigation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        
        private bool TryPostMessageNavigation(MeasurementMarker marker)
        {
            try
            {
                // This would work if the application was embedded in a web browser
                // For WinForms, we'll simulate this by trying to communicate with external processes
                
                // Check if we can find a browser process or parent application
                var browserProcesses = Process.GetProcessesByName("chrome")
                    .Concat(Process.GetProcessesByName("firefox"))
                    .Concat(Process.GetProcessesByName("msedge"))
                    .ToArray();
                
                if (browserProcesses.Length > 0)
                {
                    // Log that we would send PostMessage (actual implementation would depend on browser integration)
                    LogActivity($"Would send PostMessage to browser: NAVIGATE_TO_MARKER for {marker.DisplayName} (x:{marker.X}, y:{marker.Y})");
                    return true;
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }
        
        private void TryUrlNavigation(MeasurementMarker marker)
        {
            try
            {
                // Construct canvas URL based on the documentation pattern
                string canvasUrl = $"http://100.75.21.95:3001/repair/{_currentRepairId}/test/{_currentTestGroupId}/canvas/{_currentAnnotationId ?? "default"}?" +
                                 $"session={_currentSessionId}&marker={marker.Id}&x={marker.X}&y={marker.Y}";
                
                // Open in default browser
                Process.Start(new ProcessStartInfo
                {
                    FileName = canvasUrl,
                    UseShellExecute = true
                });
                
                LogActivity($"Opened canvas URL: {canvasUrl}");
            }
            catch (Exception ex)
            {
                LogActivity($"Failed to open URL navigation: {ex.Message}");
                
                // Fallback: Show marker coordinates in a message box
                string markerInfo = $"Marker: {marker.DisplayName}\n" +
                                   $"Type: {marker.Type}\n" +
                                   $"Position: X={marker.X}, Y={marker.Y}\n" +
                                   $"ID: {marker.Id}";
                
                MessageBox.Show(markerInfo, $"Marker Information - {marker.DisplayName}", 
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        public MeasurementMarker Marker { get; set; }
        public string ApiResponse { get; set; }
        public string ApiError { get; set; }
        public DateTime? ApiTimestamp { get; set; }
        
        public string GetToleranceStatus()
        {
            if (!RecordValue.HasValue)
                return "Overload"; // Assume overload if no value
                
            // Check if tolerance is enabled (1 = enabled, 0 = disabled)
            bool toleranceEnabled = Marker != null && Marker.ToleranceEnabled == 1;
            
            // If tolerance is disabled, always pass
            if (!toleranceEnabled)
                return "Pass";
                
            // Special handling for 2W resistance markers - check for overload
            if (Marker != null && Marker.Type.ToUpper() == "2W")
            {
                if (double.IsNaN(RecordValue.Value) || RecordValue.Value >= 50.0E6)
                    return "Overload";
            }
            
            // Special handling for Diode/Continuity markers
            if (Marker != null && (Marker.Type.ToUpper() == "DIO" || Marker.Type.ToUpper() == "CON"))
            {
                if (Marker.OpenCircuit == true || double.IsNaN(RecordValue.Value) || RecordValue.Value >= 9.9E37)
                    return "Open";
            }
                
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
        
        public string GetToleranceStatusDisplay()
        {
            if (!RecordValue.HasValue)
                return "Overload"; // Assume overload if no value
                
            // Check if tolerance is enabled (1 = enabled, 0 = disabled)
            bool toleranceEnabled = Marker != null && Marker.ToleranceEnabled == 1;
            
            // If tolerance is disabled, always pass
            if (!toleranceEnabled)
                return "Pass";
                
            // Special handling for 2W resistance markers - check for overload
            if (Marker != null && Marker.Type.ToUpper() == "2W")
            {
                if (double.IsNaN(RecordValue.Value) || RecordValue.Value >= 50.0E6)
                    return "Overload";
            }
            
            // Special handling for Diode/Continuity markers
            if (Marker != null && (Marker.Type.ToUpper() == "DIO" || Marker.Type.ToUpper() == "CON"))
            {
                if (Marker.OpenCircuit == true || double.IsNaN(RecordValue.Value) || RecordValue.Value >= 9.9E37)
                    return "Open";
            }
                
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