namespace MultiRecord
{
    partial class BatchToleranceDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.labelInfo = new System.Windows.Forms.Label();
            this.groupBoxScope = new System.Windows.Forms.GroupBox();
            this.radioButtonAll = new System.Windows.Forms.RadioButton();
            this.radioButtonSelected = new System.Windows.Forms.RadioButton();
            this.groupBoxTolerance = new System.Windows.Forms.GroupBox();
            this.checkBoxEnable = new System.Windows.Forms.CheckBox();
            this.labelToleranceType = new System.Windows.Forms.Label();
            this.comboBoxToleranceType = new System.Windows.Forms.ComboBox();
            this.labelUpper = new System.Windows.Forms.Label();
            this.textBoxUpper = new System.Windows.Forms.TextBox();
            this.labelLower = new System.Windows.Forms.Label();
            this.textBoxLower = new System.Windows.Forms.TextBox();
            this.buttonApply = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxScope.SuspendLayout();
            this.groupBoxTolerance.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelInfo
            // 
            this.labelInfo.AutoSize = true;
            this.labelInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelInfo.Location = new System.Drawing.Point(12, 9);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(100, 15);
            this.labelInfo.TabIndex = 0;
            this.labelInfo.Text = "เลือกแถว: 0 แถว";
            // 
            // groupBoxScope
            // 
            this.groupBoxScope.Controls.Add(this.radioButtonSelected);
            this.groupBoxScope.Controls.Add(this.radioButtonAll);
            this.groupBoxScope.Location = new System.Drawing.Point(15, 40);
            this.groupBoxScope.Name = "groupBoxScope";
            this.groupBoxScope.Size = new System.Drawing.Size(370, 80);
            this.groupBoxScope.TabIndex = 1;
            this.groupBoxScope.TabStop = false;
            this.groupBoxScope.Text = "ขอบเขตการใช้งาน";
            // 
            // radioButtonSelected
            // 
            this.radioButtonSelected.AutoSize = true;
            this.radioButtonSelected.Location = new System.Drawing.Point(15, 25);
            this.radioButtonSelected.Name = "radioButtonSelected";
            this.radioButtonSelected.Size = new System.Drawing.Size(142, 17);
            this.radioButtonSelected.TabIndex = 0;
            this.radioButtonSelected.TabStop = true;
            this.radioButtonSelected.Text = "ใช้กับแถวที่เลือกเท่านั้น";
            this.radioButtonSelected.UseVisualStyleBackColor = true;
            // 
            // radioButtonAll
            // 
            this.radioButtonAll.AutoSize = true;
            this.radioButtonAll.Location = new System.Drawing.Point(15, 48);
            this.radioButtonAll.Name = "radioButtonAll";
            this.radioButtonAll.Size = new System.Drawing.Size(111, 17);
            this.radioButtonAll.TabIndex = 1;
            this.radioButtonAll.TabStop = true;
            this.radioButtonAll.Text = "ใช้กับทุกแถว";
            this.radioButtonAll.UseVisualStyleBackColor = true;
            // 
            // groupBoxTolerance
            // 
            this.groupBoxTolerance.Controls.Add(this.textBoxLower);
            this.groupBoxTolerance.Controls.Add(this.labelLower);
            this.groupBoxTolerance.Controls.Add(this.textBoxUpper);
            this.groupBoxTolerance.Controls.Add(this.labelUpper);
            this.groupBoxTolerance.Controls.Add(this.comboBoxToleranceType);
            this.groupBoxTolerance.Controls.Add(this.labelToleranceType);
            this.groupBoxTolerance.Controls.Add(this.checkBoxEnable);
            this.groupBoxTolerance.Location = new System.Drawing.Point(15, 126);
            this.groupBoxTolerance.Name = "groupBoxTolerance";
            this.groupBoxTolerance.Size = new System.Drawing.Size(370, 160);
            this.groupBoxTolerance.TabIndex = 2;
            this.groupBoxTolerance.TabStop = false;
            this.groupBoxTolerance.Text = "ค่า Tolerance";
            // 
            // checkBoxEnable
            // 
            this.checkBoxEnable.AutoSize = true;
            this.checkBoxEnable.Checked = true;
            this.checkBoxEnable.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxEnable.Location = new System.Drawing.Point(15, 25);
            this.checkBoxEnable.Name = "checkBoxEnable";
            this.checkBoxEnable.Size = new System.Drawing.Size(113, 17);
            this.checkBoxEnable.TabIndex = 0;
            this.checkBoxEnable.Text = "เปิดใช้งาน Tolerance";
            this.checkBoxEnable.UseVisualStyleBackColor = true;
            this.checkBoxEnable.CheckedChanged += new System.EventHandler(this.CheckBoxEnable_CheckedChanged);
            // 
            // labelToleranceType
            // 
            this.labelToleranceType.AutoSize = true;
            this.labelToleranceType.Location = new System.Drawing.Point(15, 55);
            this.labelToleranceType.Name = "labelToleranceType";
            this.labelToleranceType.Size = new System.Drawing.Size(34, 13);
            this.labelToleranceType.TabIndex = 1;
            this.labelToleranceType.Text = "Type:";
            // 
            // comboBoxToleranceType
            // 
            this.comboBoxToleranceType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxToleranceType.FormattingEnabled = true;
            this.comboBoxToleranceType.Location = new System.Drawing.Point(100, 52);
            this.comboBoxToleranceType.Name = "comboBoxToleranceType";
            this.comboBoxToleranceType.Size = new System.Drawing.Size(250, 21);
            this.comboBoxToleranceType.TabIndex = 2;
            // 
            // labelUpper
            // 
            this.labelUpper.AutoSize = true;
            this.labelUpper.Location = new System.Drawing.Point(15, 85);
            this.labelUpper.Name = "labelUpper";
            this.labelUpper.Size = new System.Drawing.Size(67, 13);
            this.labelUpper.TabIndex = 3;
            this.labelUpper.Text = "Upper Limit:";
            // 
            // textBoxUpper
            // 
            this.textBoxUpper.Location = new System.Drawing.Point(100, 82);
            this.textBoxUpper.Name = "textBoxUpper";
            this.textBoxUpper.Size = new System.Drawing.Size(250, 20);
            this.textBoxUpper.TabIndex = 4;
            this.textBoxUpper.Text = "0";
            // 
            // labelLower
            // 
            this.labelLower.AutoSize = true;
            this.labelLower.Location = new System.Drawing.Point(15, 115);
            this.labelLower.Name = "labelLower";
            this.labelLower.Size = new System.Drawing.Size(67, 13);
            this.labelLower.TabIndex = 5;
            this.labelLower.Text = "Lower Limit:";
            // 
            // textBoxLower
            // 
            this.textBoxLower.Location = new System.Drawing.Point(100, 112);
            this.textBoxLower.Name = "textBoxLower";
            this.textBoxLower.Size = new System.Drawing.Size(250, 20);
            this.textBoxLower.TabIndex = 6;
            this.textBoxLower.Text = "0";
            // 
            // buttonApply
            // 
            this.buttonApply.Location = new System.Drawing.Point(210, 300);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(85, 30);
            this.buttonApply.TabIndex = 3;
            this.buttonApply.Text = "ใช้งาน";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.ButtonApply_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(300, 300);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(85, 30);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "ยกเลิก";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // BatchToleranceDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 345);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonApply);
            this.Controls.Add(this.groupBoxTolerance);
            this.Controls.Add(this.groupBoxScope);
            this.Controls.Add(this.labelInfo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BatchToleranceDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ตั้งค่า Tolerance แบบ Batch";
            this.groupBoxScope.ResumeLayout(false);
            this.groupBoxScope.PerformLayout();
            this.groupBoxTolerance.ResumeLayout(false);
            this.groupBoxTolerance.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.GroupBox groupBoxScope;
        private System.Windows.Forms.RadioButton radioButtonSelected;
        private System.Windows.Forms.RadioButton radioButtonAll;
        private System.Windows.Forms.GroupBox groupBoxTolerance;
        private System.Windows.Forms.CheckBox checkBoxEnable;
        private System.Windows.Forms.Label labelToleranceType;
        private System.Windows.Forms.ComboBox comboBoxToleranceType;
        private System.Windows.Forms.Label labelUpper;
        private System.Windows.Forms.TextBox textBoxUpper;
        private System.Windows.Forms.Label labelLower;
        private System.Windows.Forms.TextBox textBoxLower;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.Button buttonCancel;
    }
}


