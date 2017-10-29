using System;
using System.Collections.Generic;
using System.Linq;

namespace Colorful.Models
{
    /// <summary>
    /// 网站标签
    /// </summary>
    public partial class Label : BaseId
    {
        /// <summary>
        /// 标签名称
        /// </summary>
        public string Name
        {
            get;
            set;
        }
        /// <summary>
        /// 菜单Id
        /// </summary>
        public long MenuId { get; set; }
        /// <summary>
        /// 分类
        /// </summary>
        public int Sort { get; set; }
        /// <summary>
        /// 标签样式
        /// </summary>
        public string Style
        {
            get;
            set;
        }
    }
}