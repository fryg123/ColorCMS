using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Colorful.Models;

namespace Colorful.AspNetCore
{
    /// <summary>
    /// 枚举帮助类
    /// </summary>
    public class EnumHelper
    {
        /// <summary>
        /// 将枚举转换成列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> ToList<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToList<T>();
        }
        /// <summary>
        /// 根据指定的枚举名称获取枚举列表
        /// </summary>
        /// <param name="enumName">枚举名称</param>
        /// <returns></returns>
        public static List<JsonData<int>> ToJsonData(string enumName)
        {
            var assembly = typeof(JsonData<int>).GetTypeInfo().Assembly;
            var target = assembly.CreateInstance($"Colorful.Models.{enumName}");
            var type = target.GetType();
            return GetJsonData(type);
        }
        private static List<JsonData<int>> GetJsonData(Type type)
        {
            var values = Enum.GetValues(type);
            var datas = new List<JsonData<int>>();
            foreach (var value in values)
            {
                var attr = type.GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;
                var data = new JsonData<int>()
                {
                    id = (int)value,
                    text = attr == null ? value.ToString() : attr.Description
                };
                datas.Add(data);
            }
            return datas;
        }
        public static List<JsonData<int>> ToJsonData<T>()
        {
            return GetJsonData(typeof(T));
        }
    }
}
