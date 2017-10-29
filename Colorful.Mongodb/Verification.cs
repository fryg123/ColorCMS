using System;
using System.Collections.Generic;
using System.Linq;

namespace Colorful.Models
{
    /// <summary>
    /// 验证表
    /// </summary>
    public partial class Verification : BaseId
    {
        /// <summary>
        /// Email
        /// </summary>
        public string TargetId { get; set; }
        /// <summary>
        /// 验证码
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 验证类型
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 特殊标识
        /// </summary>
        public string Flag { get; set; }
        /// <summary>
        /// 过期时间
        /// </summary>
        [MongoDB.Bson.Serialization.Attributes.BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Expire { get; set; }
    }
}
