using Colorful.Json;
using Newtonsoft.Json;

namespace Colorful
{
    #region JsonHelper
    /// <summary>
    /// Json帮助类
    /// </summary>
    public static class JsonHelper
    {
        private static IJson _json = new JsonNet();
        /// <summary>
        /// 将指定对象转换为Json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">要转换的对象</param>
        /// <returns>Json字符串</returns>
        public static string ToJson<T>(T target)
        {
            return _json.ToJson(target);
        }
        /// <summary>
        /// 将指定的Json解析为指定的对象（T)
        /// </summary>
        /// <typeparam name="T">要解析到的对象类型</typeparam>
        /// <param name="json">Json字符串</param>
        /// <returns></returns>
        public static T Parse<T>(string json)
        {
            return _json.Parse<T>(json);
        }
    }
    #endregion
}