using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#region System
namespace System
{
    public static class SystemExtension
    {
        private delegate object CreateObject();
        private static Dictionary<Type, CreateObject> _constrcache = new Dictionary<Type, CreateObject>();

        #region 转换为指定类型
        /// <summary>
        /// 将对象转换为指定的类型
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType">要转换的类型</param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static object ConvertTo(this object value, Type targetType, string typeName = "default")
        {
            var strValue = string.Format("{0}", value);
            if (value != null && targetType.FullName == value.GetType().FullName)
                return value;
            switch (targetType.Name)
            {
                case "Byte":
                    try
                    {
                        if (string.IsNullOrEmpty(strValue))
                            if (typeName == "list")
                                return null;
                            else
                                return default(byte);
                        else
                        {
                            if (typeName == "list")
                                return ConvertList<byte>(value as IList, targetType);
                            else
                                return byte.Parse(strValue);
                        }
                    }
                    catch
                    {
                        return default(byte);
                    }
                case "Int32":
                    if (string.IsNullOrEmpty(strValue))
                        if (typeName == "list")
                            return null;
                        else
                            return default(int);
                    else
                    {
                        if (typeName == "list")
                            return ConvertList<int>(value as IList, targetType);
                        else
                        {
                            if (strValue.Contains("."))
                                return System.Convert.ToInt32(decimal.Parse(strValue));
                            else
                                return int.Parse(strValue);
                        }
                    }
                case "Int64":
                    if (string.IsNullOrEmpty(strValue))
                        if (typeName == "list")
                            return null;
                        else
                            return default(long);
                    else
                    {
                        if (typeName == "list")
                            return ConvertList<long>(value as IList, targetType);
                        else
                            return long.Parse(strValue);
                    }
                case "DateTime":
                    if (string.IsNullOrEmpty(strValue))
                        if (typeName == "list" || typeName == "null")
                            return null;
                        else
                            return default(DateTime);
                    else
                    {
                        if (typeName == "list")
                            return ConvertList<DateTime>(value as IList, targetType);
                        else
                            return DateTime.Parse(strValue);
                    }
                case "Boolean":
                    if (string.IsNullOrEmpty(strValue))
                    {
                        if (typeName == "list")
                            return null;
                        else
                            return default(bool);
                    }
                    else
                    {
                        if (strValue == "on" || strValue == "true")
                            return true;
                        else
                        {
                            if (typeName == "list")
                                return ConvertList<bool>(value as IList, targetType);
                            else
                                return bool.Parse(strValue);
                        }
                    }
                case "Decimal":
                    if (string.IsNullOrEmpty(strValue))
                        if (typeName == "list")
                            return null;
                        else
                            return default(decimal);
                    else
                    {
                        if (typeName == "list")
                            return ConvertList<Decimal>(value as IList, targetType);
                        else
                            return decimal.Parse(strValue);
                    }
                case "Double":
                    if (string.IsNullOrEmpty(strValue))
                        if (typeName == "list")
                            return null;
                        else
                            return default(double);
                    else
                    {
                        if (typeName == "list")
                            return ConvertList<double>(value as IList, targetType);
                        else
                            return double.Parse(strValue);
                    };
                case "List`1":
                    var genericType = targetType.GetGenericArguments().FirstOrDefault();
                    if (genericType.BaseType().Name == "Enum")
                    {
                        var list = (IList)targetType.CreateInstance();
                        if (value is string)
                        {
                            if (!string.IsNullOrEmpty(strValue))
                            {
                                var items = strValue.Split(',').Select(a => int.Parse(a)).ToList();
                                foreach (var item in items)
                                {
                                    list.Add(Enum.ToObject(genericType, item));
                                }
                            }
                        }
                        else
                        {
                            if (value != null)
                            {
                                foreach (var item in (IList)value)
                                {
                                    list.Add(item.ConvertTo(genericType));
                                }
                            }
                        }
                        return list;
                    }
                    else
                        return (value is string ? strValue.Split(',').ToList() : value).ConvertTo(genericType, "list");
                default:
                    if (targetType.BaseType().Name == "Enum")
                    {
                        if (string.IsNullOrEmpty(strValue))
                            return 0;
                        else
                        {
                            if (value is int)
                                return Enum.ToObject(targetType, (int)value);
                            else
                                return Enum.Parse(targetType, strValue);
                        }
                    }
                    else
                    {
                        if (typeName == "list")
                            return ConvertList<string>(value as IList, targetType);
                        else
                            return value;
                    }
            }
        }

        private static List<T> ConvertList<T>(IList targetList, Type convertType)
        {
            List<T> list = new List<T>();
            foreach (var target in targetList)
                list.Add((T)target.ConvertTo(convertType));
            return list;
        }
        #endregion

