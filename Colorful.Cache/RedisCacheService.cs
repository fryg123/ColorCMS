using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Colorful.Cache
{
    #region RedisCache
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
            }
            else
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
}
