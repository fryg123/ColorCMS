using Colorful.Models;
using Colorful.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Colorful
{
    public partial class BaseController : Controller
    {
        #region 私有变量
        private ILogger _logger;
        private MySession _session = null;
        private string _lang = null;
        private Client _client;
        private AppUser _appUser;
        private string _userId;
        private string _loginId;
        private string _url;
        private UserPermission _userPermission;
        #endregion

        #region 属性

        #region 语言
        /// <summary>
        /// 当前语言
        /// </summary>
        public string Lang
        {
            get
            {
                if (_lang == null)
                {
                    _lang = $"{this.HttpContext.Items["Lang"]}";
                    if (string.IsNullOrEmpty(_lang))
                        _lang = "CN";
                }
                return _lang;
            }
        }
        protected List<JsonData<string>> Languages
        {
            get
            {
                return MyWebConfig.Languages;
            }
        }
        #endregion

        #region Session
        /// <summary>
        /// Session对象
        /// </summary>
        protected MySession Session
        {
            get
            {
                if (_session == null)
                    _session = new MySession(HttpContext.Session);
                return _session;
            }
        }
        #endregion

        #region 日志 Logger
        /// <summary>
        /// 日志对象
        /// </summary>
        protected ILogger Logger
        {
            get
            {
                if (_logger == null)
                {
                    var controllerName = this.RouteData.Values["controller"].ToString().ToLower();
                    _logger = MyWebConfig.LoggerFactory.CreateLogger(controllerName);
                }
                return _logger;
            }
        }
        #endregion

        #region 客户信息
        /// <summary>
        /// 客户信息
        /// </summary>
        public Client Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new Client(Request);
                }
                return _client;
            }
        }
        #endregion

        #region 用户信息
        /// <summary>
        /// 当前登陆用户唯一Id
        /// </summary>
        protected string UserId
        {
            get
            {
                if (_userId == null)
                {
                    _userId = this.User.GetId();
                }
                return _userId;
            }
        }
        protected string LoginId
        {
            get
            {
                if (_loginId == null)
                {
                    _loginId = this.User.GetLoginId();
                }
                return _loginId;
            }
        }
        /// <summary>
        /// 用户对象
        /// </summary>
        protected AppUser AppUser
        {
            get
            {
                if (_appUser == null && !string.IsNullOrEmpty(this.UserId))
                {
                    _appUser = this.GetCache<AppUser>("AppUser");
                    if (_appUser == null) return null;
                    if (_appUser.Roles == null)
                        _appUser.Roles = new List<long>();
                }
                return _appUser;
            }
        }
        #endregion

        #region 当前网址
        /// <summary>
        /// 当前服务器地址
        /// </summary>
        protected string ServerUrl
        {
            get
            {
                if (_url == null)
                {
                    if (Request.Host.Port == 443)
                        _url = $"https://{Request.Host.ToString()}";
                    else
                        _url = String.Format("http://{0}{1}", Request.Host, Request.Host.Port == 80 ? "" : ":" + Request.Host.Port);
                }
                return _url;
            }
        }
        #endregion

        #region 网站设置
        protected WebSetting WebSetting
        {
            get
            {
                return MyWebConfig.WebSetting;
            }
        }
        #endregion

        #endregion

        #region 构造
        public BaseController() : base()
        {

        }
        public BaseController(ILogger logger)
            : base()
        {
            _logger = logger;
        }
        #endregion

        #region 内部方法

        #region 输出

        #region IActionResult

        #region Text
        protected IActionResult GetTextResult(string format, params object[] args)
        {
            var mediaType = new MediaTypeHeaderValue("text/html");
            mediaType.Encoding = Encoding.UTF8;
            return new ContentResult() { Content = args.Length == 0 ? format : string.Format(format, args), ContentType = mediaType.ToString() };
        }
        #endregion

        #region Json
        protected IActionResult GetJsonResult<T>(T data, Func<string, string> filterJson = null)
        {
            var mediaType = new MediaTypeHeaderValue("application/json");
            mediaType.Encoding = Encoding.UTF8;
            var json = JsonHelper.ToJson(data);
            if (filterJson != null)
                json = filterJson(json);
            return new ContentResult() { Content = json, ContentType = mediaType.ToString() };
        }
        #endregion

        #region TreeResult
        protected IActionResult GetTree<T>(List<Tree<T>> treeDatas, string defaultText = "全选")
        {
            return this.GetJsonResult(new List<Tree<T>>()
            {
                new Tree<T>
                {
                    text = defaultText,
                    id = default(T),
                    expand = true,
                    children = treeDatas
                }
            }, json =>
            {
                return json.Replace("Checked", "checked");
            });
        }
        #endregion

        #region 输出结果
        protected IActionResult GetResult(bool succ = true, string message = "")
        {
            return this.GetJsonResult(new { succ = succ, data = message });
        }
        protected IActionResult GetResult<T>(bool succ, T data)
        {
            return this.GetJsonResult(new { succ = succ, data = data });
        }
        protected IActionResult GetResult(JsonData jsonData)
        {
            return this.GetJsonResult(jsonData);
        }
        protected IActionResult GetResult(Exception ex, bool showError = false)
        {
            string message;
            if (ex is InvalidOperationException || ex is InvalidException)
                message = ex.Message;
            //else if (ex is HttpRequestValidationException)
            //    message = "您输入的表单有危险字符，请检查后重试！\r\n危险字符：" + ex.Message;
            else
            {
                this.WriteError(ex);
                if (showError)
                    message = ex.Message;
                else
                    message = "意外错误，请稍后重试！";
            }
            return this.GetJsonResult(new { succ = false, data = message });
        }
        #endregion

        #endregion

        #endregion

        #region 日志
        protected void WriteDebug(string format, params object[] args)
        {
            this.Logger.LogDebug(format, args);
        }
        protected void WriteWarn(string format, params object[] args)
        {
            Logger.LogWarning("◆◆◆" + format, args);
        }
        protected void WriteLog(string format, params object[] args)
        {
            Logger.LogWarning(format, args);
        }
        protected void WriteError(Exception ex, string format, params object[] args)
        {
            Logger.LogError(new EventId(1), ex, format, args);
        }
        protected void WriteError(Exception ex)
        {
            Logger.LogError(new EventId(0), ex, "");
        }
        #endregion

        #region 缓存

        #region 获取Key
        protected string GetCacheKey(string key)
        {
            var prefix = $"{this.UserId}";
            if (!string.IsNullOrEmpty(prefix))
                key = $"{key}_{prefix}";
            return key;
        }
        #endregion

        #region 添加缓存
        protected void SetCache<T>(string key, T data, TimeSpan? expiration = null)
        {
            if (key.StartsWith("#"))
                key = key.Substring(1);
            else
                key = this.GetCacheKey(key);
            MyWebConfig.CacheService.Set(key, data, expiration);
        }
        protected void SetGlobalCache(string key, object data, TimeSpan? expiration = null)
        {
            MyWebConfig.CacheService.Set(key, data, expiration);
        }
        #endregion

        #region 获取缓存
        protected T GetGlobalCache<T>(string key)
        {
            return MyWebConfig.CacheService.Get<T>(key);
        }
        protected T GetCache<T>(string key)
        {
            if (key.StartsWith("#"))
                key = key.Substring(1);
            else
                key = this.GetCacheKey(key);
            return MyWebConfig.CacheService.Get<T>(key);
        }
        /// <summary>
        /// 获取缓存，如果缓存不存在则根据回调设置缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">存储缓存的键值（如果是全局缓存前面加#）</param>
        /// <param name="updateCallback">如果缓存不存在则根据回调设置缓存</param>
        /// <param name="absoluteExpiration">过期时间</param>
        /// <param name="slidingExpiration">最后一次访问所插入对象时与该对象到期时之间的时间间隔。如果该值等效于 20 分钟，则对象在最后一次被访问 20 分钟之后将到期并被从缓存中移除。如果使用可调到期，则absoluteExpiration 参数必须为 System.Web.Caching.Cache.NoAbsoluteExpiration。</param>
        /// <param name="cacheDependency">依赖关系</param>
        /// <returns></returns>
        protected T GetCache<T>(string key, Func<T> updateCallback, TimeSpan? expiration = null)
        {
            if (key.StartsWith("#"))
                key = key.Substring(1);
            else
                key = this.GetCacheKey(key);
            return MyWebConfig.CacheService.Get(key, updateCallback, expiration);
        }
        #endregion

        #region 移除缓存
        protected void RemoveCache(string key)
        {
            if (key.StartsWith("#"))
                key = key.Substring(1);
            else
                key = this.GetCacheKey(key);
            MyWebConfig.CacheService.Remove(key);
        }
        #endregion

        #endregion

        #region 生成中英文链接
        /// <summary>
        /// 根据指定链接获取带语言后缀的链接
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        protected string GetLink(string url)
        {
            //Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery()
            if (string.IsNullOrEmpty(url))
                return Request.GetPathAndQuery();
            var isCN = this.Lang == LangEnum.CN.ToString();
            if (!isCN)
            {
                if (url.ToLower().Contains("lang="))
                {
                    url = Regex.Replace(url, "lang=cn", "lang=en");
                }
                else if (url.Contains("#") && !url.EndsWith("#"))
                {
                    if (url.Contains("?"))
                        url = Regex.Replace(url, "#", "&lang=en#");
                    else
                        url = Regex.Replace(url, "#", "?lang=en#");
                }
                else
                {
                    url += (url.Contains("?") ? "&" : "?") + "lang=en";
                }
            }
            return url;
        }
        #endregion

        #region 获取语文字符
        protected string GetString(string cn, string en)
        {
            if (this.Lang == LangEnum.CN.ToString())
                return cn;
            return en;
        }
        protected T GetLang<T>(T cn, T en)
        {
            if (this.Lang == LangEnum.CN.ToString())
                return cn;
            return en;
        }
        #endregion

        #region 发送手机验证码
        /// <summary>
        /// 发送手机验证码
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="expireTime">过期时间</param>
        /// <param name="maxLength">验证码位数</param>
        protected JsonData SendSms(string mobile, int type, TimeSpan expireTime, int maxLength = 4)
        {
            if (string.IsNullOrEmpty(mobile))
                return new JsonData() { succ = false, data = "手机号不能为空！" };

            var smsSendExpire = Session["SmsSendExpire"];
            var lastSend = string.IsNullOrEmpty(smsSendExpire) ? DateTime.MinValue : new DateTime(long.Parse(smsSendExpire));
            if (DateTime.Now < lastSend)
            {
                var seconds = Convert.ToInt32((DateTime.Now - lastSend).TotalSeconds);
                return new JsonData() { succ = false, data = this.GetString($"{seconds}秒后可重新发送！", $"{seconds} seconds can be re-sent!") };
            }
            using (var db = this.GetMongodb())
            {
                var sms = db.Verifications.FirstOrDefault(a => a.TargetId == mobile && a.Type == type);
                if (sms == null)
                {
                    sms = new Verification()
                    {
                        TargetId = mobile,
                        Type = type
                    };
                }
                sms.Code = CommonHelper.GetNumberStr(4);
                sms.Expire = DateTime.Now.Add(expireTime);
                sms.LastModify = DateTime.Now;
                db.Verifications.Save(sms);
                Session["SmsSendExpire"] = DateTime.Now.AddSeconds(60).Ticks.ToString();
                return new JsonData() { succ = true, data = sms.Code };
            }
        }
        #endregion

        #region 获取唯一Id
        /// <summary>
        /// 获取唯一Id
        /// </summary>
        /// <returns></returns>
        protected string GetUniqueId()
        {
            return MongoDB.Bson.ObjectId.GenerateNewId().ToString();
        }
        #endregion

        #region 获取用户权限
        protected UserPermission GetUserPermission(AppUser user = null)
        {
            if (_userPermission == null)
            {
                if (user == null)
                    user = this.AppUser;
                //_userPermission = this.GetCache("Permissions", () =>
                //{
                _userPermission = new UserPermission();
                using (var db = this.GetMongodb())
                {
                    if (user.LoginId == "admin")
                    {
                        _userPermission.Permissions.AddRange(db.Permissions.Select(a => a.Id).ToArray());
                        _userPermission.Menus.AddRange(db.Menus.Select(a=>a.Id).ToArray());
                    }
                    else
                    {
                        var roles = db.Roles.ToList();
                        foreach (var roleId in user.Roles)
                        {
                            var role = roles.FirstOrDefault(a => a.Id == roleId);
                            if (role != null)
                            {
                                if (role.Menus != null && role.Menus.Count > 0)
                                    _userPermission.Menus.AddRange(role.Menus);
                                if (role.Permissions != null && role.Permissions.Count > 0)
                                    _userPermission.Permissions.AddRange(role.Permissions);
                            }
                        }
                    }
                }
                //return _userPermission;
                //}, TimeSpan.FromSeconds(WebSetting.DataCacheTime));
            }
            return _userPermission;
        }
        #endregion

        #region 获取用户菜单
        protected List<MenuJson> GetUserMenus(int menuType, Func<List<long>, Menu, bool> filter)
        {
            //获取用户菜单
            var userPermission = this.GetUserPermission();
            var userMenus = userPermission.Menus;
            var showMenus = new List<MenuJson>();
            using (var db = this.GetMongodb())
            {
                var allMenus = db.Menus.Where(a=>a.Type == menuType).ToList();
                var rootMenus = allMenus.Where(a => a.ParentId == 0).ToList();
                foreach (var menu in rootMenus)
                    FetchMenu(allMenus, menu, userMenus, showMenus, filter);
                return showMenus;
            }
        }
        private void FetchMenu(List<Menu> allMenus, Menu menu, List<long> userMenus, List<MenuJson> showMenus, Func<List<long>, Menu, bool> filter)
        {
            if (!filter(userMenus, menu)) return;
            var menuJson = new MenuJson(menu);
            showMenus.Add(menuJson);
            var children = allMenus.Where(a => a.ParentId == menu.Id).OrderBy(a => a.ByOrder).ToList();
            if (children.Count > 0)
            {
                menuJson.children = new List<MenuJson>();
                foreach (var cMenu in children)
                    FetchMenu(allMenus, cMenu, userMenus, menuJson.children, filter);
            }
        }
        #endregion

        #region 用户登录
        protected async void Login(AppUser user)
        {
            await HttpContext.SignInAsync(IdentityService.AuthenticationScheme, IdentityService.GetIdentityUser(user), new AuthenticationProperties()
            {
                AllowRefresh = true,
                ExpiresUtc = DateTime.Now.AddHours(24).ToUniversalTime()
            });

            //await HttpContext.Authentication.SignInAsync(IdentityService.AuthenticationScheme, IdentityService.GetIdentityUser(user), new Microsoft.AspNetCore.Http.Authentication.AuthenticationProperties()
            //{
            //    AllowRefresh = true,
            //    ExpiresUtc = DateTime.Now.AddHours(24).ToUniversalTime()
            //});
            this.SetCache($"#AppUser_{user.Id}", user, TimeSpan.FromHours(24));
        }
        #endregion

        #region MapPath
        protected string MapPath(string url)
        {
            return FormHelper.MapPath(url);
        }
        #endregion

        #endregion
    }

    #region JsonResult
    public class JsonData
    {
        public bool succ { get; set; }
        public dynamic data { get; set; }
    }
    #endregion
}
