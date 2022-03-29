using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using RunAsExtension.Library;

namespace UserInfoHelper
{
    public partial class MainForm : Form
    {
        private List<UserInfo> _originalUserInfoList;
        private List<UserInfo> _unsavedUserInfoList;
        private UserInfoUtil _userInfoUtil = new UserInfoUtil();

        public MainForm()
        {
            InitializeComponent();
            _originalUserInfoList = _userInfoUtil.GetUserInfoList();
            _unsavedUserInfoList = new List<UserInfo>(_originalUserInfoList);

            foreach (var userInfo in _originalUserInfoList)
            { 
                lbUsers.Items.Add(userInfo.Description);
            }
        }

        
        private void lbUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbUsers.SelectedIndex == -1)
            {
                txtDescription.Text = "";
                txtDomain.Text = "";
                txtUserName.Text = "";
                txtPassword.Text = "";
            }
            else
            { 
                var selectedUser = _unsavedUserInfoList
                    .FirstOrDefault(u => u.Description.Equals(lbUsers.SelectedItem.ToString()));

                txtDescription.Text = selectedUser.Description;
                txtDomain.Text = selectedUser.Domain;
                txtUserName.Text = selectedUser.UserName;
                txtPassword.Text = CryptoUtil.Decrypt(selectedUser.EncryptedPassword);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            _originalUserInfoList = new List<UserInfo>(_unsavedUserInfoList);
            _userInfoUtil.SaveUserInfoList(_originalUserInfoList);
        }
        
        private void btnAdd_Click(object sender, EventArgs e)
        {
            MaintainUser();
        }

        private void MaintainUser(UserInfo userInfo = null)
        {
            var editForm = new EditForm(userInfo);
            var dialogResult = editForm.ShowDialog();
            if (dialogResult == DialogResult.Cancel)
            {
                return;
            }

            var editedUser = _unsavedUserInfoList.FirstOrDefault(u => u.Description == editForm.UserInfo.Description);

            if (editedUser != null)
            {
                editedUser.Domain = editForm.UserInfo.Domain;
                editedUser.UserName = editForm.UserInfo.UserName;
                editedUser.EncryptedPassword = editForm.UserInfo.EncryptedPassword;
            }
            else
            {
                _unsavedUserInfoList.Add(editForm.UserInfo);
                lbUsers.Items.Add(editForm.UserInfo.Description);
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            var selectedUser = _unsavedUserInfoList
                .FirstOrDefault(u => u.Description.Equals(lbUsers.SelectedItem.ToString()));

            if (selectedUser != null)
            {
                _unsavedUserInfoList.Remove(selectedUser);
                lbUsers.Items.RemoveAt(lbUsers.SelectedIndex);
            }
        }

        private void lbUsers_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = lbUsers.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                var selectedUser = _unsavedUserInfoList
                    .FirstOrDefault(u => u.Description.Equals(lbUsers.SelectedItem.ToString()));
                MaintainUser(selectedUser);
            }
        }
    }
}