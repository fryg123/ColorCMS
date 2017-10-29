using System;
using System.Collections.Generic;
using System.Linq;

namespace Colorful.Models
{
    /// <summary>
    /// 统计表
    /// </summary>
    public class Statistic
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 计数器1
        /// </summary>
        public long Count1 { get; set; }
        /// <summary>
        /// 计数器2
        /// </summary>
        public long Count2 { get; set; }
        /// <summary>
        /// 计数器3
        /// </summary>
        public long Count3 { get; set; }
        /// <summary>
        /// 计数器4
        /// </summary>
        public long Count4 { get; set; }
    }
}
