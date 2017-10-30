using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using System.Text;
using Colorful.Models;
using Microsoft.AspNetCore.Mvc;

namespace Colorful.Web.CMS.Controllers.Admin
{
    #region UserController

    [MyAuth(PermissionEnum.UserAdmin)]
    public partial class UserController : AdminBaseController
    {
        #region 首页
        [Cache(10)]
        public IActionResult Index(long mid)
        {
            var model = new AdminUserModel();
            ViewBag.DelUrl = this.GetUrl("user/del");
            ViewBag.DataUrl = this.GetUrl("user/list");
            ViewBag.SubmitUrl = this.GetUrl("user/save");
            ViewBag.InfoUrl = this.GetUrl("user/info");
            ViewBag.MenuId = mid;

            model.IsAdmin = this.IsAdmin;
            using (var db = this.GetMongodb())
            {
                var menus = this.Menus;
                if (this.IsAdmin)
                    model.Roles = db.Roles.ToList();
                else
                    model.Roles = db.Roles.Where(a => this.AppUser.Roles.Contains(a.Id)).ToList();
                model.Roles.ForEach(role =>
                {
                    role.MenuList = menus.Where(a => role.Menus.Contains(a.Id)).OrderBy(a => a.ByOrder).ToList();
                });
            }

            return View(model);
        }
        #endregion

        #region 用户列表
        [Route("user/list")]
        public IActionResult UserList(string keyword, string startDate, string endDate)
        {
            using (var db = this.GetMongodb())
            {
                var query = db.Users.GetQuery();
                if (!string.IsNullOrEmpty(keyword))
                    query = query.Where(a => a.Name.Contains(keyword) || a.LoginId.Contains(keyword));
                if (!string.IsNullOrEmpty(startDate))
                {
                    var sdate = (startDate.ParseDate() ?? DateTime.Now).MinDate();
                    var edate = (endDate.ParseDate() ?? DateTime.Now).MaxDate();
                    query = query.Where(a => a.AddDate >= sdate && a.AddDate <= edate);
                }
                if (this.LoginId != "admin")
                    query = query.Where(a => a.LoginId != "admin");
                return this.GetDataGrid(query.OrderByDescending(a => a.AddDate));
            }
        }
        #endregion

        #region 保存用户信息
        [Route("user/save")]
        public IActionResult SaveUser(string id, string password, string loginId, long menuId)
        {
            if (id == "0") id = null;
            using (var db = this.GetMongodb())
            {
                try
                {
                    var menu = this.GetMenu(menuId);
                    if (menu == null)
                        return this.GetResult(false, "无访问权限！");
                    if (string.IsNullOrEmpty(loginId))
                        return this.GetResult(false, "登陆Id不能为空！");
                    ActionHistory actionHistory = null;
                    var isNew = string.IsNullOrEmpty(id);
                    if (isNew)
                    {
                        if (db.Users.Any(a => a.LoginId == loginId))
                        {
                            return this.GetResult(false, $"登陆Id【{loginId}】已存在！");
                        }
                        if (string.IsNullOrEmpty(password) || password.Length < 6)
                            return this.GetResult(false, "登陆密码至少设置6位！");
                    }
                    Administrator administrator;
                    if (isNew)
                    {
                        administrator = new Administrator();
                    }
                    else
                    {
                        administrator = db.Users.FirstOrDefault(a => a.Id == id);
                        #region 记录修改历史
                        if (menu.HasFlag(MenuFlag.RecordHistory))
                        {
                            actionHistory = new ActionHistory()
                            {
                                TargetId = administrator.Id,
                                LastModify = DateTime.Now,
                                ModifyUser = this.LoginId,
                                IP = HttpHelper.GetIP(),
                                Type = ActionType.Article,
                                MenuId = menu.Id,
                                Before = JsonHelper.ToJson(administrator)
                            };
                        }
                        #endregion
                    }
                    FormHelper.FillTo(administrator, new FormField() { Name = "Id", Disabled = true },
                        new FormField() { Name = "Password", Disabled = true },
                        new FormField() { Name = "LoginId", Text = "登陆Id", Required = true });

                    if (!string.IsNullOrEmpty(password))
                        administrator.Password = SecurityHelper.GetPassword(password);
                    administrator.LastModify = DateTime.Now;
                    administrator.ModifyUser = this.LoginId;
                    #region 记录更新历史
                    if (actionHistory != null)
                    {
                        actionHistory.After = JsonHelper.ToJson(administrator);
                        db.ActionHistories.Add(actionHistory);
                    }
                    #endregion
                    var result = db.Users.Save(administrator);
                    return this.GetResult(true);
                }
                catch (Exception ex)
                {
                    return this.GetResult(ex);
                }
            }
        }
        #endregion

        #region 获取用户信息
        [Route("user/info")]
        public IActionResult GetUser(string id)
        {
            using (var db = this.GetMongodb())
            {
                var user = db.Users.FirstOrDefault(a => a.Id == id);
                return this.GetResult(true, user);
            }
        }
        #endregion

        #region 删除用户
        [Route("user/del")]
        public IActionResult DelUsers(string[] ids, long menuId)
        {
            if (!this.HasPermission(PermissionEnum.Delete))
                return this.GetResult(new Exception("无操作权限！"));
            var menu = this.GetMenu(menuId);
            if (menu == null)
                return this.GetResult(false, "无访问权限！");
            using (var db = this.GetMongodb())
            {
                #region 删除保护
                var files = new List<string>();
                if (menu.HasFlag(MenuFlag.DeleteProtect))
                {
                    foreach (var id in ids)
                    {
                        var user = db.Users.FirstOrDefault(a => a.Id == id);
                        if (!string.IsNullOrEmpty(user.Photo) && !user.Photo.EndsWith("default.png"))
                            files.Add(user.Photo);
                        db.Trashs.Add(new Trash()
                        {
                            TargetId = id.ToString(),
                            Type = (int)TrashType.User,
                            SortId = menu.Id,
                            Content = JsonHelper.ToJson(user),
                            AddDate = DateTime.Now,
                            LastModify = DateTime.Now,
                            Files = files,
                            ModifyUser = this.LoginId
                        });
                    }
                }
                else
                {
                    FormHelper.DeleteFiles(files.ToArray());
                }
                #endregion
                var result = db.Users.Delete(a => a.LoginId != "admin" && a.LoginId != this.LoginId && ids.Contains(a.Id));
                return this.GetResult(result.DeletedCount > 0);
            }
        }
        #endregion
    }

    #region AdminUserModel
    public class AdminUserModel
    {
        public bool IsAdmin { get; set; }
        public List<Role> Roles { get; set; }
    }
    #endregion

    #endregion

    #region ProfileController
    public class ProfileController: AdminBaseController
    {
        #region 首页
        [Cache(10)]
        public IActionResult Index()
        {
            ViewBag.SubmitUrl = this.GetUrl("profile/save");
            var user = this.AppUser;
            return View(user);
        }
        #endregion

        #region 更新信息
        [Route("profile/save")]
        public IActionResult SaveProfile(string nickname, string password)
        {
            using (var db = this.GetMongodb())
            {
                try
                {
                    var pass = SecurityHelper.GetPassword(password);
                    db.Users.Update(a => a.Id == this.UserId, b => b.Password, pass, c => c.Name, nickname);
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
    #endregion
}