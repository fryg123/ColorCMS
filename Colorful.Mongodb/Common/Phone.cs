using System;
using System.Collections.Generic;
using System.Linq;


namespace Colorful.Models
{
    /// <summary>
    /// 电话格式
    /// </summary>
    public class PhoneFormat
    {
        public PhoneFormat() { }
        public PhoneFormat(string phoneStr)
        {
            if (!string.IsNullOrEmpty(phoneStr))
            {
                var args = phoneStr.Split('-');
                if (args.Length == 1)
                    this.Number = args[0];
                else if (args.Length == 2)
                {
                    if (args[0].Length == 2 || args[0].StartsWith("+"))
                        this.CountryCode = args[0];
                    else
                        this.AreaCode = args[0];
                    this.Number = args[1];
                }else
                {
                    this.CountryCode = args[0];
                    this.AreaCode = args[1];
                    this.Number = args[2];
                }
            }
        }
        /// <summary>
        /// 国家代码：中国（86）
        /// </summary>
        public string CountryCode { get; set; }
        /// <summary>
        /// 区号
        /// </summary>
        public string AreaCode { get; set; }
        /// <summary>
        /// 号码
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// 分机号
        /// </summary>
        public string Ext { get; set; }

        public string Str
        {
            get
            {
                if (string.IsNullOrEmpty(CountryCode) && string.IsNullOrEmpty(AreaCode))
                    return string.Format("{0}{1}", this.Number, string.IsNullOrEmpty(Ext) ? "" : "-" + this.Ext);
                else if (string.IsNullOrEmpty(CountryCode))
                    return string.Format("{0}-{1}{2}", this.AreaCode, this.Number, string.IsNullOrEmpty(Ext) ? "" : "-" + this.Ext);
                return string.Format("{0}-{1}-{2}{3}", this.CountryCode, this.AreaCode, this.Number, string.IsNullOrEmpty(Ext) ? "" : "-" + this.Ext);
            }
            set
            {

            }
        }
    }
}