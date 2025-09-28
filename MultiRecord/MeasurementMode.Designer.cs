namespace MultiRecord
{
    partial class MeasurementMode
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panelMain = new System.Windows.Forms.Panel();
            this.groupBoxQWRP = new System.Windows.Forms.GroupBox();
            this.buttonLoadQWRP = new System.Windows.Forms.Button();
            this.textBoxQWRPID = new System.Windows.Forms.TextBox();
            this.labelQWRPID = new System.Windows.Forms.Label();
            this.groupBoxCurrentMeasurement = new System.Windows.Forms.GroupBox();
            this.labelLastResult = new System.Windows.Forms.Label();
            this.buttonRecord = new System.Windows.Forms.Button();
            this.labelDescription = new System.Windows.Forms.Label();
            this.labelTolerance = new System.Windows.Forms.Label();
            this.labelTargetValue = new System.Windows.Forms.Label();
            this.labelMeasurementType = new System.Windows.Forms.Label();
            this.labelCurrentPosition = new System.Windows.Forms.Label();
            this.panelRealTimeDisplay = new System.Windows.Forms.Panel();
            this.lblCurrentReading = new System.Windows.Forms.Label();
            this.lblCurrentUnit = new System.Windows.Forms.Label();
            this.lblCurrentFunction = new System.Windows.Forms.Label();
            this.groupBoxProgress = new System.Windows.Forms.GroupBox();
            this.buttonReset = new System.Windows.Forms.Button();
            this.buttonNext = new System.Windows.Forms.Button();
            this.buttonPrevious = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.labelProgress = new System.Windows.Forms.Label();
            this.groupBoxMeasurementList = new System.Windows.Forms.GroupBox();
            this.dataGridViewPoints = new System.Windows.Forms.DataGridView();
            this.Position = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MeasurementType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TargetValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Tolerance = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Description = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Status = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panelMain.SuspendLayout();
            this.groupBoxQWRP.SuspendLayout();
            this.groupBoxCurrentMeasurement.SuspendLayout();
            this.panelRealTimeDisplay.SuspendLayout();
            this.groupBoxProgress.SuspendLayout();
            this.groupBoxMeasurementList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPoints)).BeginInit();
            this.SuspendLayout();
            // 
            // panelMain
            // 
            this.panelMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.panelMain.Controls.Add(this.groupBoxMeasurementList);
            this.panelMain.Controls.Add(this.groupBoxProgress);
            this.panelMain.Controls.Add(this.groupBoxCurrentMeasurement);
            this.panelMain.Controls.Add(this.groupBoxQWRP);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Name = "panelMain";
            this.panelMain.Padding = new System.Windows.Forms.Padding(10);
            this.panelMain.Size = new System.Drawing.Size(1000, 700);
            this.panelMain.TabIndex = 0;
            // 
            // groupBoxQWRP
            // 
            this.groupBoxQWRP.Controls.Add(this.buttonLoadQWRP);
            this.groupBoxQWRP.Controls.Add(this.textBoxQWRPID);
            this.groupBoxQWRP.Controls.Add(this.labelQWRPID);
            this.groupBoxQWRP.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxQWRP.ForeColor = System.Drawing.Color.White;
            this.groupBoxQWRP.Location = new System.Drawing.Point(20, 20);
            this.groupBoxQWRP.Name = "groupBoxQWRP";
            this.groupBoxQWRP.Size = new System.Drawing.Size(960, 80);
            this.groupBoxQWRP.TabIndex = 0;
            this.groupBoxQWRP.TabStop = false;
            this.groupBoxQWRP.Text = "QWRP ID Selection";
            // 
            // buttonLoadQWRP
            // 
            this.buttonLoadQWRP.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.buttonLoadQWRP.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.buttonLoadQWRP.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonLoadQWRP.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonLoadQWRP.ForeColor = System.Drawing.Color.White;
            this.buttonLoadQWRP.Location = new System.Drawing.Point(350, 30);
            this.buttonLoadQWRP.Name = "buttonLoadQWRP";
            this.buttonLoadQWRP.Size = new System.Drawing.Size(120, 30);
            this.buttonLoadQWRP.TabIndex = 2;
            this.buttonLoadQWRP.Text = "Load QWRP";
            this.buttonLoadQWRP.UseVisualStyleBackColor = false;
            this.buttonLoadQWRP.Click += new System.EventHandler(this.buttonLoadQWRP_Click);
            // 
            // textBoxQWRPID
            // 
            this.textBoxQWRPID.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.textBoxQWRPID.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxQWRPID.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxQWRPID.ForeColor = System.Drawing.Color.White;
            this.textBoxQWRPID.Location = new System.Drawing.Point(120, 32);
            this.textBoxQWRPID.Name = "textBoxQWRPID";
            this.textBoxQWRPID.Size = new System.Drawing.Size(200, 25);
            this.textBoxQWRPID.TabIndex = 1;
            this.textBoxQWRPID.Text = "QW001";
            // 
            // labelQWRPID
            // 
            this.labelQWRPID.AutoSize = true;
            this.labelQWRPID.Location = new System.Drawing.Point(20, 35);
            this.labelQWRPID.Name = "labelQWRPID";
            this.labelQWRPID.Size = new System.Drawing.Size(60, 15);
            this.labelQWRPID.TabIndex = 0;
            this.labelQWRPID.Text = "QWRP ID:";
            // 
            // groupBoxCurrentMeasurement
            // 
            this.groupBoxCurrentMeasurement.Controls.Add(this.panelRealTimeDisplay);
            this.groupBoxCurrentMeasurement.Controls.Add(this.labelLastResult);
            this.groupBoxCurrentMeasurement.Controls.Add(this.buttonRecord);
            this.groupBoxCurrentMeasurement.Controls.Add(this.labelDescription);
            this.groupBoxCurrentMeasurement.Controls.Add(this.labelTolerance);
            this.groupBoxCurrentMeasurement.Controls.Add(this.labelTargetValue);
            this.groupBoxCurrentMeasurement.Controls.Add(this.labelMeasurementType);
            this.groupBoxCurrentMeasurement.Controls.Add(this.labelCurrentPosition);
            this.groupBoxCurrentMeasurement.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxCurrentMeasurement.ForeColor = System.Drawing.Color.White;
            this.groupBoxCurrentMeasurement.Location = new System.Drawing.Point(20, 120);
            this.groupBoxCurrentMeasurement.Name = "groupBoxCurrentMeasurement";
            this.groupBoxCurrentMeasurement.Size = new System.Drawing.Size(480, 250);
            this.groupBoxCurrentMeasurement.TabIndex = 1;
            this.groupBoxCurrentMeasurement.TabStop = false;
            this.groupBoxCurrentMeasurement.Text = "Current Measurement";
            // 
            // labelLastResult
            // 
            this.labelLastResult.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLastResult.ForeColor = System.Drawing.Color.LightGray;
            this.labelLastResult.Location = new System.Drawing.Point(20, 150);
            this.labelLastResult.Name = "labelLastResult";
            this.labelLastResult.Size = new System.Drawing.Size(440, 60);
            this.labelLastResult.TabIndex = 6;
            this.labelLastResult.Text = "";
            // 
            // buttonRecord
            // 
            this.buttonRecord.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(0)))));
            this.buttonRecord.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.buttonRecord.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonRecord.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonRecord.ForeColor = System.Drawing.Color.White;
            this.buttonRecord.Location = new System.Drawing.Point(300, 30);
            this.buttonRecord.Name = "buttonRecord";
            this.buttonRecord.Size = new System.Drawing.Size(150, 40);
            this.buttonRecord.TabIndex = 5;
            this.buttonRecord.Text = "Record (Ctrl+Q)";
            this.buttonRecord.UseVisualStyleBackColor = false;
            this.buttonRecord.Click += new System.EventHandler(this.buttonRecord_Click);
            // 
            // labelDescription
            // 
            this.labelDescription.AutoSize = true;
            this.labelDescription.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDescription.ForeColor = System.Drawing.Color.LightGray;
            this.labelDescription.Location = new System.Drawing.Point(20, 120);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(71, 15);
            this.labelDescription.TabIndex = 4;
            this.labelDescription.Text = "Description: ";
            // 
            // labelTolerance
            // 
            this.labelTolerance.AutoSize = true;
            this.labelTolerance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTolerance.ForeColor = System.Drawing.Color.Orange;
            this.labelTolerance.Location = new System.Drawing.Point(20, 95);
            this.labelTolerance.Name = "labelTolerance";
            this.labelTolerance.Size = new System.Drawing.Size(80, 19);
            this.labelTolerance.TabIndex = 3;
            this.labelTolerance.Text = "Tolerance: ";
            // 
            // labelTargetValue
            // 
            this.labelTargetValue.AutoSize = true;
            this.labelTargetValue.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTargetValue.ForeColor = System.Drawing.Color.LightBlue;
            this.labelTargetValue.Location = new System.Drawing.Point(20, 70);
            this.labelTargetValue.Name = "labelTargetValue";
            this.labelTargetValue.Size = new System.Drawing.Size(61, 19);
            this.labelTargetValue.TabIndex = 2;
            this.labelTargetValue.Text = "Target: ";
            // 
            // labelMeasurementType
            // 
            this.labelMeasurementType.AutoSize = true;
            this.labelMeasurementType.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMeasurementType.ForeColor = System.Drawing.Color.LightGreen;
            this.labelMeasurementType.Location = new System.Drawing.Point(20, 45);
            this.labelMeasurementType.Name = "labelMeasurementType";
            this.labelMeasurementType.Size = new System.Drawing.Size(46, 19);
            this.labelMeasurementType.TabIndex = 1;
            this.labelMeasurementType.Text = "Type: ";
            // 
            // labelCurrentPosition
            // 
            this.labelCurrentPosition.AutoSize = true;
            this.labelCurrentPosition.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCurrentPosition.ForeColor = System.Drawing.Color.Yellow;
            this.labelCurrentPosition.Location = new System.Drawing.Point(20, 20);
            this.labelCurrentPosition.Name = "labelCurrentPosition";
            this.labelCurrentPosition.Size = new System.Drawing.Size(82, 21);
            this.labelCurrentPosition.TabIndex = 0;
            this.labelCurrentPosition.Text = "Position: ";
            // 
            // panelRealTimeDisplay
            // 
            this.panelRealTimeDisplay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(62)))), ((int)(((byte)(76)))));
            this.panelRealTimeDisplay.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelRealTimeDisplay.Controls.Add(this.lblCurrentFunction);
            this.panelRealTimeDisplay.Controls.Add(this.lblCurrentUnit);
            this.panelRealTimeDisplay.Controls.Add(this.lblCurrentReading);
            this.panelRealTimeDisplay.Location = new System.Drawing.Point(300, 80);
            this.panelRealTimeDisplay.Name = "panelRealTimeDisplay";
            this.panelRealTimeDisplay.Size = new System.Drawing.Size(160, 120);
            this.panelRealTimeDisplay.TabIndex = 7;
            // 
            // lblCurrentFunction
            // 
            this.lblCurrentFunction.AutoSize = true;
            this.lblCurrentFunction.Font = new System.Drawing.Font("Segoe UI Semibold", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCurrentFunction.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(214)))), ((int)(((byte)(233)))));
            this.lblCurrentFunction.Location = new System.Drawing.Point(5, 5);
            this.lblCurrentFunction.Name = "lblCurrentFunction";
            this.lblCurrentFunction.Size = new System.Drawing.Size(82, 13);
            this.lblCurrentFunction.TabIndex = 2;
            this.lblCurrentFunction.Text = "NO FUNCTION";
            // 
            // lblCurrentUnit
            // 
            this.lblCurrentUnit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCurrentUnit.Font = new System.Drawing.Font("Consolas", 14F, System.Drawing.FontStyle.Bold);
            this.lblCurrentUnit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(247)))), ((int)(((byte)(255)))));
            this.lblCurrentUnit.Location = new System.Drawing.Point(120, 80);
            this.lblCurrentUnit.Name = "lblCurrentUnit";
            this.lblCurrentUnit.Size = new System.Drawing.Size(35, 30);
            this.lblCurrentUnit.TabIndex = 1;
            this.lblCurrentUnit.Text = "V";
            this.lblCurrentUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblCurrentReading
            // 
            this.lblCurrentReading.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCurrentReading.Font = new System.Drawing.Font("Consolas", 20F, System.Drawing.FontStyle.Bold);
            this.lblCurrentReading.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(247)))), ((int)(((byte)(255)))));
            this.lblCurrentReading.Location = new System.Drawing.Point(5, 25);
            this.lblCurrentReading.Name = "lblCurrentReading";
            this.lblCurrentReading.Size = new System.Drawing.Size(145, 85);
            this.lblCurrentReading.TabIndex = 0;
            this.lblCurrentReading.Text = "0.000000";
            this.lblCurrentReading.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // groupBoxProgress
            // 
            this.groupBoxProgress.Controls.Add(this.buttonReset);
            this.groupBoxProgress.Controls.Add(this.buttonNext);
            this.groupBoxProgress.Controls.Add(this.buttonPrevious);
            this.groupBoxProgress.Controls.Add(this.progressBar);
            this.groupBoxProgress.Controls.Add(this.labelProgress);
            this.groupBoxProgress.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxProgress.ForeColor = System.Drawing.Color.White;
            this.groupBoxProgress.Location = new System.Drawing.Point(520, 120);
            this.groupBoxProgress.Name = "groupBoxProgress";
            this.groupBoxProgress.Size = new System.Drawing.Size(460, 250);
            this.groupBoxProgress.TabIndex = 2;
            this.groupBoxProgress.TabStop = false;
            this.groupBoxProgress.Text = "Progress Control";
            // 
            // buttonReset
            // 
            this.buttonReset.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.buttonReset.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.buttonReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonReset.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonReset.ForeColor = System.Drawing.Color.White;
            this.buttonReset.Location = new System.Drawing.Point(310, 120);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(120, 35);
            this.buttonReset.TabIndex = 4;
            this.buttonReset.Text = "Reset All";
            this.buttonReset.UseVisualStyleBackColor = false;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // buttonNext
            // 
            this.buttonNext.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.buttonNext.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.buttonNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonNext.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonNext.ForeColor = System.Drawing.Color.White;
            this.buttonNext.Location = new System.Drawing.Point(170, 120);
            this.buttonNext.Name = "buttonNext";
            this.buttonNext.Size = new System.Drawing.Size(120, 35);
            this.buttonNext.TabIndex = 3;
            this.buttonNext.Text = "Next >";
            this.buttonNext.UseVisualStyleBackColor = false;
            this.buttonNext.Click += new System.EventHandler(this.buttonNext_Click);
            // 
            // buttonPrevious
            // 
            this.buttonPrevious.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.buttonPrevious.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.buttonPrevious.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonPrevious.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonPrevious.ForeColor = System.Drawing.Color.White;
            this.buttonPrevious.Location = new System.Drawing.Point(30, 120);
            this.buttonPrevious.Name = "buttonPrevious";
            this.buttonPrevious.Size = new System.Drawing.Size(120, 35);
            this.buttonPrevious.TabIndex = 2;
            this.buttonPrevious.Text = "< Previous";
            this.buttonPrevious.UseVisualStyleBackColor = false;
            this.buttonPrevious.Click += new System.EventHandler(this.buttonPrevious_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(30, 70);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(400, 30);
            this.progressBar.TabIndex = 1;
            // 
            // labelProgress
            // 
            this.labelProgress.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelProgress.ForeColor = System.Drawing.Color.White;
            this.labelProgress.Location = new System.Drawing.Point(30, 30);
            this.labelProgress.Name = "labelProgress";
            this.labelProgress.Size = new System.Drawing.Size(400, 30);
            this.labelProgress.TabIndex = 0;
            this.labelProgress.Text = "0 / 0";
            this.labelProgress.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBoxMeasurementList
            // 
            this.groupBoxMeasurementList.Controls.Add(this.dataGridViewPoints);
            this.groupBoxMeasurementList.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxMeasurementList.ForeColor = System.Drawing.Color.White;
            this.groupBoxMeasurementList.Location = new System.Drawing.Point(20, 390);
            this.groupBoxMeasurementList.Name = "groupBoxMeasurementList";
            this.groupBoxMeasurementList.Size = new System.Drawing.Size(960, 290);
            this.groupBoxMeasurementList.TabIndex = 3;
            this.groupBoxMeasurementList.TabStop = false;
            this.groupBoxMeasurementList.Text = "Measurement Points";
            // 
            // dataGridViewPoints
            // 
            this.dataGridViewPoints.AllowUserToAddRows = false;
            this.dataGridViewPoints.AllowUserToDeleteRows = false;
            this.dataGridViewPoints.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.dataGridViewPoints.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewPoints.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Position,
            this.MeasurementType,
            this.TargetValue,
            this.Tolerance,
            this.Description,
            this.Status});
            this.dataGridViewPoints.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewPoints.Location = new System.Drawing.Point(3, 19);
            this.dataGridViewPoints.Name = "dataGridViewPoints";
            this.dataGridViewPoints.ReadOnly = true;
            this.dataGridViewPoints.Size = new System.Drawing.Size(954, 268);
            this.dataGridViewPoints.TabIndex = 0;
            // 
            // Position
            // 
            this.Position.HeaderText = "Position";
            this.Position.Name = "Position";
            this.Position.ReadOnly = true;
            this.Position.Width = 80;
            // 
            // MeasurementType
            // 
            this.MeasurementType.HeaderText = "Type";
            this.MeasurementType.Name = "MeasurementType";
            this.MeasurementType.ReadOnly = true;
            this.MeasurementType.Width = 100;
            // 
            // TargetValue
            // 
            this.TargetValue.HeaderText = "Target Value";
            this.TargetValue.Name = "TargetValue";
            this.TargetValue.ReadOnly = true;
            this.TargetValue.Width = 120;
            // 
            // Tolerance
            // 
            this.Tolerance.HeaderText = "Tolerance";
            this.Tolerance.Name = "Tolerance";
            this.Tolerance.ReadOnly = true;
            this.Tolerance.Width = 100;
            // 
            // Description
            // 
            this.Description.HeaderText = "Description";
            this.Description.Name = "Description";
            this.Description.ReadOnly = true;
            this.Description.Width = 200;
            // 
            // Status
            // 
            this.Status.HeaderText = "Status";
            this.Status.Name = "Status";
            this.Status.ReadOnly = true;
            this.Status.Width = 60;
            // 
            // MeasurementMode
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.ClientSize = new System.Drawing.Size(1000, 700);
            this.Controls.Add(this.panelMain);
            this.KeyPreview = true;
            this.Name = "MeasurementMode";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Measurement Mode - QWAVE";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MeasurementMode_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MeasurementMode_KeyDown);
            this.panelMain.ResumeLayout(false);
            this.groupBoxQWRP.ResumeLayout(false);
            this.groupBoxQWRP.PerformLayout();
            this.groupBoxCurrentMeasurement.ResumeLayout(false);
            this.groupBoxCurrentMeasurement.PerformLayout();
            this.groupBoxProgress.ResumeLayout(false);
            this.groupBoxMeasurementList.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPoints)).EndInit();
            this.panelRealTimeDisplay.ResumeLayout(false);
            this.panelRealTimeDisplay.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.GroupBox groupBoxQWRP;
        private System.Windows.Forms.Button buttonLoadQWRP;
        private System.Windows.Forms.TextBox textBoxQWRPID;
        private System.Windows.Forms.Label labelQWRPID;
        private System.Windows.Forms.GroupBox groupBoxCurrentMeasurement;
        private System.Windows.Forms.Label labelCurrentPosition;
        private System.Windows.Forms.Label labelMeasurementType;
        private System.Windows.Forms.Label labelTargetValue;
        private System.Windows.Forms.Label labelTolerance;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.Button buttonRecord;
        private System.Windows.Forms.Label labelLastResult;
        private System.Windows.Forms.GroupBox groupBoxProgress;
        private System.Windows.Forms.Label labelProgress;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Button buttonPrevious;
        private System.Windows.Forms.Button buttonNext;
        private System.Windows.Forms.Button buttonReset;
        private System.Windows.Forms.GroupBox groupBoxMeasurementList;
        private System.Windows.Forms.DataGridView dataGridViewPoints;
        private System.Windows.Forms.DataGridViewTextBoxColumn Position;
        private System.Windows.Forms.DataGridViewTextBoxColumn MeasurementType;
        private System.Windows.Forms.DataGridViewTextBoxColumn TargetValue;
        private System.Windows.Forms.DataGridViewTextBoxColumn Tolerance;
        private System.Windows.Forms.DataGridViewTextBoxColumn Description;
        private System.Windows.Forms.DataGridViewTextBoxColumn Status;
        private System.Windows.Forms.Panel panelRealTimeDisplay;
        private System.Windows.Forms.Label lblCurrentReading;
        private System.Windows.Forms.Label lblCurrentUnit;
        private System.Windows.Forms.Label lblCurrentFunction;
    }
}