using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiRecord
{
    public partial class RDSelectionDialog : Form
    {
        public int SelectedModelId { get; private set; } = -1;
        public int SelectedSerialId { get; private set; } = -1;
        public string SelectedSerialNumber { get; private set; } = "";
        public string SelectedModelName { get; private set; } = "";
        public bool IsCreateNew { get; private set; } = false;

        private List<dynamic> _models;
        private List<dynamic> _serialNumbers;
        private MySqlManager _mysqlManager;

        public RDSelectionDialog()
        {
            InitializeComponent();
            InitializeMySqlManager();
            LoadModelsAsync();
        }

        private void InitializeMySqlManager()
        {
            try
            {
                // Use production database configuration
                _mysqlManager = DatabaseConfig.CreateMySqlManager();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize database connection: {ex.Message}",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void LoadModelsAsync()
        {
            try
            {
                _models = await LoadModelsFromDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการโหลดข้อมูล Models: {ex.Message}", 
                              "ข้อผิดพลาด", 
                              MessageBoxButtons.OK, 
                              MessageBoxIcon.Error);
            }
        }

        private async Task<List<dynamic>> LoadModelsFromDatabase()
        {
            var models = new List<dynamic>();
            
            try
            {
                // ใช้ MySqlManager แทนการเขียน SQL โดยตรง
                using (var mysqlManager = DatabaseConfig.CreateMySqlManager())
                {
                    var results = await mysqlManager.GetSoftwareModelsAsync();
                    
                    foreach (var result in results)
                    {
                        models.Add(new
                        {
                            Id = result.Id,
                            Name = result.ModelName,
                            Description = result.Description
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"ไม่สามารถโหลดข้อมูล Models จาก database: {ex.Message}");
            }
            
            return models;
        }


        private void TextBoxModelSearch_TextChanged(object sender, EventArgs e)
        {
            string searchText = textBoxModelSearch.Text.Trim();
            
            if (string.IsNullOrEmpty(searchText))
            {
                // ซ่อน ListBox และ clear serial numbers
                listBoxModelResults.Visible = false;
                listBoxModelResults.Items.Clear();
                comboBoxSerialNumbers.DataSource = null;
                comboBoxSerialNumbers.Enabled = false;
                buttonLoadExisting.Enabled = false;
                labelSerialInfo.Text = "กรุณาเลือก Model ก่อน...";
                labelSerialInfo.ForeColor = Color.Gray;
                return;
            }
            
            // ค้นหา models ที่มีข้อความที่พิมพ์ (case-insensitive, contains)
            var filteredModels = _models?.Where(m => 
                m.Name.ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0
            ).ToList();
            
            if (filteredModels != null && filteredModels.Count > 0)
            {
                // แสดงผลลัพธ์ใน ListBox
                listBoxModelResults.Items.Clear();
                foreach (var model in filteredModels)
                {
                    listBoxModelResults.Items.Add(model.Name);
                }
                listBoxModelResults.Visible = true;
                
                // Clear serial numbers เพราะยังไม่ได้เลือก model
                comboBoxSerialNumbers.DataSource = null;
                comboBoxSerialNumbers.Enabled = false;
                buttonLoadExisting.Enabled = false;
                labelSerialInfo.Text = $"พบ {filteredModels.Count} models";
                labelSerialInfo.ForeColor = Color.Gray;
            }
            else
            {
                // ไม่พบผลลัพธ์
                listBoxModelResults.Items.Clear();
                listBoxModelResults.Items.Add("ไม่พบ Model ที่ตรงกัน");
                listBoxModelResults.Visible = true;
                
                comboBoxSerialNumbers.DataSource = null;
                comboBoxSerialNumbers.Enabled = false;
                buttonLoadExisting.Enabled = false;
                labelSerialInfo.Text = "ไม่พบ Model";
                labelSerialInfo.ForeColor = Color.Orange;
            }
        }
        
        private async void ListBoxModelResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxModelResults.SelectedItem == null)
                return;
                
            string selectedModelName = listBoxModelResults.SelectedItem.ToString();
            
            // ตรวจสอบว่าไม่ใช่ข้อความ "ไม่พบ Model ที่ตรงกัน"
            if (selectedModelName == "ไม่พบ Model ที่ตรงกัน")
                return;
            
            // ใส่ชื่อ model ลงใน textbox
            textBoxModelSearch.TextChanged -= TextBoxModelSearch_TextChanged;
            textBoxModelSearch.Text = selectedModelName;
            textBoxModelSearch.TextChanged += TextBoxModelSearch_TextChanged;
            
            // ซ่อน ListBox
            listBoxModelResults.Visible = false;
            
            // หา model ที่เลือก
            var matchedModel = _models?.FirstOrDefault(m => 
                string.Equals(m.Name, selectedModelName, StringComparison.OrdinalIgnoreCase));
            
            if (matchedModel != null)
            {
                int modelId = matchedModel.Id;
                
                // โหลด Serial Numbers สำหรับ Model ที่เลือก
                await LoadSerialNumbersForModel(modelId);
            }
        }
        
        private async void ListBoxModelResults_DoubleClick(object sender, EventArgs e)
        {
            // Double click = เลือก model และไปที่ create new serial
            if (listBoxModelResults.SelectedItem == null)
                return;
                
            string selectedModelName = listBoxModelResults.SelectedItem.ToString();
            
            if (selectedModelName == "ไม่พบ Model ที่ตรงกัน")
                return;
            
            // ใส่ชื่อ model ลงใน textbox
            textBoxModelSearch.Text = selectedModelName;
            listBoxModelResults.Visible = false;
            
            // เรียก ButtonCreateNew_Click
            ButtonCreateNew_Click(sender, e);
        }

        private async Task LoadSerialNumbersForModel(int modelId)
        {
            try
            {
                _serialNumbers = await LoadSerialNumbersFromDatabase(modelId);
                
                comboBoxSerialNumbers.DisplayMember = "SerialNumber";
                comboBoxSerialNumbers.ValueMember = "Id";
                comboBoxSerialNumbers.DataSource = _serialNumbers;
                
                // เปิดใช้งาน controls
                comboBoxSerialNumbers.Enabled = _serialNumbers.Count > 0;
                buttonLoadExisting.Enabled = _serialNumbers.Count > 0;
                
                if (_serialNumbers.Count == 0)
                {
                    labelSerialInfo.Text = "ไม่มี Serial Number สำหรับ Model นี้";
                    labelSerialInfo.ForeColor = Color.Orange;
                }
                else
                {
                    labelSerialInfo.Text = $"พบ {_serialNumbers.Count} Serial Numbers";
                    labelSerialInfo.ForeColor = Color.Green;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการโหลด Serial Numbers: {ex.Message}", 
                              "ข้อผิดพลาด", 
                              MessageBoxButtons.OK, 
                              MessageBoxIcon.Error);
            }
        }

        private async Task<List<dynamic>> LoadSerialNumbersFromDatabase(int modelId)
        {
            var serialNumbers = new List<dynamic>();
            
            try
            {
                // ใช้ MySqlManager แทนการเขียน SQL โดยตรง
                using (var mysqlManager = DatabaseConfig.CreateMySqlManager())
                {
                    var results = await mysqlManager.GetSoftwareSerialNumbersByModelAsync(modelId);
                    
                    foreach (var result in results)
                    {
                        serialNumbers.Add(new
                        {
                            Id = result.Id,
                            SerialNumber = result.SerialNumber,
                            CreatedAt = result.CreatedAt
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"ไม่สามารถโหลด Serial Numbers จาก database: {ex.Message}");
            }
            
            return serialNumbers;
        }

        private void ButtonCreateNew_Click(object sender, EventArgs e)
        {
            string searchText = textBoxModelSearch.Text.Trim();
            
            if (string.IsNullOrEmpty(searchText))
            {
                MessageBox.Show("กรุณาเลือก Model ก่อน", "ต้องเลือก Model", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // Find exact match in models list
            var matchedModel = _models?.FirstOrDefault(m => 
                string.Equals(m.Name, searchText, StringComparison.OrdinalIgnoreCase));
            
            if (matchedModel == null)
            {
                MessageBox.Show("กรุณาเลือก Model ที่ถูกต้องจากรายการ", "Model ไม่ถูกต้อง", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SelectedModelId = matchedModel.Id;
            SelectedModelName = matchedModel.Name;
            IsCreateNew = true;
            
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ButtonLoadExisting_Click(object sender, EventArgs e)
        {
            string searchText = textBoxModelSearch.Text.Trim();
            
            if (string.IsNullOrEmpty(searchText))
            {
                MessageBox.Show("กรุณาเลือก Model ก่อน", "ต้องเลือก Model", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // Find exact match in models list
            var matchedModel = _models?.FirstOrDefault(m => 
                string.Equals(m.Name, searchText, StringComparison.OrdinalIgnoreCase));
            
            if (matchedModel == null)
            {
                MessageBox.Show("กรุณาเลือก Model ที่ถูกต้องจากรายการ", "Model ไม่ถูกต้อง", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (comboBoxSerialNumbers.SelectedItem == null)
            {
                MessageBox.Show("กรุณาเลือก Serial Number ก่อน", "ต้องเลือก Serial Number", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            dynamic selectedSerial = comboBoxSerialNumbers.SelectedItem;
            
            SelectedModelId = matchedModel.Id;
            SelectedModelName = matchedModel.Name;
            SelectedSerialId = selectedSerial.Id;
            SelectedSerialNumber = selectedSerial.SerialNumber;
            IsCreateNew = false;
            
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
