using System;
using System.Drawing;
using System.Windows.Forms;

namespace MultiRecord
{
    public class NewRepairSessionDialog : Form
    {
        private Label labelTitle;
        private Label labelQwid;
        private Label labelSerialNumber;
        private Label labelSessionNumber;
        private Label labelRepairType;
        private Label labelDescription;
        private TextBox textBoxQwid;
        private TextBox textBoxSerialNumber;
        private NumericUpDown numericSessionNumber;
        private ComboBox comboBoxRepairType;
        private TextBox textBoxDescription;
        private Button buttonCreate;
        private Button buttonCancel;

        public int SessionNumber => (int)numericSessionNumber.Value;
        public string RepairType => comboBoxRepairType.SelectedValue?.ToString() ?? "before_repair";
        public string Description => textBoxDescription.Text.Trim();

        public NewRepairSessionDialog(string qwId, string serialNumber, int nextSessionNumber)
        {
            InitializeComponent();
            
            // ตั้งค่าข้อมูลเริ่มต้น
            textBoxQwid.Text = qwId;
            textBoxSerialNumber.Text = serialNumber;
            numericSessionNumber.Value = nextSessionNumber;
            textBoxDescription.Text = $"รอบการซ่อมครั้งที่ {nextSessionNumber}";
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form settings
            this.Text = "สร้างรอบการซ่อมใหม่";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Title Label
            labelTitle = new Label
            {
                Text = "สร้างรอบการซ่อมใหม่",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(450, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(labelTitle);

            // QWID Label
            labelQwid = new Label
            {
                Text = "QWID:",
                Location = new Point(30, 70),
                Size = new Size(120, 23),
                TextAlign = ContentAlignment.MiddleRight
            };
            this.Controls.Add(labelQwid);

            textBoxQwid = new TextBox
            {
                Location = new Point(160, 70),
                Size = new Size(300, 23),
                ReadOnly = true,
                BackColor = SystemColors.Control
            };
            this.Controls.Add(textBoxQwid);

            // Serial Number Label
            labelSerialNumber = new Label
            {
                Text = "Serial Number:",
                Location = new Point(30, 105),
                Size = new Size(120, 23),
                TextAlign = ContentAlignment.MiddleRight
            };
            this.Controls.Add(labelSerialNumber);

            textBoxSerialNumber = new TextBox
            {
                Location = new Point(160, 105),
                Size = new Size(300, 23),
                ReadOnly = true,
                BackColor = SystemColors.Control
            };
            this.Controls.Add(textBoxSerialNumber);

            // Session Number Label
            labelSessionNumber = new Label
            {
                Text = "หมายเลขรอบการซ่อม:",
                Location = new Point(30, 140),
                Size = new Size(120, 23),
                TextAlign = ContentAlignment.MiddleRight
            };
            this.Controls.Add(labelSessionNumber);

            numericSessionNumber = new NumericUpDown
            {
                Location = new Point(160, 140),
                Size = new Size(100, 23),
                Minimum = 1,
                Maximum = 999,
                Value = 1
            };
            this.Controls.Add(numericSessionNumber);

            // Repair Type Label
            labelRepairType = new Label
            {
                Text = "ประเภท:",
                Location = new Point(30, 175),
                Size = new Size(120, 23),
                TextAlign = ContentAlignment.MiddleRight
            };
            this.Controls.Add(labelRepairType);

            comboBoxRepairType = new ComboBox
            {
                Location = new Point(160, 175),
                Size = new Size(200, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            comboBoxRepairType.Items.Add(new { Text = "ก่อนซ่อม", Value = "before_repair" });
            comboBoxRepairType.Items.Add(new { Text = "หลังซ่อม", Value = "after_repair" });
            comboBoxRepairType.DisplayMember = "Text";
            comboBoxRepairType.ValueMember = "Value";
            comboBoxRepairType.SelectedIndex = 0;
            this.Controls.Add(comboBoxRepairType);

            // Description Label
            labelDescription = new Label
            {
                Text = "คำอธิบาย:",
                Location = new Point(30, 210),
                Size = new Size(120, 23),
                TextAlign = ContentAlignment.MiddleRight
            };
            this.Controls.Add(labelDescription);

            textBoxDescription = new TextBox
            {
                Location = new Point(160, 210),
                Size = new Size(300, 60),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            this.Controls.Add(textBoxDescription);

            // Create Button
            buttonCreate = new Button
            {
                Text = "สร้าง",
                Location = new Point(250, 300),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            buttonCreate.FlatAppearance.BorderSize = 0;
            buttonCreate.Click += ButtonCreate_Click;
            this.Controls.Add(buttonCreate);

            // Cancel Button
            buttonCancel = new Button
            {
                Text = "ยกเลิก",
                Location = new Point(360, 300),
                Size = new Size(100, 35),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            buttonCancel.FlatAppearance.BorderSize = 0;
            buttonCancel.Click += ButtonCancel_Click;
            this.Controls.Add(buttonCancel);

            this.ResumeLayout(false);
        }

        private void ButtonCreate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxDescription.Text))
            {
                MessageBox.Show("กรุณากรอกคำอธิบาย", "ข้อมูลไม่ครบ", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxDescription.Focus();
                return;
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
