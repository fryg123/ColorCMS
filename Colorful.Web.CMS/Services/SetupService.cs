using Colorful.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Colorful.Web.CMS
{
    public class SetupService : IDisposable
    {
        private MongodbContext _db;

        public SetupService()
        {
            _db = WebConfig.GetMongodb();
        }

        #region 初始化管理员账号
        /// <summary>
        /// 安装初始化服务
        /// </summary>
        /// <param name="password">管理员密码</param>
        /// <param name="title">网站标题</param>
        /// <param name="adminTitle">后台管理标题</param>
        /// <returns></returns>
        public SetupService Setup(string password, string title, string adminTitle)
        {
            if (!_db.Users.Any())
            {
                #region 初始化管理员
                var admin = new Administrator()
                {
                    LoginId = "admin",
                    ByOrder = _db.Users.GetId(),
                    Password = SecurityHelper.GetPassword("liguo1987"),
                    IP = HttpHelper.GetIP(),
                    Name = "超级管理员"
                };
                admin.NumberId = 1000000 + admin.ByOrder;
                _db.Users.Add(admin);
                #endregion
                #region 初始化WebSetting
                var webSet = new WebSetting()
                {
                    Id = 1,
                    Title = title,
                    Keyword = title,
                    Description = title,
                    Footer = "<p>2017 Copyright 上海七彩网络科技有限公司 All rights reserved</p>",
                    AdminTitle = adminTitle,
                    AdminSubTitle = adminTitle,
                    AdminBackupPath = $"backup_{CommonHelper.GetRandomStr(8)}",
                    AdminBackupFolders = new List<string>(new string[] { "upFiles", "dbbak" }),
                    LastModify = DateTime.Now,
                    AddDate = DateTime.Now,
                    DataCacheTime = 10,
                    BackupLimit = 10,
                    EmailSetting = new EmailSetting()
                };
                _db.WebSettings.Add(webSet);
                #endregion
                #region 初始化菜单
                var menu = new Menu()
                {
                    Id = _db.Menus.GetMaxId(),
                    LastModify = DateTime.Now,
                    ParentId = 0,
                    Type = 1,
                    Name = "系统设置",
                    Icon = "cogs",
                    ByOrder = 1
                };
                _db.Menus.Add(menu);
                menu = new Menu()
                {
                    Id = _db.Menus.GetMaxId(),
                    LastModify = DateTime.Now,
                    ParentId = 1,
                    Type = 1,
                    Name = "用户管理",
                    Icon = "group",
                    Url = "User",
                    ByOrder = 2
                };
                _db.Menus.Add(menu);
                menu = new Menu()
                {
                    Id = _db.Menus.GetMaxId(),
                    LastModify = DateTime.Now,
                    ParentId = 1,
                    Type = 1,
                    Name = "菜单管理",
                    Icon = "list",
                    Url = "Menu",
                    ByOrder = 3
                };
                _db.Menus.Add(menu);
                menu = new Menu()
                {
                    Id = _db.Menus.GetMaxId(),
                    LastModify = DateTime.Now,
                    ParentId = 1,
                    Type = 1,
                    Name = "权限管理",
                    Icon = "lock",
                    Url = "Role",
                    ByOrder = 4
                };
                _db.Menus.Add(menu);
                menu = new Menu()
                {
                    Id = _db.Menus.GetMaxId(),
                    LastModify = DateTime.Now,
                    ParentId = 0,
                    Type = 1,
                    Name = "网站设置",
                    Icon = "home",
                    Url = "",
                    ByOrder = 5
                };
                _db.Menus.Add(menu);
                menu = new Menu()
                {
                    Id = _db.Menus.GetMaxId(),
                    LastModify = DateTime.Now,
                    ParentId = 5,
                    Type = 1,
                    Name = "网站参数",
                    Icon = "cog",
                    Url = "WebSetting",
                    ByOrder = 6
                };
                _db.Menus.Add(menu);
                menu = new Menu()
                {
                    Id = _db.Menus.GetMaxId(),
                    LastModify = DateTime.Now,
                    ParentId = 5,
                    Type = 1,
                    Name = "邮件模版",
                    Icon = "list",
                    Url = "MailTemplate",
                    ByOrder = 7
                };
                _db.Menus.Add(menu);
                menu = new Menu()
                {
                    Id = _db.Menus.GetMaxId(),
                    LastModify = DateTime.Now,
                    ParentId = 5,
                    Type = 1,
                    Name = "网站备份",
                    Icon = "database",
                    Url = "Backup",
                    ByOrder = 8
                };
                _db.Menus.Add(menu);
                #endregion
                #region 初始化权限
                var permission = new Permission()
                {
                    Id = 1,
                    AddDate = DateTime.Now,
                    LastModify = DateTime.Now,
                    Name = "后台管理"
                };
                _db.Permissions.Add(permission);
                permission = new Permission()
                {
                    Id = 2,
                    AddDate = DateTime.Now,
                    LastModify = DateTime.Now,
                    Name = "编辑权限"
                };
                _db.Permissions.Add(permission);
                permission = new Permission()
                {
                    Id = 3,
                    AddDate = DateTime.Now,
                    LastModify = DateTime.Now,
                    Name = "删除权限"
                };
                _db.Permissions.Add(permission);
                permission = new Permission()
                {
                    Id = 4,
                    AddDate = DateTime.Now,
                    LastModify = DateTime.Now,
                    Name = "网站设置"
                };
                _db.Permissions.Add(permission);
                permission = new Permission()
                {
                    Id = 5,
                    AddDate = DateTime.Now,
                    LastModify = DateTime.Now,
                    Name = "用户管理"
                };
                _db.Permissions.Add(permission);
                permission = new Permission()
                {
                    Id = 6,
                    AddDate = DateTime.Now,
                    LastModify = DateTime.Now,
                    Name = "审核权限"
                };
                _db.Permissions.Add(permission);
                permission = new Permission()
                {
                    Id = 7,
                    AddDate = DateTime.Now,
                    LastModify = DateTime.Now,
                    Name = "系统设置"
                };
                _db.Permissions.Add(permission);
                #endregion
                #region 初始化角色
                var role = new Role()
                {
                    Id = _db.Roles.GetMaxId(),
                    Name = "管理员",
                    AddDate = DateTime.Now,
                    DefaultMenu = null,
                    LastModify = DateTime.Now,
                    Permissions = new List<long>(new long[] { 1 }),
                    Menus = new List<long>()
                };
                _db.Roles.Add(role);
                #endregion
            }
            return this;
        }
        #endregion

        #region 初始化缓存
        public SetupService InitCache()
        {
            //初始化WebSetting
            WebConfig.WebSetting = _db.WebSettings.OrderBy(a => a.Id).FirstOrDefault();
            if (WebConfig.WebSetting == null)
                WebConfig.WebSetting = new WebSetting();
            //初始化SysSetting
            WebConfig.SysSetting = _db.SysSettings.FirstOrDefault();
            if (WebConfig.SysSetting == null)
                WebConfig.SysSetting = new SysSetting()
                {
                    OnlineTargets = new List<string>()
                };
            return this;
        }

        public void Dispose()
        {
            _db.Dispose();
        }
        #endregion
    }
}
