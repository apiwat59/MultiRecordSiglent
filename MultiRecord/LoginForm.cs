using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiRecord
{
    public partial class LoginForm : Form
    {
        public AuthResult LoginResult { get; private set; }
        
        public LoginForm()
        {
            InitializeComponent();
            this.KeyPreview = true;
        }
        
        private async void buttonLogin_Click(object sender, EventArgs e)
        {
            await PerformLogin();
        }
        
        private async void LoginForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                await PerformLogin();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }
        
        private async Task PerformLogin()
        {
            string email = textBoxEmail.Text.Trim();
            string password = textBoxPassword.Text;
            
            if (string.IsNullOrEmpty(email))
            {
                ShowError("Please enter your email or username");
                textBoxEmail.Focus();
                return;
            }
            
            if (string.IsNullOrEmpty(password))
            {
                ShowError("Please enter your password");
                textBoxPassword.Focus();
                return;
            }
            
            // Show loading state
            SetLoginState(false);
            labelStatus.Text = "Logging in...";
            labelStatus.ForeColor = Color.Blue;
            
            try
            {
                LoginResult = await AuthManager.LoginAsync(email, password);
                
                if (LoginResult.Success)
                {
                    labelStatus.Text = "Login successful!";
                    labelStatus.ForeColor = Color.Green;
                    
                    // Small delay to show success message
                    await Task.Delay(500);
                    
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    ShowError(GetUserFriendlyError(LoginResult.Error, LoginResult.ErrorCode));
                    textBoxPassword.Clear();
                    textBoxPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Login failed: {ex.Message}");
            }
            finally
            {
                SetLoginState(true);
            }
        }
        
        private void SetLoginState(bool enabled)
        {
            textBoxEmail.Enabled = enabled;
            textBoxPassword.Enabled = enabled;
            buttonLogin.Enabled = enabled;
            buttonLogin.Text = enabled ? "Login" : "Logging in...";
        }
        
        private void ShowError(string message)
        {
            labelStatus.Text = message;
            labelStatus.ForeColor = Color.Red;
        }
        
        private string GetUserFriendlyError(string error, string errorCode)
        {
            switch (errorCode)
            {
                case "VALIDATION_ERROR":
                    return "Please fill in all required fields";
                case "LOGIN_FAILED":
                    return "Invalid username/email or password";
                case "ACCOUNT_DISABLED":
                    return "Your account has been disabled. Please contact administrator";
                case "NETWORK_ERROR":
                    return "Network connection failed. Please check your internet connection";
                case "TIMEOUT":
                    return "Connection timeout. Please try again";
                case "SERVER_ERROR":
                    return "Server error. Please try again later";
                default:
                    return error ?? "Login failed. Please try again";
            }
        }
        
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        
        private void checkBoxShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            textBoxPassword.UseSystemPasswordChar = !checkBoxShowPassword.Checked;
        }
        
        private void textBoxEmail_Enter(object sender, EventArgs e)
        {
            labelStatus.Text = "";
        }
        
        private void textBoxPassword_Enter(object sender, EventArgs e)
        {
            labelStatus.Text = "";
        }
    }
}