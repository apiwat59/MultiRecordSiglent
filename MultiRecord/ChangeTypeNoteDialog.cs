using System;
using System.Drawing;
using System.Windows.Forms;

namespace MultiRecord
{
    /// <summary>
    /// Dialog for entering a note when changing repair session type
    /// </summary>
    public class ChangeTypeNoteDialog : Form
    {
        private Label labelInstruction;
        private TextBox textBoxNote;
        private Button buttonOK;
        private Button buttonCancel;

        public string Note => textBoxNote.Text.Trim();

        public ChangeTypeNoteDialog(string fromType, string toType)
        {
            InitializeComponents(fromType, toType);
        }

        private void InitializeComponents(string fromType, string toType)
        {
            // Form settings
            this.Text = "ใส่หมายเหตุการเปลี่ยนประเภท";
            this.Size = new Size(500, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Instruction label
            labelInstruction = new Label
            {
                Text = $"กำลังเปลี่ยนประเภทจาก \"{GetTypeDisplayName(fromType)}\" เป็น \"{GetTypeDisplayName(toType)}\"\n\nกรุณาใส่หมายเหตุ (เช่น เหตุผลที่เปลี่ยน):",
                Location = new Point(20, 20),
                Size = new Size(440, 60),
                Font = new Font("Segoe UI", 10F)
            };

            // TextBox for note
            textBoxNote = new TextBox
            {
                Location = new Point(20, 90),
                Size = new Size(440, 60),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 10F)
            };

            // OK button
            buttonOK = new Button
            {
                Text = "ตรวจสอบ",
                Location = new Point(280, 165),
                Size = new Size(90, 35),
                DialogResult = DialogResult.OK,
                Font = new Font("Segoe UI", 10F)
            };

            // Cancel button
            buttonCancel = new Button
            {
                Text = "ยกเลิก",
                Location = new Point(380, 165),
                Size = new Size(80, 35),
                DialogResult = DialogResult.Cancel,
                Font = new Font("Segoe UI", 10F)
            };

            // Add validation
            buttonOK.Click += ButtonOK_Click;

            // Add controls to form
            this.Controls.Add(labelInstruction);
            this.Controls.Add(textBoxNote);
            this.Controls.Add(buttonOK);
            this.Controls.Add(buttonCancel);

            // Set accept and cancel buttons
            this.AcceptButton = buttonOK;
            this.CancelButton = buttonCancel;
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxNote.Text))
            {
                MessageBox.Show("กรุณาใส่หมายเหตุ", "แจ้งเตือน", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None; // Prevent closing
            }
        }

        private string GetTypeDisplayName(string type)
        {
            switch (type)
            {
                case "before_repair":
                    return "ก่อนซ่อม";
                case "after_repair":
                    return "หลังซ่อม";
                default:
                    return type;
            }
        }
    }
}
