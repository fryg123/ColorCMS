using System;
using System.Collections.Generic;
using System.Linq;

namespace Colorful.Models
{
    /// <summary>
    /// 权限
    /// </summary>
    public class Permission : BaseLongId
    {
        /// <summary>
        /// 分类编号
        /// </summary>
        public int SortId
        {
            get;
            set;
        }
        /// <summary>
        /// 权限名称
        /// </summary>
        public string Name
        {
            get;
            set;
        }
        /// <summary>
        /// 权限说明
        /// </summary>
        public string Remark
        {
            get;
            set;
        }
    }
}