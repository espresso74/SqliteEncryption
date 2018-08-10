using System;
using System.Collections.Generic;
using System.Text;

namespace Demo.Encrypt
{
    public class WebEncryptEngine
    {
        public static string EncryptString(string input, string key, Guid salt)
        {
            string saltString = salt.ToString("N");

            string keySalt = string.Concat(key, saltString);
            string shaKey = SHA256CryptoEngine.SHA256Hash(keySalt);

            string val = string.Concat(input, saltString);

            List<char> valArray = new List<char>();
            for (int count = 0, total = val.Length; count < total; count++)
            {
                int dataVal = val[count];
                int hashCharVal = shaKey[count % 64];

                int finalVal = dataVal + hashCharVal;
                valArray.Add((char)finalVal);
            }

            string paddingSha = SHA256CryptoEngine.SHA256Hash(val);
            int start = valArray.Count % 64;
            for (int count = start; count < 64; count++)
            {
                int dataVal = paddingSha[count];
                int hashCharVal = shaKey[count % 64];

                int finalVal = dataVal + hashCharVal;
                valArray.Add((char)finalVal);
            }

            string joined = string.Join(string.Empty, valArray);

            byte[] bytes = Encoding.UTF8.GetBytes(joined);
            string base64 = Convert.ToBase64String(bytes);

            return base64;
        }

        public static string DecryptString(string val, string key, Guid salt)
        {
            string saltString = salt.ToString("N");

            byte[] bytes = Convert.FromBase64String(val);
            string decodedString = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

            string keySalt = string.Concat(key, saltString);
            string shaKey = SHA256CryptoEngine.SHA256Hash(keySalt);

            List<char> valArray = new List<char>();
            for (int count = 0, total = decodedString.Length; count < total; count++)
            {
                int dataVal = decodedString[count];
                int hashCharVal = shaKey[count % 64];

                int finalVal = dataVal - hashCharVal;
                valArray.Add((char)finalVal);
            }

            string joined = string.Join(string.Empty, valArray);
            int indexMatch = joined.IndexOf(saltString, StringComparison.OrdinalIgnoreCase);

            string password = null;
            if (indexMatch > 0)
            {
                password = joined.Substring(0, indexMatch);
            }

            return password;
        }
    }
}
