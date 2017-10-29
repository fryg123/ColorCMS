using System;
using System.Collections.Generic;
using System.Linq;

namespace Colorful.Models
{
    /// <summary>
    /// 邮件模板
    /// </summary>
    public partial class MailTemplate : BaseIntId
    {
        /// <summary>
        /// 菜单Id
        /// </summary>
        public long MenuId { get; set; }
        /// <summary>
        /// 邮件分组
        /// </summary>
        public string Group { get; set; }
        /// <summary>
        /// 邮件标签
        /// </summary>
        public string Label { get; set; }
		/// <summary>
        /// 标题
		/// </summary>
		public string Title
		{
			get;
			set;
		}
		/// <summary>
        /// 内容
		/// </summary>
		public string Content
		{
			get;
			set;
		}
		/// <summary>
		/// 语言
		/// </summary>
        public string Lang
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
