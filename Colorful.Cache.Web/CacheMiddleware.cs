using Colorful.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebMarkupMin.AspNet.Common;
using WebMarkupMin.AspNet.Common.Compressors;
using WebMarkupMin.Core;
using WebMarkupMin.Core.Utilities;

namespace Colorful.Cache.Web
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
        //private readonly IList<IMarkupMinificationManager> _minificationManagers;
        //private readonly IHttpCompressionManager _compressionManager;

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

            //var minificationManagers = new List<IMarkupMinificationManager>();

            //var htmlMinificationManager = services.GetService<IHtmlMinificationManager>();
            //if (htmlMinificationManager != null)
            //{
            //    minificationManagers.Add(htmlMinificationManager);
            //}

            //var xhtmlMinificationManager = services.GetService<IXhtmlMinificationManager>();
            //if (xhtmlMinificationManager != null)
            //{
            //    minificationManagers.Add(xhtmlMinificationManager);
            //}

            //var xmlMinificationManager = services.GetService<IXmlMinificationManager>();
            //if (xmlMinificationManager != null)
            //{
            //    minificationManagers.Add(xmlMinificationManager);
            //}

            //_minificationManagers = minificationManagers;

            //var compressionManager = services.GetService<IHttpCompressionManager>();
            //if (compressionManager != null)
            //{
            //    _compressionManager = compressionManager;
            //}
        }

        public async Task Invoke(HttpContext context)
        {
            var watch = new Stopwatch();
            watch.Start();
            await Cache(context);
            context.Response.Headers.Add("X-ElapsedTime", watch.ElapsedMilliseconds.ToString());
            watch.Stop();
        }

        private async Task Cache(HttpContext context)
        {
            //bool useMinification = _options.EnableMinification && _minificationManagers.Count > 0;
            //bool useCompression = _options.EnableCompression && _compressionManager != null;
            var useMinification = _options.EnableMinification;
            var useCompression = _options.EnableCompression;
            var cacheSetting = _options.Setting;
            var currentUrl = context.Request.Path.Value;
            var accept = $"{context.Request.Headers["Accept"]}";
            var isHtml = context.Request.Method == "GET" && !string.IsNullOrEmpty(accept) && accept.Contains("text/html") && context.Request.Headers["X-Requested-With"] != "XMLHttpRequest";

            #region 不做缓存处理
            if (!isHtml || (!useMinification && !useCompression) || !_options.EnableCache || !cacheSetting.IgnorePages.Contains(currentUrl))
            {
                await _next.Invoke(context);
                return;
            }
            #endregion

            var cachePage = cacheSetting.CachePages.FirstOrDefault(a => a.Url == currentUrl);
            if (cachePage == null)
                cachePage = new CachePage() { Url = currentUrl };
            if (cachePage.CacheTime == 0)
                cachePage.CacheTime = cacheSetting.CacheTime;
            var cacheKey = GetKey(context.Request, cachePage.VaryByParams);

            #region 判断缓存是否存在
            var cacheContent = _cacheService.Get<CacheContent>(cacheKey);
            if (cacheContent != null)
            {
                context.Response.Headers.Add("C-Cache", "Hit");
                if (cacheContent.Headers != null && cacheContent.Headers.Count > 0)
                {
                    foreach (var ch in cacheContent.Headers)
                    {
                        context.Response.Headers.Add(ch.Key, ch.Value);
                    }
                }
                await context.Response.Body.WriteAsync(cacheContent.Content, 0, cacheContent.Content.Length);
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

                    string content = encoding.GetString(cachedBytes);
                    string processedContent = content;
                    IHeaderDictionary responseHeaders = response.Headers;
                    bool isEncodedContent = responseHeaders.IsEncodedContent();
                    Action<string, string> appendHttpHeader = (key, value) =>
                    {
                        responseHeaders.Append(key, new StringValues(value));
                    };

                    #region Html压缩
                    if (useMinification && _options.IsAllowableResponseSize(cachedByteCount))
                    {
                        var htmlMinifier = new HtmlMinifier();
                        var result = htmlMinifier.Minify(processedContent);
                        if (result.Errors.Count == 0)
                        {
                            processedContent = result.MinifiedContent;
                            isProcessed = true;
                        }
                        //foreach (IMarkupMinificationManager minificationManager in _minificationManagers)
                        //{
                        //    if (minificationManager.IsSupportedHttpMethod(httpMethod)
                        //        && mediaType != null && minificationManager.IsSupportedMediaType(mediaType)
                        //        && minificationManager.IsProcessablePage(currentUrl))
                        //    {
                        //        if (isEncodedContent)
                        //        {
                        //            throw new InvalidOperationException(
                        //                string.Format(
                        //                    AspNetCommonStrings.MarkupMinificationIsNotApplicableToEncodedContent,
                        //                    responseHeaders["Content-Encoding"]
                        //                )
                        //            );
                        //        }

                        //        IMarkupMinifier minifier = minificationManager.CreateMinifier();

                        //        MarkupMinificationResult minificationResult = minifier.Minify(processedContent,
                        //            currentUrl, encoding, minificationManager.GenerateStatistics);
                        //        if (minificationResult.Errors.Count == 0)
                        //        {
                        //            processedContent = minificationResult.MinifiedContent;
                        //            isProcessed = true;
                        //        }
                        //    }

                        //    if (isProcessed)
                        //    {
                        //        break;
                        //    }
                        //}
                    }
                    #endregion

                    byte[] processedBytes;
                    if (isProcessed)
                        processedBytes = encoding.GetBytes(processedContent);
                    else
                        processedBytes = cachedBytes;
                    #region GZip压缩
                    if (useCompression && !isEncodedContent)
                    {
                        string acceptEncoding = request.Headers["Accept-Encoding"];
                        string[] encodingTokens = acceptEncoding
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(e => e.Trim().ToLowerInvariant())
                        .ToArray();
                        ICompressor compressor = null;
                        foreach (var et in encodingTokens)
                        {
                            if (et == EncodingTokenConstants.Deflate)
                            {
                                compressor = new DeflateCompressor(new DeflateCompressionSettings()
                                {
                                    Level = System.IO.Compression.CompressionLevel.Fastest
                                });
                                break;
                            }
                            else if (et == EncodingTokenConstants.GZip)
                            {
                                compressor = new GZipCompressor(new GZipCompressionSettings()
                                {
                                    Level = System.IO.Compression.CompressionLevel.Fastest
                                });
                            }
                        }
                        if (compressor != null)
                        {
                            using (var inputStream = new MemoryStream(processedBytes))
                            using (var outputStream = new MemoryStream())
                            {
                                using (Stream compressedStream = compressor.Compress(outputStream))
                                {
                                    await inputStream.CopyToAsync(compressedStream);
                                }
                                byte[] compressedBytes = outputStream.ToArray();
                                processedBytes = compressedBytes;
                                outputStream.Clear();
                                inputStream.Clear();
                                responseHeaders["Content-Length"] = compressedBytes.Length.ToString();
                                compressor.AppendHttpHeaders(appendHttpHeader);
                                await originalStream.WriteAsync(compressedBytes, 0, compressedBytes.Length);
                            }
                        }
                        //using (var inputStream = new MemoryStream(processedBytes))
                        //using (var outputStream = new MemoryStream())
                        //{
                        //    string acceptEncoding = request.Headers["Accept-Encoding"];
                        //    ICompressor compressor = _compressionManager.CreateCompressor(acceptEncoding);

                        //    using (Stream compressedStream = compressor.Compress(outputStream))
                        //    {
                        //        await inputStream.CopyToAsync(compressedStream);
                        //    }

                        //    byte[] compressedBytes = outputStream.ToArray();
                        //    processedBytes = compressedBytes;
                        //    int compressedByteCount = compressedBytes.Length;

                        //    outputStream.Clear();
                        //    inputStream.Clear();

                        //    responseHeaders["Content-Length"] = compressedByteCount.ToString();
                        //    compressor.AppendHttpHeaders(appendHttpHeader);
                        //    await originalStream.WriteAsync(compressedBytes, 0, compressedByteCount);
                        //}
                        isProcessed = true;
                    }
                    #endregion
                    else
                    {
                        if (isProcessed)
                        {
                            int processedByteCount = processedBytes.Length;

                            responseHeaders["Content-Length"] = processedByteCount.ToString();
                            await originalStream.WriteAsync(processedBytes, 0, processedByteCount);
                        }
                    }

                    #region 保存到缓存中
                    cacheContent = new CacheContent()
                    {
                        Content = processedBytes,
                        ContentType = contentType,
                        Headers = new Dictionary<string, string>()
                    };
                    foreach (var rh in responseHeaders)
                    {
                        cacheContent.Headers.Add(rh.Key, rh.Value);
                    }
                    await _cacheService.SetAsync(cacheKey, cacheContent, TimeSpan.FromSeconds(cachePage.CacheTime));
                    context.Response.Headers.Add("C-Cache", "Cached");
                    //Task.Factory.StartNew(obj =>
                    //{
                    //    var cc = (CacheContent)obj;

                    //}, cacheContent);
                    #endregion
                }
                if (!isProcessed)
                {
                    await originalStream.WriteAsync(cachedBytes, 0, cachedByteCount);
                }
            }
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
