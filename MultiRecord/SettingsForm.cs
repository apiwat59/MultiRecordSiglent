using System;
using System.IO;
using System.Windows.Forms;

namespace MultiRecord
{
    public partial class SettingsForm : Form
    {
        private GroupBox groupBoxSounds;
        private Label labelBeepSound;
        private Label labelDeleteSound;
        private Label labelOverSound;
        private TextBox textBoxBeepSound;
        private TextBox textBoxDeleteSound;
        private TextBox textBoxOverSound;
        private Button buttonBrowseBeep;
        private Button buttonBrowseDelete;
        private Button buttonBrowseOver;
        private Button buttonTestBeep;
        private Button buttonTestDelete;
        private Button buttonTestOver;
        private Button buttonSave;
        private Button buttonCancel;
        private Button buttonReset;

        public SettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.groupBoxSounds = new System.Windows.Forms.GroupBox();
            this.labelBeepSound = new System.Windows.Forms.Label();
            this.labelDeleteSound = new System.Windows.Forms.Label();
            this.labelOverSound = new System.Windows.Forms.Label();
            this.textBoxBeepSound = new System.Windows.Forms.TextBox();
            this.textBoxDeleteSound = new System.Windows.Forms.TextBox();
            this.textBoxOverSound = new System.Windows.Forms.TextBox();
            this.buttonBrowseBeep = new System.Windows.Forms.Button();
            this.buttonBrowseDelete = new System.Windows.Forms.Button();
            this.buttonBrowseOver = new System.Windows.Forms.Button();
            this.buttonTestBeep = new System.Windows.Forms.Button();
            this.buttonTestDelete = new System.Windows.Forms.Button();
            this.buttonTestOver = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonReset = new System.Windows.Forms.Button();
            this.groupBoxSounds.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxSounds
            // 
            this.groupBoxSounds.Controls.Add(this.labelBeepSound);
            this.groupBoxSounds.Controls.Add(this.labelDeleteSound);
            this.groupBoxSounds.Controls.Add(this.labelOverSound);
            this.groupBoxSounds.Controls.Add(this.textBoxBeepSound);
            this.groupBoxSounds.Controls.Add(this.textBoxDeleteSound);
            this.groupBoxSounds.Controls.Add(this.textBoxOverSound);
            this.groupBoxSounds.Controls.Add(this.buttonBrowseBeep);
            this.groupBoxSounds.Controls.Add(this.buttonBrowseDelete);
            this.groupBoxSounds.Controls.Add(this.buttonBrowseOver);
            this.groupBoxSounds.Controls.Add(this.buttonTestBeep);
            this.groupBoxSounds.Controls.Add(this.buttonTestDelete);
            this.groupBoxSounds.Controls.Add(this.buttonTestOver);
            this.groupBoxSounds.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.groupBoxSounds.ForeColor = System.Drawing.Color.White;
            this.groupBoxSounds.Location = new System.Drawing.Point(20, 20);
            this.groupBoxSounds.Name = "groupBoxSounds";
            this.groupBoxSounds.Size = new System.Drawing.Size(540, 280);
            this.groupBoxSounds.TabIndex = 0;
            this.groupBoxSounds.TabStop = false;
            this.groupBoxSounds.Text = "เสียงแจ้งเตือน - Sound Alerts";
            // 
            // labelBeepSound
            // 
            this.labelBeepSound.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelBeepSound.ForeColor = System.Drawing.Color.White;
            this.labelBeepSound.Location = new System.Drawing.Point(20, 40);
            this.labelBeepSound.Name = "labelBeepSound";
            this.labelBeepSound.Size = new System.Drawing.Size(150, 23);
            this.labelBeepSound.TabIndex = 0;
            this.labelBeepSound.Text = "เสียงบันทึกข้อมูล (Beep):";
            // 
            // labelDeleteSound
            // 
            this.labelDeleteSound.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelDeleteSound.ForeColor = System.Drawing.Color.White;
            this.labelDeleteSound.Location = new System.Drawing.Point(20, 100);
            this.labelDeleteSound.Name = "labelDeleteSound";
            this.labelDeleteSound.Size = new System.Drawing.Size(150, 23);
            this.labelDeleteSound.TabIndex = 1;
            this.labelDeleteSound.Text = "เสียงลบข้อมูล (Delete):";
            // 
            // labelOverSound
            // 
            this.labelOverSound.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelOverSound.ForeColor = System.Drawing.Color.White;
            this.labelOverSound.Location = new System.Drawing.Point(20, 160);
            this.labelOverSound.Name = "labelOverSound";
            this.labelOverSound.Size = new System.Drawing.Size(150, 23);
            this.labelOverSound.TabIndex = 2;
            this.labelOverSound.Text = "เสียงเกินค่า (Over):";
            // 
            // textBoxBeepSound
            // 
            this.textBoxBeepSound.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.textBoxBeepSound.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxBeepSound.ForeColor = System.Drawing.Color.White;
            this.textBoxBeepSound.Location = new System.Drawing.Point(20, 65);
            this.textBoxBeepSound.Name = "textBoxBeepSound";
            this.textBoxBeepSound.Size = new System.Drawing.Size(300, 25);
            this.textBoxBeepSound.TabIndex = 3;
            // 
            // textBoxDeleteSound
            // 
            this.textBoxDeleteSound.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.textBoxDeleteSound.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxDeleteSound.ForeColor = System.Drawing.Color.White;
            this.textBoxDeleteSound.Location = new System.Drawing.Point(20, 125);
            this.textBoxDeleteSound.Name = "textBoxDeleteSound";
            this.textBoxDeleteSound.Size = new System.Drawing.Size(300, 25);
            this.textBoxDeleteSound.TabIndex = 4;
            // 
            // textBoxOverSound
            // 
            this.textBoxOverSound.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.textBoxOverSound.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxOverSound.ForeColor = System.Drawing.Color.White;
            this.textBoxOverSound.Location = new System.Drawing.Point(20, 185);
            this.textBoxOverSound.Name = "textBoxOverSound";
            this.textBoxOverSound.Size = new System.Drawing.Size(300, 25);
            this.textBoxOverSound.TabIndex = 5;
            // 
            // buttonBrowseBeep
            // 
            this.buttonBrowseBeep.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(122)))), ((int)(((byte)(183)))));
            this.buttonBrowseBeep.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonBrowseBeep.ForeColor = System.Drawing.Color.White;
            this.buttonBrowseBeep.Location = new System.Drawing.Point(330, 65);
            this.buttonBrowseBeep.Name = "buttonBrowseBeep";
            this.buttonBrowseBeep.Size = new System.Drawing.Size(80, 37);
            this.buttonBrowseBeep.TabIndex = 6;
            this.buttonBrowseBeep.Text = "เลือกไฟล์";
            this.buttonBrowseBeep.UseVisualStyleBackColor = false;
            this.buttonBrowseBeep.Click += new System.EventHandler(this.ButtonBrowseBeep_Click);
            // 
            // buttonBrowseDelete
            // 
            this.buttonBrowseDelete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(122)))), ((int)(((byte)(183)))));
            this.buttonBrowseDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonBrowseDelete.ForeColor = System.Drawing.Color.White;
            this.buttonBrowseDelete.Location = new System.Drawing.Point(330, 125);
            this.buttonBrowseDelete.Name = "buttonBrowseDelete";
            this.buttonBrowseDelete.Size = new System.Drawing.Size(80, 37);
            this.buttonBrowseDelete.TabIndex = 7;
            this.buttonBrowseDelete.Text = "เลือกไฟล์";
            this.buttonBrowseDelete.UseVisualStyleBackColor = false;
            this.buttonBrowseDelete.Click += new System.EventHandler(this.ButtonBrowseDelete_Click);
            // 
            // buttonBrowseOver
            // 
            this.buttonBrowseOver.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(122)))), ((int)(((byte)(183)))));
            this.buttonBrowseOver.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonBrowseOver.ForeColor = System.Drawing.Color.White;
            this.buttonBrowseOver.Location = new System.Drawing.Point(330, 185);
            this.buttonBrowseOver.Name = "buttonBrowseOver";
            this.buttonBrowseOver.Size = new System.Drawing.Size(80, 35);
            this.buttonBrowseOver.TabIndex = 8;
            this.buttonBrowseOver.Text = "เลือกไฟล์";
            this.buttonBrowseOver.UseVisualStyleBackColor = false;
            this.buttonBrowseOver.Click += new System.EventHandler(this.ButtonBrowseOver_Click);
            // 
            // buttonTestBeep
            // 
            this.buttonTestBeep.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(106)))), ((int)(((byte)(153)))), ((int)(((byte)(78)))));
            this.buttonTestBeep.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonTestBeep.ForeColor = System.Drawing.Color.White;
            this.buttonTestBeep.Location = new System.Drawing.Point(420, 65);
            this.buttonTestBeep.Name = "buttonTestBeep";
            this.buttonTestBeep.Size = new System.Drawing.Size(80, 37);
            this.buttonTestBeep.TabIndex = 9;
            this.buttonTestBeep.Text = "ทดสอบ";
            this.buttonTestBeep.UseVisualStyleBackColor = false;
            this.buttonTestBeep.Click += new System.EventHandler(this.ButtonTestBeep_Click);
            // 
            // buttonTestDelete
            // 
            this.buttonTestDelete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(106)))), ((int)(((byte)(153)))), ((int)(((byte)(78)))));
            this.buttonTestDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonTestDelete.ForeColor = System.Drawing.Color.White;
            this.buttonTestDelete.Location = new System.Drawing.Point(420, 125);
            this.buttonTestDelete.Name = "buttonTestDelete";
            this.buttonTestDelete.Size = new System.Drawing.Size(80, 37);
            this.buttonTestDelete.TabIndex = 10;
            this.buttonTestDelete.Text = "ทดสอบ";
            this.buttonTestDelete.UseVisualStyleBackColor = false;
            this.buttonTestDelete.Click += new System.EventHandler(this.ButtonTestDelete_Click);
            // 
            // buttonTestOver
            // 
            this.buttonTestOver.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(106)))), ((int)(((byte)(153)))), ((int)(((byte)(78)))));
            this.buttonTestOver.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonTestOver.ForeColor = System.Drawing.Color.White;
            this.buttonTestOver.Location = new System.Drawing.Point(420, 185);
            this.buttonTestOver.Name = "buttonTestOver";
            this.buttonTestOver.Size = new System.Drawing.Size(80, 35);
            this.buttonTestOver.TabIndex = 11;
            this.buttonTestOver.Text = "ทดสอบ";
            this.buttonTestOver.UseVisualStyleBackColor = false;
            this.buttonTestOver.Click += new System.EventHandler(this.ButtonTestOver_Click);
            // 
            // buttonSave
            // 
            this.buttonSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(106)))), ((int)(((byte)(153)))), ((int)(((byte)(78)))));
            this.buttonSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonSave.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.buttonSave.ForeColor = System.Drawing.Color.White;
            this.buttonSave.Location = new System.Drawing.Point(394, 320);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(80, 30);
            this.buttonSave.TabIndex = 1;
            this.buttonSave.Text = "บันทึก";
            this.buttonSave.UseVisualStyleBackColor = false;
            this.buttonSave.Click += new System.EventHandler(this.ButtonSave_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(186)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCancel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.buttonCancel.ForeColor = System.Drawing.Color.White;
            this.buttonCancel.Location = new System.Drawing.Point(480, 320);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(80, 30);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "ยกเลิก";
            this.buttonCancel.UseVisualStyleBackColor = false;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // buttonReset
            // 
            this.buttonReset.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(140)))), ((int)(((byte)(0)))));
            this.buttonReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonReset.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.buttonReset.ForeColor = System.Drawing.Color.White;
            this.buttonReset.Location = new System.Drawing.Point(20, 320);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(100, 30);
            this.buttonReset.TabIndex = 3;
            this.buttonReset.Text = "รีเซ็ตเริ่มต้น";
            this.buttonReset.UseVisualStyleBackColor = false;
            this.buttonReset.Click += new System.EventHandler(this.ButtonReset_Click);
            // 
            // SettingsForm
            // 
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.ClientSize = new System.Drawing.Size(584, 361);
            this.Controls.Add(this.groupBoxSounds);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonReset);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ตั้งค่า - Settings";
            this.groupBoxSounds.ResumeLayout(false);
            this.groupBoxSounds.PerformLayout();
            this.ResumeLayout(false);

        }

        private void ButtonBrowseBeep_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Audio Files|*.mp3;*.wav;*.m4a;*.aac|All Files|*.*";
                dialog.Title = "เลือกไฟล์เสียงสำหรับบันทึกข้อมูล";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxBeepSound.Text = dialog.FileName;
                }
            }
        }

        private void ButtonBrowseDelete_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Audio Files|*.mp3;*.wav;*.m4a;*.aac|All Files|*.*";
                dialog.Title = "เลือกไฟล์เสียงสำหรับลบข้อมูล";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxDeleteSound.Text = dialog.FileName;
                }
            }
        }

        private void ButtonBrowseOver_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Audio Files|*.mp3;*.wav;*.m4a;*.aac|All Files|*.*";
                dialog.Title = "เลือกไฟล์เสียงสำหรับเกินค่า";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxOverSound.Text = dialog.FileName;
                }
            }
        }

        private void ButtonTestBeep_Click(object sender, EventArgs e)
        {
            if (File.Exists(textBoxBeepSound.Text))
            {
                SoundUtil.PlayCustomSound(textBoxBeepSound.Text);
            }
            else
            {
                MessageBox.Show("ไม่พบไฟล์เสียง", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ButtonTestDelete_Click(object sender, EventArgs e)
        {
            if (File.Exists(textBoxDeleteSound.Text))
            {
                SoundUtil.PlayCustomSound(textBoxDeleteSound.Text);
            }
            else
            {
                MessageBox.Show("ไม่พบไฟล์เสียง", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ButtonTestOver_Click(object sender, EventArgs e)
        {
            if (File.Exists(textBoxOverSound.Text))
            {
                SoundUtil.PlayCustomSound(textBoxOverSound.Text);
            }
            else
            {
                MessageBox.Show("ไม่พบไฟล์เสียง", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            SaveSettings();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void ButtonReset_Click(object sender, EventArgs e)
        {
            textBoxBeepSound.Text = "sound/beep.mp3";
            textBoxDeleteSound.Text = "sound/delete.mp3";
            textBoxOverSound.Text = "sound/over.mp3";
        }

        private void LoadSettings()
        {
            textBoxBeepSound.Text = SettingsManager.GetSoundPath("Beep");
            textBoxDeleteSound.Text = SettingsManager.GetSoundPath("Delete");
            textBoxOverSound.Text = SettingsManager.GetSoundPath("Over");
        }

        private void SaveSettings()
        {
            SettingsManager.SetSoundPath("Beep", textBoxBeepSound.Text);
            SettingsManager.SetSoundPath("Delete", textBoxDeleteSound.Text);
            SettingsManager.SetSoundPath("Over", textBoxOverSound.Text);
            SettingsManager.SaveSettings();
        }
    }
}