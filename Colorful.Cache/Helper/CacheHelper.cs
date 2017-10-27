using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Colorful.AspNetCore
{
    #region CacheHelper
    public static class CacheHelper
    {
        private static ILogger _logger;
        public static ICacheService GetCacheService(ILoggerFactory loggerFactory, IMemoryCache memoryCache, string projectId, string connectionString)
        {
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

    #region ICacheService
    public interface ICacheService
    {
        /// <summary>
        /// 验证缓存项是否存在
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <returns></returns>
        bool Exists(string key);
        /// <summary>
        /// 添加缓存
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="value">缓存Value</param>
        /// <param name="expiressAbsoulte">绝对过期时长（不填写则永不过期）</param>
        /// <returns></returns>
        bool Set<T>(string key, T value, TimeSpan? expiressAbsoulte = null);
        /// <summary>
        /// 添加缓存（异步方式）
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="value">缓存Value</param>
        /// <param name="expiresSliding">滑动过期时长（如果在过期时间内有操作，则以当前时间点延长过期时间）</param>
        /// <param name="expiressAbsoulte">绝对过期时长</param>
        /// <returns></returns>
        Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiressAbsoulte = null);
        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <returns></returns>
        bool Remove(string key);
        /// <summary>
        /// 删除缓存（异步方式）
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <returns></returns>
        Task<bool> RemoveAsync(string key);
        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="requireData">若缓存不存在则执行该回调</param>
        /// <param name="expiressAbsoulte">绝对过期时间</param>
        /// <returns></returns>
        T Get<T>(string key, Func<T> requireData = null, TimeSpan? expiressAbsoulte = null);
        /// <summary>
        /// 获取缓存（异步方式）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="requireData">若缓存不存在则执行该回调</param>
        /// <param name="expiressAbsoulte">绝对过期时间</param>
        Task<T> GetAsync<T>(string key, Func<T> requireData = null, TimeSpan? expiressAbsoulte = null);
    }
    #endregion

    #region RedisCacheService
    /// <summary>
    /// Redis缓存
    /// </summary>
    public class RedisCacheService : ICacheService, IDisposable
    {
        private ConnectionMultiplexer _redis;
        private IDatabase _db;
        private string _prefix;

        #region 属性
        /// <summary>
        /// 获取Redis数据库实例
        /// </summary>
        public IDatabase Database
        {
            get
            {
                return _db;
            }
        }
        #endregion

        #region 初始化
        /// <summary>
        /// 初始化RedisCacheHelper
        /// </summary>
        /// <param name="prefix">用于存储Key的前辍（必填）</param>
        public RedisCacheService(string prefix)
            : this(prefix, "localhost:6379,password=liguo1987")
        {
        }
        /// <summary>
        /// 初始化RedisCacheHelper
        /// </summary>
        /// <param name="prefix">缓存前辍如：colorful</param>
        /// <param name="connectionString">连接字符串，如：localhost:6379,password=xxx</param>
        public RedisCacheService(string prefix, string connectionString)
        {
            _prefix = prefix;
            _redis = ConnectionMultiplexer.Connect(connectionString);
            _db = _redis.GetDatabase();
        }
        #endregion

        public bool Exists(string key)
        {
            key = this.GetKey(key);
            return _db.KeyExists(key);
        }

        public bool Set<T>(string key, T value, TimeSpan? expiressAbsoulte = default(TimeSpan?))
        {
            key = this.GetKey(key);
            return _db.StringSet(key, JsonHelper.ToJson(value), expiressAbsoulte);
        }

        public Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiressAbsoulte = default(TimeSpan?))
        {
            key = this.GetKey(key);
            return _db.StringSetAsync(key, JsonHelper.ToJson(value), expiressAbsoulte);
        }

        public bool Remove(string key)
        {
            key = this.GetKey(key);
            return _db.KeyDelete(key);
        }

        public Task<bool> RemoveAsync(string key)
        {
            key = this.GetKey(key);
            return _db.KeyDeleteAsync(key);
        }

        public T Get<T>(string key, Func<T> requireData = null, TimeSpan? expiressAbsoulte = default(TimeSpan?))
        {
            key = this.GetKey(key);
            var value = _db.StringGet(key);
            if (string.IsNullOrEmpty(value))
            {
                if (requireData != null)
                {
                    var data = requireData();
                    _db.StringSet(key, JsonHelper.ToJson(data), expiressAbsoulte);
                    return data;
                }
            }else
            {
                return JsonHelper.Parse<T>(value);
            }
            return default(T);
        }

        public Task<T> GetAsync<T>(string key, Func<T> requireData = null, TimeSpan? expiressAbsoulte = default(TimeSpan?))
        {
            key = this.GetKey(key);
            var task = new TaskCompletionSource<T>();
            Task.Run(() =>
            {
                var data = this.Get(key, requireData, expiressAbsoulte);
                task.TrySetResult(data);
            });
            return task.Task;
        }

        #region Dispose
        public void Dispose()
        {
            _redis.Dispose();
        }
        #endregion

        #region 私有方法
        private string GetKey(string key)
        {
            return string.Format("{0}_{1}", _prefix, key);
        }
        #endregion
    }
    #endregion

    #region MemoryCacheService 
    public class MemoryCacheService : ICacheService, IDisposable
    {
        protected IMemoryCache _cache;

        public MemoryCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public bool Exists(string key)
        {
            return _cache.TryGetValue(key, out var cached);
        }

        public T Get<T>(string key, Func<T> requireData = null, TimeSpan? expiressAbsoulte = default(TimeSpan?))
        {
            if (requireData == null)
            {
                _cache.TryGetValue<T>(key, out var data);
                return data;
            }
            return _cache.GetOrCreate(key, factory =>
            {
                if (expiressAbsoulte != null)
                    factory.SetAbsoluteExpiration(expiressAbsoulte.Value);
                return requireData();
            });
        }

        public Task<T> GetAsync<T>(string key, Func<T> requireData = null, TimeSpan? expiressAbsoulte = default(TimeSpan?))
        {
            if (requireData == null)
            {
                TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
                Task.Run(() =>
                {
                    _cache.TryGetValue<T>(key, out var data);
                    tcs.TrySetResult(data);
                });
                return tcs.Task;
            }
            return _cache.GetOrCreateAsync(key, factory =>
            {
                if (expiressAbsoulte != null)
                    factory.SetAbsoluteExpiration(expiressAbsoulte.Value);
                var task = new TaskCompletionSource<T>();
                Task.Run(() =>
                {
                    var data = requireData();
                    task.TrySetResult(data);
                });
                return task.Task;
            });
        }

        public bool Remove(string key)
        {
            _cache.Remove(key);
            return true;
        }

        public Task<bool> RemoveAsync(string key)
        {
            var tcs = new TaskCompletionSource<bool>();
            Task.Run(() =>
            {
                _cache.Remove(key);
                tcs.TrySetResult(true);
            });
            return tcs.Task;
        }

        public bool Set<T>(string key, T value, TimeSpan? expiressAbsoulte = default(TimeSpan?))
        {
            _cache.Set(key, value);
            return true;
        }

        public Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiressAbsoulte = default(TimeSpan?))
        {
            var tcs = new TaskCompletionSource<bool>();
            Task.Run(() =>
            {
                _cache.Set(key, value);
                tcs.TrySetResult(true);
            });
            return tcs.Task;
        }
        public void Dispose()
        {
            _cache.Dispose();
        }
    }
    #endregion
}