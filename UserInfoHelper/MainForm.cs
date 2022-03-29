using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using RunAsExtension.Library;

namespace UserInfoHelper
{
    public partial class MainForm : Form
    {
        private List<UserInfo> _originalUserInfoList;
        private List<UserInfo> _unsavedUserInfoList;
        private string _userInfoPath;

        public MainForm()
        {
            InitializeComponent();
            _originalUserInfoList = GetUserInfoList();
            _unsavedUserInfoList = new List<UserInfo>(_originalUserInfoList);

            foreach (var userInfo in _originalUserInfoList)
            { 
                lbUsers.Items.Add(userInfo.Description);
            }
        }

        private List<UserInfo> GetUserInfoList()
        {
            _userInfoPath = EnsureUserInfoPath();
            var jsonData = File.ReadAllText(_userInfoPath);

            return JsonConvert.DeserializeObject<List<UserInfo>>(jsonData);
        }

        private static string EnsureUserInfoPath()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var path = Path.Combine(appDataPath, @"RunAsExtension\");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var userInfoPath = Path.Combine(path, "UserInfo.json");
            if (!File.Exists(userInfoPath))
            {
                var serializedList = JsonConvert.SerializeObject(new List<UserInfo>());
                File.WriteAllText(userInfoPath, serializedList);
            }

            return userInfoPath;
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
            var serializedList = JsonConvert.SerializeObject(_originalUserInfoList, Formatting.Indented);
            File.WriteAllText(_userInfoPath, serializedList);
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