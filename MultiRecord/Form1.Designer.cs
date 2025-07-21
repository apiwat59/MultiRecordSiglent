namespace MultiRecord
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.panelChassis = new System.Windows.Forms.Panel();
            this.panelParameters = new System.Windows.Forms.Panel();
            this.buttonSetRelative = new System.Windows.Forms.Button();
            this.comboBoxSpeed = new System.Windows.Forms.ComboBox();
            this.labelSpeed = new System.Windows.Forms.Label();
            this.comboBoxRange = new System.Windows.Forms.ComboBox();
            this.labelRange = new System.Windows.Forms.Label();
            this.labelRelativeState = new System.Windows.Forms.Label();
            this.groupBoxRecord = new System.Windows.Forms.GroupBox();
            this.buttonClearTable = new System.Windows.Forms.Button();
            this.buttonDeleteRecord = new System.Windows.Forms.Button();
            this.buttonExportCsv = new System.Windows.Forms.Button();
            this.buttonRecord = new System.Windows.Forms.Button();
            this.dataGridViewRecords = new System.Windows.Forms.DataGridView();
            this.groupBoxSystem = new System.Windows.Forms.GroupBox();
            this.textBoxSystemInfo = new System.Windows.Forms.TextBox();
            this.buttonGetError = new System.Windows.Forms.Button();
            this.buttonReset = new System.Windows.Forms.Button();
            this.buttonGetIDN = new System.Windows.Forms.Button();
            this.groupBoxFunctions = new System.Windows.Forms.GroupBox();
            this.buttonMeasureDiode = new System.Windows.Forms.Button();
            this.buttonMeasureTemp = new System.Windows.Forms.Button();
            this.buttonMeasureFreq = new System.Windows.Forms.Button();
            this.buttonMeasureCap = new System.Windows.Forms.Button();
            this.buttonMeasureRes4W = new System.Windows.Forms.Button();
            this.buttonMeasureRes2W = new System.Windows.Forms.Button();
            this.buttonMeasureIAC = new System.Windows.Forms.Button();
            this.buttonMeasureIDC = new System.Windows.Forms.Button();
            this.buttonMeasureVAC = new System.Windows.Forms.Button();
            this.buttonMeasureVDC = new System.Windows.Forms.Button();
            this.panelScreen = new System.Windows.Forms.Panel();
            this.lblMeasurementType = new System.Windows.Forms.Label();
            this.lblUnit = new System.Windows.Forms.Label();
            this.lblReading = new System.Windows.Forms.Label();
            this.groupBoxConnection = new System.Windows.Forms.GroupBox();
            this.buttonDisconnect = new System.Windows.Forms.Button();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.labelStatus = new System.Windows.Forms.Label();
            this.textBoxPort = new System.Windows.Forms.TextBox();
            this.labelPort = new System.Windows.Forms.Label();
            this.textBoxIP = new System.Windows.Forms.TextBox();
            this.labelIP = new System.Windows.Forms.Label();
            this.groupBoxLog = new System.Windows.Forms.GroupBox();
            this.buttonClearLog = new System.Windows.Forms.Button();
            this.richTextBoxLog = new System.Windows.Forms.RichTextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.panelChassis.SuspendLayout();
            this.panelParameters.SuspendLayout();
            this.groupBoxRecord.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewRecords)).BeginInit();
            this.groupBoxSystem.SuspendLayout();
            this.groupBoxFunctions.SuspendLayout();
            this.panelScreen.SuspendLayout();
            this.groupBoxConnection.SuspendLayout();
            this.groupBoxLog.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelChassis
            // 
            this.panelChassis.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.panelChassis.Controls.Add(this.panelParameters);
            this.panelChassis.Controls.Add(this.groupBoxRecord);
            this.panelChassis.Controls.Add(this.groupBoxSystem);
            this.panelChassis.Controls.Add(this.groupBoxFunctions);
            this.panelChassis.Controls.Add(this.panelScreen);
            this.panelChassis.Controls.Add(this.groupBoxConnection);
            this.panelChassis.Controls.Add(this.groupBoxLog);
            this.panelChassis.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelChassis.Location = new System.Drawing.Point(0, 0);
            this.panelChassis.Name = "panelChassis";
            this.panelChassis.Size = new System.Drawing.Size(1528, 624);
            this.panelChassis.TabIndex = 0;
            // 
            // panelParameters
            // 
            this.panelParameters.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.panelParameters.Controls.Add(this.buttonSetRelative);
            this.panelParameters.Controls.Add(this.comboBoxSpeed);
            this.panelParameters.Controls.Add(this.labelSpeed);
            this.panelParameters.Controls.Add(this.comboBoxRange);
            this.panelParameters.Controls.Add(this.labelRange);
            this.panelParameters.Controls.Add(this.labelRelativeState);
            this.panelParameters.Location = new System.Drawing.Point(12, 344);
            this.panelParameters.Name = "panelParameters";
            this.panelParameters.Size = new System.Drawing.Size(721, 50);
            this.panelParameters.TabIndex = 6;
            // 
            // buttonSetRelative
            // 
            this.buttonSetRelative.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(151)))), ((int)(((byte)(234)))));
            this.buttonSetRelative.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.buttonSetRelative.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonSetRelative.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.buttonSetRelative.ForeColor = System.Drawing.Color.White;
            this.buttonSetRelative.Location = new System.Drawing.Point(544, 11);
            this.buttonSetRelative.Name = "buttonSetRelative";
            this.buttonSetRelative.Size = new System.Drawing.Size(90, 28);
            this.buttonSetRelative.TabIndex = 5;
            this.buttonSetRelative.Text = "Relative";
            this.buttonSetRelative.UseVisualStyleBackColor = false;
            this.buttonSetRelative.Click += new System.EventHandler(this.buttonSetRelative_Click);
            // 
            // comboBoxSpeed
            // 
            this.comboBoxSpeed.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.comboBoxSpeed.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSpeed.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBoxSpeed.ForeColor = System.Drawing.Color.White;
            this.comboBoxSpeed.FormattingEnabled = true;
            this.comboBoxSpeed.Location = new System.Drawing.Point(320, 14);
            this.comboBoxSpeed.Name = "comboBoxSpeed";
            this.comboBoxSpeed.Size = new System.Drawing.Size(121, 21);
            this.comboBoxSpeed.TabIndex = 3;
            this.comboBoxSpeed.SelectedIndexChanged += new System.EventHandler(this.comboBoxSpeed_SelectedIndexChanged);
            // 
            // labelSpeed
            // 
            this.labelSpeed.AutoSize = true;
            this.labelSpeed.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSpeed.ForeColor = System.Drawing.Color.White;
            this.labelSpeed.Location = new System.Drawing.Point(265, 15);
            this.labelSpeed.Name = "labelSpeed";
            this.labelSpeed.Size = new System.Drawing.Size(48, 17);
            this.labelSpeed.TabIndex = 2;
            this.labelSpeed.Text = "Speed:";
            // 
            // comboBoxRange
            // 
            this.comboBoxRange.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.comboBoxRange.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxRange.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBoxRange.ForeColor = System.Drawing.Color.White;
            this.comboBoxRange.FormattingEnabled = true;
            this.comboBoxRange.Location = new System.Drawing.Point(70, 14);
            this.comboBoxRange.Name = "comboBoxRange";
            this.comboBoxRange.Size = new System.Drawing.Size(160, 21);
            this.comboBoxRange.TabIndex = 1;
            this.comboBoxRange.SelectedIndexChanged += new System.EventHandler(this.comboBoxRange_SelectedIndexChanged);
            // 
            // labelRange
            // 
            this.labelRange.AutoSize = true;
            this.labelRange.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelRange.ForeColor = System.Drawing.Color.White;
            this.labelRange.Location = new System.Drawing.Point(15, 15);
            this.labelRange.Name = "labelRange";
            this.labelRange.Size = new System.Drawing.Size(48, 17);
            this.labelRange.TabIndex = 0;
            this.labelRange.Text = "Range:";
            // 
            // labelRelativeState
            // 
            this.labelRelativeState.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelRelativeState.ForeColor = System.Drawing.Color.Gainsboro;
            this.labelRelativeState.Location = new System.Drawing.Point(640, 11);
            this.labelRelativeState.Name = "labelRelativeState";
            this.labelRelativeState.Size = new System.Drawing.Size(100, 28);
            this.labelRelativeState.TabIndex = 6;
            this.labelRelativeState.Text = "REL: OFF";
            this.labelRelativeState.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // groupBoxRecord
            // 
            this.groupBoxRecord.Controls.Add(this.buttonClearTable);
            this.groupBoxRecord.Controls.Add(this.buttonDeleteRecord);
            this.groupBoxRecord.Controls.Add(this.buttonExportCsv);
            this.groupBoxRecord.Controls.Add(this.buttonRecord);
            this.groupBoxRecord.Controls.Add(this.dataGridViewRecords);
            this.groupBoxRecord.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.groupBoxRecord.ForeColor = System.Drawing.Color.White;
            this.groupBoxRecord.Location = new System.Drawing.Point(751, 12);
            this.groupBoxRecord.Name = "groupBoxRecord";
            this.groupBoxRecord.Size = new System.Drawing.Size(765, 600);
            this.groupBoxRecord.TabIndex = 5;
            this.groupBoxRecord.TabStop = false;
            this.groupBoxRecord.Text = "Data Recording";
            // 
            // buttonClearTable
            // 
            this.buttonClearTable.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.buttonClearTable.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.buttonClearTable.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonClearTable.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.buttonClearTable.Location = new System.Drawing.Point(675, 22);
            this.buttonClearTable.Name = "buttonClearTable";
            this.buttonClearTable.Size = new System.Drawing.Size(79, 33);
            this.buttonClearTable.TabIndex = 4;
            this.buttonClearTable.Text = "Clear All";
            this.buttonClearTable.UseVisualStyleBackColor = false;
            this.buttonClearTable.Click += new System.EventHandler(this.buttonClearTable_Click);
            // 
            // buttonDeleteRecord
            // 
            this.buttonDeleteRecord.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(186)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.buttonDeleteRecord.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.buttonDeleteRecord.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonDeleteRecord.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.buttonDeleteRecord.Location = new System.Drawing.Point(590, 22);
            this.buttonDeleteRecord.Name = "buttonDeleteRecord";
            this.buttonDeleteRecord.Size = new System.Drawing.Size(79, 33);
            this.buttonDeleteRecord.TabIndex = 3;
            this.buttonDeleteRecord.Text = "Delete";
            this.buttonDeleteRecord.UseVisualStyleBackColor = false;
            this.buttonDeleteRecord.Click += new System.EventHandler(this.buttonDeleteRecord_Click);
            // 
            // buttonExportCsv
            // 
            this.buttonExportCsv.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(106)))), ((int)(((byte)(153)))), ((int)(((byte)(78)))));
            this.buttonExportCsv.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.buttonExportCsv.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonExportCsv.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.buttonExportCsv.Location = new System.Drawing.Point(465, 22);
            this.buttonExportCsv.Name = "buttonExportCsv";
            this.buttonExportCsv.Size = new System.Drawing.Size(119, 33);
            this.buttonExportCsv.TabIndex = 2;
            this.buttonExportCsv.Text = "Export to CSV";
            this.buttonExportCsv.UseVisualStyleBackColor = false;
            this.buttonExportCsv.Click += new System.EventHandler(this.buttonExportCsv_Click);
            // 
            // buttonRecord
            // 
            this.buttonRecord.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(151)))), ((int)(((byte)(234)))));
            this.buttonRecord.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.buttonRecord.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonRecord.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.buttonRecord.Location = new System.Drawing.Point(340, 22);
            this.buttonRecord.Name = "buttonRecord";
            this.buttonRecord.Size = new System.Drawing.Size(119, 33);
            this.buttonRecord.TabIndex = 1;
            this.buttonRecord.Text = "Record Value";
            this.buttonRecord.UseVisualStyleBackColor = false;
            this.buttonRecord.Click += new System.EventHandler(this.buttonRecord_Click);
            // 
            // dataGridViewRecords
            // 
            this.dataGridViewRecords.AllowUserToAddRows = false;
            this.dataGridViewRecords.AllowUserToDeleteRows = false;
            this.dataGridViewRecords.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewRecords.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.dataGridViewRecords.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewRecords.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewRecords.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewRecords.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridViewRecords.EnableHeadersVisualStyles = false;
            this.dataGridViewRecords.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.dataGridViewRecords.Location = new System.Drawing.Point(9, 61);
            this.dataGridViewRecords.Name = "dataGridViewRecords";
            this.dataGridViewRecords.ReadOnly = true;
            this.dataGridViewRecords.RowHeadersVisible = false;
            this.dataGridViewRecords.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewRecords.Size = new System.Drawing.Size(745, 520);
            this.dataGridViewRecords.TabIndex = 0;
            // 
            // groupBoxSystem
            // 
            this.groupBoxSystem.Controls.Add(this.textBoxSystemInfo);
            this.groupBoxSystem.Controls.Add(this.buttonGetError);
            this.groupBoxSystem.Controls.Add(this.buttonReset);
            this.groupBoxSystem.Controls.Add(this.buttonGetIDN);
            this.groupBoxSystem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxSystem.ForeColor = System.Drawing.Color.White;
            this.groupBoxSystem.Location = new System.Drawing.Point(12, 238);
            this.groupBoxSystem.Name = "groupBoxSystem";
            this.groupBoxSystem.Size = new System.Drawing.Size(460, 100);
            this.groupBoxSystem.TabIndex = 4;
            this.groupBoxSystem.TabStop = false;
            this.groupBoxSystem.Text = "System Commands";
            // 
            // textBoxSystemInfo
            // 
            this.textBoxSystemInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.textBoxSystemInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxSystemInfo.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxSystemInfo.ForeColor = System.Drawing.Color.Gainsboro;
            this.textBoxSystemInfo.Location = new System.Drawing.Point(9, 61);
            this.textBoxSystemInfo.Name = "textBoxSystemInfo";
            this.textBoxSystemInfo.ReadOnly = true;
            this.textBoxSystemInfo.Size = new System.Drawing.Size(445, 22);
            this.textBoxSystemInfo.TabIndex = 3;
            // 
            // buttonGetError
            // 
            this.buttonGetError.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.buttonGetError.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.buttonGetError.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonGetError.Location = new System.Drawing.Point(191, 22);
            this.buttonGetError.Name = "buttonGetError";
            this.buttonGetError.Size = new System.Drawing.Size(85, 33);
            this.buttonGetError.TabIndex = 2;
            this.buttonGetError.Text = "Get Error";
            this.buttonGetError.UseVisualStyleBackColor = false;
            this.buttonGetError.Click += new System.EventHandler(this.buttonGetError_Click);
            // 
            // buttonReset
            // 
            this.buttonReset.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.buttonReset.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.buttonReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonReset.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonReset.Location = new System.Drawing.Point(100, 22);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(85, 33);
            this.buttonReset.TabIndex = 1;
            this.buttonReset.Text = "Reset";
            this.buttonReset.UseVisualStyleBackColor = false;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // buttonGetIDN
            // 
            this.buttonGetIDN.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.buttonGetIDN.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.buttonGetIDN.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonGetIDN.Location = new System.Drawing.Point(9, 22);
            this.buttonGetIDN.Name = "buttonGetIDN";
            this.buttonGetIDN.Size = new System.Drawing.Size(85, 33);
            this.buttonGetIDN.TabIndex = 0;
            this.buttonGetIDN.Text = "Get IDN";
            this.buttonGetIDN.UseVisualStyleBackColor = false;
            this.buttonGetIDN.Click += new System.EventHandler(this.buttonGetIDN_Click);
            // 
            // groupBoxFunctions
            // 
            this.groupBoxFunctions.Controls.Add(this.buttonMeasureDiode);
            this.groupBoxFunctions.Controls.Add(this.buttonMeasureTemp);
            this.groupBoxFunctions.Controls.Add(this.buttonMeasureFreq);
            this.groupBoxFunctions.Controls.Add(this.buttonMeasureCap);
            this.groupBoxFunctions.Controls.Add(this.buttonMeasureRes4W);
            this.groupBoxFunctions.Controls.Add(this.buttonMeasureRes2W);
            this.groupBoxFunctions.Controls.Add(this.buttonMeasureIAC);
            this.groupBoxFunctions.Controls.Add(this.buttonMeasureIDC);
            this.groupBoxFunctions.Controls.Add(this.buttonMeasureVAC);
            this.groupBoxFunctions.Controls.Add(this.buttonMeasureVDC);
            this.groupBoxFunctions.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxFunctions.ForeColor = System.Drawing.Color.White;
            this.groupBoxFunctions.Location = new System.Drawing.Point(478, 12);
            this.groupBoxFunctions.Name = "groupBoxFunctions";
            this.groupBoxFunctions.Size = new System.Drawing.Size(255, 326);
            this.groupBoxFunctions.TabIndex = 3;
            this.groupBoxFunctions.TabStop = false;
            this.groupBoxFunctions.Text = "Measurement Functions";
            // 
            // buttonMeasureDiode
            // 
            this.buttonMeasureDiode.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.buttonMeasureDiode.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.buttonMeasureDiode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonMeasureDiode.Location = new System.Drawing.Point(129, 185);
            this.buttonMeasureDiode.Name = "buttonMeasureDiode";
            this.buttonMeasureDiode.Size = new System.Drawing.Size(104, 35);
            this.buttonMeasureDiode.TabIndex = 9;
            this.buttonMeasureDiode.Text = "Diode Test";
            this.buttonMeasureDiode.UseVisualStyleBackColor = false;
            this.buttonMeasureDiode.Click += new System.EventHandler(this.buttonMeasureDiode_Click);
            // 
            // buttonMeasureTemp
            // 
            this.buttonMeasureTemp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.buttonMeasureTemp.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.buttonMeasureTemp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonMeasureTemp.Location = new System.Drawing.Point(15, 185);
            this.buttonMeasureTemp.Name = "buttonMeasureTemp";
            this.buttonMeasureTemp.Size = new System.Drawing.Size(108, 35);
            this.buttonMeasureTemp.TabIndex = 8;
            this.buttonMeasureTemp.Text = "Temperature";
            this.buttonMeasureTemp.UseVisualStyleBackColor = false;
            this.buttonMeasureTemp.Click += new System.EventHandler(this.buttonMeasureTemp_Click);
            // 
            // buttonMeasureFreq
            // 
            this.buttonMeasureFreq.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.buttonMeasureFreq.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.buttonMeasureFreq.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonMeasureFreq.Location = new System.Drawing.Point(129, 144);
            this.buttonMeasureFreq.Name = "buttonMeasureFreq";
            this.buttonMeasureFreq.Size = new System.Drawing.Size(104, 35);
            this.buttonMeasureFreq.TabIndex = 7;
            this.buttonMeasureFreq.Text = "Frequency";
            this.buttonMeasureFreq.UseVisualStyleBackColor = false;
            this.buttonMeasureFreq.Click += new System.EventHandler(this.buttonMeasureFreq_Click);
            // 
            // buttonMeasureCap
            // 
            this.buttonMeasureCap.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.buttonMeasureCap.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.buttonMeasureCap.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonMeasureCap.Location = new System.Drawing.Point(15, 144);
            this.buttonMeasureCap.Name = "buttonMeasureCap";
            this.buttonMeasureCap.Size = new System.Drawing.Size(108, 35);
            this.buttonMeasureCap.TabIndex = 6;
            this.buttonMeasureCap.Text = "Capacitance";
            this.buttonMeasureCap.UseVisualStyleBackColor = false;
            this.buttonMeasureCap.Click += new System.EventHandler(this.buttonMeasureCap_Click);
            // 
            // buttonMeasureRes4W
            // 
            this.buttonMeasureRes4W.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.buttonMeasureRes4W.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.buttonMeasureRes4W.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonMeasureRes4W.Location = new System.Drawing.Point(129, 103);
            this.buttonMeasureRes4W.Name = "buttonMeasureRes4W";
            this.buttonMeasureRes4W.Size = new System.Drawing.Size(104, 35);
            this.buttonMeasureRes4W.TabIndex = 5;
            this.buttonMeasureRes4W.Text = "Resistance (4W)";
            this.buttonMeasureRes4W.UseVisualStyleBackColor = false;
            this.buttonMeasureRes4W.Click += new System.EventHandler(this.buttonMeasureRes4W_Click);
            // 
            // buttonMeasureRes2W
            // 
            this.buttonMeasureRes2W.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.buttonMeasureRes2W.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.buttonMeasureRes2W.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonMeasureRes2W.Location = new System.Drawing.Point(15, 103);
            this.buttonMeasureRes2W.Name = "buttonMeasureRes2W";
            this.buttonMeasureRes2W.Size = new System.Drawing.Size(108, 35);
            this.buttonMeasureRes2W.TabIndex = 4;
            this.buttonMeasureRes2W.Text = "Resistance (2W)";
            this.buttonMeasureRes2W.UseVisualStyleBackColor = false;
            this.buttonMeasureRes2W.Click += new System.EventHandler(this.buttonMeasureRes2W_Click);
            // 
            // buttonMeasureIAC
            // 
            this.buttonMeasureIAC.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.buttonMeasureIAC.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.buttonMeasureIAC.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonMeasureIAC.Location = new System.Drawing.Point(129, 62);
            this.buttonMeasureIAC.Name = "buttonMeasureIAC";
            this.buttonMeasureIAC.Size = new System.Drawing.Size(104, 35);
            this.buttonMeasureIAC.TabIndex = 3;
            this.buttonMeasureIAC.Text = "Current AC";
            this.buttonMeasureIAC.UseVisualStyleBackColor = false;
            this.buttonMeasureIAC.Click += new System.EventHandler(this.buttonMeasureIAC_Click);
            // 
            // buttonMeasureIDC
            // 
            this.buttonMeasureIDC.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.buttonMeasureIDC.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.buttonMeasureIDC.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonMeasureIDC.Location = new System.Drawing.Point(15, 62);
            this.buttonMeasureIDC.Name = "buttonMeasureIDC";
            this.buttonMeasureIDC.Size = new System.Drawing.Size(108, 35);
            this.buttonMeasureIDC.TabIndex = 2;
            this.buttonMeasureIDC.Text = "Current DC";
            this.buttonMeasureIDC.UseVisualStyleBackColor = false;
            this.buttonMeasureIDC.Click += new System.EventHandler(this.buttonMeasureIDC_Click);
            // 
            // buttonMeasureVAC
            // 
            this.buttonMeasureVAC.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.buttonMeasureVAC.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.buttonMeasureVAC.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonMeasureVAC.Location = new System.Drawing.Point(129, 22);
            this.buttonMeasureVAC.Name = "buttonMeasureVAC";
            this.buttonMeasureVAC.Size = new System.Drawing.Size(104, 35);
            this.buttonMeasureVAC.TabIndex = 1;
            this.buttonMeasureVAC.Text = "Voltage AC";
            this.buttonMeasureVAC.UseVisualStyleBackColor = false;
            this.buttonMeasureVAC.Click += new System.EventHandler(this.buttonMeasureVAC_Click);
            // 
            // buttonMeasureVDC
            // 
            this.buttonMeasureVDC.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.buttonMeasureVDC.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.buttonMeasureVDC.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonMeasureVDC.Location = new System.Drawing.Point(15, 22);
            this.buttonMeasureVDC.Name = "buttonMeasureVDC";
            this.buttonMeasureVDC.Size = new System.Drawing.Size(108, 35);
            this.buttonMeasureVDC.TabIndex = 0;
            this.buttonMeasureVDC.Text = "Voltage DC";
            this.buttonMeasureVDC.UseVisualStyleBackColor = false;
            this.buttonMeasureVDC.Click += new System.EventHandler(this.buttonMeasureVDC_Click);
            // 
            // panelScreen
            // 
            this.panelScreen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(62)))), ((int)(((byte)(76)))));
            this.panelScreen.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelScreen.Controls.Add(this.lblMeasurementType);
            this.panelScreen.Controls.Add(this.lblUnit);
            this.panelScreen.Controls.Add(this.lblReading);
            this.panelScreen.Location = new System.Drawing.Point(12, 12);
            this.panelScreen.Name = "panelScreen";
            this.panelScreen.Size = new System.Drawing.Size(460, 110);
            this.panelScreen.TabIndex = 2;
            // 
            // lblMeasurementType
            // 
            this.lblMeasurementType.AutoSize = true;
            this.lblMeasurementType.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMeasurementType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(214)))), ((int)(((byte)(233)))));
            this.lblMeasurementType.Location = new System.Drawing.Point(10, 10);
            this.lblMeasurementType.Name = "lblMeasurementType";
            this.lblMeasurementType.Size = new System.Drawing.Size(111, 20);
            this.lblMeasurementType.TabIndex = 2;
            this.lblMeasurementType.Text = "NO FUNCTION";
            // 
            // lblUnit
            // 
            this.lblUnit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblUnit.Font = new System.Drawing.Font("Consolas", 27.75F, System.Drawing.FontStyle.Bold);
            this.lblUnit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(247)))), ((int)(((byte)(255)))));
            this.lblUnit.Location = new System.Drawing.Point(365, 40);
            this.lblUnit.Name = "lblUnit";
            this.lblUnit.Size = new System.Drawing.Size(84, 58);
            this.lblUnit.TabIndex = 1;
            this.lblUnit.Text = "V";
            this.lblUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblReading
            // 
            this.lblReading.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblReading.Font = new System.Drawing.Font("Consolas", 48F, System.Drawing.FontStyle.Bold);
            this.lblReading.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(247)))), ((int)(((byte)(255)))));
            this.lblReading.Location = new System.Drawing.Point(3, 30);
            this.lblReading.Name = "lblReading";
            this.lblReading.Size = new System.Drawing.Size(370, 76);
            this.lblReading.TabIndex = 0;
            this.lblReading.Text = "0.000000";
            this.lblReading.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBoxConnection
            // 
            this.groupBoxConnection.Controls.Add(this.buttonDisconnect);
            this.groupBoxConnection.Controls.Add(this.buttonConnect);
            this.groupBoxConnection.Controls.Add(this.labelStatus);
            this.groupBoxConnection.Controls.Add(this.textBoxPort);
            this.groupBoxConnection.Controls.Add(this.labelPort);
            this.groupBoxConnection.Controls.Add(this.textBoxIP);
            this.groupBoxConnection.Controls.Add(this.labelIP);
            this.groupBoxConnection.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxConnection.ForeColor = System.Drawing.Color.White;
            this.groupBoxConnection.Location = new System.Drawing.Point(12, 128);
            this.groupBoxConnection.Name = "groupBoxConnection";
            this.groupBoxConnection.Size = new System.Drawing.Size(460, 104);
            this.groupBoxConnection.TabIndex = 1;
            this.groupBoxConnection.TabStop = false;
            this.groupBoxConnection.Text = "Connection";
            // 
            // buttonDisconnect
            // 
            this.buttonDisconnect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.buttonDisconnect.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.buttonDisconnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonDisconnect.Location = new System.Drawing.Point(350, 57);
            this.buttonDisconnect.Name = "buttonDisconnect";
            this.buttonDisconnect.Size = new System.Drawing.Size(100, 33);
            this.buttonDisconnect.TabIndex = 6;
            this.buttonDisconnect.Text = "Disconnect";
            this.buttonDisconnect.UseVisualStyleBackColor = false;
            this.buttonDisconnect.Click += new System.EventHandler(this.buttonDisconnect_Click);
            // 
            // buttonConnect
            // 
            this.buttonConnect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(151)))), ((int)(((byte)(234)))));
            this.buttonConnect.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.buttonConnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonConnect.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonConnect.Location = new System.Drawing.Point(350, 18);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(100, 33);
            this.buttonConnect.TabIndex = 5;
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.UseVisualStyleBackColor = false;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // labelStatus
            // 
            this.labelStatus.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStatus.ForeColor = System.Drawing.Color.OrangeRed;
            this.labelStatus.Location = new System.Drawing.Point(9, 57);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(325, 33);
            this.labelStatus.TabIndex = 4;
            this.labelStatus.Text = "สถานะ: ไม่ได้เชื่อมต่อ";
            this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxPort
            // 
            this.textBoxPort.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.textBoxPort.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxPort.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxPort.ForeColor = System.Drawing.Color.White;
            this.textBoxPort.Location = new System.Drawing.Point(234, 22);
            this.textBoxPort.Name = "textBoxPort";
            this.textBoxPort.Size = new System.Drawing.Size(100, 25);
            this.textBoxPort.TabIndex = 3;
            this.textBoxPort.Text = "5025";
            this.textBoxPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // labelPort
            // 
            this.labelPort.AutoSize = true;
            this.labelPort.Location = new System.Drawing.Point(197, 27);
            this.labelPort.Name = "labelPort";
            this.labelPort.Size = new System.Drawing.Size(32, 15);
            this.labelPort.TabIndex = 2;
            this.labelPort.Text = "Port:";
            // 
            // textBoxIP
            // 
            this.textBoxIP.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.textBoxIP.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxIP.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxIP.ForeColor = System.Drawing.Color.White;
            this.textBoxIP.Location = new System.Drawing.Point(79, 22);
            this.textBoxIP.Name = "textBoxIP";
            this.textBoxIP.Size = new System.Drawing.Size(112, 25);
            this.textBoxIP.TabIndex = 1;
            this.textBoxIP.Text = "10.11.13.220";
            this.textBoxIP.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // labelIP
            // 
            this.labelIP.AutoSize = true;
            this.labelIP.Location = new System.Drawing.Point(6, 27);
            this.labelIP.Name = "labelIP";
            this.labelIP.Size = new System.Drawing.Size(65, 15);
            this.labelIP.TabIndex = 0;
            this.labelIP.Text = "IP Address:";
            // 
            // groupBoxLog
            // 
            this.groupBoxLog.Controls.Add(this.buttonClearLog);
            this.groupBoxLog.Controls.Add(this.richTextBoxLog);
            this.groupBoxLog.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxLog.ForeColor = System.Drawing.Color.White;
            this.groupBoxLog.Location = new System.Drawing.Point(12, 410);
            this.groupBoxLog.Name = "groupBoxLog";
            this.groupBoxLog.Size = new System.Drawing.Size(721, 202);
            this.groupBoxLog.TabIndex = 0;
            this.groupBoxLog.TabStop = false;
            this.groupBoxLog.Text = "Activity Log";
            // 
            // buttonClearLog
            // 
            this.buttonClearLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.buttonClearLog.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.buttonClearLog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonClearLog.Location = new System.Drawing.Point(620, 12);
            this.buttonClearLog.Name = "buttonClearLog";
            this.buttonClearLog.Size = new System.Drawing.Size(79, 25);
            this.buttonClearLog.TabIndex = 1;
            this.buttonClearLog.Text = "Clear Log";
            this.buttonClearLog.UseVisualStyleBackColor = false;
            this.buttonClearLog.Click += new System.EventHandler(this.buttonClearLog_Click);
            // 
            // richTextBoxLog
            // 
            this.richTextBoxLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.richTextBoxLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxLog.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxLog.ForeColor = System.Drawing.Color.Gainsboro;
            this.richTextBoxLog.Location = new System.Drawing.Point(9, 43);
            this.richTextBoxLog.Name = "richTextBoxLog";
            this.richTextBoxLog.ReadOnly = true;
            this.richTextBoxLog.Size = new System.Drawing.Size(690, 140);
            this.richTextBoxLog.TabIndex = 0;
            this.richTextBoxLog.Text = "";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1528, 624);
            this.Controls.Add(this.panelChassis);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SIGLENT SDM3055 Remote Control";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.panelChassis.ResumeLayout(false);
            this.panelParameters.ResumeLayout(false);
            this.panelParameters.PerformLayout();
            this.groupBoxRecord.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewRecords)).EndInit();
            this.groupBoxSystem.ResumeLayout(false);
            this.groupBoxSystem.PerformLayout();
            this.groupBoxFunctions.ResumeLayout(false);
            this.panelScreen.ResumeLayout(false);
            this.panelScreen.PerformLayout();
            this.groupBoxConnection.ResumeLayout(false);
            this.groupBoxConnection.PerformLayout();
            this.groupBoxLog.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelChassis;
        private System.Windows.Forms.GroupBox groupBoxLog;
        private System.Windows.Forms.RichTextBox richTextBoxLog;
        private System.Windows.Forms.Panel panelScreen;
        private System.Windows.Forms.GroupBox groupBoxConnection;
        private System.Windows.Forms.GroupBox groupBoxFunctions;
        private System.Windows.Forms.GroupBox groupBoxSystem;
        private System.Windows.Forms.Button buttonClearLog;
        private System.Windows.Forms.Label lblReading;
        private System.Windows.Forms.Label lblUnit;
        private System.Windows.Forms.Label lblMeasurementType;
        private System.Windows.Forms.TextBox textBoxIP;
        private System.Windows.Forms.Label labelIP;
        private System.Windows.Forms.TextBox textBoxPort;
        private System.Windows.Forms.Label labelPort;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.Button buttonDisconnect;
        private System.Windows.Forms.Button buttonMeasureVDC;
        private System.Windows.Forms.Button buttonMeasureDiode;
        private System.Windows.Forms.Button buttonMeasureTemp;
        private System.Windows.Forms.Button buttonMeasureFreq;
        private System.Windows.Forms.Button buttonMeasureCap;
        private System.Windows.Forms.Button buttonMeasureRes4W;
        private System.Windows.Forms.Button buttonMeasureRes2W;
        private System.Windows.Forms.Button buttonMeasureIAC;
        private System.Windows.Forms.Button buttonMeasureIDC;
        private System.Windows.Forms.Button buttonMeasureVAC;
        private System.Windows.Forms.Button buttonGetError;
        private System.Windows.Forms.Button buttonReset;
        private System.Windows.Forms.Button buttonGetIDN;
        private System.Windows.Forms.TextBox textBoxSystemInfo;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.GroupBox groupBoxRecord;
        private System.Windows.Forms.Button buttonExportCsv;
        private System.Windows.Forms.Button buttonRecord;
        private System.Windows.Forms.DataGridView dataGridViewRecords;
        private System.Windows.Forms.Button buttonDeleteRecord;
        private System.Windows.Forms.Button buttonClearTable;
        private System.Windows.Forms.Panel panelParameters;
        private System.Windows.Forms.Button buttonSetRelative;
        private System.Windows.Forms.ComboBox comboBoxSpeed;
        private System.Windows.Forms.Label labelSpeed;
        private System.Windows.Forms.ComboBox comboBoxRange;
        private System.Windows.Forms.Label labelRange;
        private System.Windows.Forms.Label labelRelativeState;
    }
}