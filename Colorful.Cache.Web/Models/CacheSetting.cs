using System;
using System.Collections.Generic;
using System.Linq;

namespace Colorful.Models
{
    /// <summary>
    /// 缓存设置
    /// </summary>
    public partial class CacheSetting
    {
        /// <summary>
        /// 是否屏蔽缓存
        /// </summary>
        public bool DisabledCache { get; set; }
        /// <summary>
        /// 是否屏蔽压缩
        /// </summary>
        public bool DisabledCompression { get; set; }
        /// <summary>
        /// 缓存时间
        /// </summary>
        public int CacheTime { get; set; }
        /// <summary>
        /// 缓存页面列表
        /// </summary>
        public List<CachePage> CachePages { get; set; }
        /// <summary>
        /// 不缓存的页面列表
        /// </summary>
        public List<string> IgnorePages { get; set; }
        /// <summary>
        /// 超过指定大小则不缓存
        /// </summary>
        public int MaxResponseSize { get; set; }
    }

    #region CachePage
    public class CachePage
    {
        /// <summary>
        /// 缓存地址
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 缓存时间（秒）
        /// </summary>
        public int CacheTime { get; set; }
        /// <summary>
        /// Url参数
        /// </summary>
        public string[] VaryByParams { get; set; }
    }
    #endregion
}