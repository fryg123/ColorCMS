using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Driver;
using Colorful.Models;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Colorful.Web.CMS.Controllers.Admin
{
    [MyAuth(PermissionEnum.SysSetting)]
    public class MenuController : AdminBaseController
    {
        #region 菜单

        #region 首页
        [Cache(10)]
        public IActionResult Index(long mid)
        {
            if (!this.HasMenu(mid))
                return this.GetResult(false, "无菜单访问权限！");
            var model = new MenuModel()
            {
                MenuTreeUrl = this.GetUrl("menus/list"),
                MenuModeuleUrl = this.GetUrl("menus/module"),
                InfoUrl = this.GetUrl("menus/info"),
                OrderUrl = this.GetUrl("menus/order"),
                DelUrl = this.GetUrl("menus/del"),
                GetFieldsUrl = this.GetUrl("menus/getfields"),
                FieldTypes = EnumHelper.ToList<MenuFieldType>().Select(a => new JsonData<string>() { id = a.ToString(), text = a.GetDescription() }).ToList(),
                MenuTypes = EnumHelper.ToJsonData<MenuType>()
            };
            ViewBag.SubmitUrl = this.GetUrl("menus/save");

            using (var db = this.GetMongodb())
            {
                #region 当前用户菜单列表
                var menus = this.AppUser.Menus;
                model.Menus = db.Menus.Where(a => menus.Contains(a.Id)).ToList();
                #endregion

                #region 加载MenuFlags
                model.Flags = EnumHelper.ToList<MenuFlag>().Select(a => new JsonData<int>() { id = (int)a, text = a.GetDescription() }).ToList();
                #endregion

                #region 加载常用模块
                model.Modules = db.MenuModules.ToList();
                if (model.Modules.Count == 0)
                {
                    model.Modules.Add(new MenuModule()
                    {
                        Id = 1,
                        Name = "文章模块",
                        Url = "Article",
                        Model = "Article"
                    });
                    model.Modules.Add(new MenuModule()
                    {
                        Id = 2,
                        Name = "图文链接",
                        Url = "Link",
                        Model = "Link"
                    });
                    model.Modules.Add(new MenuModule()
                    {
                        Id = 3,
                        Name = "数据字典",
                        Url = "Code",
                        Model = "Code"
                    });
                    db.MenuModules.AddMany(model.Modules);
                }
                #endregion
            }
            return View(model);
        }
        #endregion

        #region 获取菜单信息
        [Route("menus/info")]
        public IActionResult GetMenuInfo(long id)
        {
            using (var db = this.GetMongodb())
            {
                var menu = db.Menus.FirstOrDefault(a => a.Id == id);
                return this.GetJsonResult(menu);
            }
        }
        #endregion

        #region 菜单列表
        [Route("menus/list")]
        public IActionResult GetMenuList(int type, bool? open)
        {
            if (type == 0) type = 1;
            return this.GetMyMenus(open, type);
        }
        #endregion

        #region 保存菜单
        [Route("menus/save")]
        public IActionResult SaveMenu(long id, string config)
        {
            using (var db = this.GetMongodb())
            {
                try
                {
                    Menu menu;
                    if (id > 0)
                    {
                        menu = db.Menus.FirstOrDefault(a => a.Id == id);
                    }
                    else
                    {
                        menu = new Menu();
                        menu.Id = db.Menus.GetMaxId();
                        menu.ByOrder = menu.Id;
                    }
                    menu.LastModify = DateTime.Now;
                    menu.ModifyUser = this.LoginId;
                    FormHelper.FillTo(menu, new DisableField("Id"), new RequireField("Name", "菜单名称"), new DisableField("Config"));
                    if (!string.IsNullOrEmpty(config))
                        menu.Config = JsonHelper.Parse<MenuConfig>(config);
                    else
                        menu.Config = null;
                    db.Menus.Save(menu);
                    return this.GetResult(true, menu);
                }
                catch (Exception ex)
                {
                    return this.GetResult(ex);
                }
            }
        }
        #endregion

        #region 删除菜单
        [Route("menus/del")]
        public IActionResult DelMenu(long id)
        {
            using (var db = this.GetMongodb())
            {
                var menus = this.Menus;
                db.Trashs.Add(new Trash()
                {
                    LastModify = DateTime.Now,
                    ModifyUser = this.LoginId,
                    Type = (int)TrashType.Menu,
                    TargetId = id.ToString(),
                    Content = JsonHelper.ToJson(menus)
                });
                List<long> ids = new List<long>();
                GetMenuDels(menus, id, ref ids);
                var result = db.Menus.Delete(a => ids.Contains(a.Id));
                return this.GetResult(result.DeletedCount > 0);
            }
        }
        private void GetMenuDels(List<Menu> menus, long id, ref List<long> ids)
        {
            ids.Add(id);
            var children = menus.Where(a => a.ParentId == id).ToList();
            foreach (var c in children)
            {
                GetMenuDels(menus, c.Id, ref ids);
            }
        }
        #endregion

        #region 菜单排序
        [Route("menus/order")]
        public IActionResult OrderMenu(long[] ids)
        {
            using (var db = this.GetMongodb())
            {
                for (var i = 0; i < ids.Length; i++)
                {
                    db.Menus.UpdateByPK(ids[i], b => b.ByOrder, i);
                }
                return this.GetResult(true);
            }
        }
        #endregion

        #region 获取字段列表
        [Route("menus/getfields")]
        public IActionResult GetFields(string model)
        {
            var assembly = typeof(WebSetting).GetTypeInfo().Assembly;
            var target = assembly.CreateInstance($"Colorful.Models.{model}");
            var fields = target.GetType().GetProperties();
            var list = new List<BindField>();
            foreach (var field in fields)
            {
                if (!field.CanWrite) continue;
                var bindField = field.GetCustomAttribute<BindField>();
                if (bindField != null)
                {
                    bindField.Id = field.Name;
                    if (bindField.Id == "AddDate")
                        bindField.Type = MenuFieldType.DateTime;
                    else if (field.PropertyType.Name == "DateTime")
                        bindField.Type = MenuFieldType.Date;
                    list.Add(bindField);
                }
            }
            return this.GetResult(true, list.Select(a => new { id = a.Id, text = a.Name, type = a.Type.ToString() }).ToList());
        }
        #endregion

        #endregion

        #region 菜单模块

        #region 首页
        [Route("menus/module")]
        public IActionResult Module(string t, long mid)
        {
            ViewBag.DelUrl = this.GetUrl("menus/module/del");
            ViewBag.DataUrl = this.GetUrl("menus/module/list");
            ViewBag.SaveUrl = this.GetUrl("menus/module/save");
            ViewData["Flags"] = EnumHelper.ToList<MenuFlag>().Select(a => new JsonData<int>() { id = (int)a, text = a.GetDescription() }).ToList();
            return View();
        }
        #endregion

        #region 获取列表
        [Route("menus/module/list")]
        public IActionResult ModuleList(string keyword)
        {
            using (var db = this.GetMongodb())
            {
                var q = db.MenuModules.GetQuery();
                if (!string.IsNullOrEmpty(keyword))
                    q = q.Where(a => a.Name.Contains(keyword));
                return this.GetDataGrid(q);
            }
        }
        #endregion

        #region 保存
        [Route("menus/module/save")]
        public IActionResult ModuleSave(long id)
        {
            try
            {
                using (var db = this.GetMongodb())
                {
                    MenuModule module;
                    if (id == 0)
                        module = new MenuModule()
                        {
                            Id = db.MenuModules.GetMaxId()
                        };
                    else
                    {
                        module = db.MenuModules.FirstOrDefault(a => a.Id == id);
                    }
                    FormHelper.FillTo(module, new DisableField("Id"));
                    db.MenuModules.Save(module);
                    return this.GetResult(true);
                }
            }
            catch (Exception ex)
            {
                return this.GetResult(ex);
            }
        }
        #endregion

        #region 删除
        [Route("menus/module/del")]
        public IActionResult ModuleDel(long[] ids)
        {
            using (var db = this.GetMongodb())
            {
                db.MenuModules.Delete(a => ids.Contains(a.Id));
                return this.GetResult(true);
            }
        }
        #endregion

        #endregion
    }

    #region MenuModel
    public class MenuModel
    {
        /// <summary>
        /// 菜单Tree
        /// </summary>
        public string MenuTreeUrl { get; set; }
        /// <summary>
        /// 菜单模块配置Url
        /// </summary>
        public string MenuModeuleUrl { get; set; }
        /// <summary>
        /// 获取菜单JsonUrl
        /// </summary>
        public string InfoUrl { get; set; }
        /// <summary>
        /// 排序Url
        /// </summary>
        public string OrderUrl { get; set; }
        /// <summary>
        /// 删除
        /// </summary>
        public string DelUrl { get; set; }
        /// <summary>
        /// 获取字段列表
        /// </summary>
        public string GetFieldsUrl { get; set; }
        /// <summary>
        /// 菜单标识
        /// </summary>
        public List<JsonData<int>> Flags { get; set; }
        /// <summary>
        /// 当前菜单列表
        /// </summary>
        public List<Menu> Menus { get; set; }
        /// <summary>
        /// 常用模块
        /// </summary>
        public List<MenuModule> Modules { get; set; }
        /// <summary>
        /// 字段类型
        /// </summary>
        public List<JsonData<string>> FieldTypes { get; set; }
        /// <summary>
        /// 菜单类型枚举
        /// </summary>
        public List<JsonData<int>> MenuTypes { get; set; }
    }
    #endregion
}