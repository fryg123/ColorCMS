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
    [MyAuth(PermissionEnum.WebSetting)]
    public class MailTemplateController : AdminBaseController
    {
        #region 初始化
        [Cache(10)]
        public IActionResult Index(long mid)
        {
            if (!this.HasMenu(mid))
                return this.GetResult(false, "无访问权限！");

            ViewBag.DelUrl = this.GetUrl("mailtemplate/del");
            ViewBag.DataUrl = this.GetUrl("mailtemplate/list");
            ViewBag.SubmitUrl = this.GetUrl("mailtemplate/save");
            ViewBag.InfoUrl = this.GetUrl("mailtemplate/info");
            ViewBag.SetOrderUrl = this.GetUrl("mailtemplate/order");
            ViewBag.ViewUrl = this.GetUrl("mailtemplate/view");
            var model = new MailTemplateModel()
            {
                MenuId = mid,
                Languages = this.Languages
            };
            return View(model);
        }
        #endregion

        #region 邮件模板列表
        [Route("mailtemplate/list")]
        public IActionResult MailTemplateList(long menuId, string keyword, string startDate, string endDate, string group)
        {
            if (!HasMenu(menuId))
                return this.GetResult(false, "无访问权限！");
            using (var db = this.GetMongodb())
            {
                var query = db.MailTemplates.GetQuery();
                if (!string.IsNullOrEmpty(group))
                    query = query.Where(a => a.Group == group);
                if (!string.IsNullOrEmpty(keyword))
                    query = query.Where(a => a.Title.Contains(keyword));
                if (!string.IsNullOrEmpty(startDate))
                {
                    var sdate = (startDate.ParseDate() ?? DateTime.Now).MinDate();
                    var edate = (endDate.ParseDate() ?? DateTime.Now).MaxDate();
                    query = query.Where(a => a.LastModify >= sdate && a.LastModify <= edate);
                }
                return this.GetDataGrid(query.OrderBy(a => a.Label).ThenBy(b => b.ByOrder).Select(a => new { a.Id, a.Title, a.Label, a.ByOrder, a.LastModify, a.Lang }));
            }
        }
        #endregion

        #region 保存邮件模板信息
        [Route("mailtemplate/save")]
        public IActionResult SaveMailTemplate(long menuId, int id, string group)
        {
            if (!this.HasMenu(menuId))
                return this.GetResult(false, "无访问权限！");
            using (var db = this.GetMongodb())
            {
                try
                {
                    var menu = this.GetMenu(menuId);
                    var isNew = id == 0;
                    ActionHistory actionHistory = null;
                    MailTemplate mailtemplate;
                    if (isNew)
                    {
                        id = (int)db.MailTemplates.GetMaxId();
                        mailtemplate = new MailTemplate();
                    }
                    else
                    {
                        mailtemplate = db.MailTemplates.FirstOrDefault(a => a.Id == id && a.MenuId == menuId);
                        #region 记录修改历史
                        if (menu.HasFlag(MenuFlag.RecordHistory))
                        {
                            actionHistory = new ActionHistory()
                            {
                                TargetId = mailtemplate.Id.ToString(),
                                LastModify = DateTime.Now,
                                ModifyUser = this.LoginId,
                                IP = HttpHelper.GetIP(),
                                Type = ActionType.Article,
                                MenuId = menuId,
                                Before = JsonHelper.ToJson(mailtemplate)
                            };
                        }
                        #endregion
                    }
                    FormHelper.FillTo(mailtemplate, new FormField() { Name = "Id", Disabled = true },
                        new FormField() { Name = "Title", Text = "邮件标题", Required = true },
                        new FormField() { Name = "Content", Text = "邮件内容", Required = true });
                    mailtemplate.Id = id;
                    mailtemplate.ByOrder = mailtemplate.Id;
                    mailtemplate.Group = group;
                    mailtemplate.MenuId = menuId;
                    mailtemplate.LastModify = DateTime.Now;
                    mailtemplate.ModifyUser = this.LoginId;
                    #region 记录更新历史
                    if (actionHistory != null)
                    {
                        actionHistory.After = JsonHelper.ToJson(mailtemplate);
                        db.ActionHistories.Add(actionHistory);
                    }
                    #endregion
                    var result = db.MailTemplates.Save(mailtemplate);
                    #region 清除缓存
                    var key = $"#mail_{mailtemplate.Id}";
                    this.RemoveCache(key);
                    #endregion
                    return this.GetResult(true);
                }
                catch (Exception ex)
                {
                    return this.GetResult(ex);
                }
            }
        }
        #endregion

        #region 获取邮件模板信息
        [Route("mailtemplate/info")]
        public IActionResult GetMailTemplate(int id, long menuId)
        {
            if (!this.HasMenu(menuId))
                return this.GetResult(false, "无访问权限！");
            using (var db = this.GetMongodb())
            {
                var mailtemplate = db.MailTemplates.FirstOrDefault(a => a.Id == id);
                return this.GetResult(true, new
                {
                    mailtemplate.Id,
                    mailtemplate.Label,
                    mailtemplate.Title,
                    mailtemplate.Content,
                    mailtemplate.Group,
                    mailtemplate.Lang
                });
            }
        }
        #endregion

        #region 删除邮件模板
        [Route("mailtemplate/del")]
        public IActionResult DelMailTemplates(long menuId, int[] ids)
        {
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
                        var mailTemplate = db.MailTemplates.FirstOrDefault(a => a.Id == id);
                        db.Trashs.Add(new Trash()
                        {
                            TargetId = id.ToString(),
                            Type = (int)TrashType.MailTemplate,
                            SortId = menuId,
                            Content = JsonHelper.ToJson(mailTemplate),
                            AddDate = DateTime.Now,
                            LastModify = DateTime.Now,
                            ModifyUser = this.LoginId
                        });
                    }
                }
                #endregion
                var result = db.MailTemplates.Delete(a => a.MenuId == menuId && ids.Contains(a.Id));
                return this.GetResult(result.DeletedCount > 0);
            }
        }
        #endregion

        #region 排序
        [Route("mailtemplate/order")]
        public IActionResult SetMailTemaplateOrder(int id, int order, string type)
        {
            using (var db = this.GetMongodb())
            {
                int byOrder;
                if (type == "up")
                    byOrder = db.MailTemplates.Where(a => a.ByOrder < order).OrderByDescending(a => a.ByOrder).Select(a => a.ByOrder).FirstOrDefault();
                else
                    byOrder = db.MailTemplates.Where(a => a.ByOrder > order).OrderBy(a => a.ByOrder).Select(a => a.ByOrder).FirstOrDefault();
                if (byOrder > 0)
                {
                    db.MailTemplates.Update(a => a.ByOrder == byOrder, b => b.ByOrder, order);
                    db.MailTemplates.UpdateByPK(id, b => b.ByOrder, byOrder);
                }
                return this.GetResult(true);
            }
        }
        #endregion

        #region 查看模板
        [Route("mailtemplate/view")]
        [HttpGet]
        public IActionResult ViewMailTemaplte(int id, long menuId)
        {
            using (var db = this.GetMongodb())
            {
                var mail = db.MailTemplates.FirstOrDefault(a => a.Id == id && a.MenuId == menuId);
                if (mail == null)
                    return this.GetTextResult("无访问权限！");
                var html = new StringBuilder("<html>");
                html.AppendFormat("<head><title>{0}</title></head>", mail.Title);
                html.AppendFormat("<body><div style='width:80%;margin:0 auto'><h3 style='text-align:center'>{1}</h3><div>{0}</div></div></body>", mail.Content.Replace("#url", this.ServerUrl), mail.Title);
                html.Append("</html>");
                return this.GetTextResult(html.ToString());
            }
        }
        #endregion
    }

    #region MailTemplateModel
    public class MailTemplateModel
    {
        /// <summary>
        /// 菜单Id
        /// </summary>
        public long MenuId { get; set; }
        /// <summary>
        /// 语言
        /// </summary>
        public List<JsonData<string>> Languages { get; set; }
    }
    #endregion
}