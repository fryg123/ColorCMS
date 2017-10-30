using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Driver;
using Colorful.Models;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Colorful.Web.CMS.Controllers.Admin
{
    /// <summary>
    /// 网站设置
    /// </summary>
    [MyAuth(PermissionEnum.WebSetting)]
    public class WebSettingController : AdminBaseController
    {
        #region 首页
        [Cache(10)]
        public IActionResult Index(long mid)
        {
            using (var db = this.GetMongodb())
            {
                var menuName = this.GetMenu(mid).Name;
                ViewBag.MenuName = menuName;

                ViewBag.WebSetting = this.WebSetting;
                ViewBag.SubmitUrl = this.GetUrl("websetting/save");
                ViewBag.ClearCacheUrl = this.GetUrl("clearcache");
                ViewBag.IsAdmin = this.IsAdmin;
                ViewData["Title"] = menuName;
                return View();
            }
        }
        #endregion

        #region 保存
        [Route("websetting/save")]
        public IActionResult SettingSave()
        {
            using (var db = this.GetMongodb())
            {
                try
                {
                    var setting = db.WebSettings.FirstOrDefault();
                    if (setting == null)
                    {
                        setting = new WebSetting();
                        setting.Id = 1;
                        setting.AddDate = DateTime.Now;
                    }
                    setting.ModifyUser = this.LoginId;
                    setting.LastModify = DateTime.Now;
                    FormHelper.FillTo(setting, new DisableField("Id"));
                    db.WebSettings.Save(setting);
                    MyWebConfig.WebSetting = setting;
                    return this.GetResult(true);
                }
                catch (Exception ex)
                {
                    return this.GetResult(ex);
                }
            }
        }
        #endregion
    }
}