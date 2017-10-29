using System;
using System.Collections.Generic;
using System.Linq;

namespace Colorful.Models
{
    /// <summary>
    /// 资源表
    /// </summary>
    public partial class Resource : BaseId
    {
        /// <summary>
        /// 资源类型
        /// </summary>
        public ResourceType Type { get; set; }
        /// <summary>
        /// 栏目编号
        /// </summary>
        public long MenuId { get; set; }
        /// <summary>
        /// 资源分类
        /// </summary>
        public int SortId { get; set; }
        /// <summary>
        /// 添加人
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 资源地址
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 目标Id（如文章Id、友情链接Id等）
        /// </summary>
        public string TargetId { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public long ByOrder { get; set; }
    }
}