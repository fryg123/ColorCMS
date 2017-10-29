using System;
using System.Collections.Generic;
using System.Linq;

namespace Colorful.Models
{
    public partial class SysSetting : BaseIntId
    {
        /// <summary>
        /// 是否启用短消息
        /// </summary>
        public bool EnableMessage { get; set; }
        /// <summary>
        /// 是否启用提醒
        /// </summary>
        public bool EnableAlert { get; set; }
        /// <summary>
        /// 是否启用任务
        /// </summary>
        public bool EnableTask { get; set; }
        /// <summary>
        /// 是否启用菜单筛选
        /// </summary>
        public bool EnableMenuFilter { get; set; }
        /// <summary>
        /// 是否记录访客日志
        /// </summary>
        public bool EnableLogs { get; set; }
        /// <summary>
        /// 上线目标
        /// </summary>
        public List<string> OnlineTargets { get; set; }
        /// <summary>
        /// 网站加密Key
        /// </summary>
        public string EncryptKey { get; set; }
    }
}