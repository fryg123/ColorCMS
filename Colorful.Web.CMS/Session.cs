using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Colorful.AspNetCore
{
    /// <summary>
    /// 自定义Session类
    /// </summary>
    public class MySession
    {
        private ISession _session;

        #region 属性
        /// <summary>
        /// 设置Session（只支持String类型）
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>值</returns>
        public string this[string key]
        {
            get
            {
                return _session.GetString(key);
            }
            set
            {
                _session.SetString(key, value);
            }
        }
        #endregion

        #region init
        public MySession(ISession session) {
            _session = session;
        }
        #endregion

        #region 方法

        #region Get
        /// <summary>
        /// 获取Session
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            if (_session.Keys.Contains(key))
            {
                byte[] v;
                if (_session.TryGetValue(key, out v))
                {
                    var str = v.GetString();
                    if (typeof(T) == typeof(string))
                    {
                        var obj = Convert.ChangeType(str, typeof(T));
                        return (T)obj;
                    }
                    else
                    {
                        return JsonConvert.DeserializeObject<T>(str);
                    }
                }
            }
            return default(T);
        }
        #endregion

        #region Set
        /// <summary>
        /// 设置Session
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void Set<T>(string key, T value)
        {
            if (typeof(T) == typeof(string))
            {
                _session.SetString(key, value as string);
            }
            else
            {
                var json = JsonConvert.SerializeObject(value);
                _session.SetString(key, json);
            }
        }
        #endregion

        #endregion
    }
}