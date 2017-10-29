using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Colorful.Models
{
    #region 管理员表
    /// <summary>
    /// 后台用户表
    /// </summary>
    public partial class Administrator : UserBase
    {
        /// <summary>
        /// 职务
        /// </summary>
        public string Position { get; set; }
        /// <summary>
        /// 默认登录打开地址
        /// </summary>
        public string DefaultUrl { get; set; }
        /// <summary>
        /// 菜单访问记录(数组1：菜单Id，数组2：访问次数)
        /// </summary>
        public List<long[]> MenuHistory { get; set; }
        /// <summary>
        /// 收藏菜单
        /// </summary>
        public List<long> FavoriteMenus { get; set; }
    }
    #endregion
}