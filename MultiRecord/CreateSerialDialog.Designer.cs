namespace MultiRecord
{
    partial class CreateSerialDialog
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
            this.labelTitle = new System.Windows.Forms.Label();
            this.labelModelName = new System.Windows.Forms.Label();
            this.groupBoxSerialInput = new System.Windows.Forms.GroupBox();
            this.labelSerialNumber = new System.Windows.Forms.Label();
            this.textBoxSerialNumber = new System.Windows.Forms.TextBox();
            this.labelInstructions = new System.Windows.Forms.Label();
            this.buttonCreate = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxSerialInput.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelTitle
            // 
            this.labelTitle.AutoSize = true;
            this.labelTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTitle.ForeColor = System.Drawing.Color.White;
            this.labelTitle.Location = new System.Drawing.Point(12, 15);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(169, 20);
            this.labelTitle.TabIndex = 0;
            this.labelTitle.Text = "สร้าง ชื่อชุดทดสอบ ใหม่";
            // 
            // labelModelName
            // 
            this.labelModelName.AutoSize = true;
            this.labelModelName.ForeColor = System.Drawing.Color.LightGray;
            this.labelModelName.Location = new System.Drawing.Point(12, 45);
            this.labelModelName.Name = "labelModelName";
            this.labelModelName.Size = new System.Drawing.Size(42, 13);
            this.labelModelName.TabIndex = 1;
            this.labelModelName.Text = "Model: ";
            // 
            // groupBoxSerialInput
            // 
            this.groupBoxSerialInput.Controls.Add(this.labelSerialNumber);
            this.groupBoxSerialInput.Controls.Add(this.textBoxSerialNumber);
            this.groupBoxSerialInput.Controls.Add(this.labelInstructions);
            this.groupBoxSerialInput.ForeColor = System.Drawing.Color.White;
            this.groupBoxSerialInput.Location = new System.Drawing.Point(12, 70);
            this.groupBoxSerialInput.Name = "groupBoxSerialInput";
            this.groupBoxSerialInput.Size = new System.Drawing.Size(400, 100);
            this.groupBoxSerialInput.TabIndex = 2;
            this.groupBoxSerialInput.TabStop = false;
            this.groupBoxSerialInput.Text = "ชื่อชุดทดสอบ";
            // 
            // labelSerialNumber
            // 
            this.labelSerialNumber.AutoSize = true;
            this.labelSerialNumber.ForeColor = System.Drawing.Color.White;
            this.labelSerialNumber.Location = new System.Drawing.Point(15, 25);
            this.labelSerialNumber.Name = "labelSerialNumber";
            this.labelSerialNumber.Size = new System.Drawing.Size(73, 13);
            this.labelSerialNumber.TabIndex = 0;
            this.labelSerialNumber.Text = "ชื่อชุดทดสอบ :";
            // 
            // textBoxSerialNumber
            // 
            this.textBoxSerialNumber.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.textBoxSerialNumber.ForeColor = System.Drawing.Color.White;
            this.textBoxSerialNumber.Location = new System.Drawing.Point(110, 22);
            this.textBoxSerialNumber.MaxLength = 50;
            this.textBoxSerialNumber.Name = "textBoxSerialNumber";
            this.textBoxSerialNumber.Size = new System.Drawing.Size(270, 20);
            this.textBoxSerialNumber.TabIndex = 1;
            this.textBoxSerialNumber.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBoxSerialNumber_KeyPress);
            // 
            // labelInstructions
            // 
            this.labelInstructions.ForeColor = System.Drawing.Color.LightGray;
            this.labelInstructions.Location = new System.Drawing.Point(15, 55);
            this.labelInstructions.Name = "labelInstructions";
            this.labelInstructions.Size = new System.Drawing.Size(365, 30);
            this.labelInstructions.TabIndex = 2;
            this.labelInstructions.Text = "ใส่ชื่อชุดทดสอบที่ต้องการสร้าง \r\nกด Enter เพื่อสร้าง หรือ Escape เพื่อยกเลิก";
            // 
            // buttonCreate
            // 
            this.buttonCreate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.buttonCreate.FlatAppearance.BorderSize = 0;
            this.buttonCreate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCreate.ForeColor = System.Drawing.Color.White;
            this.buttonCreate.Location = new System.Drawing.Point(257, 190);
            this.buttonCreate.Name = "buttonCreate";
            this.buttonCreate.Size = new System.Drawing.Size(75, 30);
            this.buttonCreate.TabIndex = 3;
            this.buttonCreate.Text = "สร้าง";
            this.buttonCreate.UseVisualStyleBackColor = false;
            this.buttonCreate.Click += new System.EventHandler(this.ButtonCreate_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(183)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.buttonCancel.FlatAppearance.BorderSize = 0;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCancel.ForeColor = System.Drawing.Color.White;
            this.buttonCancel.Location = new System.Drawing.Point(337, 190);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 30);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "ยกเลิก";
            this.buttonCancel.UseVisualStyleBackColor = false;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // CreateSerialDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.ClientSize = new System.Drawing.Size(424, 232);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonCreate);
            this.Controls.Add(this.groupBoxSerialInput);
            this.Controls.Add(this.labelModelName);
            this.Controls.Add(this.labelTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CreateSerialDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "สร้าง Serial Number ใหม่";
            this.groupBoxSerialInput.ResumeLayout(false);
            this.groupBoxSerialInput.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Label labelModelName;
        private System.Windows.Forms.GroupBox groupBoxSerialInput;
        private System.Windows.Forms.Label labelSerialNumber;
        private System.Windows.Forms.TextBox textBoxSerialNumber;
        private System.Windows.Forms.Label labelInstructions;
        private System.Windows.Forms.Button buttonCreate;
        private System.Windows.Forms.Button buttonCancel;
    }
}
