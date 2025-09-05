using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MultiRecord
{
    public partial class PCBAnnotationForm : Form
    {
        private PictureBox pictureBoxPCB;
        private DataGridView dataGridViewMeasurements;
        private Button buttonLoadPCB;
        private Button buttonSaveAnnotations;
        private Button buttonClearMarkers;
        private Label labelInstructions;
        private Panel panelControls;
        private TrackBar trackBarZoom;
        private Label labelZoom;
        
        private Image pcbImage;
        private float zoomFactor = 1.0f;
        private List<PCBMarker> markers;
        private DataTable measurementData;
        private int selectedMeasurementIndex = -1;
        
        private readonly string annotationsFilePath;
        
        public PCBAnnotationForm(DataTable mainDataTable)
        {
            InitializeComponent();
            measurementData = mainDataTable;
            markers = new List<PCBMarker>();
            
            string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MultiRecordApp");
            annotationsFilePath = Path.Combine(appDataFolder, "pcb_annotations.txt");
            
            this.Load += PCBAnnotationForm_Load;
            LoadAnnotations();
        }
        
        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Form settings
            this.Text = "PCB Annotation";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(800, 600);
            
            // Create main layout panels
            var mainPanel = new TableLayoutPanel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.ColumnCount = 2;
            mainPanel.RowCount = 1;
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            
            // Left panel for PCB image
            var leftPanel = new Panel();
            leftPanel.Dock = DockStyle.Fill;
            leftPanel.BorderStyle = BorderStyle.FixedSingle;
            
            // Controls panel
            panelControls = new Panel();
            panelControls.Height = 80;
            panelControls.Dock = DockStyle.Top;
            panelControls.BackColor = Color.LightGray;
            
            // Load PCB button
            buttonLoadPCB = new Button();
            buttonLoadPCB.Text = "Load PCB Image";
            buttonLoadPCB.Size = new Size(120, 30);
            buttonLoadPCB.Location = new Point(10, 10);
            buttonLoadPCB.Click += ButtonLoadPCB_Click;
            
            // Save annotations button
            buttonSaveAnnotations = new Button();
            buttonSaveAnnotations.Text = "Save Annotations";
            buttonSaveAnnotations.Size = new Size(120, 30);
            buttonSaveAnnotations.Location = new Point(140, 10);
            buttonSaveAnnotations.Click += ButtonSaveAnnotations_Click;
            
            // Clear markers button
            buttonClearMarkers = new Button();
            buttonClearMarkers.Text = "Clear Markers";
            buttonClearMarkers.Size = new Size(100, 30);
            buttonClearMarkers.Location = new Point(270, 10);
            buttonClearMarkers.Click += ButtonClearMarkers_Click;
            
            // Zoom controls
            labelZoom = new Label();
            labelZoom.Text = "Zoom: 100%";
            labelZoom.Location = new Point(10, 45);
            labelZoom.AutoSize = true;
            
            trackBarZoom = new TrackBar();
            trackBarZoom.Minimum = 25;
            trackBarZoom.Maximum = 500;
            trackBarZoom.Value = 100;
            trackBarZoom.TickFrequency = 25;
            trackBarZoom.Location = new Point(80, 45);
            trackBarZoom.Size = new Size(200, 30);
            trackBarZoom.ValueChanged += TrackBarZoom_ValueChanged;
            
            // Instructions label
            labelInstructions = new Label();
            labelInstructions.Text = "1. Load PCB image\n2. Select measurement from right panel\n3. Click on PCB to assign marker";
            labelInstructions.Location = new Point(300, 45);
            labelInstructions.AutoSize = true;
            labelInstructions.ForeColor = Color.DarkBlue;
            
            panelControls.Controls.AddRange(new Control[] { 
                buttonLoadPCB, buttonSaveAnnotations, buttonClearMarkers, 
                labelZoom, trackBarZoom, labelInstructions 
            });
            
            // PCB Picture Box
            pictureBoxPCB = new PictureBox();
            pictureBoxPCB.Dock = DockStyle.Fill;
            pictureBoxPCB.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxPCB.BackColor = Color.White;
            pictureBoxPCB.BorderStyle = BorderStyle.FixedSingle;
            pictureBoxPCB.MouseClick += PictureBoxPCB_MouseClick;
            pictureBoxPCB.Paint += PictureBoxPCB_Paint;
            
            leftPanel.Controls.Add(pictureBoxPCB);
            leftPanel.Controls.Add(panelControls);
            
            // Right panel for measurements list
            var rightPanel = new Panel();
            rightPanel.Dock = DockStyle.Fill;
            rightPanel.Padding = new Padding(5);
            
            var rightLabel = new Label();
            rightLabel.Text = "Measurements Data";
            rightLabel.Font = new Font("Arial", 10, FontStyle.Bold);
            rightLabel.Dock = DockStyle.Top;
            rightLabel.Height = 25;
            
            dataGridViewMeasurements = new DataGridView();
            dataGridViewMeasurements.Dock = DockStyle.Fill;
            dataGridViewMeasurements.ReadOnly = true;
            dataGridViewMeasurements.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewMeasurements.MultiSelect = false;
            dataGridViewMeasurements.AllowUserToAddRows = false;
            dataGridViewMeasurements.AllowUserToDeleteRows = false;
            dataGridViewMeasurements.SelectionChanged += DataGridViewMeasurements_SelectionChanged;
            
            rightPanel.Controls.Add(dataGridViewMeasurements);
            rightPanel.Controls.Add(rightLabel);
            
            // Add panels to main layout
            mainPanel.Controls.Add(leftPanel, 0, 0);
            mainPanel.Controls.Add(rightPanel, 1, 0);
            
            this.Controls.Add(mainPanel);
            this.ResumeLayout();
        }
        
        private void InitializeDataGrid()
        {
            if (measurementData != null)
            {
                dataGridViewMeasurements.DataSource = measurementData;
                
                // Wait for columns to be created, then format them
                if (dataGridViewMeasurements.Columns.Count > 0)
                {
                    // Set specific column sizes
                    foreach (DataGridViewColumn column in dataGridViewMeasurements.Columns)
                    {
                        if (column != null)
                        {
                            if (column.Name == "No")
                            {
                                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                                column.Width = 50;
                            }
                            else if (column.Name == "Function")
                            {
                                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                                column.Width = 80;
                            }
                            else if (column.Name == "Tolerance")
                            {
                                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                                column.Width = 60;
                            }
                            else
                            {
                                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                            }
                        }
                    }
                }
            }
        }
        
        private void ButtonLoadPCB_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                openFileDialog.Title = "Select PCB Image";
                
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        pcbImage = Image.FromFile(openFileDialog.FileName);
                        pictureBoxPCB.Image = pcbImage;
                        pictureBoxPCB.SizeMode = PictureBoxSizeMode.Zoom;
                        
                        // Reset zoom
                        trackBarZoom.Value = 100;
                        zoomFactor = 1.0f;
                        UpdateZoomLabel();
                        
                        pictureBoxPCB.Invalidate();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading image: {ex.Message}", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        
        private void TrackBarZoom_ValueChanged(object sender, EventArgs e)
        {
            zoomFactor = trackBarZoom.Value / 100.0f;
            UpdateZoomLabel();
            pictureBoxPCB.Invalidate();
        }
        
        private void UpdateZoomLabel()
        {
            labelZoom.Text = $"Zoom: {trackBarZoom.Value}%";
        }
        
        private void DataGridViewMeasurements_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridViewMeasurements.SelectedRows.Count > 0)
            {
                selectedMeasurementIndex = dataGridViewMeasurements.SelectedRows[0].Index;
            }
        }
        
        private void PictureBoxPCB_MouseClick(object sender, MouseEventArgs e)
        {
            if (pcbImage == null || selectedMeasurementIndex == -1) return;
            
            if (e.Button == MouseButtons.Left)
            {
                // Calculate actual image coordinates considering zoom and aspect ratio
                var imageRect = GetImageRect();
                
                // Check if click is within image bounds
                if (!imageRect.Contains(e.Location)) return;
                
                // Convert screen coordinates to image coordinates
                float imageX = (e.X - imageRect.X) * pcbImage.Width / imageRect.Width;
                float imageY = (e.Y - imageRect.Y) * pcbImage.Height / imageRect.Height;
                
                // Get measurement data
                var row = measurementData.Rows[selectedMeasurementIndex];
                int measurementNo = Convert.ToInt32(row["No"]);
                string function = row["Function"].ToString();
                string measurement = row["Measurement"].ToString();
                string unit = row["Unit"].ToString();
                
                // Remove existing marker for this measurement if any
                markers.RemoveAll(m => m.MeasurementNo == measurementNo);
                
                // Add new marker
                var marker = new PCBMarker
                {
                    MeasurementNo = measurementNo,
                    X = imageX,
                    Y = imageY,
                    Function = function,
                    Measurement = measurement,
                    Unit = unit,
                    Label = $"#{measurementNo}: {measurement} {unit}"
                };
                
                markers.Add(marker);
                pictureBoxPCB.Invalidate();
                
                labelInstructions.Text = $"Marker assigned to measurement #{measurementNo}";
                labelInstructions.ForeColor = Color.Green;
            }
        }
        
        private Rectangle GetImageRect()
        {
            if (pcbImage == null) return Rectangle.Empty;
            
            var pictureBox = pictureBoxPCB;
            float imageAspect = (float)pcbImage.Width / pcbImage.Height;
            float boxAspect = (float)pictureBox.Width / pictureBox.Height;
            
            int drawWidth, drawHeight, drawX, drawY;
            
            if (imageAspect > boxAspect)
            {
                drawWidth = pictureBox.Width;
                drawHeight = (int)(pictureBox.Width / imageAspect);
                drawX = 0;
                drawY = (pictureBox.Height - drawHeight) / 2;
            }
            else
            {
                drawHeight = pictureBox.Height;
                drawWidth = (int)(pictureBox.Height * imageAspect);
                drawY = 0;
                drawX = (pictureBox.Width - drawWidth) / 2;
            }
            
            return new Rectangle(drawX, drawY, drawWidth, drawHeight);
        }
        
        private void PictureBoxPCB_Paint(object sender, PaintEventArgs e)
        {
            if (pcbImage == null || markers.Count == 0) return;
            
            var imageRect = GetImageRect();
            if (imageRect.IsEmpty) return;
            
            var graphics = e.Graphics;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            foreach (var marker in markers)
            {
                // Convert image coordinates to screen coordinates
                float screenX = imageRect.X + (marker.X * imageRect.Width / pcbImage.Width);
                float screenY = imageRect.Y + (marker.Y * imageRect.Height / pcbImage.Height);
                
                // Draw marker circle
                var markerRect = new RectangleF(screenX - 8, screenY - 8, 16, 16);
                graphics.FillEllipse(Brushes.Red, markerRect);
                graphics.DrawEllipse(Pens.White, markerRect);
                
                // Draw measurement number
                var font = new Font("Arial", 8, FontStyle.Bold);
                var text = marker.MeasurementNo.ToString();
                var textSize = graphics.MeasureString(text, font);
                var textRect = new PointF(screenX - textSize.Width / 2, screenY - textSize.Height / 2);
                
                graphics.DrawString(text, font, Brushes.White, textRect);
                
                // Draw tooltip-style label
                var labelFont = new Font("Arial", 9);
                var labelSize = graphics.MeasureString(marker.Label, labelFont);
                var labelRect = new RectangleF(screenX + 15, screenY - 10, labelSize.Width + 6, labelSize.Height + 4);
                
                graphics.FillRectangle(new SolidBrush(Color.FromArgb(220, Color.Yellow)), labelRect);
                graphics.DrawRectangle(Pens.Black, Rectangle.Round(labelRect));
                graphics.DrawString(marker.Label, labelFont, Brushes.Black, screenX + 18, screenY - 8);
            }
        }
        
        private void ButtonSaveAnnotations_Click(object sender, EventArgs e)
        {
            SaveAnnotations();
            MessageBox.Show("Annotations saved successfully!", "Success", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        private void ButtonClearMarkers_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear all markers?", "Confirm", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                markers.Clear();
                pictureBoxPCB.Invalidate();
                labelInstructions.Text = "All markers cleared";
                labelInstructions.ForeColor = Color.Orange;
            }
        }
        
        private void SaveAnnotations()
        {
            try
            {
                string appDataFolder = Path.GetDirectoryName(annotationsFilePath);
                Directory.CreateDirectory(appDataFolder);
                
                var lines = new List<string>();
                lines.Add("# PCB Annotations - MeasurementNo,X,Y,Function,Measurement,Unit");
                
                foreach (var marker in markers)
                {
                    lines.Add($"{marker.MeasurementNo},{marker.X},{marker.Y},{marker.Function},{marker.Measurement},{marker.Unit}");
                }
                
                File.WriteAllLines(annotationsFilePath, lines);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving annotations: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void LoadAnnotations()
        {
            try
            {
                if (File.Exists(annotationsFilePath))
                {
                    var lines = File.ReadAllLines(annotationsFilePath);
                    
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line)) continue;
                        
                        var parts = line.Split(',');
                        if (parts.Length >= 6)
                        {
                            var marker = new PCBMarker
                            {
                                MeasurementNo = int.Parse(parts[0]),
                                X = float.Parse(parts[1]),
                                Y = float.Parse(parts[2]),
                                Function = parts[3],
                                Measurement = parts[4],
                                Unit = parts[5],
                                Label = $"#{parts[0]}: {parts[4]} {parts[5]}"
                            };
                            markers.Add(marker);
                        }
                    }
                    
                    if (markers.Count > 0)
                    {
                        pictureBoxPCB.Invalidate();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading annotations: {ex.Message}", "Warning", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        
        public void RefreshMeasurementData()
        {
            dataGridViewMeasurements.Refresh();
        }
        
        private void PCBAnnotationForm_Load(object sender, EventArgs e)
        {
            InitializeDataGrid();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveAnnotations();
            base.OnFormClosing(e);
        }
    }
    
    public class PCBMarker
    {
        public int MeasurementNo { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public string Function { get; set; }
        public string Measurement { get; set; }
        public string Unit { get; set; }
        public string Label { get; set; }
    }
}