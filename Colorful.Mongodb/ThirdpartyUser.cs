using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Colorful.Models
{
    #region 第三方用户表
    /// <summary>
    /// 第三方用户表
    /// </summary>
    public partial class ThirdpartyUser : BaseId
    {
        /// <summary>
        /// 第三方类型
        /// </summary>
        public ThirdpartyUserType Type { get; set; }
        /// <summary>
        /// OpenId
        /// </summary>
        public string OpenId { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public string Nickname { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        public string Sex { get; set; }
        /// <summary>
        /// 用户头像
        /// </summary>
        public string Photo { get; set; }
        public string Token { get; set; }
    }
    #endregion

    #region 第三方类型
    public enum ThirdpartyUserType : int
    {
        QQ = 1,
        Wechat = 2,
        Webo = 3,
        Github = 4
    }
    #endregion
}