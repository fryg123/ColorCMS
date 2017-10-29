using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Colorful.Models
{
    #region 用户基础类
    public abstract class UserBase : BaseId
    {
        /// <summary>
        /// 数字Id
        /// </summary>
        public long NumberId { get; set; }
        /// <summary>
        /// 登录Id
        /// </summary>
        public string LoginId { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// SessionId
        /// </summary>
        public string SessionId { get; set; }
        /// <summary>
        /// 用户IP
        /// </summary>
        public string IP { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 头像
        /// </summary>
        public string Photo { get; set; }
        /// <summary>
        /// 角色列表
        /// </summary>
        public List<long> Roles { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public long ByOrder { get; set; }
        /// <summary>
        /// 最后登陆时间
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime LastLogin { get; set; }
        /// <summary>
        /// 最后活动时间
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime ActiveTime { get; set; }
    }
    #endregion
}