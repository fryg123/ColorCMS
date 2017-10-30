using Colorful.Web.CMS;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
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

        #region IFormFile
        public static void SaveAs(this IFormFile file, string savePath)
        {
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }
        }
        #endregion

        #region HttpRequest
        public static Uri GetUri(this HttpRequest request)
        {
            return new Uri($"{request.Scheme}://{request.Host}{request.Path.Value}{(request.QueryString.HasValue ? request.QueryString.Value : "")}");
        }
        public static string GetPathAndQuery(this HttpRequest request)
        {
            return $"{request.Path.Value}{(request.QueryString.HasValue ? request.QueryString.Value : "")}";
        }
        public static string GetEncodedUrl(this HttpRequest request)
        {
            return Net.WebUtility.UrlEncode(request.GetUri().ToString());
        }
        public static string Get(this HttpRequest request, string key)
        {
            if (request.Method == "GET")
            {
                return request.Query[key];
            }
            else if (request.Method == "POST")
                return request.Form[key];
            return null;
        }
        #endregion

        #region HttpContext
        public static string MapPath(this HttpContext context, string url)
        {
            if (!url.StartsWith("/"))
                throw new InvalidOperationException("无效的路径！");
            var rootPath = WebConfig.RootPath;
            var path = rootPath + url.Replace("/", "\\");
            return path;
        }
        #endregion
    }
}
#endregion

#region System.Web
namespace System.Web
{
    public static class HttpContext
    {
        private static IHttpContextAccessor _contextAccessor;

        public static Microsoft.AspNetCore.Http.HttpContext Current => _contextAccessor.HttpContext;

        internal static void Configure(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }
    }

    #region Extensions
    public static class StaticHttpContextExtensions
    {
        public static void AddHttpContextAccessor(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        public static IApplicationBuilder UseStaticHttpContext(this IApplicationBuilder app)
        {
            var httpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
            System.Web.HttpContext.Configure(httpContextAccessor);
            return app;
        }
    }
    #endregion
}

#endregion


