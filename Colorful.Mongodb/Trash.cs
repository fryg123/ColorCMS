using System;
using System.Collections.Generic;
using System.Linq;

namespace Colorful.Models
{
    /// <summary>
    /// 回收站
    /// </summary>
    public class Trash : BaseId
    {
        /// <summary>
        /// 垃圾分类：见TrashType枚举
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 删除的目标Id
        /// </summary>
        public string TargetId { get; set; }
        /// <summary>
        /// 分类Id
        /// </summary>
        public long SortId { get; set; }
        /// <summary>
        /// 内容（被删除的对象序列化）
        /// </summary>
        public string Content
        {
            get;
            set;
        }
        /// <summary>
        /// 需要被删除的文件列表
        /// </summary>
        public List<string> Files { get; set; }
    }
}