using Colorful.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebMarkupMin.Core;

namespace Colorful.Cache.Web
{
    /// <summary>
    /// 缓存Web服务扩展
    /// </summary>
    public static class CacheServiceExtensions
    {
        public static void AddCacheService(this IServiceCollection services, Action<CacheOptions> configure)
        {
            services.AddDistributedMemoryCache();
            services.AddSingleton<IConfigureOptions<CacheOptions>, CacheOptionsSetup>();
            if (configure != null)
            {
                services.Configure(configure);
            }
            services.AddSingleton<CacheService>();
        }
    }

    #region CacheOptions
    /// <summary>
    /// 缓存选项
    /// </summary>
    public class CacheOptions
    {
        private string _prefix;
        private bool _isMinificationEnabled = true, _isCompressionEnabled = true;
        private CacheSetting _cacheSetting;

        /// <summary>
        /// 前辍
        /// </summary>
        public string Prefix
        {
            get
            {
                if (string.IsNullOrEmpty(_prefix))
                {
                    _prefix = Guid.NewGuid().ToString().Replace("-", "").ToLower();
                }
                return _prefix;
            }
            set
            {
                _prefix = value;
            }
        }
        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString
        {
            get; set;
        }

        /// <summary>
        /// 缓存设置
        /// </summary>
        public CacheSetting Setting
        {
            get
            {
                if (_cacheSetting == null)
                {
                    _cacheSetting = new CacheSetting()
                    {
                        CachePages = new List<CachePage>(),
                        CacheTime = 10,
                        IgnorePages = new List<string>()
                    };
                }
                return _cacheSetting;
            }
            set
            {
                _cacheSetting = value;
            }
        }
        /// <summary>
        /// 是否在开发模式中启用缓存
        /// </summary>
        public bool EnableDevelopmentCache { get; set; }
        /// <summary>
        /// 是否在开发模式中启用Html压缩
        /// </summary>
        public bool EnableDevelopmentMinification { get; set; }
        /// <summary>
        /// 是否在开发模式中启用GZip压缩
        /// </summary>
        public bool EnableDevelopmentCompression { get; set; }

        /// <summary>
        /// 是否启用缓存
        /// </summary>
        public bool EnableCache
        {
            get
            {
                return this.EnableDevelopmentCache || !this.Setting.DisabledCache;
            }
        }
        /// <summary>
        /// 是否开启Html压缩
        /// </summary>
        public bool EnableMinification
        {
            get
            {
                return _isMinificationEnabled || !this.Setting.DisabledCache;
            }
        }
        /// <summary>
        /// 是否启用GZip压缩
        /// </summary>
        public bool EnableCompression
        {
            get
            {
                return _isCompressionEnabled || !this.Setting.DisabledCompression;
            }
        }

        public bool IsAllowableResponseSize(long responseSize)
        {
            return this.Setting.MaxResponseSize > 0 && responseSize <= this.Setting.MaxResponseSize;
        }

        public CacheOptions()
        {

        }
    }
    #endregion

    #region CacheOptionsSetup
    public class CacheOptionsSetup : ConfigureOptions<CacheOptions>
    {
        public CacheOptionsSetup()
            : base(ConfigureCacheOptions)
        {

        }

        /// <summary>
        /// Sets a default options
        /// </summary>
        public static void ConfigureCacheOptions(CacheOptions options)
        { }

        public override void Configure(CacheOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            base.Configure(options);
        }
    }
    #endregion

    #region CacheService
    public class CacheService
    {
        private ICacheService _cacheService;

        public CacheService(ILoggerFactory loggerFactory, IMemoryCache memoryCache, IOptions<CacheOptions> options)
        {
            _cacheService = CacheHelper.GetCacheService(loggerFactory, memoryCache, options.Value.Prefix, options.Value.ConnectionString);
        }

        public ICacheService GetCacheService()
        {
            return _cacheService;
        }
    }
    #endregion
}
