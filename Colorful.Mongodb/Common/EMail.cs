using System;
using System.Collections.Generic;
using System.Linq;

namespace Colorful.Models
{
    /// <summary>
    /// 邮件结构
    /// </summary>
    public class Mail
    {
        /// <summary>
        /// 发送到
        /// </summary>
        public string SendTo
        {
            get;
            set;
        }
        /// <summary>
        /// 标题
        /// </summary>
        public string Subject
        {
            get;
            set;
        }
        /// <summary>
        /// 内容
        /// </summary>
        public string Body
        {
            get;
            set;
        }
    }
}