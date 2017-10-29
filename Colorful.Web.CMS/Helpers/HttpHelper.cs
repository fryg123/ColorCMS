using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Colorful.AspNetCore
{
    #region HttpClientHandler
    class MyHttpClientHandler : HttpClientHandler
    {
        private string _referrer;
        public MyHttpClientHandler(string referrer = null) : base()
        {
            _referrer = referrer;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(_referrer))
                request.Headers.Referrer = new Uri(_referrer);
            request.Headers.Add("UserAgent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36");
            var response = await base.SendAsync(request, cancellationToken);
            var contentType = response.Content.Headers.ContentType;
            contentType.CharSet = await getCharSetAsync(response.Content);
            return response;
        }
        private async Task<string> getCharSetAsync(HttpContent httpContent)
        {
            var charset = httpContent.Headers.ContentType.CharSet;
            if (!string.IsNullOrEmpty(charset))
                return charset;

            var content = await httpContent.ReadAsStringAsync();
            var match = Regex.Match(content, @"charset=(?<charset>.+?)""", RegexOptions.IgnoreCase);
            if (!match.Success)
                return charset;

            return match.Groups["charset"].Value;
        }
    }
    #endregion

    public class HttpHelper : HttpClient
    {
        #region 构造函数
        /// <summary>
        /// Url
        /// </summary>
        /// <param name="url">Url</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="referrer"></param>
        public HttpHelper(int timeout = 15000, string referrer = null)
            : base(new MyHttpClientHandler(referrer))
        {
            this.Timeout = TimeSpan.FromSeconds(timeout);
        }
        #endregion

        #region 下载文件
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="url">文件地址</param>
        /// <param name="savePath">保存路径</param>
        public static async void DownloadFile(string url, string savePath)
        {
            using (var httpClient = new HttpHelper(1000*60*60))
            {
                using (var stream = await httpClient.GetStreamAsync(url))
                {
                    using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await stream.CopyToAsync(fileStream);
                    }
                }
            }
        }
        #endregion

        #region 获取指定内容
        public static async Task<string> GetContent(string url)
        {
            using (var httpClient = new HttpHelper())
            {
                return await httpClient.GetStringAsync(url);
            }
        }
        #endregion

        #region 测试Url
        /// <summary>
        /// 测试指定的Url列表
        /// </summary>
        /// <param name="urls"></param>
        /// <returns>不通的Url列表</returns>
        public static async Task<List<string>> TestUrlAsync(params string[] urls)
        {
            var task = Task.Run<List<string>>(async () =>
            {
                var urlList = new List<string>();
                var urlEnumerator = urls.GetEnumerator();
                while (urlEnumerator.MoveNext())
                {
                    var url = ((string)urlEnumerator.Current);
                    if (!url.StartsWith("http://"))
                        url = "http://" + url;
                    try
                    {
                        var webReq = WebRequest.CreateHttp(new Uri(url));
                        webReq.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36";
                        webReq.Headers[HttpRequestHeader.Referer] = url;
                        webReq.ContinueTimeout = 3000;
                        var response = (HttpWebResponse)await webReq.GetResponseAsync();
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            urlList.Add(url);
                        }
                        response.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        urlList.Add(url);
                    }
                }
                return urlList;
            });
            return await task;
        }
        #endregion

        #region 获取IP地址
        public static string GetIP()
        {
            if (System.Web.HttpContext.Current == null) return "";
            if (!string.IsNullOrEmpty(System.Web.HttpContext.Current.Request.Headers["HTTP_X_FORWARDED_FOR"]))
            {
                return System.Web.HttpContext.Current.Request.Headers["HTTP_X_FORWARDED_FOR"].ToString();
            }
            else if (!string.IsNullOrEmpty(System.Web.HttpContext.Current.Request.Headers["HTTP_X_REAL_IP"]))
                return System.Web.HttpContext.Current.Request.Headers["HTTP_X_REAL_IP"].ToString();
            else
            {
                return System.Web.HttpContext.Current.Request.Headers["REMOTE_ADDR"].ToString();
            }
        }
        #endregion

        #region Cookie操作
        public static void SetCookie(string key, string value, DateTime? expires = null)
        {
            SetCookie(key, value, false, expires);
        }
        private static void SetCookie(string key, string value, bool encrypt, DateTime? expires = null)
        {
            if (encrypt)
                value = AESHelper.Encrypt(value);
            System.Web.HttpContext.Current.Response.Cookies.Append(key, value, new Microsoft.AspNetCore.Http.CookieOptions()
            {
                Expires = expires == null ? DateTime.Now.AddDays(360) : expires.Value
            });
        }
        public static void SetEncryptCookie(string key, string value, DateTime? expires = null)
        {
            SetCookie(key, value, true, expires);
        }
        public static string GetCookies(string key, bool encrypt = false)
        {
            string value = "";
            System.Web.HttpContext.Current.Request.Cookies.TryGetValue("password", out value);
            return value;
        }
        public static void RemoveCookie(string key)
        {
            System.Web.HttpContext.Current.Response.Cookies.Delete(key);
        }
        #endregion
    }
}