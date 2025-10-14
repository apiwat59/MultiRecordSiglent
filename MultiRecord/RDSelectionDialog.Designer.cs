namespace MultiRecord
{
    partial class RDSelectionDialog
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
            this.groupBoxModelSelection = new System.Windows.Forms.GroupBox();
            this.listBoxModelResults = new System.Windows.Forms.ListBox();
            this.labelModel = new System.Windows.Forms.Label();
            this.textBoxModelSearch = new System.Windows.Forms.TextBox();
            this.groupBoxAction = new System.Windows.Forms.GroupBox();
            this.buttonCreateNew = new System.Windows.Forms.Button();
            this.buttonLoadExisting = new System.Windows.Forms.Button();
            this.groupBoxExistingSerial = new System.Windows.Forms.GroupBox();
            this.labelSerialNumber = new System.Windows.Forms.Label();
            this.comboBoxSerialNumbers = new System.Windows.Forms.ComboBox();
            this.labelSerialInfo = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelTitle = new System.Windows.Forms.Label();
            this.groupBoxModelSelection.SuspendLayout();
            this.groupBoxAction.SuspendLayout();
            this.groupBoxExistingSerial.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxModelSelection
            // 
            this.groupBoxModelSelection.Controls.Add(this.listBoxModelResults);
            this.groupBoxModelSelection.Controls.Add(this.labelModel);
            this.groupBoxModelSelection.Controls.Add(this.textBoxModelSearch);
            this.groupBoxModelSelection.ForeColor = System.Drawing.Color.White;
            this.groupBoxModelSelection.Location = new System.Drawing.Point(12, 50);
            this.groupBoxModelSelection.Name = "groupBoxModelSelection";
            this.groupBoxModelSelection.Size = new System.Drawing.Size(460, 180);
            this.groupBoxModelSelection.TabIndex = 0;
            this.groupBoxModelSelection.TabStop = false;
            this.groupBoxModelSelection.Text = "ค้นหา Model";
            // 
            // labelModel
            // 
            this.labelModel.AutoSize = true;
            this.labelModel.ForeColor = System.Drawing.Color.White;
            this.labelModel.Location = new System.Drawing.Point(15, 25);
            this.labelModel.Name = "labelModel";
            this.labelModel.Size = new System.Drawing.Size(39, 13);
            this.labelModel.TabIndex = 0;
            this.labelModel.Text = "Model:";
            // 
            // textBoxModelSearch
            // 
            this.textBoxModelSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.textBoxModelSearch.ForeColor = System.Drawing.Color.White;
            this.textBoxModelSearch.Location = new System.Drawing.Point(70, 22);
            this.textBoxModelSearch.Name = "textBoxModelSearch";
            this.textBoxModelSearch.Size = new System.Drawing.Size(370, 20);
            this.textBoxModelSearch.TabIndex = 1;
            this.textBoxModelSearch.TextChanged += new System.EventHandler(this.TextBoxModelSearch_TextChanged);
            // 
            // listBoxModelResults
            // 
            this.listBoxModelResults.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.listBoxModelResults.ForeColor = System.Drawing.Color.White;
            this.listBoxModelResults.FormattingEnabled = true;
            this.listBoxModelResults.Location = new System.Drawing.Point(70, 48);
            this.listBoxModelResults.Name = "listBoxModelResults";
            this.listBoxModelResults.Size = new System.Drawing.Size(370, 121);
            this.listBoxModelResults.TabIndex = 2;
            this.listBoxModelResults.Visible = false;
            this.listBoxModelResults.SelectedIndexChanged += new System.EventHandler(this.ListBoxModelResults_SelectedIndexChanged);
            this.listBoxModelResults.DoubleClick += new System.EventHandler(this.ListBoxModelResults_DoubleClick);
            // 
            // groupBoxAction
            // 
            this.groupBoxAction.Controls.Add(this.buttonCreateNew);
            this.groupBoxAction.Controls.Add(this.buttonLoadExisting);
            this.groupBoxAction.ForeColor = System.Drawing.Color.White;
            this.groupBoxAction.Location = new System.Drawing.Point(12, 350);
            this.groupBoxAction.Name = "groupBoxAction";
            this.groupBoxAction.Size = new System.Drawing.Size(460, 80);
            this.groupBoxAction.TabIndex = 2;
            this.groupBoxAction.TabStop = false;
            this.groupBoxAction.Text = "เลือกการดำเนินการ";
            // 
            // buttonCreateNew
            // 
            this.buttonCreateNew.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.buttonCreateNew.FlatAppearance.BorderSize = 0;
            this.buttonCreateNew.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCreateNew.ForeColor = System.Drawing.Color.White;
            this.buttonCreateNew.Location = new System.Drawing.Point(15, 25);
            this.buttonCreateNew.Name = "buttonCreateNew";
            this.buttonCreateNew.Size = new System.Drawing.Size(200, 40);
            this.buttonCreateNew.TabIndex = 0;
            this.buttonCreateNew.Text = "สร้าง Serial Number ใหม่";
            this.buttonCreateNew.UseVisualStyleBackColor = false;
            this.buttonCreateNew.Click += new System.EventHandler(this.ButtonCreateNew_Click);
            // 
            // buttonLoadExisting
            // 
            this.buttonLoadExisting.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(125)))), ((int)(((byte)(50)))));
            this.buttonLoadExisting.Enabled = false;
            this.buttonLoadExisting.FlatAppearance.BorderSize = 0;
            this.buttonLoadExisting.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonLoadExisting.ForeColor = System.Drawing.Color.White;
            this.buttonLoadExisting.Location = new System.Drawing.Point(240, 25);
            this.buttonLoadExisting.Name = "buttonLoadExisting";
            this.buttonLoadExisting.Size = new System.Drawing.Size(200, 40);
            this.buttonLoadExisting.TabIndex = 1;
            this.buttonLoadExisting.Text = "โหลด Serial Number ที่มีอยู่";
            this.buttonLoadExisting.UseVisualStyleBackColor = false;
            this.buttonLoadExisting.Click += new System.EventHandler(this.ButtonLoadExisting_Click);
            // 
            // groupBoxExistingSerial
            // 
            this.groupBoxExistingSerial.Controls.Add(this.labelSerialNumber);
            this.groupBoxExistingSerial.Controls.Add(this.comboBoxSerialNumbers);
            this.groupBoxExistingSerial.Controls.Add(this.labelSerialInfo);
            this.groupBoxExistingSerial.ForeColor = System.Drawing.Color.White;
            this.groupBoxExistingSerial.Location = new System.Drawing.Point(12, 240);
            this.groupBoxExistingSerial.Name = "groupBoxExistingSerial";
            this.groupBoxExistingSerial.Size = new System.Drawing.Size(460, 100);
            this.groupBoxExistingSerial.TabIndex = 1;
            this.groupBoxExistingSerial.TabStop = false;
            this.groupBoxExistingSerial.Text = "Serial Numbers ที่มีอยู่";
            // 
            // labelSerialNumber
            // 
            this.labelSerialNumber.AutoSize = true;
            this.labelSerialNumber.ForeColor = System.Drawing.Color.White;
            this.labelSerialNumber.Location = new System.Drawing.Point(15, 25);
            this.labelSerialNumber.Name = "labelSerialNumber";
            this.labelSerialNumber.Size = new System.Drawing.Size(77, 13);
            this.labelSerialNumber.TabIndex = 0;
            this.labelSerialNumber.Text = "Serial Number:";
            // 
            // comboBoxSerialNumbers
            // 
            this.comboBoxSerialNumbers.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.comboBoxSerialNumbers.Enabled = false;
            this.comboBoxSerialNumbers.ForeColor = System.Drawing.Color.White;
            this.comboBoxSerialNumbers.FormattingEnabled = true;
            this.comboBoxSerialNumbers.Location = new System.Drawing.Point(110, 22);
            this.comboBoxSerialNumbers.Name = "comboBoxSerialNumbers";
            this.comboBoxSerialNumbers.Size = new System.Drawing.Size(330, 21);
            this.comboBoxSerialNumbers.TabIndex = 1;
            // 
            // labelSerialInfo
            // 
            this.labelSerialInfo.AutoSize = true;
            this.labelSerialInfo.ForeColor = System.Drawing.Color.Gray;
            this.labelSerialInfo.Location = new System.Drawing.Point(15, 55);
            this.labelSerialInfo.Name = "labelSerialInfo";
            this.labelSerialInfo.Size = new System.Drawing.Size(142, 13);
            this.labelSerialInfo.TabIndex = 2;
            this.labelSerialInfo.Text = "กรุณาเลือก Model ก่อน...";
            // 
            // buttonCancel
            // 
            this.buttonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(183)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.buttonCancel.FlatAppearance.BorderSize = 0;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCancel.ForeColor = System.Drawing.Color.White;
            this.buttonCancel.Location = new System.Drawing.Point(397, 450);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 30);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "ยกเลิก";
            this.buttonCancel.UseVisualStyleBackColor = false;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // labelTitle
            // 
            this.labelTitle.AutoSize = true;
            this.labelTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTitle.ForeColor = System.Drawing.Color.White;
            this.labelTitle.Location = new System.Drawing.Point(12, 15);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(258, 20);
            this.labelTitle.TabIndex = 4;
            this.labelTitle.Text = "เลือก Model และ Serial Number";
            // 
            // RDSelectionDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.ClientSize = new System.Drawing.Size(484, 492);
            this.Controls.Add(this.labelTitle);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.groupBoxExistingSerial);
            this.Controls.Add(this.groupBoxAction);
            this.Controls.Add(this.groupBoxModelSelection);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RDSelectionDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "R&D - เลือก Model และ Serial Number";
            this.groupBoxModelSelection.ResumeLayout(false);
            this.groupBoxModelSelection.PerformLayout();
            this.groupBoxAction.ResumeLayout(false);
            this.groupBoxExistingSerial.ResumeLayout(false);
            this.groupBoxExistingSerial.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxModelSelection;
        private System.Windows.Forms.Label labelModel;
        private System.Windows.Forms.TextBox textBoxModelSearch;
        private System.Windows.Forms.ListBox listBoxModelResults;
        private System.Windows.Forms.GroupBox groupBoxAction;
        private System.Windows.Forms.Button buttonCreateNew;
        private System.Windows.Forms.Button buttonLoadExisting;
        private System.Windows.Forms.GroupBox groupBoxExistingSerial;
        private System.Windows.Forms.Label labelSerialNumber;
        private System.Windows.Forms.ComboBox comboBoxSerialNumbers;
        private System.Windows.Forms.Label labelSerialInfo;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelTitle;
    }
}
