using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Colorful.Models
{
    /// <summary>
    /// 公告
    /// </summary>
    public partial class Notice : BaseLongId
    {
        public Notice()
        {
        }
        /// <summary>
        /// 发件人
        /// </summary>
        public string Sender { get; set; }
        /// <summary>
        /// 主题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 已阅读用户列表
        /// </summary>
        public List<string> ReadUsers { get; set; }
        /// <summary>
        /// 公告类型
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 特殊标记
        /// </summary>
        public int Flag { get; set; }
        /// <summary>
        /// 语言
        /// </summary>
        public string Lang { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public long ByOrder { get; set; }

        #region Ignore
        /// <summary>
        /// 是否已读
        /// </summary>
        [BsonIgnore]
        public bool IsRead { get; set; }
        #endregion
    }
}