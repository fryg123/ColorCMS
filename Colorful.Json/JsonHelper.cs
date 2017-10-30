using Colorful.Json;
using Newtonsoft.Json;

namespace Colorful
{
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