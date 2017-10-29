using System;
using System.Collections.Generic;
using System.Linq;

namespace Colorful.Models
{
    /// <summary>
    /// ��֤��
    /// </summary>
    public partial class Verification : BaseId
    {
        /// <summary>
        /// Email
        /// </summary>
        public string TargetId { get; set; }
        /// <summary>
        /// ��֤��
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// ��֤����
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// �����ʶ
        /// </summary>
        public string Flag { get; set; }
        /// <summary>
        /// ����ʱ��
        /// </summary>
        [MongoDB.Bson.Serialization.Attributes.BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Expire { get; set; }
    }
}
