using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Colorful.Models
{
    /// <summary>
    /// 角色
    /// </summary>
    public class Role : BaseLongId
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
        /// 名称
        /// </summary>
        public string Name
        {
            get;
            set;
        }
        /// <summary>
        /// 角色说明
        /// </summary>
        public string Remark
        {
            get;
            set;
        }
        /// <summary>
        /// 默认打开的菜单
        /// </summary>
        public string DefaultMenu { get; set; }
        /// <summary>
        /// 权限列表
        /// </summary>
        public List<long> Permissions
        {
            get;
            set;
        }
        /// <summary>
        /// 分配到的菜单列表
        /// </summary>
        public List<long> Menus
        {
            get;
            set;
        }
        #region Ignore
        [BsonIgnore]
        public List<Menu> MenuList { get; set; }
        #endregion
    }
}