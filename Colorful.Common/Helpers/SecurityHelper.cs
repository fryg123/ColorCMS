using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colorful.AspNetCore
{
    /// <summary>
    /// 安全类Helper
    /// </summary>
    public static class SecurityHelper
    {
        /// <summary>
        /// 根据传入的明文密码获取加密后的密码
        /// </summary>
        /// <param name="password">明文密码</param>
        /// <returns></returns>
        public static string GetPassword(string password)
        {
            password = AESHelper.Encrypt(password);
            
            var sha256 = System.Security.Cryptography.SHA256.Create();
            var data = sha256.ComputeHash(System.Text.Encoding.ASCII.GetBytes(password));
            var result = new StringBuilder();
            for (var i = 0; i < data.Length; i++)
                result.Append(data[i].ToString("x2"));
            return result.ToString();
        }
    }
}
