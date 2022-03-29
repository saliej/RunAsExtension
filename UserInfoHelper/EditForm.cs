using System;
using System.Windows.Forms;
using RunAsExtension.Library;

namespace UserInfoHelper
{
    public partial class EditForm : Form
    {
        public UserInfo UserInfo {get; private set;}
        public EditForm(UserInfo userInfo = null)
        {
            InitializeComponent();
            UserInfo = userInfo;
        }

        private void EditForm_Load(object sender, EventArgs e)
        {
            if (UserInfo != null)
            {
                txtDescription.Text = UserInfo.Description;
                txtDomain.Text = UserInfo.Domain;
                txtUserName.Text = UserInfo.UserName;
                txtPassword.Text = CryptoUtil.Decrypt(UserInfo.EncryptedPassword);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (!ValidateUserInfo())
            {
                return;     
            }

            UserInfo = new UserInfo
            {
                Description = txtDescription.Text,
                Domain = txtDomain.Text,
                UserName = txtUserName.Text,
                EncryptedPassword = CryptoUtil.Encrypt(txtPassword.Text),
            };
            
            DialogResult = DialogResult.OK;
            Close();
        }

        private bool ValidateUserInfo()
        {
            if (String.IsNullOrWhiteSpace(txtDescription.Text))
            {
                MessageBox.Show("Description is required", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtDescription.Focus();
                return false;
            }

            if (String.IsNullOrWhiteSpace(txtDomain.Text))
            {
                MessageBox.Show("Domain is required", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtDescription.Focus();
                return false;
            }

            if (String.IsNullOrWhiteSpace(txtUserName.Text))
            {
                MessageBox.Show("UserName is required", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtUserName.Focus();
                return false;
            }

            if (String.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Password is required", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPassword.Focus();
                return false;
            }

            return true;
        }
    }
}
