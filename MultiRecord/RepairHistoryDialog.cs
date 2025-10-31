using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MultiRecord
{
    public class RepairHistoryDialog : Form
    {
        private Label labelTitle;
        private Label labelInfo;
        private DataGridView dataGridViewHistory;
        private Button buttonClose;
        private Button buttonSelect;
        private Button buttonDelete;
        private Button buttonChangeType;

        public RepairSession SelectedSession { get; private set; }
        public bool SessionDeleted { get; private set; }
        public bool SessionTypeChanged { get; private set; }

        private List<RepairSession> _sessions;
        private MySqlManager _mysqlManager;

        public RepairHistoryDialog(List<RepairSession> sessions, MySqlManager mysqlManager, string qwId, string serialNumber)
        {
            _sessions = sessions;
            _mysqlManager = mysqlManager;
            
            InitializeComponent();
            LoadSessionInfo(qwId, serialNumber);
            LoadSessions();
        }

        private void InitializeComponent()
        {
            // Form settings
            this.Text = "ประวัติการซ่อม";
            this.Size = new Size(800, 500);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(600, 400);
            this.BackColor = Color.White;

            // Title
            labelTitle = new Label
            {
                Text = "ประวัติการซ่อม",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(740, 30),
                ForeColor = Color.FromArgb(41, 128, 185),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // Info label
            labelInfo = new Label
            {
                Text = "QWID: - | Serial Number: -",
                Font = new Font("Segoe UI", 10F),
                Location = new Point(20, 55),
                Size = new Size(740, 25),
                ForeColor = Color.FromArgb(127, 140, 141),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // DataGridView
            dataGridViewHistory = new DataGridView
            {
                Location = new Point(20, 90),
                Size = new Size(740, 300),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                ColumnHeadersHeight = 40,
                RowTemplate = { Height = 35 }
            };

            // Setup columns
            dataGridViewHistory.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "SessionNumber",
                HeaderText = "ครั้งที่",
                DataPropertyName = "SessionNumber",
                FillWeight = 15
            });

            dataGridViewHistory.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CreatedAt",
                HeaderText = "วันที่สร้าง",
                DataPropertyName = "CreatedAt",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" },
                FillWeight = 25
            });

            dataGridViewHistory.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "RepairType",
                HeaderText = "ประเภท",
                DataPropertyName = "RepairType",
                FillWeight = 20
            });

            dataGridViewHistory.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "SessionNote",
                HeaderText = "หมายเหตุ",
                DataPropertyName = "SessionNote",
                FillWeight = 40
            });

            dataGridViewHistory.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Status",
                HeaderText = "สถานะ",
                DataPropertyName = "Status",
                FillWeight = 20
            });

            // Style header
            dataGridViewHistory.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dataGridViewHistory.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dataGridViewHistory.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridViewHistory.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewHistory.EnableHeadersVisualStyles = false;

            // Style cells
            dataGridViewHistory.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            dataGridViewHistory.DefaultCellStyle.SelectionBackColor = Color.FromArgb(52, 152, 219);
            dataGridViewHistory.DefaultCellStyle.SelectionForeColor = Color.White;
            dataGridViewHistory.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(236, 240, 241);

            // Buttons
            buttonDelete = new Button
            {
                Text = "ลบ",
                Font = new Font("Segoe UI", 10F),
                Location = new Point(20, 410),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            buttonDelete.FlatAppearance.BorderSize = 0;
            buttonDelete.Click += ButtonDelete_Click;

            buttonChangeType = new Button
            {
                Text = "เปลี่ยนประเภท",
                Font = new Font("Segoe UI", 10F),
                Location = new Point(130, 410),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            buttonChangeType.FlatAppearance.BorderSize = 0;
            buttonChangeType.Click += ButtonChangeType_Click;

            buttonClose = new Button
            {
                Text = "ปิด",
                Font = new Font("Segoe UI", 10F),
                Location = new Point(550, 410),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            buttonClose.FlatAppearance.BorderSize = 0;
            buttonClose.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            buttonSelect = new Button
            {
                Text = "เลือก",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(660, 410),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            buttonSelect.FlatAppearance.BorderSize = 0;
            buttonSelect.Click += ButtonSelect_Click;

            // Add controls
            Controls.AddRange(new Control[]
            {
                labelTitle,
                labelInfo,
                dataGridViewHistory,
                buttonDelete,
                buttonChangeType,
                buttonClose,
                buttonSelect
            });

            // Event handlers
            dataGridViewHistory.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    ButtonSelect_Click(s, e);
                }
            };
            
            dataGridViewHistory.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex >= 0 && dataGridViewHistory.Columns[e.ColumnIndex].Name == "RepairType")
                {
                    if (e.Value != null)
                    {
                        string repairType = e.Value.ToString();
                        e.Value = repairType == "before_repair" ? "ก่อนซ่อม" : "หลังซ่อม";
                        e.FormattingApplied = true;
                    }
                }
            };
        }

        private void LoadSessionInfo(string qwId, string serialNumber)
        {
            labelInfo.Text = $"QWID: {qwId} | Serial Number: {serialNumber}";
        }

        private void LoadSessions()
        {
            dataGridViewHistory.DataSource = null;
            
            if (_sessions != null && _sessions.Any())
            {
                // Create display list with formatted data
                var displaySessions = _sessions.Select(s => new
                {
                    SessionNumber = s.SessionNumber,
                    CreatedAt = s.CreatedAt,
                    RepairType = s.RepairType,
                    SessionNote = string.IsNullOrEmpty(s.SessionNote) ? "-" : s.SessionNote,
                    Status = GetStatusText(s.Status),
                    Session = s
                }).ToList();

                dataGridViewHistory.DataSource = displaySessions;
                
                // Hide the Session column
                if (dataGridViewHistory.Columns.Contains("Session"))
                {
                    dataGridViewHistory.Columns["Session"].Visible = false;
                }
            }
        }

        private string GetStatusText(string status)
        {
            switch (status?.ToLower())
            {
                case "in_progress":
                    return "กำลังดำเนินการ";
                case "completed":
                    return "เสร็จสิ้น ✓";
                case "cancelled":
                    return "ยกเลิก";
                default:
                    return status ?? "-";
            }
        }

        private void ButtonSelect_Click(object sender, EventArgs e)
        {
            if (dataGridViewHistory.SelectedRows.Count > 0)
            {
                var selectedRow = dataGridViewHistory.SelectedRows[0];
                if (selectedRow.DataBoundItem != null)
                {
                    var item = selectedRow.DataBoundItem;
                    var sessionProperty = item.GetType().GetProperty("Session");
                    if (sessionProperty != null)
                    {
                        SelectedSession = (RepairSession)sessionProperty.GetValue(item);
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                }
            }
            else
            {
                MessageBox.Show("กรุณาเลือกรอบการซ่อมที่ต้องการ", "แจ้งเตือน", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void ButtonDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewHistory.SelectedRows.Count > 0)
            {
                var selectedRow = dataGridViewHistory.SelectedRows[0];
                if (selectedRow.DataBoundItem != null)
                {
                    var item = selectedRow.DataBoundItem;
                    var sessionProperty = item.GetType().GetProperty("Session");
                    if (sessionProperty != null)
                    {
                        var session = (RepairSession)sessionProperty.GetValue(item);
                        
                        var result = MessageBox.Show(
                            $"คุณต้องการลบรอบการซ่อมครั้งที่ {session.SessionNumber} หรือไม่?\n\n" +
                            $"หมายเหตุ: {(string.IsNullOrEmpty(session.SessionNote) ? "-" : session.SessionNote)}\n\n" +
                            "การลบจะไม่สามารถกู้คืนได้!",
                            "ยืนยันการลบ",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning);

                        if (result == DialogResult.Yes)
                        {
                            try
                            {
                                bool deleted = await _mysqlManager.DeleteRepairSessionAsync(session.Id);
                                if (deleted)
                                {
                                    MessageBox.Show("ลบรอบการซ่อมสำเร็จ", "สำเร็จ", 
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    
                                    // Remove from list
                                    _sessions.Remove(session);
                                    LoadSessions();
                                    
                                    SessionDeleted = true;
                                }
                                else
                                {
                                    MessageBox.Show("ไม่สามารถลบรอบการซ่อมได้", "ข้อผิดพลาด", 
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}", "ข้อผิดพลาด", 
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("กรุณาเลือกรอบการซ่อมที่ต้องการลบ", "แจ้งเตือน", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void ButtonChangeType_Click(object sender, EventArgs e)
        {
            if (dataGridViewHistory.SelectedRows.Count > 0)
            {
                var selectedRow = dataGridViewHistory.SelectedRows[0];
                if (selectedRow.DataBoundItem != null)
                {
                    var item = selectedRow.DataBoundItem;
                    var sessionProperty = item.GetType().GetProperty("Session");
                    if (sessionProperty != null)
                    {
                        var session = (RepairSession)sessionProperty.GetValue(item);
                        
                        // Toggle repair type
                        string newType = session.RepairType == "before_repair" ? "after_repair" : "before_repair";
                        string newTypeText = newType == "before_repair" ? "ก่อนซ่อม" : "หลังซ่อม";
                        string currentTypeText = session.RepairType == "before_repair" ? "ก่อนซ่อม" : "หลังซ่อม";
                        
                        // Show dialog to input note
                        using (var noteDialog = new ChangeTypeNoteDialog(currentTypeText, newTypeText))
                        {
                            if (noteDialog.ShowDialog(this) == DialogResult.OK)
                            {
                                string note = noteDialog.Note;
                                
                                try
                                {
                                    bool updated = await _mysqlManager.UpdateRepairSessionTypeAsync(session.Id, newType, note);
                                    if (updated)
                                    {
                                        MessageBox.Show($"เปลี่ยนประเภทเป็น '{newTypeText}' สำเร็จ", "สำเร็จ", 
                                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        
                                        // Update session in memory
                                        session.RepairType = newType;
                                        LoadSessions();
                                        
                                        // Set flag for Main.cs to reload
                                        SessionTypeChanged = true;
                                    }
                                    else
                                    {
                                        MessageBox.Show("ไม่สามารถเปลี่ยนประเภทได้", "ข้อผิดพลาด", 
                                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}", "ข้อผิดพลาด", 
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("กรุณาเลือกรอบการซ่อมที่ต้องการเปลี่ยนประเภท", "แจ้งเตือน", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}



