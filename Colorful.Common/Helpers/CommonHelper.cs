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

        #region ��ȡ�����
        /// <summary>
        /// ��ȡ�������
        /// </summary>
        /// <returns></returns>
        public static Random GetRandom()
        {
            long tick = DateTime.Now.Ticks;
            return new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));
        }
        #endregion

        #region ��ȡΨһ������
        /// <summary>
        /// Ψһ����������
        /// </summary>
        /// <returns></returns>
        public static string GetOrderId()
        {
            string strDateTimeNumber = DateTime.Now.ToString("yyyyMMddHHmmssms");
            string strRandomResult = _random.Next(1000, 9999).ToString();
            return strDateTimeNumber + strRandomResult;
        }
        #endregion

        #region ��ȡ��������
        public static string GetWeek(DayOfWeek day)
        {
            string[] days = new[] { "������", "����һ", "���ڶ�", "������", "������", "������", "������" };
            return days[(int)day];
        }
        public static string GetShortWeek(DayOfWeek day)
        {
            string[] days = new[] { "����", "��һ", "�ܶ�", "����", "����", "����", "����" };
            return days[(int)day];
        }
        #endregion

        #region ��ȡӢ�����������
        /// <summary>
        /// ��ȡӢ�����������
        /// </summary>
        /// <param name="length">���س���</param>
        /// <param name="rnd">�������</param>
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
        /// ��ȡӢ�����������
        /// </summary>
        /// <param name="length">���س���</param>
        /// <param name="rnd">�������</param>
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

        #region ��ȡ���������
        /// <summary>
        /// ��ȡ���������
        /// </summary>
        /// <param name="length">���ص����ֳ���</param>
        /// <param name="rnd">�������</param>
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

        #region ����Ҵ�Сдת��
        /// <summary>
        /// ת������Ҵ�С���
        /// </summary>
        /// <param name="num">���</param>
        /// <returns>���ش�д��ʽ</returns>
        public static string ToRMB(decimal num)
        {
            string str1 = "��Ҽ��������½��ƾ�";            //0-9����Ӧ�ĺ���
            string str2 = "��Ǫ��ʰ��Ǫ��ʰ��Ǫ��ʰԪ�Ƿ�"; //����λ����Ӧ�ĺ���
            string str3 = "";    //��ԭnumֵ��ȡ����ֵ
            string str4 = "";    //���ֵ��ַ�����ʽ
            string str5 = "";  //����Ҵ�д�����ʽ
            int i;    //ѭ������
            int j;    //num��ֵ����100���ַ�������
            string ch1 = "";    //���ֵĺ������
            string ch2 = "";    //����λ�ĺ��ֶ���
            int nzero = 0;  //����������������ֵ�Ǽ���
            int temp;            //��ԭnumֵ��ȡ����ֵ

            num = Math.Round(Math.Abs(num), 2);    //��numȡ����ֵ����������ȡ2λС��
            str4 = ((long)(num * 100)).ToString();        //��num��100��ת�����ַ�����ʽ
            j = str4.Length;      //�ҳ����λ
            if (j > 15) { return "���"; }
            str2 = str2.Substring(15 - j);   //ȡ����Ӧλ����str2��ֵ���磺200.55,jΪ5����str2=��ʰԪ�Ƿ�

            //ѭ��ȡ��ÿһλ��Ҫת����ֵ
            for (i = 0; i < j; i++)
            {
                str3 = str4.Substring(i, 1);          //ȡ����ת����ĳһλ��ֵ
                temp = Convert.ToInt32(str3);      //ת��Ϊ����
                if (i != (j - 3) && i != (j - 7) && i != (j - 11) && i != (j - 15))
                {
                    //����ȡλ����ΪԪ�����ڡ������ϵ�����ʱ
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
                            ch1 = "��" + str1.Substring(temp * 1, 1);
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
                    //��λ�����ڣ��ڣ���Ԫλ�ȹؼ�λ
                    if (str3 != "0" && nzero != 0)
                    {
                        ch1 = "��" + str1.Substring(temp * 1, 1);
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
                    //�����λ����λ��Ԫλ�������д��
                    ch2 = str2.Substring(i, 1);
                }
                str5 = str5 + ch1 + ch2;

                if (i == j - 1 && str3 == "0")
                {
                    //���һλ���֣�Ϊ0ʱ�����ϡ�����
                    str5 = str5 + '��';
                }
            }
            if (num == 0)
            {
                str5 = "��Ԫ��";
            }
            return str5;
        }
        #endregion

        #region �����ַ��е�Size
        /// <summary>
        /// �����ַ��е�ͼƬ��С
        /// </summary>
        /// <param name="str">Ҫ�������ַ���</param>
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
        /// �����ַ��е��ļ���С
        /// </summary>
        /// <param name="str">Ҫ�������ַ���</param>
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