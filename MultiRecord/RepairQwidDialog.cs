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
        private Label labelModelInfo;
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
        private int _selectedModelId = -1;

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
            this.textBoxQwid = new System.Windows.Forms.TextBox();
            this.labelQwid = new System.Windows.Forms.Label();
            this.labelModelInfo = new System.Windows.Forms.Label();
            this.labelSerial = new System.Windows.Forms.Label();
            this.textBoxSerial = new System.Windows.Forms.TextBox();
            this.buttonCreate = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.listBoxQwidSuggestions = new System.Windows.Forms.ListBox();
            this.listBoxSerialSuggestions = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // textBoxQwid
            // 
            this.textBoxQwid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.textBoxQwid.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.textBoxQwid.ForeColor = System.Drawing.Color.White;
            this.textBoxQwid.Location = new System.Drawing.Point(20, 45);
            this.textBoxQwid.Name = "textBoxQwid";
            this.textBoxQwid.Size = new System.Drawing.Size(360, 25);
            this.textBoxQwid.TabIndex = 1;
            this.textBoxQwid.TextChanged += new System.EventHandler(this.TextBoxQwid_TextChanged);
            this.textBoxQwid.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxQwid_KeyDown);
            // 
            // labelQwid
            // 
            this.labelQwid.AutoSize = true;
            this.labelQwid.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.labelQwid.ForeColor = System.Drawing.Color.White;
            this.labelQwid.Location = new System.Drawing.Point(20, 20);
            this.labelQwid.Name = "labelQwid";
            this.labelQwid.Size = new System.Drawing.Size(50, 19);
            this.labelQwid.TabIndex = 0;
            this.labelQwid.Text = "QWID:";
            // 
            // labelModelInfo
            // 
            this.labelModelInfo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.labelModelInfo.ForeColor = System.Drawing.Color.LightGreen;
            this.labelModelInfo.Location = new System.Drawing.Point(20, 195);
            this.labelModelInfo.Name = "labelModelInfo";
            this.labelModelInfo.Size = new System.Drawing.Size(360, 20);
            this.labelModelInfo.TabIndex = 8;
            this.labelModelInfo.Visible = false;
            // 
            // labelSerial
            // 
            this.labelSerial.AutoSize = true;
            this.labelSerial.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.labelSerial.ForeColor = System.Drawing.Color.White;
            this.labelSerial.Location = new System.Drawing.Point(20, 220);
            this.labelSerial.Name = "labelSerial";
            this.labelSerial.Size = new System.Drawing.Size(71, 19);
            this.labelSerial.TabIndex = 3;
            this.labelSerial.Text = "ชุดทดสอบ :";
            // 
            // textBoxSerial
            // 
            this.textBoxSerial.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.textBoxSerial.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.textBoxSerial.ForeColor = System.Drawing.Color.White;
            this.textBoxSerial.Location = new System.Drawing.Point(20, 245);
            this.textBoxSerial.Name = "textBoxSerial";
            this.textBoxSerial.Size = new System.Drawing.Size(360, 25);
            this.textBoxSerial.TabIndex = 4;
            this.textBoxSerial.TextChanged += new System.EventHandler(this.TextBoxSerial_TextChanged);
            this.textBoxSerial.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxSerial_KeyDown);
            // 
            // buttonCreate
            // 
            this.buttonCreate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.buttonCreate.Enabled = false;
            this.buttonCreate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCreate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.buttonCreate.ForeColor = System.Drawing.Color.White;
            this.buttonCreate.Location = new System.Drawing.Point(180, 395);
            this.buttonCreate.Name = "buttonCreate";
            this.buttonCreate.Size = new System.Drawing.Size(100, 35);
            this.buttonCreate.TabIndex = 6;
            this.buttonCreate.Text = "สร้าง";
            this.buttonCreate.UseVisualStyleBackColor = false;
            this.buttonCreate.Click += new System.EventHandler(this.ButtonCreate_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCancel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.buttonCancel.ForeColor = System.Drawing.Color.White;
            this.buttonCancel.Location = new System.Drawing.Point(290, 395);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(90, 35);
            this.buttonCancel.TabIndex = 7;
            this.buttonCancel.Text = "ยกเลิก";
            this.buttonCancel.UseVisualStyleBackColor = false;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // listBoxQwidSuggestions
            // 
            this.listBoxQwidSuggestions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.listBoxQwidSuggestions.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.listBoxQwidSuggestions.ForeColor = System.Drawing.Color.White;
            this.listBoxQwidSuggestions.ItemHeight = 15;
            this.listBoxQwidSuggestions.Location = new System.Drawing.Point(20, 70);
            this.listBoxQwidSuggestions.Name = "listBoxQwidSuggestions";
            this.listBoxQwidSuggestions.Size = new System.Drawing.Size(360, 109);
            this.listBoxQwidSuggestions.TabIndex = 2;
            this.listBoxQwidSuggestions.Visible = false;
            this.listBoxQwidSuggestions.Click += new System.EventHandler(this.ListBoxQwidSuggestions_Click);
            this.listBoxQwidSuggestions.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ListBoxQwidSuggestions_KeyDown);
            // 
            // listBoxSerialSuggestions
            // 
            this.listBoxSerialSuggestions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.listBoxSerialSuggestions.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.listBoxSerialSuggestions.ForeColor = System.Drawing.Color.White;
            this.listBoxSerialSuggestions.ItemHeight = 15;
            this.listBoxSerialSuggestions.Location = new System.Drawing.Point(20, 270);
            this.listBoxSerialSuggestions.Name = "listBoxSerialSuggestions";
            this.listBoxSerialSuggestions.Size = new System.Drawing.Size(360, 109);
            this.listBoxSerialSuggestions.TabIndex = 5;
            this.listBoxSerialSuggestions.Visible = false;
            this.listBoxSerialSuggestions.Click += new System.EventHandler(this.ListBoxSerialSuggestions_Click);
            this.listBoxSerialSuggestions.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ListBoxSerialSuggestions_KeyDown);
            // 
            // RepairQwidDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.ClientSize = new System.Drawing.Size(400, 450);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonCreate);
            this.Controls.Add(this.listBoxSerialSuggestions);
            this.Controls.Add(this.textBoxSerial);
            this.Controls.Add(this.labelSerial);
            this.Controls.Add(this.labelModelInfo);
            this.Controls.Add(this.listBoxQwidSuggestions);
            this.Controls.Add(this.textBoxQwid);
            this.Controls.Add(this.labelQwid);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RepairQwidDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
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
                // Check if _mySqlManager is null
                if (_mySqlManager == null)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ _mySqlManager is null in LoadQwidData");
                    return;
                }
                
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
                    System.Diagnostics.Debug.WriteLine($"✅ โหลด QWID สำเร็จ: {_qwidList.Count} รายการ");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ ไม่สามารถโหลด QWID: Success={result.Success}, Data={result.Data != null}");
                    if (!result.Success && !string.IsNullOrEmpty(result.ErrorMessage))
                    {
                        System.Diagnostics.Debug.WriteLine($"Error: {result.ErrorMessage}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Exception ในการโหลด QWID: {ex.Message}");
                MessageBox.Show($"เกิดข้อผิดพลาดในการโหลดข้อมูล QWID: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void LoadSerialData()
        {
            try
            {
                // Check if _mySqlManager is null
                if (_mySqlManager == null)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ _mySqlManager is null in LoadSerialData");
                    return;
                }
                
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
                    System.Diagnostics.Debug.WriteLine($"✅ โหลด Serial สำเร็จ: {_serialList.Count} รายการ");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ ไม่สามารถโหลด Serial: Success={result.Success}, Data={result.Data != null}");
                    if (!result.Success && !string.IsNullOrEmpty(result.ErrorMessage))
                    {
                        System.Diagnostics.Debug.WriteLine($"Error: {result.ErrorMessage}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Exception ในการโหลด Serial: {ex.Message}");
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
            System.Diagnostics.Debug.WriteLine($"🔍 FilterQwidSuggestions: searchText='{searchText}'");
            
            if (string.IsNullOrEmpty(searchText))
            {
                listBoxQwidSuggestions.Visible = false;
                return;
            }

            if (_qwidList == null)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ _qwidList is null!");
                listBoxQwidSuggestions.Visible = false;
                return;
            }

            System.Diagnostics.Debug.WriteLine($"📊 _qwidList มี {_qwidList.Count} รายการ");

            // ค้นหาใน buffer ก่อน
            var filteredFromBuffer = _qwidList
                .Where(q => q.Qwid.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                .Take(10)
                .ToList();

            System.Diagnostics.Debug.WriteLine($"📋 พบ {filteredFromBuffer.Count} รายการใน buffer");

            listBoxQwidSuggestions.Items.Clear();
            
            if (filteredFromBuffer.Count > 0)
            {
                foreach (var item in filteredFromBuffer)
                {
                    listBoxQwidSuggestions.Items.Add($"{item.Qwid} - {item.Serial}");
                }
                listBoxQwidSuggestions.Visible = true;
                System.Diagnostics.Debug.WriteLine($"✅ แสดง listBoxQwidSuggestions");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"🔎 ไม่พบใน buffer, ค้นหาใน database...");
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

            // ถ้ายังไม่ได้เลือก QWID (ไม่มี model_id) ให้แสดง warning
            if (_selectedModelId <= 0)
            {
                listBoxSerialSuggestions.Items.Clear();
                listBoxSerialSuggestions.Items.Add("⚠️ กรุณาเลือก QWID ก่อน");
                listBoxSerialSuggestions.Visible = true;
                return;
            }

            // ค้นหาใน buffer ก่อน และกรองตาม model_id
            var filteredFromBuffer = _serialList
                .Where(s => s.ModelId == _selectedModelId && 
                           s.SerialNumber.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
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
            // ถ้ายังไม่ได้เลือก QWID ให้แสดง warning
            if (_selectedModelId <= 0)
            {
                listBoxSerialSuggestions.Items.Clear();
                listBoxSerialSuggestions.Items.Add("⚠️ กรุณาเลือก QWID ก่อน");
                listBoxSerialSuggestions.Visible = true;
                return;
            }

            try
            {
                string sql = $@"SELECT id, serial_number, model_id, status, created_at 
                              FROM Orbitz.software_serial_numbers 
                              WHERE status = 'active'
                              AND model_id = {_selectedModelId}
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
                        listBoxSerialSuggestions.Items.Clear();
                        listBoxSerialSuggestions.Items.Add("❌ ไม่พบ ชุดทดสอบ สำหรับ Model นี้");
                        listBoxSerialSuggestions.Visible = true;
                    }
                }
                else
                {
                    listBoxSerialSuggestions.Items.Clear();
                    listBoxSerialSuggestions.Items.Add("❌ ไม่พบ ชุดทดสอบ สำหรับ Model นี้");
                    listBoxSerialSuggestions.Visible = true;
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
                labelModelInfo.Visible = false;
                _selectedModelId = -1;
                return;
            }

            // Find QWID in buffer first
            var qwidItem = _qwidList.FirstOrDefault(q => 
                q.Qwid.Equals(qwid, StringComparison.OrdinalIgnoreCase));

            if (qwidItem != null)
            {
                SelectedQwid = qwidItem.Qwid;
                _selectedModelId = qwidItem.ModelId;
                
                // Query model information from spaze.models
                await LoadAndDisplayModelInfo(qwidItem.ModelId);
                
                // Clear serial selection เพื่อให้เลือกใหม่
                textBoxSerial.Clear();
                
                // แสดง ListBox ของชุดทดสอบทันทีหลังจากเลือก QWID
                await ShowAllSerialSuggestionsForModel(_selectedModelId);
                
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

        private async Task ShowAllSerialSuggestionsForModel(int modelId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"📋 แสดง Serial ทั้งหมดสำหรับ Model ID: {modelId}");
                
                if (modelId <= 0)
                {
                    listBoxSerialSuggestions.Visible = false;
                    return;
                }

                // ค้นหาใน buffer ก่อน
                var serialsForModel = _serialList?
                    .Where(s => s.ModelId == modelId)
                    .Take(20)
                    .ToList();

                listBoxSerialSuggestions.Items.Clear();
                
                if (serialsForModel != null && serialsForModel.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ พบ {serialsForModel.Count} Serial ใน buffer");
                    foreach (var item in serialsForModel)
                    {
                        listBoxSerialSuggestions.Items.Add(item.SerialNumber);
                    }
                    listBoxSerialSuggestions.Visible = true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"🔎 ไม่พบใน buffer, ค้นหาใน database...");
                    // ถ้าไม่พบใน buffer ให้ค้นหาใน database
                    await LoadSerialFromDatabaseByModel(modelId);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Exception: {ex.Message}");
                listBoxSerialSuggestions.Visible = false;
            }
        }

        private async Task LoadSerialFromDatabaseByModel(int modelId)
        {
            try
            {
                string sql = $@"SELECT id, serial_number, model_id, status, created_at 
                              FROM Orbitz.software_serial_numbers 
                              WHERE status = 'active'
                              AND model_id = {modelId}
                              ORDER BY created_at DESC 
                              LIMIT 20";
                
                var result = await _mySqlManager.ExecuteQuery(sql);
                
                if (result.Success && result.Data != null && result.Data.Rows.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ พบ {result.Data.Rows.Count} Serial ใน database");
                    
                    listBoxSerialSuggestions.Items.Clear();
                    foreach (DataRow row in result.Data.Rows)
                    {
                        string serialNumber = row["serial_number"]?.ToString() ?? "";
                        listBoxSerialSuggestions.Items.Add(serialNumber);
                    }
                    listBoxSerialSuggestions.Visible = true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ ไม่พบ Serial สำหรับ Model ID: {modelId}");
                    listBoxSerialSuggestions.Items.Clear();
                    listBoxSerialSuggestions.Items.Add("❌ ไม่พบชุดทดสอบสำหรับ Model นี้");
                    listBoxSerialSuggestions.Visible = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Exception: {ex.Message}");
                listBoxSerialSuggestions.Items.Clear();
                listBoxSerialSuggestions.Items.Add("❌ เกิดข้อผิดพลาดในการโหลดข้อมูล");
                listBoxSerialSuggestions.Visible = true;
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
                    _selectedModelId = row["model_id"] != DBNull.Value ? Convert.ToInt32(row["model_id"]) : -1;
                    
                    // Query model information
                    await LoadAndDisplayModelInfo(_selectedModelId);
                    
                    // Clear serial selection เพื่อให้เลือกใหม่
                    textBoxSerial.Clear();
                    
                    // แสดง ListBox ของชุดทดสอบทันทีหลังจากเลือก QWID
                    await ShowAllSerialSuggestionsForModel(_selectedModelId);
                    
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
                    labelModelInfo.Visible = false;
                    _selectedModelId = -1;
                    buttonCreate.Enabled = false;
                }
            }
            catch (Exception)
            {
                labelModelInfo.Visible = false;
                _selectedModelId = -1;
                buttonCreate.Enabled = false;
            }
        }

        private async Task LoadAndDisplayModelInfo(int modelId)
        {
            if (modelId <= 0)
            {
                labelModelInfo.Visible = false;
                return;
            }

            try
            {
                string sql = $@"SELECT id, name, description 
                              FROM spaze.models 
                              WHERE id = {modelId}";
                
                var result = await _mySqlManager.ExecuteQuery(sql);
                
                if (result.Success && result.Data != null && result.Data.Rows.Count > 0)
                {
                    var row = result.Data.Rows[0];
                    string modelName = row["name"]?.ToString() ?? "";
                    string modelDesc = row["description"]?.ToString() ?? "";
                    
                    // แสดงข้อมูล Model
                    if (!string.IsNullOrEmpty(modelDesc))
                    {
                        labelModelInfo.Text = $"📦 Model: {modelName} ({modelDesc})";
                    }
                    else
                    {
                        labelModelInfo.Text = $"📦 Model: {modelName}";
                    }
                    labelModelInfo.Visible = true;
                }
                else
                {
                    labelModelInfo.Text = $"⚠️ ไม่พบข้อมูล Model (ID: {modelId})";
                    labelModelInfo.ForeColor = Color.Orange;
                    labelModelInfo.Visible = true;
                }
            }
            catch (Exception ex)
            {
                labelModelInfo.Text = $"❌ Error loading model: {ex.Message}";
                labelModelInfo.ForeColor = Color.Red;
                labelModelInfo.Visible = true;
            }
        }

        private async void SelectCurrentSerial()
        {
            string serial = textBoxSerial.Text.Trim();
            
            if (string.IsNullOrEmpty(serial))
            {
                return;
            }

            // ตรวจสอบว่าเลือก QWID แล้วหรือยัง
            if (_selectedModelId <= 0)
            {
                MessageBox.Show("กรุณาเลือก QWID ก่อนเลือก Serial Number", 
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                buttonCreate.Enabled = false;
                return;
            }

            // Find Serial in buffer first และตรวจสอบ model_id
            var serialItem = _serialList?.FirstOrDefault(s => 
                s.SerialNumber.Equals(serial, StringComparison.OrdinalIgnoreCase) &&
                s.ModelId == _selectedModelId);

            if (serialItem != null)
            {
                // Serial exists in buffer และ model_id ตรงกัน
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
                
                // ถ้ามี model_id ให้กรองด้วย
                if (_selectedModelId > 0)
                {
                    sql += $" AND model_id = {_selectedModelId}";
                }
                
                var result = await _mySqlManager.ExecuteQuery(sql);
                
                if (result.Success && result.Data != null && result.Data.Rows.Count > 0)
                {
                    var row = result.Data.Rows[0];
                    int serialModelId = row["model_id"] != DBNull.Value ? Convert.ToInt32(row["model_id"]) : 0;
                    
                    // ตรวจสอบว่า model_id ตรงกันหรือไม่
                    if (_selectedModelId > 0 && serialModelId != _selectedModelId)
                    {
                        MessageBox.Show($"Serial Number นี้ไม่ตรงกับ Model ที่เลือก!\nSerial Model ID: {serialModelId}\nQWID Model ID: {_selectedModelId}", 
                            "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        buttonCreate.Enabled = false;
                        return;
                    }
                    
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
                    // Serial doesn't exist หรือไม่ตรงกับ model
                    if (_selectedModelId > 0)
                    {
                        MessageBox.Show($"ไม่พบ ชุดทดสอบ นี้สำหรับ Model ที่เลือก", 
                            "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
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

