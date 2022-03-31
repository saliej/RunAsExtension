using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
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
        private UserInfoUtil _userInfoUtil = new UserInfoUtil();

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

            try
            {
                var userInfo = _userInfoUtil.GetUserInfoList();
                foreach (var user in userInfo)
                {
                    var userMenu = new ToolStripMenuItem
                    {
                        Text = user.Description,
                    };
                    userMenu.Click += (sender, args) => RunAsUser(((ToolStripMenuItem)sender).Text, userInfo);

                    mainMenu.DropDownItems.Add(userMenu);
                }

                if (mainMenu.DropDownItems.Count == 0)
                {
                    mainMenu.Click += (sender, args) => DisplayError("No Users Defined");
                }
            }
            catch (Exception ex)
            {
                DisplayError(ex.Message);
                mainMenu.Click += (sender, args) => DisplayError(ex.Message);
            }

            _menu.Items.Clear();
            _menu.Items.Add(mainMenu);

            return _menu;
        }

        private void RunAsUser(string userDescription, List<UserInfo> users)
        {
            var selectedUser = users.FirstOrDefault(u => u.Description == userDescription);

            if (selectedUser != null)
            {
                RunProcessAsUser(selectedUser, SelectedItemPaths.First());
            }
        }

        private static void RunProcessAsUser(UserInfo selectedUser, string filePath)
        {
            try
            {
                var process = new Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.WorkingDirectory = Path.GetDirectoryName(filePath);
                process.StartInfo.Domain = selectedUser.Domain;
                process.StartInfo.UserName = selectedUser.UserName;
                process.StartInfo.Password = GetSecureString(CryptoUtil.Decrypt(selectedUser.EncryptedPassword));

                if (Path.GetExtension(filePath).ToLower() == ".msi")
                {
                    process.StartInfo.FileName = "msiexec";
                    process.StartInfo.Arguments = $"/i \"{filePath}\"";
                }
                else
                {
                    process.StartInfo.FileName = filePath;
                }

                process.Start();
            }
            catch (Exception ex)
            {
                DisplayError(ex.Message);
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