        #region FastCreateInstance
        public static object CreateInstance(this Type type)
        {
            try
            {
                CreateObject c = null;
                if (_constrcache.TryGetValue(type, out c))
                {
                    return c();
                }
                else
                {
                    if (type.GetTypeInfo().IsClass)
                    {
                        DynamicMethod dynMethod = new DynamicMethod("_", type, null);
                        ILGenerator ilGen = dynMethod.GetILGenerator();
                        ilGen.Emit(OpCodes.Newobj, type.GetConstructor(Type.EmptyTypes));
                        ilGen.Emit(OpCodes.Ret);
                        c = (CreateObject)dynMethod.CreateDelegate(typeof(CreateObject));
                        _constrcache.Add(type, c);
                    }
                    else // structs
                    {
                        DynamicMethod dynMethod = new DynamicMethod("_", typeof(object), null);
                        ILGenerator ilGen = dynMethod.GetILGenerator();
                        var lv = ilGen.DeclareLocal(type);
                        ilGen.Emit(OpCodes.Ldloca_S, lv);
                        ilGen.Emit(OpCodes.Initobj, type);
                        ilGen.Emit(OpCodes.Ldloc_0);
                        ilGen.Emit(OpCodes.Box, type);
                        ilGen.Emit(OpCodes.Ret);
                        c = (CreateObject)dynMethod.CreateDelegate(typeof(CreateObject));
                        _constrcache.Add(type, c);
                    }
                    return c();
                }
            }
            catch (Exception exc)
            {
                throw new Exception(string.Format("Failed to fast create instance for type '{0}' from assembly '{1}'",
                    type.FullName, type.AssemblyQualifiedName), exc);
            }
        }
        #endregion

        #region Type
        public static Type BaseType(this Type type)
        {
            return type.GetTypeInfo().BaseType;
        }
        #endregion

        #region Enum
        /// <summary>
        /// 获取枚举描述
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum value)
        {
            var attr = value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;
            return attr.Description;
        }
        public static DisplayAttribute GetDisplay(this Enum value)
        {
            var attr = value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault() as DisplayAttribute;
            return attr;
        }
        #endregion

        #region String
        /// <summary>
        /// 将字符串转换为字节
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] ToBytes(this string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
        /// <summary>
        /// 将指定的字符串转换为Md5
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToMd5(this string str)
        {
            var md5 = System.Security.Cryptography.MD5.Create();
            var data = md5.ComputeHash(Encoding.ASCII.GetBytes(str));
            var result = new StringBuilder();
            for (var i = 0; i < data.Length; i++)
                result.Append(data[i].ToString("x2"));
            return result.ToString();
        }
        /// <summary>
        /// Url编码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string UrlEncode(this string str)
        {
            return Net.WebUtility.UrlEncode(str);
        }
        /// <summary>
        /// Url解码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string UrlDecode(this string str)
        {
            return Net.WebUtility.UrlEncode(str);
        }

        public static Boolean IsMatch(this String input, String pattern, RegexOptions options = RegexOptions.IgnoreCase)
        {
            return Regex.IsMatch(input, pattern, options);
        }
        public static Match Match(this String input, String pattern, RegexOptions options = RegexOptions.IgnoreCase)
        {
            return Regex.Match(input, pattern, options);
        }
        public static MatchCollection Matches(this String input, String pattern, RegexOptions options = RegexOptions.IgnoreCase)
        {
            return Regex.Matches(input, pattern, options);
        }
        public static int? ParseInt(this string source)
        {
            int num;
            if (int.TryParse(source, out num))
                return num;
            return null;
        }
        public static DateTime? ParseDate(this string source)
        {
            DateTime date;
            if (DateTime.TryParse(source, out date))
                return date;
            return null;
        }
        public static long ToLong(this string input)
        {
            if (string.IsNullOrEmpty(input)) return 0;
            long result;
            long.TryParse(input, out result);
            return result;
        }
        public static int ToInt(this string input)
        {
            if (string.IsNullOrEmpty(input)) return 0;
            int result;
            int.TryParse(input, out result);
            return result;
        }
        public static decimal ToDecimal(this string input)
        {
            if (string.IsNullOrEmpty(input)) return 0;
            decimal result;
            decimal.TryParse(input, out result);
            return result;
        }
        public static double ToDouble(this string input)
        {
            if (string.IsNullOrEmpty(input)) return 0;
            double result;
            double.TryParse(input, out result);
            return result;
        }
        public static DateTime ToDateTime(this string input)
        {
            if (string.IsNullOrEmpty(input)) return DateTime.MinValue;
            DateTime result;
            DateTime.TryParse(input, out result);
            return result;
        }
        #endregion

        #region bytes
        /// <summary>
        /// 将字节转换为String
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string GetString(this byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }
        #endregion

        #region DateTime
        public static DateTime MinDate(this DateTime @this)
        {
            return new DateTime(@this.Year, @this.Month, @this.Day, 0, 0, 0);
        }

        public static DateTime MaxDate(this DateTime @this)
        {
            return new DateTime(@this.Year, @this.Month, @this.Day, 23, 59, 59);
        }
        #endregion
    }
}
#endregion


