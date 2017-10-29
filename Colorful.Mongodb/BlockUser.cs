using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Colorful.Models
{
    /// <summary>
    /// 网站访问黑名单
    /// </summary>
    public partial class BlockUser : BaseLongId
    {
        /// <summary>
        /// IP
        /// </summary>
        public string IP { get; set; }
        /// <summary>
        /// 过期时间
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime ExpireDate { get; set; }
    }
}