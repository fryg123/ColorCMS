using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using System.Text;
using Colorful.Models;
using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Colorful.Web.CMS.Controllers.Admin
{
    public partial class LinkController : AdminBaseController
    {
        #region 初始化
        [Cache(10)]
        public IActionResult Index(long mid)
        {
            if (!HasMenu(mid))
                return this.GetResult(false, "无访问权限！");
            var menu = this.GetMenu(mid);
            var model = new LinkModel()
            {
                Menu = menu,
                NoDelete = menu.HasFlag(MenuFlag.NoDelete),
                IsAdmin = this.IsAdmin
            };
            
            ViewBag.DataUrl = this.GetUrl("link/list");
            ViewBag.SubmitUrl = this.GetUrl("link/save");
            ViewBag.DelUrl = this.GetUrl("link/del");
            ViewBag.SortUrl = this.GetUrl("link/sort");
            ViewBag.InfoUrl = this.GetUrl("link/info");

            #region 处理数据源
            if (menu.Config != null && menu.Config.Fields.Count > 0)
            {
                this.SetDataSource(menu.Config.Fields);
            }
            #endregion

            return View(model);
        }
        #endregion

        #region 根据Id获取Link信息
        [Route("link/info")]
        public IActionResult GetLinkInfo(int id)
        {
            using (var db = this.GetMongodb())
            {
                var link = db.Links.FirstOrDefault(a => a.Id == id);
                if (!HasMenu(link.MenuId))
                    return this.GetResult(false, "无访问权限！");
                return this.GetResult(true, link);
            }
        }
        #endregion

        #region 获取列表
        [Route("link/list")]
        public IActionResult GetList(long menuId, int sortId, string lang)
        {
            using (var db = this.GetMongodb())
            {
                var query = db.Links.Where(a => a.MenuId == menuId);
                if (sortId > 0)
                    query = query.Where(a => a.SortId == sortId);
                if (!string.IsNullOrEmpty(lang))
                    query = query.Where(a => a.Lang == lang);
                var datalist =  query.OrderByDescending(a => a.ByOrder).Select(a => new { a.Id, a.Photo, a.Title, a.Url, a.File }).ToList();
                return this.GetJsonResult(datalist);
            }
        }
        #endregion

        #region 排序
        [Route("link/sort")]
        public IActionResult LinksSort(long menuId, int[] ids)
        {
            if (!this.HasPermission(PermissionEnum.Edit))
                return this.GetResult(new Exception("无操作权限！"));
            if (!HasMenu(menuId))
                return this.GetResult(false, "无访问权限！");
            using (var db = this.GetMongodb())
            {
                for (var i = 0; i < ids.Length; i++)
                {
                    db.Links.UpdateByPK(ids[i], b => b.ByOrder, ids.Length - i);
                }
                return this.GetResult(true);
            }
        }
        #endregion

        #region 保存
        [Route("link/save")]
        public IActionResult LinksSave(int id, long menuId, string videoSource)
        {
            if (!this.HasPermission(PermissionEnum.Edit))
                return this.GetResult(new Exception("无操作权限！"));
            var menu = this.GetMenu(menuId);
            if (menu == null)
                return this.GetResult(false, "无访问权限！");
            try
            {
                using (var db = this.GetMongodb())
                {
                    ActionHistory actionHistory = null;
                    Link link;
                    if (id > 0)
                    {
                        link = db.Links.FirstOrDefault(a => a.Id == id && a.MenuId == menuId);
                        #region 记录修改历史
                        if (menu.HasFlag(MenuFlag.RecordHistory))
                        {
                            actionHistory = new ActionHistory()
                            {
                                TargetId = link.Id.ToString(),
                                LastModify = DateTime.Now,
                                ModifyUser = this.LoginId,
                                IP = HttpHelper.GetIP(),
                                Type = ActionType.Article,
                                MenuId = link.MenuId,
                                SortId = link.SortId,
                                Before = JsonHelper.ToJson(link)
                            };
                        }
                        #endregion
                    }
                    else
                    {
                        link = new Link()
                        {
                            Id = (int)db.Links.GetMaxId(),
                            MenuId = menuId
                        };
                        link.ByOrder = link.Id;
                    }

                    var fields = new List<FormField>();
                    fields.Add(new TextField("MenuId"));
                    fields.Add(new TextField("SortId"));
                    #region 解析菜单设置
                    var config = menu.Config;
                    if (config != null && config.Fields.Count > 0)
                    {
                        foreach (var item in config.Fields)
                        {
                            var fieldType = Enum.Parse<MenuFieldType>(item.Type);
                            if (fieldType == MenuFieldType.Image)
                            {
                                var imageField = new ImageField(item.Name) { Text = item.Title, SavePath = $"/upFiles/link/{link.Id}/{item.Name}", MaxLength = item.MaxLength > 0 ? item.MaxLength * 1024 : 2048, Required = item.Required };
                                if (item.MaxSize != null && item.MaxSize.Width > 0 && item.MaxSize.Height > 0)
                                {
                                    imageField.Compress = true;
                                    imageField.CompressSize = new System.Drawing.Size(item.MaxSize.Width, item.MaxSize.Height.GetValueOrDefault());
                                }
                                fields.Add(imageField);
                            }
                            else if (fieldType == MenuFieldType.Video)
                            {
                                if (videoSource == "1")
                                    fields.Add(new FormField() { Name = item.Name, Text = item.Title, Required = item.Required });
                                else
                                {
                                    fields.Add(new VideoField(item.Name) { Text = item.Title, SavePath = $"/upFiles/link/{link.Id}/{item.Name}", MaxLength = item.MaxLength > 0 ? item.MaxLength * 1024 : 51200, Required = item.Required });
                                }
                            }
                            else if (fieldType == MenuFieldType.File)
                            {
                                fields.Add(new FileField(item.Name) { Text = item.Title, SavePath = $"/upFiles/link/{link.Id}/{item.Name}", MaxLength = item.MaxLength > 0 ? item.MaxLength * 1024 : 20480, Required = item.Required });
                            }
                            else if (fieldType == MenuFieldType.Editor || fieldType == MenuFieldType.SmallEditor)
                            {
                                fields.Add(new EditorField(item.Name) { Text = item.Title, Required = item.Name == "Content" ? true : item.Required, XXSFilter = false });
                            }
                            else
                            {
                                fields.Add(new TextField(item.Name) { Text = item.Title });
                            }
                        }
                    }
                    #endregion

                    FormHelper.SafeFill(link, fields.ToArray());

                    link.LastModify = DateTime.Now;
                    link.ModifyUser = this.LoginId;
                    link.Lang = this.Lang;
                    #region 记录更新历史
                    if (actionHistory != null)
                    {
                        actionHistory.After = JsonHelper.ToJson(link);
                        db.ActionHistories.Add(actionHistory);
                    }
                    #endregion
                    db.Links.Save(link);
                    CheckCache(menuId);
                    if (id > 0)
                        return this.GetResult(true, new { link.Title, link.Photo, link.File, link.ByOrder, link.Url });
                    else
                        return this.GetResult(true, new { link.Id, link.Title, link.Photo, link.File, link.ByOrder, link.Url });
                }
            }
            catch (Exception ex)
            {
                return this.GetResult(ex);
            }
        }
        #endregion

        #region Partial Methods
        partial void CheckCache(long menuid);
        #endregion

        #region 删除
        [Route("link/del")]
        public IActionResult LinksDel(int id, long menuId)
        {
            if (!this.HasPermission(PermissionEnum.Delete))
                return this.GetResult(new Exception("无操作权限！"));
            if (!HasMenu(menuId))
                return this.GetResult(false, "无访问权限！");
            var menu = this.GetMenu(menuId);
            using (var db = this.GetMongodb())
            {
                try
                {
                    var link = db.Links.FirstOrDefault(a => a.Id == id);
                    #region 删除保护
                    if (menu.HasFlag(MenuFlag.DeleteProtect))
                    {
                        db.Trashs.Add(new Trash()
                        {
                            TargetId = id.ToString(),
                            Type = (int)TrashType.Link,
                            SortId = menuId,
                            Content = JsonHelper.ToJson(link),
                            AddDate = DateTime.Now,
                            LastModify = DateTime.Now,
                            ModifyUser = this.LoginId
                        });
                    }else
                    {
                        FormHelper.DeleteDirectory($"/upFiles/link/{link.Id}");
                    }
                    #endregion
                    db.Links.Delete(a => a.MenuId == menuId && a.Id == id);
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

    #region LinkModel
    public class LinkModel
    {
        public bool IsAdmin { get; set; }
        /// <summary>
        /// 是否显示特殊标记
        /// </summary>
        public bool NoDelete { get; set; }
        public Menu Menu { get; set; }
    }
    #endregion
}