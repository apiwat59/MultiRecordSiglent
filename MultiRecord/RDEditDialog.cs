using System;
using System.Windows.Forms;
using System.Drawing;

namespace MultiRecord
{
    public partial class RDEditDialog : Form
    {
        private Label labelMeasurementName;
        private TextBox textBoxMeasurementName;
        private Label labelFunction;
        private ComboBox comboBoxFunction;
        private Label labelMeasurementValue;
        private TextBox textBoxMeasurementValue;
        private CheckBox checkBoxToleranceEnable;
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
        private Button button1;
        private Button buttonCancel;

        public string MeasurementName { get; set; }
        public string Function { get; set; }
        public string MeasurementValue { get; set; }
        public bool ToleranceEnable { get; set; }
        public string ToleranceType { get; set; }
        public decimal? UpperLimit { get; set; }
        public decimal? LowerLimit { get; set; }
        public string Note { get; set; }

        public RDEditDialog(string currentName = "", string currentFunction = "", string currentMeasurementValue = "", bool currentToleranceEnable = true, string currentType = "percent", decimal? currentUpper = null, decimal? currentLower = null, string currentNote = "")
        {
            MeasurementName = currentName;
            Function = currentFunction;
            MeasurementValue = currentMeasurementValue;
            ToleranceEnable = currentToleranceEnable;
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
            this.labelFunction = new System.Windows.Forms.Label();
            this.comboBoxFunction = new System.Windows.Forms.ComboBox();
            this.labelMeasurementValue = new System.Windows.Forms.Label();
            this.textBoxMeasurementValue = new System.Windows.Forms.TextBox();
            this.checkBoxToleranceEnable = new System.Windows.Forms.CheckBox();
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
            this.button1 = new System.Windows.Forms.Button();
            this.panelToleranceType.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelMeasurementName
            // 
            this.labelMeasurementName.AutoSize = true;
            this.labelMeasurementName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelMeasurementName.ForeColor = System.Drawing.Color.White;
            this.labelMeasurementName.Location = new System.Drawing.Point(17, 17);
            this.labelMeasurementName.Name = "labelMeasurementName";
            this.labelMeasurementName.Size = new System.Drawing.Size(91, 15);
            this.labelMeasurementName.TabIndex = 0;
            this.labelMeasurementName.Text = "ชื่อจุดวัด (Name):";
            // 
            // textBoxMeasurementName
            // 
            this.textBoxMeasurementName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.textBoxMeasurementName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxMeasurementName.ForeColor = System.Drawing.Color.White;
            this.textBoxMeasurementName.Location = new System.Drawing.Point(17, 39);
            this.textBoxMeasurementName.Name = "textBoxMeasurementName";
            this.textBoxMeasurementName.Size = new System.Drawing.Size(377, 20);
            this.textBoxMeasurementName.TabIndex = 1;
            // 
            // labelFunction
            // 
            this.labelFunction.AutoSize = true;
            this.labelFunction.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelFunction.ForeColor = System.Drawing.Color.White;
            this.labelFunction.Location = new System.Drawing.Point(17, 76);
            this.labelFunction.Name = "labelFunction";
            this.labelFunction.Size = new System.Drawing.Size(57, 15);
            this.labelFunction.TabIndex = 2;
            this.labelFunction.Text = "Function:";
            // 
            // comboBoxFunction
            // 
            this.comboBoxFunction.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.comboBoxFunction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFunction.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBoxFunction.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.comboBoxFunction.ForeColor = System.Drawing.Color.White;
            this.comboBoxFunction.FormattingEnabled = true;
            this.comboBoxFunction.Items.AddRange(new object[] {
            "VoltageDC",
            "VoltageAC",
            "CurrentDC",
            "CurrentAC",
            "Resistance2W",
            "Resistance4W",
            "Capacitance",
            "Frequency",
            "Temperature",
            "Diode",
            "Continuous"});
            this.comboBoxFunction.Location = new System.Drawing.Point(129, 74);
            this.comboBoxFunction.Name = "comboBoxFunction";
            this.comboBoxFunction.Size = new System.Drawing.Size(266, 23);
            this.comboBoxFunction.TabIndex = 3;
            // 
            // labelMeasurementValue
            // 
            this.labelMeasurementValue.AutoSize = true;
            this.labelMeasurementValue.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelMeasurementValue.ForeColor = System.Drawing.Color.White;
            this.labelMeasurementValue.Location = new System.Drawing.Point(17, 115);
            this.labelMeasurementValue.Name = "labelMeasurementValue";
            this.labelMeasurementValue.Size = new System.Drawing.Size(90, 15);
            this.labelMeasurementValue.TabIndex = 4;
            this.labelMeasurementValue.Text = "ค่าการวัด (Value):";
            // 
            // textBoxMeasurementValue
            // 
            this.textBoxMeasurementValue.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.textBoxMeasurementValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxMeasurementValue.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.textBoxMeasurementValue.ForeColor = System.Drawing.Color.White;
            this.textBoxMeasurementValue.Location = new System.Drawing.Point(129, 113);
            this.textBoxMeasurementValue.Name = "textBoxMeasurementValue";
            this.textBoxMeasurementValue.Size = new System.Drawing.Size(170, 23);
            this.textBoxMeasurementValue.TabIndex = 5;
            // 
            // checkBoxToleranceEnable
            // 
            this.checkBoxToleranceEnable.AutoSize = true;
            this.checkBoxToleranceEnable.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.checkBoxToleranceEnable.ForeColor = System.Drawing.Color.White;
            this.checkBoxToleranceEnable.Location = new System.Drawing.Point(17, 154);
            this.checkBoxToleranceEnable.Name = "checkBoxToleranceEnable";
            this.checkBoxToleranceEnable.Size = new System.Drawing.Size(107, 19);
            this.checkBoxToleranceEnable.TabIndex = 6;
            this.checkBoxToleranceEnable.Text = "เปิดใช้ Tolerance";
            this.checkBoxToleranceEnable.UseVisualStyleBackColor = true;
            this.checkBoxToleranceEnable.CheckedChanged += new System.EventHandler(this.CheckBoxToleranceEnable_CheckedChanged);
            // 
            // labelToleranceType
            // 
            this.labelToleranceType.AutoSize = true;
            this.labelToleranceType.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelToleranceType.ForeColor = System.Drawing.Color.White;
            this.labelToleranceType.Location = new System.Drawing.Point(17, 180);
            this.labelToleranceType.Name = "labelToleranceType";
            this.labelToleranceType.Size = new System.Drawing.Size(97, 15);
            this.labelToleranceType.TabIndex = 2;
            this.labelToleranceType.Text = "ประเภท Tolerance:";
            // 
            // radioButtonPercent
            // 
            this.radioButtonPercent.AutoSize = true;
            this.radioButtonPercent.Checked = true;
            this.radioButtonPercent.ForeColor = System.Drawing.Color.White;
            this.radioButtonPercent.Location = new System.Drawing.Point(4, 2);
            this.radioButtonPercent.Name = "radioButtonPercent";
            this.radioButtonPercent.Size = new System.Drawing.Size(79, 17);
            this.radioButtonPercent.TabIndex = 0;
            this.radioButtonPercent.TabStop = true;
            this.radioButtonPercent.Text = "Percent (%)";
            this.radioButtonPercent.UseVisualStyleBackColor = true;
            // 
            // radioButtonAbsolute
            // 
            this.radioButtonAbsolute.AutoSize = true;
            this.radioButtonAbsolute.ForeColor = System.Drawing.Color.White;
            this.radioButtonAbsolute.Location = new System.Drawing.Point(120, 2);
            this.radioButtonAbsolute.Name = "radioButtonAbsolute";
            this.radioButtonAbsolute.Size = new System.Drawing.Size(66, 17);
            this.radioButtonAbsolute.TabIndex = 1;
            this.radioButtonAbsolute.Text = "Absolute";
            this.radioButtonAbsolute.UseVisualStyleBackColor = true;
            // 
            // panelToleranceType
            // 
            this.panelToleranceType.Controls.Add(this.radioButtonPercent);
            this.panelToleranceType.Controls.Add(this.radioButtonAbsolute);
            this.panelToleranceType.Location = new System.Drawing.Point(129, 178);
            this.panelToleranceType.Name = "panelToleranceType";
            this.panelToleranceType.Size = new System.Drawing.Size(266, 22);
            this.panelToleranceType.TabIndex = 7;
            // 
            // labelUpperLimit
            // 
            this.labelUpperLimit.AutoSize = true;
            this.labelUpperLimit.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelUpperLimit.ForeColor = System.Drawing.Color.White;
            this.labelUpperLimit.Location = new System.Drawing.Point(126, 211);
            this.labelUpperLimit.Name = "labelUpperLimit";
            this.labelUpperLimit.Size = new System.Drawing.Size(109, 15);
            this.labelUpperLimit.TabIndex = 4;
            this.labelUpperLimit.Text = "ค่าบน (Upper Limit):";
            // 
            // textBoxUpperLimit
            // 
            this.textBoxUpperLimit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.textBoxUpperLimit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxUpperLimit.ForeColor = System.Drawing.Color.White;
            this.textBoxUpperLimit.Location = new System.Drawing.Point(129, 235);
            this.textBoxUpperLimit.Name = "textBoxUpperLimit";
            this.textBoxUpperLimit.Size = new System.Drawing.Size(129, 20);
            this.textBoxUpperLimit.TabIndex = 8;
            // 
            // labelLowerLimit
            // 
            this.labelLowerLimit.AutoSize = true;
            this.labelLowerLimit.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelLowerLimit.ForeColor = System.Drawing.Color.White;
            this.labelLowerLimit.Location = new System.Drawing.Point(266, 211);
            this.labelLowerLimit.Name = "labelLowerLimit";
            this.labelLowerLimit.Size = new System.Drawing.Size(111, 15);
            this.labelLowerLimit.TabIndex = 6;
            this.labelLowerLimit.Text = "ค่าล่าง (Lower Limit):";
            // 
            // textBoxLowerLimit
            // 
            this.textBoxLowerLimit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.textBoxLowerLimit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxLowerLimit.ForeColor = System.Drawing.Color.White;
            this.textBoxLowerLimit.Location = new System.Drawing.Point(266, 235);
            this.textBoxLowerLimit.Name = "textBoxLowerLimit";
            this.textBoxLowerLimit.Size = new System.Drawing.Size(129, 20);
            this.textBoxLowerLimit.TabIndex = 9;
            // 
            // labelNote
            // 
            this.labelNote.AutoSize = true;
            this.labelNote.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelNote.ForeColor = System.Drawing.Color.White;
            this.labelNote.Location = new System.Drawing.Point(17, 265);
            this.labelNote.Name = "labelNote";
            this.labelNote.Size = new System.Drawing.Size(89, 15);
            this.labelNote.TabIndex = 8;
            this.labelNote.Text = "หมายเหตุ (Note):";
            // 
            // textBoxNote
            // 
            this.textBoxNote.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.textBoxNote.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxNote.ForeColor = System.Drawing.Color.White;
            this.textBoxNote.Location = new System.Drawing.Point(17, 290);
            this.textBoxNote.Multiline = true;
            this.textBoxNote.Name = "textBoxNote";
            this.textBoxNote.Size = new System.Drawing.Size(377, 52);
            this.textBoxNote.TabIndex = 10;
            // 
            // buttonSave
            // 
            this.buttonSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(106)))), ((int)(((byte)(153)))), ((int)(((byte)(78)))));
            this.buttonSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonSave.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.buttonSave.ForeColor = System.Drawing.Color.White;
            this.buttonSave.Location = new System.Drawing.Point(252, 359);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(69, 26);
            this.buttonSave.TabIndex = 11;
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
            this.buttonCancel.Location = new System.Drawing.Point(326, 359);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(69, 26);
            this.buttonCancel.TabIndex = 12;
            this.buttonCancel.Text = "ยกเลิก";
            this.buttonCancel.UseVisualStyleBackColor = false;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(106)))), ((int)(((byte)(153)))), ((int)(((byte)(78)))));
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.button1.ForeColor = System.Drawing.Color.White;
            this.button1.Location = new System.Drawing.Point(305, 110);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(89, 26);
            this.button1.TabIndex = 13;
            this.button1.Text = "OVERLOAD";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // RDEditDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.ClientSize = new System.Drawing.Size(415, 402);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.labelMeasurementName);
            this.Controls.Add(this.textBoxMeasurementName);
            this.Controls.Add(this.labelFunction);
            this.Controls.Add(this.comboBoxFunction);
            this.Controls.Add(this.labelMeasurementValue);
            this.Controls.Add(this.textBoxMeasurementValue);
            this.Controls.Add(this.checkBoxToleranceEnable);
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
            textBoxMeasurementValue.Text = MeasurementValue;
            textBoxNote.Text = Note;
            checkBoxToleranceEnable.Checked = ToleranceEnable;
            
            // Set function selection
            if (!string.IsNullOrEmpty(Function))
            {
                comboBoxFunction.SelectedItem = Function;
            }
            
            // Display Upper/Lower values without formatting (just the number)
            textBoxUpperLimit.Text = UpperLimit.HasValue ? UpperLimit.Value.ToString() : "";
            textBoxLowerLimit.Text = LowerLimit.HasValue ? LowerLimit.Value.ToString() : "";
            
            if (ToleranceType.ToLower() == "absolute" || ToleranceType.ToLower() == "abs")
            {
                radioButtonAbsolute.Checked = true;
            }
            else
            {
                radioButtonPercent.Checked = true;
            }
            
            // Enable/disable tolerance controls based on checkbox
            UpdateToleranceControls();
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            MeasurementName = textBoxMeasurementName.Text.Trim();
            Function = comboBoxFunction.SelectedItem?.ToString() ?? "";
            MeasurementValue = textBoxMeasurementValue.Text.Trim();
            ToleranceEnable = checkBoxToleranceEnable.Checked;
            ToleranceType = radioButtonPercent.Checked ? "percent" : "absolute";
            Note = textBoxNote.Text.Trim();
            
            // Validate measurement value if provided
            if (!string.IsNullOrWhiteSpace(MeasurementValue) && MeasurementValue != "OVERLOAD")
            {
                if (!decimal.TryParse(MeasurementValue, out _))
                {
                    MessageBox.Show("กรุณาระบุค่าการวัดเป็นตัวเลข", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            
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
        
        private void CheckBoxToleranceEnable_CheckedChanged(object sender, EventArgs e)
        {
            UpdateToleranceControls();
        }
        
        private void UpdateToleranceControls()
        {
            bool enabled = checkBoxToleranceEnable.Checked;
            
            labelToleranceType.Enabled = enabled;
            panelToleranceType.Enabled = enabled;
            radioButtonPercent.Enabled = enabled;
            radioButtonAbsolute.Enabled = enabled;
            labelUpperLimit.Enabled = enabled;
            textBoxUpperLimit.Enabled = enabled;
            labelLowerLimit.Enabled = enabled;
            textBoxLowerLimit.Enabled = enabled;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBoxMeasurementValue.Text = "-1000000.0";
        }
    }
}

