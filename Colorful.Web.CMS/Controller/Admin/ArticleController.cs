using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Driver;
using System.Text;
using Colorful.Models;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Reflection;

namespace Colorful.Web.CMS.Controllers.Admin
{
    public class ArticleController : AdminBaseController
    {
        #region 文章首页
        [Cache(10)]
        public IActionResult Index(long mid, int? sortId, string lang)
        {
            if (!this.HasMenu(mid))
                return this.GetTextResult("无菜单访问权限！");

            var menu = this.GetMenu(mid);
            var auditStatus = EnumHelper.ToList<AuditingStatus>().Where(a => a > 0).Select(a => new JsonData<int>() { id = (int)a, text = a.GetDescription() }).Where(a => a.id > 0).ToList();
            var model = new ArticleModel()
            {
                Title = menu.Name,
                MenuName = menu.Name.Replace("管理", "").Replace("列表", ""),
                Menu = menu,
                MenuId = mid,
                SortId = sortId.GetValueOrDefault(),
                ShowEdit = this.HasPermission(PermissionEnum.Edit),
                ShowAudit = menu.HasFlag(MenuFlag.Audit) && HasPermission(PermissionEnum.Audit),
                InfoUrl = this.GetUrl("article/info"),
                DataUrl = this.GetUrl("article/data"),
                DelUrl = this.GetUrl("article/del"),
                SubmitUrl = this.GetUrl("article/save"),
                AuditUrl = this.GetUrl("article/audit"),
                OrderUrl = this.GetUrl("article/order"),
                Flags = EnumHelper.ToList<ArticleFlag>().Select(a => new JsonData<int>() { id = (int)a, text = a.GetDescription() }).ToList(),
                Languages = this.Languages,
                AuditStatus = auditStatus
            };

            #region 处理数据源
            if (menu.Config != null && menu.Config.Fields.Count > 0)
            {
                this.SetDataSource(menu.Config.Fields);
            }
            #endregion

            model.ShowNew = model.ShowEdit;
            using (var db = this.GetMongodb())
            {
                model.Tags = db.Labels.Where(a => a.MenuId == mid && a.Sort == model.SortId).Select(a => a.Name).ToList();
                #region 处理单篇文章
                if (model.IsArticle)
                {
                    if (string.IsNullOrEmpty(lang))
                        lang = this.Languages[0].id;
                    else
                        lang = this.Languages.FirstOrDefault(a => a.id == lang.ToUpper()).id;
                    var defaultSortId = sortId.GetValueOrDefault();
                    model.ArticleId = db.Articles.Where(a => a.MenuId == mid && a.SortId == defaultSortId && a.Lang == lang).Select(a => a.Id).FirstOrDefault();
                }
                #endregion
            }
            return View(model);
        }
        #endregion

        #region 文章信息
        [Route("article/info")]
        public IActionResult GetArticle(string id, string lang, long? mid, int? sortId)
        {
            using (var db = this.GetMongodb())
            {
                var defaultSortId = sortId.GetValueOrDefault();
                if (!string.IsNullOrEmpty(id))
                {
                    var article = db.Articles.FirstOrDefault(a => a.Id == id);
                    if (!HasMenu(article.MenuId))
                        return this.GetResult(false, "无访问权限！");
                    return this.GetJsonResult(article);
                }
                else
                {
                    if (!HasMenu(mid ?? 0))
                        return this.GetResult(false, "无访问权限！");
                    var data = db.Articles.FirstOrDefault(a => a.MenuId == mid && a.SortId == defaultSortId && a.Lang == lang);
                    if (data == null)
                        data = new Article() { Id = "", Content = "", MenuId = mid.GetValueOrDefault(), SortId = sortId.GetValueOrDefault() };
                    return this.GetJsonResult(data);
                }
            }
        }
        #endregion

