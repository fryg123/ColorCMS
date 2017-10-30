using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Colorful.Cache
{
    #region CacheHelper
    public static class CacheHelper
    {
        private static ILogger _logger;
        /// <summary>
        /// 获取缓存服务
        /// </summary>
        /// <param name="loggerFactory">LoggerFactory</param>
        /// <param name="memoryCache">内存缓存接口</param>
        /// <param name="connectionString">Redis连接字符串</param>
        /// <param name="projectId">项目Id</param>
        /// <returns></returns>
        public static ICacheService GetCacheService(ILoggerFactory loggerFactory, IMemoryCache memoryCache, string connectionString, string projectId = null)
        {
            if (string.IsNullOrEmpty(projectId))
                projectId = Guid.NewGuid().ToString().Replace("-", "").ToLower();
            _logger = loggerFactory.CreateLogger("CacheHelper");
            try
            {
                return new RedisCacheService(projectId, connectionString);
            }catch (Exception ex)
            {
                _logger.LogError(new EventId(1000, "CacheHelper"), ex, "GetCacheService");
            }
            return new MemoryCacheService(memoryCache);
        }
    }
    #endregion
 }