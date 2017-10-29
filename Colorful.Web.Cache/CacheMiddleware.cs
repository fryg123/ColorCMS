using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebMarkupMin.AspNet.Common;
using WebMarkupMin.AspNet.Common.Compressors;
using WebMarkupMin.Core;
using WebMarkupMin.Core.Utilities;
using AspNetCommonStrings = WebMarkupMin.AspNet.Common.Resources.Strings;

namespace Colorful.AspNetCore
{
    public static class BuilderExtensions
    {
        public static IApplicationBuilder UseWebCache(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }
            return app.UseMiddleware<CacheMiddleware>();
        }
    }

    public class CacheMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly CacheOptions _options;
        private readonly ICacheService _cacheService;
        private readonly IList<IMarkupMinificationManager> _minificationManagers;
        private readonly IHttpCompressionManager _compressionManager;

        public CacheMiddleware(RequestDelegate next,
            IOptions<CacheOptions> options,
            IServiceProvider services)
        {
            if (next == null)
            {
                throw new ArgumentNullException("next");
            }

            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            if (services == null)
            {
                throw new ArgumentNullException("services");
            }

            _next = next;
            _options = options.Value;

            _cacheService = services.GetService<CacheService>().GetCacheService();

            var minificationManagers = new List<IMarkupMinificationManager>();

            var htmlMinificationManager = services.GetService<IHtmlMinificationManager>();
            if (htmlMinificationManager != null)
            {
                minificationManagers.Add(htmlMinificationManager);
            }

            var xhtmlMinificationManager = services.GetService<IXhtmlMinificationManager>();
            if (xhtmlMinificationManager != null)
            {
                minificationManagers.Add(xhtmlMinificationManager);
            }

            var xmlMinificationManager = services.GetService<IXmlMinificationManager>();
            if (xmlMinificationManager != null)
            {
                minificationManagers.Add(xmlMinificationManager);
            }

            _minificationManagers = minificationManagers;

            var compressionManager = services.GetService<IHttpCompressionManager>();
            if (compressionManager != null)
            {
                _compressionManager = compressionManager;
            }
        }

        public async Task Invoke(HttpContext context)
        {
            bool useMinification = _options.EnableMinification && _minificationManagers.Count > 0;
            bool useCompression = _options.EnableCompression && _compressionManager != null;
            var cacheSetting = _options.Setting;
            var urlPath = context.Request.Path.Value;
            var accept = context.Request.Headers["Accept"].ToString();
            var isHtml = !string.IsNullOrEmpty(accept) && accept.Contains("text/html") && context.Request.Headers["X-Requested-With"] != "XMLHttpRequest";

            #region 不做缓存处理
            if (!isHtml || (!useMinification && !useCompression) || !_options.EnableCache || !cacheSetting.IgnorePages.Contains(urlPath))
            {
                await _next.Invoke(context);
                return;
            }
            #endregion

            var cachePage = cacheSetting.CachePages.FirstOrDefault(a => a.Url == urlPath);
            if (cachePage == null)
                cachePage = new Models.CachePage() { CacheTime = cacheSetting.CacheTime, Url = urlPath };
            var cacheKey = GetKey(context.Request, cachePage.VaryByParams);

            #region 判断缓存是否存在
            var cachedResult = _cacheService.Get<string>(cacheKey);
            if (!string.IsNullOrEmpty(cachedResult))
            {
                context.Response.OnStarting(state => {
                    var httpContext = (HttpContext)state;
                    httpContext.Response.Headers.Add("C-Cache", "Hit");
                    return Task.FromResult(0);
                }, context);

                await context.Response.WriteAsync(cachedResult);
                return;
            }
            #endregion

            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            using (var cachedStream = new MemoryStream())
            {
                Stream originalStream = response.Body;
                response.Body = cachedStream;

                try
                {
                    await _next.Invoke(context);
                }
                catch (Exception)
                {
                    response.Body = originalStream;
                    cachedStream.Clear();

                    throw;
                }

                byte[] cachedBytes = cachedStream.ToArray();
                int cachedByteCount = cachedBytes.Length;
                bool isProcessed = false;

                response.Body = originalStream;
                cachedStream.Clear();

                if (cachedByteCount == 0)
                {
                    return;
                }

                if (response.StatusCode == 200)
                {
                    string httpMethod = request.Method;
                    string contentType = response.ContentType;
                    string mediaType = null;
                    Encoding encoding = null;

                    if (contentType != null)
                    {
                        MediaTypeHeaderValue mediaTypeHeader;

                        if (MediaTypeHeaderValue.TryParse(contentType, out mediaTypeHeader))
                        {
                            mediaType = mediaTypeHeader.MediaType.Value;
                            encoding = mediaTypeHeader.Encoding;
                        }
                    }

                    encoding = encoding ?? Encoding.GetEncoding(0);

                    string currentUrl = request.Path.Value;
                    QueryString queryString = request.QueryString;
                    if (queryString.HasValue)
                    {
                        currentUrl += queryString.Value;
                    }

                    string content = encoding.GetString(cachedBytes);
                    string processedContent = content;
                    IHeaderDictionary responseHeaders = response.Headers;
                    bool isEncodedContent = responseHeaders.IsEncodedContent();
                    Action<string, string> appendHttpHeader = (key, value) =>
                    {
                        responseHeaders.Append(key, new StringValues(value));
                    };

                    if (useMinification && _options.IsAllowableResponseSize(cachedByteCount))
                    {
                        foreach (IMarkupMinificationManager minificationManager in _minificationManagers)
                        {
                            if (minificationManager.IsSupportedHttpMethod(httpMethod)
                                && mediaType != null && minificationManager.IsSupportedMediaType(mediaType)
                                && minificationManager.IsProcessablePage(currentUrl))
                            {
                                if (isEncodedContent)
                                {
                                    throw new InvalidOperationException(
                                        string.Format(
                                            AspNetCommonStrings.MarkupMinificationIsNotApplicableToEncodedContent,
                                            responseHeaders["Content-Encoding"]
                                        )
                                    );
                                }

                                IMarkupMinifier minifier = minificationManager.CreateMinifier();

                                MarkupMinificationResult minificationResult = minifier.Minify(processedContent,
                                    currentUrl, encoding, minificationManager.GenerateStatistics);
                                if (minificationResult.Errors.Count == 0)
                                {
                                    processedContent = minificationResult.MinifiedContent;
                                    isProcessed = true;
                                }
                            }

                            if (isProcessed)
                            {
                                break;
                            }
                        }
                    }

                    if (useCompression && !isEncodedContent
                        && _compressionManager.IsSupportedHttpMethod(httpMethod)
                        && _compressionManager.IsSupportedMediaType(mediaType)
                        && _compressionManager.IsProcessablePage(currentUrl))
                    {
                        byte[] processedBytes = encoding.GetBytes(processedContent);

                        using (var inputStream = new MemoryStream(processedBytes))
                        using (var outputStream = new MemoryStream())
                        {
                            string acceptEncoding = request.Headers["Accept-Encoding"];
                            ICompressor compressor = _compressionManager.CreateCompressor(acceptEncoding);

                            using (Stream compressedStream = compressor.Compress(outputStream))
                            {
                                await inputStream.CopyToAsync(compressedStream);
                            }

                            byte[] compressedBytes = outputStream.ToArray();
                            int compressedByteCount = compressedBytes.Length;

                            outputStream.Clear();
                            inputStream.Clear();

                            responseHeaders["Content-Length"] = compressedByteCount.ToString();
                            compressor.AppendHttpHeaders(appendHttpHeader);
                            await originalStream.WriteAsync(compressedBytes, 0, compressedByteCount);
                        }

                        isProcessed = true;
                    }
                    else
                    {
                        if (isProcessed)
                        {
                            byte[] processedBytes = encoding.GetBytes(processedContent);
                            int processedByteCount = processedBytes.Length;

                            responseHeaders["Content-Length"] = processedByteCount.ToString();
                            await originalStream.WriteAsync(processedBytes, 0, processedByteCount);
                        }
                    }
                }

                if (!isProcessed)
                {
                    await originalStream.WriteAsync(cachedBytes, 0, cachedByteCount);
                }
            }

            context.Response.OnStarting(state => {
                var httpContext = (HttpContext)state;
                httpContext.Response.Headers.Add("C-Cache", "Cache");
                return Task.FromResult(0);
            }, context);
        }

        protected string GetKey(HttpRequest request, params string[] varyByParams)
        {
            StringBuilder urlStr;
            if (varyByParams.Length == 0)
                urlStr = new StringBuilder($"{request.Scheme}://{request.Host}{request.Path.Value}");
            else
            {
                urlStr = new StringBuilder($"{request.Scheme}://{request.Host}{request.Path.Value}?");
                var i = 0;
                foreach (var p in varyByParams)
                {
                    var value = request.Query[p];
                    if (i > 0) urlStr.Append("&");
                    urlStr.Append($"{p}={value}");
                    i++;
                }
            }
            return urlStr.ToString().UrlEncode().ToMd5();
        }
    }

    internal static class HeaderDictionaryExtensions
    {
        /// <summary>
        /// Checks whether the content is encoded
        /// </summary>
        /// <param name="headers">The <see cref="IHeaderDictionary"/> to use</param>
        /// <returns>Result of check (true - content is encoded; false - content is not encoded)</returns>
        public static bool IsEncodedContent(this IHeaderDictionary headers)
        {
            return headers.ContainsKey("Content-Encoding")
                && !headers["Content-Encoding"].ToString().Equals("identity", StringComparison.OrdinalIgnoreCase);
        }
    }
}
