using System;
using System.IO;

namespace Colorful.Captcha
{
    /// <summary>
    /// 验证码帮助类
    /// </summary>
    public static class CaptchaHelper
    {
        /// <summary>
        /// 获取验证码
        /// </summary>
        /// <param name="width">验证码宽度</param>
        /// <param name="height">验证码高度</param>
        /// <returns></returns>
        public static byte[] GetCaptcha(int width, int height)
        {
            var cc = new ALSoft.Captcha.Core.CaptchaControl();
            cc.IsForegroundDynamic = true;
            if (height > 0)
                cc.Height = 38;
            if (width > 0)
                cc.Width = width;
            using (var mStream = new MemoryStream())
            {
                new ALSoft.Captcha.Core.Captcha().GenerateCaptchaImage(mStream);
                var buffer = new byte[mStream.Length];
                mStream.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }
    }
}
