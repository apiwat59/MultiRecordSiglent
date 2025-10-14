using System;
using System.Windows.Forms;
using System.Drawing;

namespace MultiRecord
{
    public partial class RDEditDialog : Form
    {
        private Label labelMeasurementName;
        private TextBox textBoxMeasurementName;
        private Label labelToleranceType;
        private RadioButton radioButtonPercent;
        private RadioButton radioButtonAbsolute;
        private Panel panelToleranceType;
        private Label labelUpperLimit;
        private TextBox textBoxUpperLimit;
        private Label labelLowerLimit;
        private TextBox textBoxLowerLimit;
        private Label labelNote;
        private TextBox textBoxNote;
        private Button buttonSave;
        private Button buttonCancel;

        public string MeasurementName { get; set; }
        public string ToleranceType { get; set; }
        public decimal? UpperLimit { get; set; }
        public decimal? LowerLimit { get; set; }
        public string Note { get; set; }

        public RDEditDialog(string currentName = "", string currentType = "percent", decimal? currentUpper = null, decimal? currentLower = null, string currentNote = "")
        {
            MeasurementName = currentName;
            ToleranceType = currentType;
            UpperLimit = currentUpper;
            LowerLimit = currentLower;
            Note = currentNote;
            
            InitializeComponent();
            LoadCurrentValues();
        }

