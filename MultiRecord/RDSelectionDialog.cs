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
                
                comboBoxModels.DisplayMember = "Name";
                comboBoxModels.ValueMember = "Id";
                comboBoxModels.DataSource = _models;
                
                if (_models.Count > 0)
                {
                    comboBoxModels.SelectedIndex = 0;
                }
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


        private async void ComboBoxModels_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxModels.SelectedItem != null)
            {
                dynamic selectedModel = comboBoxModels.SelectedItem;
                int modelId = selectedModel.Id;
                
                // โหลด Serial Numbers สำหรับ Model ที่เลือก
                await LoadSerialNumbersForModel(modelId);
            }
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
            if (comboBoxModels.SelectedItem == null)
            {
                MessageBox.Show("กรุณาเลือก Model ก่อน", "ต้องเลือก Model", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            dynamic selectedModel = comboBoxModels.SelectedItem;
            SelectedModelId = selectedModel.Id;
            SelectedModelName = selectedModel.Name;
            IsCreateNew = true;
            
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ButtonLoadExisting_Click(object sender, EventArgs e)
        {
            if (comboBoxModels.SelectedItem == null)
            {
                MessageBox.Show("กรุณาเลือก Model ก่อน", "ต้องเลือก Model", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (comboBoxSerialNumbers.SelectedItem == null)
            {
                MessageBox.Show("กรุณาเลือก Serial Number ก่อน", "ต้องเลือก Serial Number", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            dynamic selectedModel = comboBoxModels.SelectedItem;
            dynamic selectedSerial = comboBoxSerialNumbers.SelectedItem;
            
            SelectedModelId = selectedModel.Id;
            SelectedModelName = selectedModel.Name;
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
