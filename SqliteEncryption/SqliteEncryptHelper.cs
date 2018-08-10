using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Demo.Encrypt
{
    public class SqliteEncryptHelper
    {
        /// <summary>
        /// TODO : this settings need to be setup in KeyChain
        /// </summary>
        // 
        private static string Password = "Demo4dGwcJy@!%";
        private static string Salt = "tXB2o3BRd4ObXOW4p5qMQg==";

        public static List<T> EncryptList<T>(List<T> lists) where T : new()
        {
            foreach (T list in lists)
            {
                EncryptItem(list);
            }

            return lists;
        }

        public static T EncryptItem<T>(T item) where T : new()
        {
            if (item == null)
                return item;

            foreach (var property in item.GetType().GetTypeInfo().DeclaredProperties)
            {
                if (property.GetCustomAttributes(typeof(EncryptedAttribute), true).Count() > 0)
                {
                    var propObj = property.GetValue(item, null);
                    if (propObj == null)
                        continue;
                    
                    var encryptedVal = EncryptString(propObj.ToString());
                    property.SetValue(item, encryptedVal);
                }
            }

            return item;
        }

        public static string EncryptString(string val)
        {
            if (val == null)
                return null;

            var saltBytes = Convert.FromBase64String(Salt);

            var encryptedBytes = AES256CryptoEngine.EncryptAes(val, Password, saltBytes);
            string encryptedVal = Convert.ToBase64String(encryptedBytes);

            return encryptedVal;
        }

        public static List<T> DecryptList<T>(List<T> lists) where T : new()
        {
            foreach (T list in lists)
            {
                DecryptItem(list);
            }

            return lists;
        }

        public static T DecryptItem<T>(T item) where T : new()
        {
            if (item == null)
                return item;

            foreach (var property in item.GetType().GetTypeInfo().DeclaredProperties)
            {
                if (property.GetCustomAttributes(typeof(EncryptedAttribute), true).Count() > 0)
                {
                    var propObj = property.GetValue(item);
                    if (propObj == null)
                        continue;

                    var decryptedVal = DecryptString(propObj.ToString());
                    property.SetValue(item, decryptedVal);
                }
            }

            return item;
        }

        public static string DecryptString(string encryptedVal)
        {
            if (encryptedVal == null)
                return null;

            var saltBytes = Convert.FromBase64String(Salt);

            var encryptedBytes = Convert.FromBase64String(encryptedVal);
            var decryptedVal = AES256CryptoEngine.DecryptAes(encryptedBytes, Password, saltBytes);

            return decryptedVal;
        }
    }
}
