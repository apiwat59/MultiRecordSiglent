using System;
using System.Drawing;
using System.Windows.Forms;

namespace MultiRecord
{
    public partial class LoginForm : Form
    {
        public bool LoginSuccessful { get; private set; } = false;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            string username = textBoxUsername.Text.Trim();
            string password = textBoxPassword.Text;

            // Hard-coded credentials for mockup (implement proper authentication later)
            if (username == "admin" && password == "admin")
            {
                LoginSuccessful = true;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                labelErrorMessage.Text = "Invalid username or password!";
                labelErrorMessage.Visible = true;
                textBoxPassword.Clear();
                textBoxUsername.Focus();
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void textBoxPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                buttonLogin_Click(sender, e);
            }
        }

        private void textBoxUsername_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                textBoxPassword.Focus();
            }
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            textBoxUsername.Focus();
            labelErrorMessage.Visible = false;
        }
    }
}