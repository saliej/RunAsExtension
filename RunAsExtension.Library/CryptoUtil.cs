using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RunAsExtension.Library
{
    public static class CryptoUtil
    {
        public static string Encrypt(string data)
        {
            var bytesToEncrypt = Encoding.UTF8.GetBytes(data);
            var encryptedBytes = ProtectedData.Protect(bytesToEncrypt, null, DataProtectionScope.CurrentUser);

            return Convert.ToBase64String(encryptedBytes);
        }

        public static string Decrypt(string encryptedData)
        {
            var bytesToDecrypt = Convert.FromBase64String(encryptedData);
            var unencryptedBytes = ProtectedData.Unprotect(bytesToDecrypt, null, DataProtectionScope.CurrentUser);

            return Encoding.UTF8.GetString(unencryptedBytes);
        }
    }
}
