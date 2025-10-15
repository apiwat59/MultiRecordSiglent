using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiRecord
{
    public partial class RepairQwidDialog : Form
    {
        private MySqlManager _mySqlManager;
        private TextBox textBoxQwid;
        private Label labelQwid;
        private Label labelSerial;
        private TextBox textBoxSerial;
        private Button buttonCreate;
        private Button buttonCancel;
        private ListBox listBoxQwidSuggestions;
        private ListBox listBoxSerialSuggestions;
        private List<QwidItem> _qwidList;
        private List<SerialItem> _serialList;
        private Timer _qwidSearchTimer;
        private Timer _serialSearchTimer;

        public string SelectedQwid { get; private set; }
        public int SelectedSerialId { get; private set; }
        public string SelectedSerialNumber { get; private set; }

        public RepairQwidDialog(MySqlManager mySqlManager)
        {
            _mySqlManager = mySqlManager;
            InitializeComponent();
            InitializeSearchTimers();
            LoadQwidData();
            LoadSerialData();
        }

        private void InitializeComponent()
        {
            this.textBoxQwid = new TextBox();
            this.labelQwid = new Label();
            this.labelSerial = new Label();
            this.textBoxSerial = new TextBox();
            this.buttonCreate = new Button();
            this.buttonCancel = new Button();
            this.listBoxQwidSuggestions = new ListBox();
            this.listBoxSerialSuggestions = new ListBox();
            this.SuspendLayout();

            // 
            // labelQwid
            // 
            this.labelQwid.AutoSize = true;
            this.labelQwid.Font = new Font("Segoe UI", 10F);
            this.labelQwid.ForeColor = Color.White;
            this.labelQwid.Location = new Point(20, 20);
            this.labelQwid.Name = "labelQwid";
            this.labelQwid.Size = new Size(100, 19);
            this.labelQwid.TabIndex = 0;
            this.labelQwid.Text = "QWID:";

            // 
            // textBoxQwid
            // 
            this.textBoxQwid.BackColor = Color.FromArgb(30, 30, 30);
            this.textBoxQwid.ForeColor = Color.White;
            this.textBoxQwid.Font = new Font("Segoe UI", 10F);
            this.textBoxQwid.Location = new Point(20, 45);
            this.textBoxQwid.Name = "textBoxQwid";
            this.textBoxQwid.Size = new Size(360, 25);
            this.textBoxQwid.TabIndex = 1;
            this.textBoxQwid.TextChanged += TextBoxQwid_TextChanged;
            this.textBoxQwid.KeyDown += TextBoxQwid_KeyDown;

            // 
            // listBoxQwidSuggestions
            // 
            this.listBoxQwidSuggestions.BackColor = Color.FromArgb(45, 45, 48);
            this.listBoxQwidSuggestions.ForeColor = Color.White;
            this.listBoxQwidSuggestions.Font = new Font("Segoe UI", 9F);
            this.listBoxQwidSuggestions.Location = new Point(20, 70);
            this.listBoxQwidSuggestions.Name = "listBoxQwidSuggestions";
            this.listBoxQwidSuggestions.Size = new Size(360, 120);
            this.listBoxQwidSuggestions.TabIndex = 2;
            this.listBoxQwidSuggestions.Visible = false;
            this.listBoxQwidSuggestions.Click += ListBoxQwidSuggestions_Click;
            this.listBoxQwidSuggestions.KeyDown += ListBoxQwidSuggestions_KeyDown;

            // 
            // labelSerial
            // 
            this.labelSerial.AutoSize = true;
            this.labelSerial.Font = new Font("Segoe UI", 10F);
            this.labelSerial.ForeColor = Color.White;
            this.labelSerial.Location = new Point(20, 200);
            this.labelSerial.Name = "labelSerial";
            this.labelSerial.Size = new Size(100, 19);
            this.labelSerial.TabIndex = 3;
            this.labelSerial.Text = "Serial Number:";

            // 
            // textBoxSerial
            // 
            this.textBoxSerial.BackColor = Color.FromArgb(30, 30, 30);
            this.textBoxSerial.ForeColor = Color.White;
            this.textBoxSerial.Font = new Font("Segoe UI", 10F);
            this.textBoxSerial.Location = new Point(20, 225);
            this.textBoxSerial.Name = "textBoxSerial";
            this.textBoxSerial.Size = new Size(360, 25);
            this.textBoxSerial.TabIndex = 4;
            this.textBoxSerial.TextChanged += TextBoxSerial_TextChanged;
            this.textBoxSerial.KeyDown += TextBoxSerial_KeyDown;

            // 
            // listBoxSerialSuggestions
            // 
            this.listBoxSerialSuggestions.BackColor = Color.FromArgb(45, 45, 48);
            this.listBoxSerialSuggestions.ForeColor = Color.White;
            this.listBoxSerialSuggestions.Font = new Font("Segoe UI", 9F);
            this.listBoxSerialSuggestions.Location = new Point(20, 250);
            this.listBoxSerialSuggestions.Name = "listBoxSerialSuggestions";
            this.listBoxSerialSuggestions.Size = new Size(360, 120);
            this.listBoxSerialSuggestions.TabIndex = 5;
            this.listBoxSerialSuggestions.Visible = false;
            this.listBoxSerialSuggestions.Click += ListBoxSerialSuggestions_Click;
            this.listBoxSerialSuggestions.KeyDown += ListBoxSerialSuggestions_KeyDown;

            // 
            // buttonCreate
            // 
            this.buttonCreate.BackColor = Color.FromArgb(0, 122, 204);
            this.buttonCreate.FlatStyle = FlatStyle.Flat;
            this.buttonCreate.Font = new Font("Segoe UI", 9F);
            this.buttonCreate.ForeColor = Color.White;
            this.buttonCreate.Location = new Point(180, 380);
            this.buttonCreate.Name = "buttonCreate";
            this.buttonCreate.Size = new Size(100, 35);
            this.buttonCreate.TabIndex = 6;
            this.buttonCreate.Text = "สร้าง";
            this.buttonCreate.UseVisualStyleBackColor = false;
            this.buttonCreate.Enabled = false;
            this.buttonCreate.Click += ButtonCreate_Click;

            // 
            // buttonCancel
            // 
            this.buttonCancel.BackColor = Color.FromArgb(63, 63, 70);
            this.buttonCancel.FlatStyle = FlatStyle.Flat;
            this.buttonCancel.Font = new Font("Segoe UI", 9F);
            this.buttonCancel.ForeColor = Color.White;
            this.buttonCancel.Location = new Point(290, 380);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new Size(90, 35);
            this.buttonCancel.TabIndex = 7;
            this.buttonCancel.Text = "ยกเลิก";
            this.buttonCancel.UseVisualStyleBackColor = false;
            this.buttonCancel.Click += ButtonCancel_Click;

            // 
            // RepairQwidDialog
            // 
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ClientSize = new Size(400, 430);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonCreate);
            this.Controls.Add(this.listBoxSerialSuggestions);
            this.Controls.Add(this.textBoxSerial);
            this.Controls.Add(this.labelSerial);
            this.Controls.Add(this.listBoxQwidSuggestions);
            this.Controls.Add(this.textBoxQwid);
            this.Controls.Add(this.labelQwid);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RepairQwidDialog";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "เลือก QWID สำหรับ Repair";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void InitializeSearchTimers()
        {
            _qwidSearchTimer = new Timer();
            _qwidSearchTimer.Interval = 300; // 300ms delay
            _qwidSearchTimer.Tick += QwidSearchTimer_Tick;

            _serialSearchTimer = new Timer();
            _serialSearchTimer.Interval = 300; // 300ms delay
            _serialSearchTimer.Tick += SerialSearchTimer_Tick;
        }

        private async void LoadQwidData()
        {
            try
            {
                string sql = @"SELECT io.id as io_id, io.qwid, io.serial, io.model_id 
                              FROM spaze.io 
                              WHERE io.qwid IS NOT NULL 
                              ORDER BY io.created_timestamp DESC 
                              LIMIT 1000";
                
                var result = await _mySqlManager.ExecuteQuery(sql);
                
                if (result.Success && result.Data != null)
                {
                    _qwidList = new List<QwidItem>();
                    foreach (DataRow row in result.Data.Rows)
                    {
                        _qwidList.Add(new QwidItem
                        {
                            IoId = row["io_id"] != DBNull.Value ? Convert.ToInt32(row["io_id"]) : 0,
                            Qwid = row["qwid"]?.ToString() ?? "",
                            Serial = row["serial"]?.ToString() ?? "",
                            ModelId = row["model_id"] != DBNull.Value ? Convert.ToInt32(row["model_id"]) : 0
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการโหลดข้อมูล QWID: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void LoadSerialData()
        {
            try
            {
                string sql = @"SELECT id, serial_number, model_id, status, created_at 
                              FROM Orbitz.software_serial_numbers 
                              WHERE status = 'active'
                              ORDER BY created_at DESC 
                              LIMIT 1000";
                
                var result = await _mySqlManager.ExecuteQuery(sql);
                
                if (result.Success && result.Data != null)
                {
                    _serialList = new List<SerialItem>();
                    foreach (DataRow row in result.Data.Rows)
                    {
                        _serialList.Add(new SerialItem
                        {
                            Id = row["id"] != DBNull.Value ? Convert.ToInt32(row["id"]) : 0,
                            SerialNumber = row["serial_number"]?.ToString() ?? "",
                            ModelId = row["model_id"] != DBNull.Value ? Convert.ToInt32(row["model_id"]) : 0,
                            Status = row["status"]?.ToString() ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการโหลดข้อมูล Serial: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TextBoxQwid_TextChanged(object sender, EventArgs e)
        {
            _qwidSearchTimer.Stop();
            _qwidSearchTimer.Start();
        }

        private void TextBoxSerial_TextChanged(object sender, EventArgs e)
        {
            _serialSearchTimer.Stop();
            _serialSearchTimer.Start();
        }

        private void QwidSearchTimer_Tick(object sender, EventArgs e)
        {
            _qwidSearchTimer.Stop();
            FilterQwidSuggestions();
        }

        private void SerialSearchTimer_Tick(object sender, EventArgs e)
        {
            _serialSearchTimer.Stop();
            FilterSerialSuggestions();
        }

        private async void FilterQwidSuggestions()
        {
            string searchText = textBoxQwid.Text.Trim();
            
            if (string.IsNullOrEmpty(searchText))
            {
                listBoxQwidSuggestions.Visible = false;
                return;
            }

            // ค้นหาใน buffer ก่อน
            var filteredFromBuffer = _qwidList
                .Where(q => q.Qwid.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                .Take(10)
                .ToList();

            listBoxQwidSuggestions.Items.Clear();
            
            if (filteredFromBuffer.Count > 0)
            {
                foreach (var item in filteredFromBuffer)
                {
                    listBoxQwidSuggestions.Items.Add($"{item.Qwid} - {item.Serial}");
                }
                listBoxQwidSuggestions.Visible = true;
            }
            else
            {
                // ถ้าไม่พบใน buffer ให้ค้นหาใน database
                await SearchQwidFromDatabase(searchText);
            }
        }

        private async Task SearchQwidFromDatabase(string searchText)
        {
            try
            {
                string sql = $@"SELECT io.id as io_id, io.qwid, io.serial, io.model_id 
                              FROM spaze.io 
                              WHERE io.qwid IS NOT NULL 
                              AND io.qwid LIKE '%{searchText.Replace("'", "''")}%'
                              ORDER BY io.created_timestamp DESC 
                              LIMIT 20";
                
                var result = await _mySqlManager.ExecuteQuery(sql);
                
                if (result.Success && result.Data != null)
                {
                    var databaseResults = new List<QwidItem>();
                    foreach (DataRow row in result.Data.Rows)
                    {
                        databaseResults.Add(new QwidItem
                        {
                            IoId = row["io_id"] != DBNull.Value ? Convert.ToInt32(row["io_id"]) : 0,
                            Qwid = row["qwid"]?.ToString() ?? "",
                            Serial = row["serial"]?.ToString() ?? "",
                            ModelId = row["model_id"] != DBNull.Value ? Convert.ToInt32(row["model_id"]) : 0
                        });
                    }

                    if (databaseResults.Count > 0)
                    {
                        foreach (var item in databaseResults.Take(10))
                        {
                            listBoxQwidSuggestions.Items.Add($"{item.Qwid} - {item.Serial}");
                        }
                        listBoxQwidSuggestions.Visible = true;
                    }
                    else
                    {
                        listBoxQwidSuggestions.Visible = false;
                    }
                }
                else
                {
                    listBoxQwidSuggestions.Visible = false;
                }
            }
            catch (Exception)
            {
                // ถ้าเกิดข้อผิดพลาด ให้ซ่อน suggestion list
                listBoxQwidSuggestions.Visible = false;
            }
        }

        private async void FilterSerialSuggestions()
        {
            string searchText = textBoxSerial.Text.Trim();
            
            if (string.IsNullOrEmpty(searchText))
            {
                listBoxSerialSuggestions.Visible = false;
                return;
            }

            if (_serialList == null)
            {
                listBoxSerialSuggestions.Visible = false;
                return;
            }

            // ค้นหาใน buffer ก่อน
            var filteredFromBuffer = _serialList
                .Where(s => s.SerialNumber.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                .Take(10)
                .ToList();

            listBoxSerialSuggestions.Items.Clear();
            
            if (filteredFromBuffer.Count > 0)
            {
                foreach (var item in filteredFromBuffer)
                {
                    listBoxSerialSuggestions.Items.Add(item.SerialNumber);
                }
                listBoxSerialSuggestions.Visible = true;
            }
            else
            {
                // ถ้าไม่พบใน buffer ให้ค้นหาใน database
                await SearchSerialFromDatabase(searchText);
            }
        }

        private async Task SearchSerialFromDatabase(string searchText)
        {
            try
            {
                string sql = $@"SELECT id, serial_number, model_id, status, created_at 
                              FROM Orbitz.software_serial_numbers 
                              WHERE status = 'active'
                              AND serial_number LIKE '%{searchText.Replace("'", "''")}%'
                              ORDER BY created_at DESC 
                              LIMIT 20";
                
                var result = await _mySqlManager.ExecuteQuery(sql);
                
                if (result.Success && result.Data != null)
                {
                    var databaseResults = new List<SerialItem>();
                    foreach (DataRow row in result.Data.Rows)
                    {
                        databaseResults.Add(new SerialItem
                        {
                            Id = row["id"] != DBNull.Value ? Convert.ToInt32(row["id"]) : 0,
                            SerialNumber = row["serial_number"]?.ToString() ?? "",
                            ModelId = row["model_id"] != DBNull.Value ? Convert.ToInt32(row["model_id"]) : 0,
                            Status = row["status"]?.ToString() ?? ""
                        });
                    }

                    if (databaseResults.Count > 0)
                    {
                        foreach (var item in databaseResults.Take(10))
                        {
                            listBoxSerialSuggestions.Items.Add(item.SerialNumber);
                        }
                        listBoxSerialSuggestions.Visible = true;
                    }
                    else
                    {
                        listBoxSerialSuggestions.Visible = false;
                    }
                }
                else
                {
                    listBoxSerialSuggestions.Visible = false;
                }
            }
            catch (Exception)
            {
                // ถ้าเกิดข้อผิดพลาด ให้ซ่อน suggestion list
                listBoxSerialSuggestions.Visible = false;
            }
        }

        private void TextBoxQwid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down && listBoxQwidSuggestions.Visible && listBoxQwidSuggestions.Items.Count > 0)
            {
                listBoxQwidSuggestions.Focus();
                listBoxQwidSuggestions.SelectedIndex = 0;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                SelectCurrentQwid();
                e.Handled = true;
            }
        }

        private void TextBoxSerial_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down && listBoxSerialSuggestions.Visible && listBoxSerialSuggestions.Items.Count > 0)
            {
                listBoxSerialSuggestions.Focus();
                listBoxSerialSuggestions.SelectedIndex = 0;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                SelectCurrentSerial();
                e.Handled = true;
            }
        }

        private void ListBoxQwidSuggestions_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectFromQwidList();
                e.Handled = true;
            }
        }

        private void ListBoxSerialSuggestions_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectFromSerialList();
                e.Handled = true;
            }
        }

        private void ListBoxQwidSuggestions_Click(object sender, EventArgs e)
        {
            SelectFromQwidList();
        }

        private void ListBoxSerialSuggestions_Click(object sender, EventArgs e)
        {
            SelectFromSerialList();
        }

        private void SelectFromQwidList()
        {
            if (listBoxQwidSuggestions.SelectedItem != null)
            {
                string selected = listBoxQwidSuggestions.SelectedItem.ToString();
                string qwid = selected.Split(new[] { " - " }, StringSplitOptions.None)[0];
                textBoxQwid.Text = qwid;
                listBoxQwidSuggestions.Visible = false;
                textBoxQwid.Focus();
                SelectCurrentQwid();
            }
        }

        private void SelectFromSerialList()
        {
            if (listBoxSerialSuggestions.SelectedItem != null)
            {
                string serialNumber = listBoxSerialSuggestions.SelectedItem.ToString();
                textBoxSerial.Text = serialNumber;
                listBoxSerialSuggestions.Visible = false;
                textBoxSerial.Focus();
                SelectCurrentSerial();
            }
        }

        private async void SelectCurrentQwid()
        {
            string qwid = textBoxQwid.Text.Trim();
            
            if (string.IsNullOrEmpty(qwid))
            {
                return;
            }

            // Find QWID in buffer first
            var qwidItem = _qwidList.FirstOrDefault(q => 
                q.Qwid.Equals(qwid, StringComparison.OrdinalIgnoreCase));

            if (qwidItem != null)
            {
                SelectedQwid = qwidItem.Qwid;
                
                // ถ้ามี Serial Number อยู่แล้ว ให้เปิดปุ่ม Create
                if (!string.IsNullOrEmpty(textBoxSerial.Text.Trim()))
                {
                    // Validate serial exists
                    SelectCurrentSerial();
                }
                else
                {
                    buttonCreate.Enabled = false;
                }
            }
            else
            {
                // ถ้าไม่พบใน buffer ให้ค้นหาใน database
                await ValidateQwidFromDatabase(qwid);
            }
        }

        private async Task ValidateQwidFromDatabase(string qwid)
        {
            try
            {
                string sql = $@"SELECT io.id as io_id, io.qwid, io.serial, io.model_id 
                              FROM spaze.io 
                              WHERE io.qwid = '{qwid.Replace("'", "''")}'";
                
                var result = await _mySqlManager.ExecuteQuery(sql);
                
                if (result.Success && result.Data != null && result.Data.Rows.Count > 0)
                {
                    var row = result.Data.Rows[0];
                    SelectedQwid = row["qwid"]?.ToString() ?? "";
                    
                    // ถ้ามี Serial Number อยู่แล้ว ให้เปิดปุ่ม Create
                    if (!string.IsNullOrEmpty(textBoxSerial.Text.Trim()))
                    {
                        // Validate serial exists
                        SelectCurrentSerial();
                    }
                    else
                    {
                        buttonCreate.Enabled = false;
                    }
                }
                else
                {
                    buttonCreate.Enabled = false;
                }
            }
            catch (Exception)
            {
                buttonCreate.Enabled = false;
            }
        }

        private async void SelectCurrentSerial()
        {
            string serial = textBoxSerial.Text.Trim();
            
            if (string.IsNullOrEmpty(serial))
            {
                return;
            }

            // Find Serial in buffer first
            var serialItem = _serialList?.FirstOrDefault(s => 
                s.SerialNumber.Equals(serial, StringComparison.OrdinalIgnoreCase));

            if (serialItem != null)
            {
                // Serial exists in buffer
                SelectedSerialId = serialItem.Id;
                SelectedSerialNumber = serialItem.SerialNumber;
                
                // ถ้ามี QWID อยู่แล้ว ให้เปิดปุ่ม Create
                if (!string.IsNullOrEmpty(textBoxQwid.Text.Trim()))
                {
                    buttonCreate.Enabled = true;
                }
            }
            else
            {
                // ถ้าไม่พบใน buffer ให้ค้นหาใน database
                await ValidateSerialFromDatabase(serial);
            }
        }

        private async Task ValidateSerialFromDatabase(string serial)
        {
            try
            {
                string sql = $@"SELECT id, serial_number, model_id, status, created_at 
                              FROM Orbitz.software_serial_numbers 
                              WHERE status = 'active'
                              AND serial_number = '{serial.Replace("'", "''")}'";
                
                var result = await _mySqlManager.ExecuteQuery(sql);
                
                if (result.Success && result.Data != null && result.Data.Rows.Count > 0)
                {
                    var row = result.Data.Rows[0];
                    SelectedSerialId = row["id"] != DBNull.Value ? Convert.ToInt32(row["id"]) : 0;
                    SelectedSerialNumber = row["serial_number"]?.ToString() ?? "";
                    
                    // ถ้ามี QWID อยู่แล้ว ให้เปิดปุ่ม Create
                    if (!string.IsNullOrEmpty(textBoxQwid.Text.Trim()))
                    {
                        buttonCreate.Enabled = true;
                    }
                }
                else
                {
                    // Serial doesn't exist - ต้องสร้างใหม่
                    buttonCreate.Enabled = false;
                }
            }
            catch (Exception)
            {
                buttonCreate.Enabled = false;
            }
        }

        private void ButtonCreate_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private class QwidItem
        {
            public int IoId { get; set; }
            public string Qwid { get; set; }
            public string Serial { get; set; }
            public int ModelId { get; set; }
        }

        private class SerialItem
        {
            public int Id { get; set; }
            public string SerialNumber { get; set; }
            public int ModelId { get; set; }
            public string Status { get; set; }
        }
    }
}

