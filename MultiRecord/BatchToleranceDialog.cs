using System;
using System.Windows.Forms;

namespace MultiRecord
{
    public partial class BatchToleranceDialog : Form
    {
        public double UpperLimit { get; private set; }
        public double LowerLimit { get; private set; }
        public bool ToleranceEnabled { get; private set; }
        public bool ApplyToSelected { get; private set; }
        public string ToleranceType { get; private set; }

        public BatchToleranceDialog(int selectedCount, int totalCount)
        {
            InitializeComponent();
            
            // ตั้งค่าข้อความแสดงจำนวนแถว
            if (selectedCount > 0)
            {
                labelInfo.Text = $"เลือกแถว: {selectedCount} แถว\nทั้งหมด: {totalCount} แถว";
                radioButtonSelected.Enabled = true;
                radioButtonSelected.Checked = true;
            }
            else
            {
                labelInfo.Text = $"ไม่ได้เลือกแถว\nทั้งหมด: {totalCount} แถว";
                radioButtonSelected.Enabled = false;
                radioButtonAll.Checked = true;
            }
            
            // ตั้งค่าเริ่มต้นสำหรับ tolerance type
            comboBoxToleranceType.Items.Add("percent");
            comboBoxToleranceType.Items.Add("abs");
            comboBoxToleranceType.SelectedIndex = 0; // default: percent
        }

        private void ButtonApply_Click(object sender, EventArgs e)
        {
            try
            {
                // ตรวจสอบค่า Upper Limit
                if (!double.TryParse(textBoxUpper.Text.Trim(), out double upper))
                {
                    MessageBox.Show("กรุณาใส่ค่า Upper Limit ที่ถูกต้อง", "ข้อมูลไม่ถูกต้อง", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxUpper.Focus();
                    return;
                }

                // ตรวจสอบค่า Lower Limit
                if (!double.TryParse(textBoxLower.Text.Trim(), out double lower))
                {
                    MessageBox.Show("กรุณาใส่ค่า Lower Limit ที่ถูกต้อง", "ข้อมูลไม่ถูกต้อง", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxLower.Focus();
                    return;
                }

                // ตรวจสอบว่า Upper > Lower
                if (upper <= lower)
                {
                    MessageBox.Show("Upper Limit ต้องมากกว่า Lower Limit", "ข้อมูลไม่ถูกต้อง", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxUpper.Focus();
                    return;
                }

                UpperLimit = upper;
                LowerLimit = lower;
                ToleranceEnabled = checkBoxEnable.Checked;
                ApplyToSelected = radioButtonSelected.Checked;
                ToleranceType = comboBoxToleranceType.SelectedItem?.ToString() ?? "percent";

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}", "ข้อผิดพลาด", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void CheckBoxEnable_CheckedChanged(object sender, EventArgs e)
        {
            // เปิด/ปิดการใช้งาน textbox ตามสถานะของ checkbox
            textBoxUpper.Enabled = checkBoxEnable.Checked;
            textBoxLower.Enabled = checkBoxEnable.Checked;
        }
    }
}


