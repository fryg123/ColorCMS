using System;
using System.Drawing;
using System.IO;

namespace Colorful
{
    /// <summary>
    /// CommonHelper
    /// </summary>
    public static class CommonHelper
    {
        private static Random _random = GetRandom();

        #region 获取随机数
        /// <summary>
        /// 获取随机对象
        /// </summary>
        /// <returns></returns>
        public static Random GetRandom()
        {
            long tick = DateTime.Now.Ticks;
            return new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));
        }
        #endregion

        #region 获取唯一订单号
        /// <summary>
        /// 唯一订单号生成
        /// </summary>
        /// <returns></returns>
        public static string GetOrderId()
        {
            string strDateTimeNumber = DateTime.Now.ToString("yyyyMMddHHmmssms");
            string strRandomResult = _random.Next(1000, 9999).ToString();
            return strDateTimeNumber + strRandomResult;
        }
        #endregion

        #region 获取中文星期
        public static string GetWeek(DayOfWeek day)
        {
            string[] days = new[] { "星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六" };
            return days[(int)day];
        }
        public static string GetShortWeek(DayOfWeek day)
        {
            string[] days = new[] { "周日", "周一", "周二", "周三", "周四", "周五", "周六" };
            return days[(int)day];
        }
        #endregion

        #region 获取英文数字随机数
        /// <summary>
        /// 获取英文数字随机数
        /// </summary>
        /// <param name="length">返回长度</param>
        /// <param name="rnd">随机序列</param>
        /// <returns></returns>
        public static string GetRandomStr(int length, Random rnd = null)
        {
            string strTemp = string.Empty;
            string nums = "0123456789abcdefghijklmnopqrstuvwxyz";
            if (rnd == null)
                rnd = GetRandom();
            for (int i = 0; i < length; i++)
            {
                strTemp += nums[rnd.Next(0, nums.Length)].ToString();
            }
            return strTemp;
        }
        /// <summary>
        /// 获取英文数字随机数
        /// </summary>
        /// <param name="length">返回长度</param>
        /// <param name="rnd">随机序列</param>
        /// <returns></returns>
        public static string GetRandomAllStr(int length, Random rnd = null)
        {
            string strTemp = string.Empty;
            string nums = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (rnd == null)
                rnd = GetRandom();
            for (int i = 0; i < length; i++)
            {
                strTemp += nums[rnd.Next(0, nums.Length)].ToString();
            }
            return strTemp;
        }
        #endregion

        #region 获取数字随机数
        /// <summary>
        /// 获取数字随机数
        /// </summary>
        /// <param name="length">返回的数字长度</param>
        /// <param name="rnd">随机序列</param>
        /// <returns></returns>
        public static string GetNumberStr(int length, Random rnd = null)
        {
            if (rnd == null)
                rnd = _random;
            var strTmp = string.Empty;
            for (var i = 0; i < length; i++)
                strTmp += rnd.Next(0, 10);
            return strTmp;
        }
        #endregion

        #region 人民币大小写转换
        /// <summary>
        /// 转换人民币大小金额
        /// </summary>
        /// <param name="num">金额</param>
        /// <returns>返回大写形式</returns>
        public static string ToRMB(decimal num)
        {
            string str1 = "零壹贰叁肆伍陆柒捌玖";            //0-9所对应的汉字
            string str2 = "万仟佰拾亿仟佰拾万仟佰拾元角分"; //数字位所对应的汉字
            string str3 = "";    //从原num值中取出的值
            string str4 = "";    //数字的字符串形式
            string str5 = "";  //人民币大写金额形式
            int i;    //循环变量
            int j;    //num的值乘以100的字符串长度
            string ch1 = "";    //数字的汉语读法
            string ch2 = "";    //数字位的汉字读法
            int nzero = 0;  //用来计算连续的零值是几个
            int temp;            //从原num值中取出的值

            num = Math.Round(Math.Abs(num), 2);    //将num取绝对值并四舍五入取2位小数
            str4 = ((long)(num * 100)).ToString();        //将num乘100并转换成字符串形式
            j = str4.Length;      //找出最高位
            if (j > 15) { return "溢出"; }
            str2 = str2.Substring(15 - j);   //取出对应位数的str2的值。如：200.55,j为5所以str2=佰拾元角分

            //循环取出每一位需要转换的值
            for (i = 0; i < j; i++)
            {
                str3 = str4.Substring(i, 1);          //取出需转换的某一位的值
                temp = Convert.ToInt32(str3);      //转换为数字
                if (i != (j - 3) && i != (j - 7) && i != (j - 11) && i != (j - 15))
                {
                    //当所取位数不为元、万、亿、万亿上的数字时
                    if (str3 == "0")
                    {
                        ch1 = "";
                        ch2 = "";
                        nzero = nzero + 1;
                    }
                    else
                    {
                        if (str3 != "0" && nzero != 0)
                        {
                            ch1 = "零" + str1.Substring(temp * 1, 1);
                            ch2 = str2.Substring(i, 1);
                            nzero = 0;
                        }
                        else
                        {
                            ch1 = str1.Substring(temp * 1, 1);
                            ch2 = str2.Substring(i, 1);
                            nzero = 0;
                        }
                    }
                }
                else
                {
                    //该位是万亿，亿，万，元位等关键位
                    if (str3 != "0" && nzero != 0)
                    {
                        ch1 = "零" + str1.Substring(temp * 1, 1);
                        ch2 = str2.Substring(i, 1);
                        nzero = 0;
                    }
                    else
                    {
                        if (str3 != "0" && nzero == 0)
                        {
                            ch1 = str1.Substring(temp * 1, 1);
                            ch2 = str2.Substring(i, 1);
                            nzero = 0;
                        }
                        else
                        {
                            if (str3 == "0" && nzero >= 3)
                            {
                                ch1 = "";
                                ch2 = "";
                                nzero = nzero + 1;
                            }
                            else
                            {
                                if (j >= 11)
                                {
                                    ch1 = "";
                                    nzero = nzero + 1;
                                }
                                else
                                {
                                    ch1 = "";
                                    ch2 = str2.Substring(i, 1);
                                    nzero = nzero + 1;
                                }
                            }
                        }
                    }
                }
                if (i == (j - 11) || i == (j - 3))
                {
                    //如果该位是亿位或元位，则必须写上
                    ch2 = str2.Substring(i, 1);
                }
                str5 = str5 + ch1 + ch2;

                if (i == j - 1 && str3 == "0")
                {
                    //最后一位（分）为0时，加上“整”
                    str5 = str5 + '整';
                }
            }
            if (num == 0)
            {
                str5 = "零元整";
            }
            return str5;
        }
        #endregion

        #region 解析字符中的Size
        /// <summary>
        /// 解析字符中的图片大小
        /// </summary>
        /// <param name="str">要解析的字符串</param>
        /// <returns></returns>
        public static System.Drawing.Size ParsePhotoSize(string str)
        {
            var match = System.Text.RegularExpressions.Regex.Match(str, "([0-9]{1,})[*x]{1,1}([0-9]{1,})");
            if (match.Success)
            {
                return new System.Drawing.Size(match.Groups[1].Value.ToInt(), match.Groups[2].Value.ToInt());
            }
            return Size.Empty;
        }
        /// <summary>
        /// 解析字符中的文件大小
        /// </summary>
        /// <param name="str">要解析的字符串</param>
        /// <returns></returns>
        public static long ParseFileLength(string str)
        {
            var match = System.Text.RegularExpressions.Regex.Match(str, "([0-9]{1,})[mMkK]{1,1}");
            if (match.Success)
            {
                return match.Groups[2].Value.ToLong();
            }
            return 0;
        }
        #endregion
    }
}