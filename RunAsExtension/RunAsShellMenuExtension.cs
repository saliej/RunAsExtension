using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using Newtonsoft.Json;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using RunAsExtension.Library;

namespace RunAsExtension
{
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".exe", ".msi", ".bat", ".cmd")]
    public class RunAsShellMenuExtension : SharpContextMenu
    {
        private ContextMenuStrip _menu = new ContextMenuStrip();

        protected override bool CanShowMenu()
        {
            if (IsSingleFileSelected())
            {
                UpdateMenu();
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsSingleFileSelected() => SelectedItemPaths.Count() == 1;
        protected override ContextMenuStrip CreateMenu()
        {
            _menu.Items.Clear();
            var mainMenu = new ToolStripMenuItem
            {
                Text = "Run As...",
            };

            var usersInfo = GetUsers();
            foreach (var user in usersInfo)
            { 
                var userMenu = new ToolStripMenuItem
                {
                    Text = user.Description,
                };
                userMenu.Click += (sender, args) => RunAsUser(((ToolStripMenuItem)sender).Text);

                mainMenu.DropDownItems.Add(userMenu);
            }

            if (mainMenu.DropDownItems.Count == 0)
            {
                mainMenu.Click += (sender, args) => DisplayError("No Users Defined");
            }

            _menu.Items.Clear();
            _menu.Items.Add(mainMenu);

            return _menu;
        }

        private void RunAsUser(string userDescription)
        {
            var users = GetUsers();
            var selectedUser = users.FirstOrDefault(u => u.Description == userDescription);

            if (selectedUser != null)
            {
                try
                {
                    var process = new Process();
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.WorkingDirectory = Path.GetDirectoryName(SelectedItemPaths.First());
                    process.StartInfo.FileName = SelectedItemPaths.First();
                    process.StartInfo.Domain = selectedUser.Domain;
                    process.StartInfo.UserName = selectedUser.UserName;
                    process.StartInfo.Password = GetSecureString(CryptoUtil.Decrypt(selectedUser.EncryptedPassword));
                    process.Start();
                }
                catch (Exception ex)
                {
                    DisplayError(ex.Message);
                }
            }
        }

        private static SecureString GetSecureString(string password)
        {
            var secureString = new SecureString();
            foreach (var letter in password)
            {
                secureString.AppendChar(letter);
            }

            return secureString;
        }

        private List<UserInfo> GetUsers()
        {
            try
            {
                var userInfoPath = EnsureUserInfoPath();
                var json = File.ReadAllText(userInfoPath);
                return JsonConvert.DeserializeObject<List<UserInfo>>(json);
            }
            catch (Exception ex)
            {
                DisplayError(ex.Message);
            }

            return new List<UserInfo>();
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

        private void UpdateMenu()
        {
            _menu?.Dispose();
            _menu = CreateMenu();
        }

        private static void DisplayError(string errorMessage)
        {
            MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