        #region 获取文章列表
        [Route("article/data")]
        public IActionResult GetArticleList(int? status, long menuId, int sortId, string keyword, string startDate, string endDate)
        {
            if (!HasMenu(menuId))
                return this.GetResult(false, "无访问权限！");
            using (var db = this.GetMongodb())
            {
                var query = db.Articles.Where(a => a.MenuId == menuId);
                if (sortId > 0)
                    query = query.Where(a => a.SortId == sortId);
                if (!string.IsNullOrEmpty(keyword))
                    query = query.Where(a => a.Title.Contains(keyword));
                if (!string.IsNullOrEmpty(startDate))
                {
                    var sdate = (startDate.ParseDate() ?? DateTime.Now).MinDate();
                    var edate = (endDate.ParseDate() ?? DateTime.Now).MaxDate();
                    query = query.Where(a => a.AddDate >= sdate && a.AddDate <= edate);
                }
                #region 其他查询选项
                if (status > 0)
                {
                    if (status > 1)
                        query = query.Where(a => a.Status == status);
                    else
                        query = query.Where(a => a.Status == status || a.Status == 0);
                }
                #endregion
                return this.GetDataGrid(query.OrderByDescending(a => a.ByOrder).Select(a => new { a.Id, a.Title, a.Status, a.LastModify, a.AddDate, a.Publish, a.ByOrder }));
            }
        }
        #endregion

