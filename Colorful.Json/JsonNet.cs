using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Colorful.Json
{
    #region Json.Net
    public class JsonNet : IJson
    {
        private JsonSerializerSettings _jsonSettings = null;

        public JsonNet()
        {
            _jsonSettings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatString = "yyyy/MM/dd HH:mm:ss"
            };
        }
        /// <summary>
        /// 将指定对象转换为Json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">要转换的对象</param>
        /// <returns>Json字符串</returns>
        public string ToJson<T>(T target)
        {
            return JsonConvert.SerializeObject(target, _jsonSettings);
        }
        /// <summary>
        /// 将指定的Json解析为指定的对象（T)
        /// </summary>
        /// <typeparam name="T">要解析到的对象类型</typeparam>
        /// <param name="json">Json字符串</param>
        /// <returns></returns>
        public T Parse<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, _jsonSettings);
        }
    }
    #endregion
}