        private void InitializeComponent()
        {
            this.labelMeasurementName = new System.Windows.Forms.Label();
            this.textBoxMeasurementName = new System.Windows.Forms.TextBox();
            this.labelToleranceType = new System.Windows.Forms.Label();
            this.radioButtonPercent = new System.Windows.Forms.RadioButton();
            this.radioButtonAbsolute = new System.Windows.Forms.RadioButton();
            this.panelToleranceType = new System.Windows.Forms.Panel();
            this.labelUpperLimit = new System.Windows.Forms.Label();
            this.textBoxUpperLimit = new System.Windows.Forms.TextBox();
            this.labelLowerLimit = new System.Windows.Forms.Label();
            this.textBoxLowerLimit = new System.Windows.Forms.TextBox();
            this.labelNote = new System.Windows.Forms.Label();
            this.textBoxNote = new System.Windows.Forms.TextBox();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.panelToleranceType.SuspendLayout();
            this.SuspendLayout();
            
            // 
            // labelMeasurementName
            // 
            this.labelMeasurementName.AutoSize = true;
            this.labelMeasurementName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelMeasurementName.ForeColor = System.Drawing.Color.White;
            this.labelMeasurementName.Location = new System.Drawing.Point(20, 20);
            this.labelMeasurementName.Name = "labelMeasurementName";
            this.labelMeasurementName.Size = new System.Drawing.Size(100, 15);
            this.labelMeasurementName.TabIndex = 0;
            this.labelMeasurementName.Text = "ชื่อจุดวัด (Name):";
            
            // 
            // textBoxMeasurementName
            // 
            this.textBoxMeasurementName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.textBoxMeasurementName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxMeasurementName.ForeColor = System.Drawing.Color.White;
            this.textBoxMeasurementName.Location = new System.Drawing.Point(20, 45);
            this.textBoxMeasurementName.Name = "textBoxMeasurementName";
            this.textBoxMeasurementName.Size = new System.Drawing.Size(440, 23);
            this.textBoxMeasurementName.TabIndex = 1;
            
            // 
            // labelToleranceType
            // 
            this.labelToleranceType.AutoSize = true;
            this.labelToleranceType.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelToleranceType.ForeColor = System.Drawing.Color.White;
            this.labelToleranceType.Location = new System.Drawing.Point(20, 85);
            this.labelToleranceType.Name = "labelToleranceType";
            this.labelToleranceType.Size = new System.Drawing.Size(120, 15);
            this.labelToleranceType.TabIndex = 2;
            this.labelToleranceType.Text = "ประเภท Tolerance:";
            
            // 
            // panelToleranceType
            // 
            this.panelToleranceType.Controls.Add(this.radioButtonPercent);
            this.panelToleranceType.Controls.Add(this.radioButtonAbsolute);
            this.panelToleranceType.Location = new System.Drawing.Point(20, 110);
            this.panelToleranceType.Name = "panelToleranceType";
            this.panelToleranceType.Size = new System.Drawing.Size(440, 40);
            this.panelToleranceType.TabIndex = 3;
            
            // 
            // radioButtonPercent
            // 
            this.radioButtonPercent.AutoSize = true;
            this.radioButtonPercent.Checked = true;
            this.radioButtonPercent.ForeColor = System.Drawing.Color.White;
            this.radioButtonPercent.Location = new System.Drawing.Point(10, 10);
            this.radioButtonPercent.Name = "radioButtonPercent";
            this.radioButtonPercent.Size = new System.Drawing.Size(125, 19);
            this.radioButtonPercent.TabIndex = 0;
            this.radioButtonPercent.TabStop = true;
            this.radioButtonPercent.Text = "Percent (%)";
            this.radioButtonPercent.UseVisualStyleBackColor = true;
            
            // 
            // radioButtonAbsolute
            // 
            this.radioButtonAbsolute.AutoSize = true;
            this.radioButtonAbsolute.ForeColor = System.Drawing.Color.White;
            this.radioButtonAbsolute.Location = new System.Drawing.Point(180, 10);
            this.radioButtonAbsolute.Name = "radioButtonAbsolute";
            this.radioButtonAbsolute.Size = new System.Drawing.Size(100, 19);
            this.radioButtonAbsolute.TabIndex = 1;
            this.radioButtonAbsolute.Text = "Absolute";
            this.radioButtonAbsolute.UseVisualStyleBackColor = true;
            
            // 
            // labelUpperLimit
            // 
            this.labelUpperLimit.AutoSize = true;
            this.labelUpperLimit.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelUpperLimit.ForeColor = System.Drawing.Color.White;
            this.labelUpperLimit.Location = new System.Drawing.Point(20, 165);
            this.labelUpperLimit.Name = "labelUpperLimit";
            this.labelUpperLimit.Size = new System.Drawing.Size(120, 15);
            this.labelUpperLimit.TabIndex = 4;
            this.labelUpperLimit.Text = "ค่าบน (Upper Limit):";
            
            // 
            // textBoxUpperLimit
            // 
            this.textBoxUpperLimit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.textBoxUpperLimit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxUpperLimit.ForeColor = System.Drawing.Color.White;
            this.textBoxUpperLimit.Location = new System.Drawing.Point(20, 190);
            this.textBoxUpperLimit.Name = "textBoxUpperLimit";
            this.textBoxUpperLimit.Size = new System.Drawing.Size(210, 23);
            this.textBoxUpperLimit.TabIndex = 5;
            
            // 
            // labelLowerLimit
            // 
            this.labelLowerLimit.AutoSize = true;
            this.labelLowerLimit.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelLowerLimit.ForeColor = System.Drawing.Color.White;
            this.labelLowerLimit.Location = new System.Drawing.Point(250, 165);
            this.labelLowerLimit.Name = "labelLowerLimit";
            this.labelLowerLimit.Size = new System.Drawing.Size(120, 15);
            this.labelLowerLimit.TabIndex = 6;
            this.labelLowerLimit.Text = "ค่าล่าง (Lower Limit):";
            
            // 
            // textBoxLowerLimit
            // 
            this.textBoxLowerLimit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.textBoxLowerLimit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxLowerLimit.ForeColor = System.Drawing.Color.White;
            this.textBoxLowerLimit.Location = new System.Drawing.Point(250, 190);
            this.textBoxLowerLimit.Name = "textBoxLowerLimit";
            this.textBoxLowerLimit.Size = new System.Drawing.Size(210, 23);
            this.textBoxLowerLimit.TabIndex = 7;
            
            // 
            // labelNote
            // 
            this.labelNote.AutoSize = true;
            this.labelNote.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelNote.ForeColor = System.Drawing.Color.White;
            this.labelNote.Location = new System.Drawing.Point(20, 230);
            this.labelNote.Name = "labelNote";
            this.labelNote.Size = new System.Drawing.Size(85, 15);
            this.labelNote.TabIndex = 8;
            this.labelNote.Text = "หมายเหตุ (Note):";
            
            // 
            // textBoxNote
            // 
            this.textBoxNote.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.textBoxNote.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxNote.ForeColor = System.Drawing.Color.White;
            this.textBoxNote.Location = new System.Drawing.Point(20, 255);
            this.textBoxNote.Multiline = true;
            this.textBoxNote.Name = "textBoxNote";
            this.textBoxNote.Size = new System.Drawing.Size(440, 80);
            this.textBoxNote.TabIndex = 9;
            
            // 
            // buttonSave
            // 
            this.buttonSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(106)))), ((int)(((byte)(153)))), ((int)(((byte)(78)))));
            this.buttonSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonSave.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.buttonSave.ForeColor = System.Drawing.Color.White;
            this.buttonSave.Location = new System.Drawing.Point(294, 355);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(80, 30);
            this.buttonSave.TabIndex = 10;
            this.buttonSave.Text = "บันทึก";
            this.buttonSave.UseVisualStyleBackColor = false;
            this.buttonSave.Click += new System.EventHandler(this.ButtonSave_Click);
            
            // 
            // buttonCancel
            // 
            this.buttonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(186)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCancel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.buttonCancel.ForeColor = System.Drawing.Color.White;
            this.buttonCancel.Location = new System.Drawing.Point(380, 355);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(80, 30);
            this.buttonCancel.TabIndex = 11;
            this.buttonCancel.Text = "ยกเลิก";
            this.buttonCancel.UseVisualStyleBackColor = false;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            
            // 
            // RDEditDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.ClientSize = new System.Drawing.Size(484, 405);
            this.Controls.Add(this.labelMeasurementName);
            this.Controls.Add(this.textBoxMeasurementName);
            this.Controls.Add(this.labelToleranceType);
            this.Controls.Add(this.panelToleranceType);
            this.Controls.Add(this.labelUpperLimit);
            this.Controls.Add(this.textBoxUpperLimit);
            this.Controls.Add(this.labelLowerLimit);
            this.Controls.Add(this.textBoxLowerLimit);
            this.Controls.Add(this.labelNote);
            this.Controls.Add(this.textBoxNote);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.buttonCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RDEditDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "แก้ไขข้อมูลจุดวัด - Edit Measurement Point";
            this.panelToleranceType.ResumeLayout(false);
            this.panelToleranceType.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadCurrentValues()
        {
            textBoxMeasurementName.Text = MeasurementName;
            textBoxNote.Text = Note;
            textBoxUpperLimit.Text = UpperLimit.HasValue ? UpperLimit.Value.ToString() : "";
            textBoxLowerLimit.Text = LowerLimit.HasValue ? LowerLimit.Value.ToString() : "";
            
            if (ToleranceType.ToLower() == "absolute")
            {
                radioButtonAbsolute.Checked = true;
            }
            else
            {
                radioButtonPercent.Checked = true;
            }
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            MeasurementName = textBoxMeasurementName.Text.Trim();
            ToleranceType = radioButtonPercent.Checked ? "percent" : "absolute";
            Note = textBoxNote.Text.Trim();
            
            // Parse Upper/Lower Limit
            if (!string.IsNullOrWhiteSpace(textBoxUpperLimit.Text))
            {
                if (decimal.TryParse(textBoxUpperLimit.Text, out decimal upperValue))
                {
                    UpperLimit = upperValue;
                }
                else
                {
                    MessageBox.Show("กรุณาระบุค่าบนเป็นตัวเลข", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else
            {
                UpperLimit = null;
            }
            
            if (!string.IsNullOrWhiteSpace(textBoxLowerLimit.Text))
            {
                if (decimal.TryParse(textBoxLowerLimit.Text, out decimal lowerValue))
                {
                    LowerLimit = lowerValue;
                }
                else
                {
                    MessageBox.Show("กรุณาระบุค่าล่างเป็นตัวเลข", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else
            {
                LowerLimit = null;
            }
            
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}

