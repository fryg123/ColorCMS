using Colorful;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Microsoft.AspNetCore.Mvc.Rendering
{
    public static partial class MyHtmlHelper
    {
        #region Json格式化
        public static IHtmlContent ToJson<T>(this IHtmlHelper htmlHelper, T target)
        {
            if (target == null) return htmlHelper.Raw("{}");
            return htmlHelper.Raw(Colorful.JsonHelper.ToJson(target));
        }
        #endregion

        #region 中英文识别
        public static IHtmlContent GetString(this IHtmlHelper htmlHelper, string cn, string en)
        {
            var isCN = htmlHelper.ViewBag == null || htmlHelper.ViewBag.Lang == null ? true : htmlHelper.ViewBag.Lang == "CN";
            return htmlHelper.Raw(isCN ? cn : en);
        }
        public static T GetLang<T>(this IHtmlHelper htmlHelper, T cn, T en)
        {
            var isCN = htmlHelper.ViewBag == null || htmlHelper.ViewBag.Lang == null ? true : htmlHelper.ViewBag.Lang == "CN";
            return isCN ? cn : en;
        }
        #endregion

        #region 获取Url后缀字符串
        public static IHtmlContent GetLink(this IHtmlHelper htmlHelper, string url)
        {
            url = htmlHelper.GetLinkToString(url);
            return htmlHelper.Raw(url);
        }
        public static string GetLinkToString(this IHtmlHelper htmlHelper, string url)
        {
            var path = htmlHelper.ViewContext.HttpContext.Request.GetPathAndQuery();
            if (string.IsNullOrEmpty(url))
            {
                return path.ToLower().Replace("?lang=en", "").Replace("&lang=en", "");
            }
            var isCN = htmlHelper.ViewBag == null ? true : htmlHelper.ViewBag.Lang == "CN";
            if (!isCN)
            {
                //if (url.StartsWith("?"))
                //    url = htmlHelper.ViewContext.HttpContext.Request.Url.AbsolutePath;
                if (url.ToLower().Contains("lang="))
                {
                    url = Regex.Replace(url, "lang=cn", "lang=en");
                }else if (url.Contains("#") && !url.EndsWith("#"))
                {
                    if (url.Contains("?"))
                        url = Regex.Replace(url, "#", "&lang=en#");
                    else
                        url = Regex.Replace(url, "#", "?lang=en#");
                }
                else
                {
                    url += (url.Contains("?")?"&": "?") + "lang=en";
                }
                //var args = url.Split('?');
                //string query;
                //if (args.Length > 1)
                //{
                //    query = args[1].ToLower();
                //    if (query.Contains("lang="))
                //        query = query.Replace("lang=cn", "lang=en");
                //    else
                //        query += $"&lang=en";
                //    url = args[0] + "?" + query;
                //}
                //else
                //    url += "?lang=en";
            }
            return url;
        }
        public static IHtmlContent GetLink(this IHtmlHelper htmlHelper, object url)
        {
            return htmlHelper.GetLink($"{url}");
        }
        #endregion

        #region 截断字符串
        public static IHtmlContent Truncate(this IHtmlHelper htmlHelper, string source, int maxLength, bool autoAddEllipsis = false)
        {
            if (string.IsNullOrEmpty(source))
                return htmlHelper.Raw("");
            source = Regex.Replace(source, "<.+?>", "", RegexOptions.IgnoreCase);
            if (autoAddEllipsis)
                return htmlHelper.Raw((source.Length > maxLength ? source.Substring(0, maxLength) : source) + "...");
            else
                return htmlHelper.Raw((source.Length > maxLength ? source.Substring(0, maxLength) + "..." : source));
        }
        #endregion

        #region 根据链接显示对应内容
        public static IHtmlContent GetStringForUrl(this IHtmlHelper htmlHelper, string link, string content, bool absoluteMatch = true)
        {
            var url = htmlHelper.ViewContext.HttpContext.Request.GetPathAndQuery().ToLower();
            if ((absoluteMatch && url.EndsWith(link)) || (!absoluteMatch && url.Contains(link)))
                return htmlHelper.Raw(content);
            return htmlHelper.Raw("");
        }
        #endregion

        #region 根据条件显示对应内容
        public static IHtmlContent GetString(this IHtmlHelper htmlHelper, bool expression, string trueStr, string falseStr = "")
        {
            return htmlHelper.Raw(expression ? trueStr : falseStr);
        }
        #endregion

        #region 根据调试模式显示对应内容
        public static IHtmlContent OutputDebug(this IHtmlHelper htmlHelper, string debugStr, string releaseStr)
        {
            return htmlHelper.Raw(WebConfig.IsDebug ? debugStr : releaseStr);
        }
        /// <summary>
        /// 上线与当前版本
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="onlineStr">上线内容</param>
        /// <param name="currentStr">当前内容</param>
        /// <param name="target">上线标识（OnlineTarget枚举）</param>
        /// <returns></returns>
        public static IHtmlContent OutputOnline(this IHtmlHelper htmlHelper, string onlineStr, string currentStr, string target)
        {
            if (htmlHelper.ViewContext.HttpContext.Request.Host.Host.Contains("test112311."))
                return htmlHelper.Raw(onlineStr);
            return htmlHelper.Raw(WebConfig.SysSetting.OnlineTargets.Contains(target) ? onlineStr : currentStr);
        }
        #endregion

        #region 拼接字符串
        public static IHtmlContent Join(this IHtmlHelper htmlHelper, params object[] str)
        {
            return htmlHelper.Raw(string.Join("", str));
        }
        #endregion

        #region RazorToString
        public static string RazorViewToString(this Controller controller, string viewName, object model = null)
        {
            if (model != null)
                controller.ViewData.Model = model;
            return WebConfig.RazorViewService.ToHtml(viewName, controller.ViewData);
        }
        #endregion

        #region 获取公共缓存
        /// <summary>
        /// 获取公共缓存
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="path">路径</param>
        /// <returns></returns>
        public static IHtmlContent GetRes(this IHtmlHelper htmlHelper, string path)
        {
            if (path.StartsWith("/"))
                path = path.Substring(1);
            return htmlHelper.Raw($"{WebConfig.ResUrl}{path}");
        }
        /// <summary>
        /// 获取编辑器脚本标签
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <returns></returns>
        public static IHtmlContent GetEditorScripts(this IHtmlHelper htmlHelper)
        {
            return htmlHelper.Raw($"<script src=\"{WebConfig.ResUrl}lib/UEditor/ueditor.all.min.js\"></script>\r\n<script src=\"{WebConfig.ResUrl}lib/UEditor/lang/zh-cn/zh-cn.js\"></script>");
        }
        #endregion
    }
}