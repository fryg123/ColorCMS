using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Colorful.Models
{
    /// <summary>
    /// 短消息
    /// </summary>
    public class Message : BaseId
    {
        private List<string> _receivers;

        /// <summary>
        /// 发件人
        /// </summary>
        public string Sender { get; set; }
        /// <summary>
        /// 收件人
        /// </summary>
        public List<string> Receivers
        {
            get
            {
                return _receivers;
            }
            set
            {
                _receivers = value;
            }
        }
        /// <summary>
        /// 发送对象
        /// </summary>
        public SendTarget SendTarget { get; set; }
        /// <summary>
        /// 已读用户列表
        /// </summary>
        public List<string> ReadUsers { get; set; }
        /// <summary>
        /// 已删除用户列表
        /// </summary>
        public List<string> DelUsers { get; set; }
        /// <summary>
        /// 主题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 消息内容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 消息类型
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 审核状态
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 置顶
        /// </summary>
        public int Top { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public long ByOrder { get; set; }

        #region Ignore
        /// <summary>
        /// 消息是否已读
        /// </summary>
        [BsonIgnore]
        public bool IsRead
        {
            get;
            set;
        }
        #endregion

    }
}