using Newtonsoft.Json;

namespace Colorful.AspNetCore
{
    #region IJson
    public interface IJson
    {
        string ToJson<T>(T target);
        T Parse<T>(string json);
    }
    #endregion

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
        /// ��ָ������ת��ΪJson
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">Ҫת���Ķ���</param>
        /// <returns>Json�ַ���</returns>
        public string ToJson<T>(T target)
        {
            return JsonConvert.SerializeObject(target, _jsonSettings);
        }
        /// <summary>
        /// ��ָ����Json����Ϊָ���Ķ���T)
        /// </summary>
        /// <typeparam name="T">Ҫ�������Ķ�������</typeparam>
        /// <param name="json">Json�ַ���</param>
        /// <returns></returns>
        public T Parse<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, _jsonSettings);
        }
    }
    #endregion

    #region JsonHelper
    /// <summary>
    /// Json������
    /// </summary>
    public static class JsonHelper
    {
        private static IJson _json = new JsonNet();
        /// <summary>
        /// ��ָ������ת��ΪJson
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">Ҫת���Ķ���</param>
        /// <returns>Json�ַ���</returns>
        public static string ToJson<T>(T target)
        {
            return _json.ToJson(target);
        }
        /// <summary>
        /// ��ָ����Json����Ϊָ���Ķ���T)
        /// </summary>
        /// <typeparam name="T">Ҫ�������Ķ�������</typeparam>
        /// <param name="json">Json�ַ���</param>
        /// <returns></returns>
        public static T Parse<T>(string json)
        {
            return _json.Parse<T>(json);
        }
    }
    #endregion
}