using System;
using System.Collections.Generic;
using System.Linq;

namespace Colorful.Models
{
    /// <summary>
    /// 网站模板设置
    /// </summary>
    public class Template : BaseIntId
    {
        /// <summary>
        /// 模板分类
        /// </summary>
        public int Sort { get; set; }
        /// <summary>
        /// 父编号
        /// </summary>
        public int ParentId
        {
            get;
            set;
        }
        /// <summary>
        /// 模板代码
        /// </summary>
        public string Code
        {
            get;
            set;
        }
        /// <summary>
        /// 模板名称
        /// </summary>
        public string Name
        {
            get;
            set;
        }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark
        {
            get;set;
        }
        /// <summary>
        /// 图标
        /// </summary>
        public string Icon
        {
            get;
            set;
        }
        /// <summary>
        /// 模板内容
        /// </summary>
        public string Content
        {
            get;
            set;
        }
        /// <summary>
        /// 排序
        /// </summary>
        public int ByOrder
        {
            get;
            set;
        }
    }
}