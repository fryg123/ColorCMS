using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Colorful.Cache
{
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
}
