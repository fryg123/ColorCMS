using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Colorful.AspNetCore
{
    #region CacheOptions
    public class CacheOptions
    {
        /// <summary>
        /// 是否禁用Html压缩
        /// </summary>
        public bool DisableMinification
        {
            get;
            set;
        }
        /// <summary>
        /// 是否禁用GZip压缩
        /// </summary>
        public bool DisableCompression
        {
            get;
            set;
        }
        /// <summary>
        /// 最大请求大小（超出设置的大小则不缓存）
        /// </summary>
        public int MaxResponseSize
        {
            get;
            set;
        }
        /// <summary>
        /// 是否允许在开发模式中启用Html压缩
        /// </summary>
        public bool AllowMinificationInDevelopmentEnvironment
        {
            get;
            set;
        }

        /// <summary>
        /// 是否允许在开发模式中启用GZip压缩
        /// </summary>
        public bool AllowCompressionInDevelopmentEnvironment
        {
            get;
            set;
        }
        public IHostingEnvironment HostingEnvironment
        {
            get;
            set;
        }

        public bool IsMinificationEnabled
        {
            get
            {
                return !this.HostingEnvironment.IsDevelopment() || this.AllowMinificationInDevelopmentEnvironment;
            }
        }

        public bool IsCompressionEnabled
        {
            get
            {
                return !HostingEnvironment.IsDevelopment() || this.AllowCompressionInDevelopmentEnvironment;
            }
        }

        public bool IsAllowableResponseSize(long responseSize)
        {
            return this.MaxResponseSize > 0 && responseSize <= this.MaxResponseSize;
        }
    }
    #endregion

    #region CacheOptionsSetup
    public class CacheOptionsSetup : ConfigureOptions<CacheOptions>
    {
        /// <summary>
        /// Hosting environment
        /// </summary>
        private readonly IHostingEnvironment _hostingEnvironment;


        /// <summary>
        /// Constructs a instance of <see cref="CacheOptionsSetup"/>
        /// </summary>
        public CacheOptionsSetup(IHostingEnvironment hostingEnvironment)
            : base(ConfigureCacheOptions)
        {
            _hostingEnvironment = hostingEnvironment;
        }


        /// <summary>
        /// Sets a default options
        /// </summary>
        public static void ConfigureCacheOptions(CacheOptions options)
        {
        }

        public override void Configure(CacheOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            options.HostingEnvironment = _hostingEnvironment;

            base.Configure(options);
        }
    }
    #endregion
}