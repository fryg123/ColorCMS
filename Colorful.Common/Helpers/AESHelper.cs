using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Colorful
{
    public class AESHelper
    {
        private static readonly string KEY = "31E784126CE449B694UCE41EAEDBF0AA";
        private static readonly string IV = "a2xhcgAAABLAAAAM";
        #region AES加密
        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="toEncrypt">要加密的内容</param>
        /// <param name="strKey">密钥（16或者32位）</param>
        /// <returns>Base64转码后的密文</returns>
        public static string Encrypt(string toEncrypt, string strKey = null, string iv = null)
        {
            if (string.IsNullOrEmpty(strKey))
                strKey = KEY;
            if (string.IsNullOrEmpty(iv))
                iv = IV;
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(strKey);
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);
            using (var aes = Aes.Create())
            {
                aes.Key = keyArray;
                aes.Mode = CipherMode.CBC;//using System.Security.Cryptography;    
                aes.Padding = PaddingMode.PKCS7;//using System.Security.Cryptography;    
                //aes.BlockSize = 128;
                aes.IV = UTF8Encoding.UTF8.GetBytes(iv);
                ICryptoTransform cTransform = aes.CreateEncryptor();//using System.Security.Cryptography;    
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
        }
        #endregion AES加密

        #region AES解密
        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="toDecrypt">要解密的内容</param>
        /// <param name="strKey">密钥（16或者32位）</param>
        /// <returns>解密后的明文</returns>
        public static string Decrypt(string toDecrypt, string strKey = null, string iv = null)
        {
            if (string.IsNullOrEmpty(strKey))
                strKey = KEY;
            if (string.IsNullOrEmpty(iv))
                iv = IV;
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(strKey);
            byte[] toEncryptArray = Convert.FromBase64String(toDecrypt);

            using (var ase = Aes.Create())
            {
                ase.Key = keyArray;
                ase.Mode = CipherMode.CBC;
                ase.Padding = PaddingMode.PKCS7;
               // ase.BlockSize = 128;
                ase.IV = Encoding.UTF8.GetBytes(iv);

                ICryptoTransform cTransform = ase.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                return UTF8Encoding.UTF8.GetString(resultArray);
            }
        }
        #endregion AES解密
    }
}
