using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Colorful.Cache
{
    #region MemoryCache 
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
