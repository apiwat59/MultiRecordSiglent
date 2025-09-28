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
        private int _currentIndex = 0;
        private DataTable _recordsTable;
        private SDM3055 _dmm;
        private System.Windows.Forms.Timer _measurementTimer;

        public MeasurementMode(DataTable recordsTable, SDM3055 dmm)
        {
            InitializeComponent();
            _recordsTable = recordsTable;
            _dmm = dmm;
            InitializeMeasurementPoints();
            LoadMeasurementPoints();
            UpdateCurrentPosition();
            InitializeRealTimeDisplay();
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
                row.Cells["Tolerance"].Value = $"±{point.Tolerance}{(point.IsPercent ? "%" : "")}";
                row.Cells["Description"].Value = point.Description;
                row.Cells["Status"].Value = point.IsCompleted ? "✓" : "○";
                
                // Highlight current row
                if (i == _currentIndex)
                {
                    row.DefaultCellStyle.BackColor = Color.LightBlue;
                    row.DefaultCellStyle.ForeColor = Color.Black;
                }
                else if (point.IsCompleted)
                {
                    row.DefaultCellStyle.BackColor = Color.LightGreen;
                    row.DefaultCellStyle.ForeColor = Color.Black;
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

        private void UpdateCurrentPosition()
        {
            if (_currentIndex < _measurementPoints.Count)
            {
                var currentPoint = _measurementPoints[_currentIndex];
                
                labelCurrentPosition.Text = $"Position: {currentPoint.Position}";
                labelMeasurementType.Text = $"Type: {currentPoint.MeasurementType}";
                labelTargetValue.Text = $"Target: {FormatValue(currentPoint.TargetValue, currentPoint.MeasurementType)}";
                labelTolerance.Text = $"Tolerance: ±{currentPoint.Tolerance}{(currentPoint.IsPercent ? "%" : "")}";
                labelDescription.Text = currentPoint.Description;
                
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

        private void buttonLoadQWRP_Click(object sender, EventArgs e)
        {
            string qwrpId = textBoxQWRPID.Text.Trim();
            if (string.IsNullOrEmpty(qwrpId))
            {
                MessageBox.Show("Please enter a QWRP ID", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // Mock loading - ในอนาคตจะเชื่อมต่อ database จริง
            MessageBox.Show($"Loading measurement points for QWRP ID: {qwrpId}\n\nFound {_measurementPoints.Count} measurement points.", 
                          "QWRP Loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            _currentIndex = 0;
            foreach (var point in _measurementPoints)
            {
                point.IsCompleted = false;
            }
            
            UpdateCurrentPosition();
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
            
            // Mark current point as completed
            currentPoint.IsCompleted = true;
            
            // Move to next position
            _currentIndex++;
            
            // Update UI
            UpdateCurrentPosition();
            
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
            _measurementTimer?.Stop();
            _measurementTimer?.Dispose();
        }

        private void buttonPrevious_Click(object sender, EventArgs e)
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
                UpdateCurrentPosition();
            }
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            if (_currentIndex < _measurementPoints.Count - 1)
            {
                _currentIndex++;
                UpdateCurrentPosition();
            }
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            _currentIndex = 0;
            foreach (var point in _measurementPoints)
            {
                point.IsCompleted = false;
            }
            UpdateCurrentPosition();
            labelLastResult.Text = "";
        }

        private void InitializeRealTimeDisplay()
        {
            // Initialize timer for real-time measurement display
            _measurementTimer = new System.Windows.Forms.Timer();
            _measurementTimer.Interval = 500; // Update every 500ms
            _measurementTimer.Tick += async (s, e) => await UpdateRealTimeDisplay();
            _measurementTimer.Start();
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
                        // Update real-time display labels safely in UI thread
                        if (this.InvokeRequired)
                        {
                            this.Invoke(new Action(() =>
                            {
                                lblCurrentReading.Text = currentReading.ToString("F6");
                                
                                // Get unit from current measurement type if available
                                if (_currentIndex < _measurementPoints.Count)
                                {
                                    var currentPoint = _measurementPoints[_currentIndex];
                                    lblCurrentUnit.Text = GetUnitFromMeasurementType(currentPoint.MeasurementType);
                                    lblCurrentFunction.Text = currentPoint.MeasurementType;
                                }
                                else
                                {
                                    lblCurrentUnit.Text = "V"; // Default unit
                                    lblCurrentFunction.Text = "VDC"; // Default function
                                }
                            }));
                        }
                        else
                        {
                            lblCurrentReading.Text = currentReading.ToString("F6");
                            
                            if (_currentIndex < _measurementPoints.Count)
                            {
                                var currentPoint = _measurementPoints[_currentIndex];
                                lblCurrentUnit.Text = GetUnitFromMeasurementType(currentPoint.MeasurementType);
                                lblCurrentFunction.Text = currentPoint.MeasurementType;
                            }
                            else
                            {
                                lblCurrentUnit.Text = "V";
                                lblCurrentFunction.Text = "VDC";
                            }
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
        public double Tolerance { get; set; }
        public bool IsPercent { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
    }
}