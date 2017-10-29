using System;
using System.Collections.Generic;
using System.Linq;


namespace Colorful.Models
{
    /// <summary>
    /// 地址通用格式
    /// </summary>
    public class AddressFormat
    {
        public AddressFormat() { }
        /// <summary>
        /// 区域
        /// </summary>
        public string Area { get; set; }
        /// <summary>
        /// 国家/地区
        /// </summary>
        public string Country { get; set; }
        /// <summary>
        /// 省
        /// </summary>
        public string Province { get; set; }
        /// <summary>
        /// 市
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 邮编
        /// </summary>
        public string Postcode { get; set; }

        public string Str
        {
            get
            {
                if (string.IsNullOrEmpty(City) || this.City == this.Province)
                    return $"{this.Country} {this.Province} {this.Address}";
                else
                    return $"{this.Country} {this.Province} {this.City} {this.Address}";
            }
        }
    }
}