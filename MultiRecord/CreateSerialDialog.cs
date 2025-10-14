using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiRecord
{
    public partial class CreateSerialDialog : Form
    {
        public string SerialNumber { get; private set; } = "";
        public int ModelId { get; private set; }
        public string ModelName { get; private set; }

        public CreateSerialDialog(int modelId, string modelName)
        {
            InitializeComponent();
            ModelId = modelId;
            ModelName = modelName;
            
            labelModelName.Text = $"Model: {modelName}";
            textBoxSerialNumber.Focus();
        }

        private async void ButtonCreate_Click(object sender, EventArgs e)
        {
            string serialNumber = textBoxSerialNumber.Text.Trim();
            
            if (string.IsNullOrEmpty(serialNumber))
            {
                MessageBox.Show("กรุณาใส่ Serial Number", "ข้อมูลไม่ครบ", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxSerialNumber.Focus();
                return;
            }

            // ตรวจสอบว่า Serial Number ซ้ำหรือไม่
            if (await IsSerialNumberExists(serialNumber))
            {
                MessageBox.Show($"Serial Number '{serialNumber}' มีอยู่แล้วในระบบ\nกรุณาใช้ Serial Number อื่น", 
                              "Serial Number ซ้ำ", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxSerialNumber.SelectAll();
                textBoxSerialNumber.Focus();
                return;
            }

            try
            {
                buttonCreate.Enabled = false;
                buttonCreate.Text = "กำลังสร้าง...";

                // สร้าง Serial Number ใหม่ในฐานข้อมูล
                int serialId = await CreateSerialNumberInDatabase(serialNumber);
                
                if (serialId > 0)
                {
                    SerialNumber = serialNumber;
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show("ไม่สามารถสร้าง Serial Number ได้", "ข้อผิดพลาด", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการสร้าง Serial Number: {ex.Message}", 
                              "ข้อผิดพลาด", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                buttonCreate.Enabled = true;
                buttonCreate.Text = "สร้าง";
            }
        }

        private async Task<bool> IsSerialNumberExists(string serialNumber)
        {
            try
            {
                // ใช้ MySqlManager แทนการเขียน SQL โดยตรง
                using (var mysqlManager = DatabaseConfig.CreateMySqlManager())
                {
                    return await mysqlManager.CheckSoftwareSerialNumberExistsAsync(ModelId, serialNumber);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"ไม่สามารถตรวจสอบ Serial Number: {ex.Message}");
            }
        }

        private async Task<int> CreateSerialNumberInDatabase(string serialNumber)
        {
            try
            {
                // ใช้ MySqlManager แทนการเขียน SQL โดยตรง
                using (var mysqlManager = DatabaseConfig.CreateMySqlManager())
                {
                    return await mysqlManager.CreateSoftwareSerialNumberAsync(ModelId, serialNumber);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"ไม่สามารถสร้าง Serial Number ในฐานข้อมูล: {ex.Message}");
            }
        }


        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void TextBoxSerialNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            // กด Enter เพื่อสร้าง
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                ButtonCreate_Click(sender, e);
            }
            // กด Escape เพื่อยกเลิก
            else if (e.KeyChar == (char)Keys.Escape)
            {
                e.Handled = true;
                ButtonCancel_Click(sender, e);
            }
        }
    }
}
