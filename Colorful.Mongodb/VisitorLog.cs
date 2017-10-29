using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Colorful.Models
{
    /// <summary>
    /// 网站访客记录表
    /// </summary>
    public class VisitorLog : BaseId
    {
        /// <summary>
        /// 访问地址
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 提交方法
        /// </summary>
        public string HttpMethod { get; set; }
        /// <summary>
        /// 页面名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 浏览器
        /// </summary>
        public string Browser { get; set; }
        /// <summary>
        /// 浏览器版本
        /// </summary>
        public string BrowserVersion { get; set; }
        /// <summary>
        /// 来源
        /// </summary>
        public string Source { get; set; }
        public string IP { get; set; }
        /// <summary>
        /// 浏览器标识
        /// </summary>
        public string AgentStr { get; set; }
        /// <summary>
        /// 目标Id
        /// </summary>
        public string TargetId { get; set; }
        /// <summary>
        /// 特殊标识
        /// </summary>
        public string Flag { get; set; }
        /// <summary>
        /// 语言
        /// </summary>
        public string Lang { get; set; }
    }
}