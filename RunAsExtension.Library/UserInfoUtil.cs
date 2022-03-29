using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace RunAsExtension.Library
{
    public class UserInfoUtil
    {
        private string _userInfoPath;

        public List<UserInfo> GetUserInfoList()
        {
            _userInfoPath = EnsureUserInfoPath();
            var jsonData = File.ReadAllText(_userInfoPath);

            return JsonConvert.DeserializeObject<List<UserInfo>>(jsonData);
        }

        public void SaveUserInfoList(List<UserInfo> userInfoList)
        {
            var serializedList = JsonConvert.SerializeObject(userInfoList, Formatting.Indented);
            File.WriteAllText(_userInfoPath, serializedList);
        }

        private string EnsureUserInfoPath()
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

    }
}