        #region 添加/编辑文章
        [Route("article/save")]
        public IActionResult AddNews(string id, long menuId, int sortId, string videoSource)
        {
            //权限
            if (!this.HasPermission(PermissionEnum.Edit))
                return this.GetResult(false, "无操作权限！");
            var menu = this.GetMenu(menuId);
            if (menu == null)
                return this.GetResult(false, "无访问权限！");
            //是否为单篇文章
            var isArticle = menu.Config == null || menu.Config.Fields == null || menu.Config.Fields.Count == 0;
            using (var db = this.GetMongodb())
            {
                try
                {
                    ActionHistory actionHistory = null;
                    Article article = null;
                    var isNew = string.IsNullOrEmpty(id);
                    if (isNew)
                    {
                        var lang = $"{Request.Form["Lang"]}".ToUpper();
                        if (string.IsNullOrEmpty(lang))
                            lang = this.Languages[0].id;
                        if (isArticle)
                            article = db.Articles.FirstOrDefault(a => a.MenuId == menuId && a.SortId == sortId && a.Lang == lang);
                        isNew = article == null;
                        if (isNew)
                        {
                            article = new Article();
                            article.NumberId = db.Articles.GetId() + 100000;
                            article.ByOrder = article.NumberId - 100000;
                            article.MenuId = menuId;
                            article.SortId = sortId;
                            article.AddDate = DateTime.Now;
                        }
                    }
                    else
                    {
                        article = db.Articles.FirstOrDefault(a => a.Id == id && a.MenuId == menuId);
                        #region 记录修改历史
                        if (menu.HasFlag(MenuFlag.RecordHistory))
                        {
                            actionHistory = new ActionHistory()
                            {
                                TargetId = article.Id.ToString(),
                                LastModify = DateTime.Now,
                                ModifyUser = this.LoginId,
                                IP = HttpHelper.GetIP(),
                                Type = ActionType.Article,
                                MenuId = article.MenuId,
                                SortId = article.SortId,
                                Before = JsonHelper.ToJson(article)
                            };
                        }
                        #endregion
                    }
                    #region 处理视频
                    //var file = Request.Files["Video"];
                    //if (file != null && file.ContentLength > 0)
                    //{
                    //    if (file.ContentLength > 524288000)
                    //        return this.GetResult(new Exception("视频文件不能大于500M"));
                    //    if (!FormHelper.IsVideo(file.FileName))
                    //        return this.GetResult(new Exception("不支持的视频格式！"));
                    //    var savePath = $"/upFiles/article/{article.Id}/video";
                    //    var fileName = string.Format("{0}/{1}{2}", savePath, CommonHelper.GenerateOrderNumber().Substring(7), Path.GetExtension(file.FileName));
                    //    file.SaveAs(FormHelper.MapPath(fileName));
                    //    article.Video = fileName;
                    //}
                    //else
                    //{
                    //    article.Video = Request.Form["Video"];
                    //}
                    #endregion

                    List<FormField> fields = new List<FormField>();
                    fields.Add(new TextField("MenuId"));
                    fields.Add(new TextField("SortId"));
                    fields.Add(new TextField("Publish"));

                    #region 解析菜单设置
                    var config = menu.Config;
                    if (config != null && config.Fields.Count > 0)
                    {
                        foreach (var item in config.Fields)
                        {
                            var fieldType = Enum.Parse<MenuFieldType>(item.Type);
                            if (fieldType == MenuFieldType.Image)
                            {
                                var imageField = new ImageField(item.Name) { Text = item.Title, SavePath = $"/upFiles/article/{article.Id}/{item.Name}", MaxLength = item.MaxLength > 0 ? item.MaxLength * 1024 : 2048, Required = item.Required };
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
                                    fields.Add(new VideoField(item.Name) { Text = item.Title, SavePath = $"/upFiles/article/{article.Id}/{item.Name}", MaxLength = item.MaxLength > 0 ? item.MaxLength * 1024 : 51200, Required = item.Required });
                                }
                            }
                            else if (fieldType == MenuFieldType.File)
                            {
                                fields.Add(new FileField(item.Name) { Text = item.Title, SavePath = $"/upFiles/article/{article.Id}/{item.Name}", MaxLength = item.MaxLength > 0 ? item.MaxLength * 1024 : 20480, Required = item.Required });
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
                    else
                    {
                        fields.Add(new EditorField("Content") { Text = "文章内容", Required = true, XXSFilter = false });
                    }
                    #endregion

                    FormHelper.SafeFill(article, fields.ToArray());

                    var isShowAudit = menu.HasFlag(MenuFlag.Audit) && HasPermission(PermissionEnum.Audit);
                    if (isShowAudit)
                        article.Publish = false;

                    #region 保存标签
                    if (article.Tags != null && article.Tags.Count > 0)
                    {
                        foreach (var tag in article.Tags)
                        {
                            if (!db.Labels.Any(a => a.MenuId == menuId && a.Sort == sortId && a.Name == tag))
                            {
                                db.Labels.Add(new Label()
                                {
                                    Name = tag,
                                    MenuId = menuId,
                                    Sort = sortId
                                });
                            }
                        }
                    }
                    #endregion
                    if (string.IsNullOrEmpty(article.Lang))
                        article.Lang = this.Languages[0].id;
                    article.LastModify = DateTime.Now;
                    if (menu.HasFlag(MenuFlag.Audit))
                        article.Status = (int)AuditingStatus.Waiting;
                    else
                        article.Status = (int)AuditingStatus.Success;
                    article.ModifyUser = this.LoginId;
                    #region 记录更新历史
                    if (actionHistory != null)
                    {
                        actionHistory.After = JsonHelper.ToJson(article);
                        db.ActionHistories.Add(actionHistory);
                    }
                    #endregion
                    db.Articles.Save(article);
                    return this.GetResult(true);
                }
                catch (Exception ex)
                {
                    return this.GetResult(ex);
                }
            }
        }
        #endregion

        #region 删除文章
        [Route("article/del")]
        public IActionResult DelNews(long menuId, string[] ids)
        {
            if (!this.HasPermission(PermissionEnum.Delete))
                return this.GetResult(new Exception("无操作权限！"));
            if (!this.HasMenu(menuId))
                return this.GetResult(false, "无访问权限！");
            var menu = this.GetMenu(menuId);
            using (var db = this.GetMongodb())
            {
                #region 删除保护
                if (menu.HasFlag(MenuFlag.DeleteProtect))
                {
                    foreach (var id in ids)
                    {
                        var article = db.Articles.FirstOrDefault(a => a.Id == id);
                        db.Trashs.Add(new Trash()
                        {
                            TargetId = id.ToString(),
                            Type = (int)TrashType.Article,
                            SortId = article.MenuId,
                            Content = JsonHelper.ToJson(article),
                            LastModify = DateTime.Now,
                            ModifyUser = this.LoginId
                        });
                    }
                }
                else
                {
                    foreach (var articleId in ids)
                        FormHelper.DeleteDirectory($"/upFiles/article/{articleId}");
                }
                #endregion
                var result = db.Articles.Delete(a => a.MenuId == menuId && ids.Contains(a.Id));
                return this.GetResult(result.DeletedCount > 0);
            }
        }
        #endregion

        #region 排序
        [Route("article/order")]
        public IActionResult SetArticleOrder(string id, long order, string type)
        {
            using (var db = this.GetMongodb())
            {
                var article = db.Articles.FirstOrDefault(a => a.Id == id);
                if (!this.HasMenu(article.MenuId))
                    return this.GetResult(false, "无访问权限！");
                Article byOrderArticle;
                if (type == "up")
                    byOrderArticle = db.Articles.Where(a => a.MenuId == article.MenuId && a.SortId == article.SortId && a.ByOrder > order).OrderBy(a => a.ByOrder).FirstOrDefault();
                else
                    byOrderArticle = db.Articles.Where(a => a.MenuId == article.MenuId && a.SortId == article.SortId && a.ByOrder < order).OrderByDescending(a => a.ByOrder).FirstOrDefault();
                if (byOrderArticle != null)
                {
                    db.Articles.UpdateByPK(byOrderArticle.Id, b => b.ByOrder, order);
                    db.Articles.UpdateByPK(id, b => b.ByOrder, byOrderArticle.ByOrder);
                }
                return this.GetResult(true);
            }
        }
        #endregion

        #region 审核
        [Route("article/audit")]
        public IActionResult AuditArticle(long menuId, string[] ids, int type)
        {
            if (!this.HasPermission(PermissionEnum.Audit))
                return this.GetResult(false, "无审核权限！");
            if (!this.HasMenu(menuId))
                return this.GetResult(false, "无访问权限！");
            using (var db = this.GetMongodb())
            {
                foreach (var id in ids)
                {
                    var auditingStatus = type;
                    db.Articles.Update(a => ids.Contains(a.Id), b => b.Status, auditingStatus, b => b.Publish, ((AuditingStatus)type) == AuditingStatus.Success);
                }
                return this.GetResult(true);
            }
        }
        #endregion
    }

    #region Model
    public class ArticleModel
    {
        public string Title { get; set; }
        public string MenuName { get; set; }
        public string MenuIcon { get; set; }
        public List<JsonData<int>> Flags { get; set; }
        public List<JsonData<string>> Languages { get; set; }
        public List<string> Tags { get; set; }
        public long MenuId { get; set; }
        public int SortId { get; set; }
        /// <summary>
        /// 栏目对象
        /// </summary>
        public Menu Menu { get; set; }
        /// <summary>
        /// 是否为单篇文章
        /// </summary>
        public bool IsArticle
        {
            get
            {
                if (this.Menu.Config == null || this.Menu.Config.Fields == null || this.Menu.Config.Fields.Count == 0)
                    return true;
                return false;
            }
        }
        public bool ShowEdit { get; set; }
        public bool ShowNew { get; set; }
        public bool ShowAudit { get; set; }

        public string DataUrl { get; set; }
        public string InfoUrl { get; set; }
        public string DelUrl { get; set; }
        public string SubmitUrl { get; set; }
        public string AuditUrl { get; set; }
        public string OrderUrl { get; set; }

        public string ArticleId { get; set; }
        public List<JsonData<int>> AuditStatus { get; set; }
    }
    #endregion
}