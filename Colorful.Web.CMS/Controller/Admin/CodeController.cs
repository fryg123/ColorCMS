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
    [MyAuth(PermissionEnum.WebSetting)]
    public class CodeController : AdminBaseController
    {
        #region 初始化
        [Cache]
        public IActionResult Index(long mid, int? sort)
        {
            if (mid == 0)
                return this.GetTextResult("菜单不能为空！");
            if (!this.HasMenu(mid))
                return this.GetTextResult("无菜单访问权限！");

            var menu = this.Menus.FirstOrDefault(a => a.Id == mid);
            #region 处理数据源
            if (menu.Config != null && menu.Config.Fields.Count > 0)
            {
                foreach (var field in menu.Config.Fields)
                {
                    if (field.DataSource == null || string.IsNullOrEmpty(field.DataSource.Data))
                        continue;
                    switch (field.DataSource.DataType)
                    {
                        case "Enum":
                            ViewData[field.Name] = EnumHelper.ToJsonData(field.DataSource.Data).Select(a => new JsonData<string>() { id = a.id.ToString(), text = a.text }).ToList();
                            break;
                        case "Data":
                            var assembly = typeof(WebSetting).GetTypeInfo().Assembly;
                            var target = (IDataSource)assembly.CreateInstance($"Colorful.Models.{field.DataSource.Data}");
                            ViewData[field.Name] = target.Datas;
                            break;
                        default:
                            ViewData[field.Name] = JsonHelper.Parse<List<JsonData<int>>>(field.DataSource.Data).Select(a => new JsonData<string>() { id = a.id.ToString(), text = a.text }).ToList();
                            break;
                    }
                }
            }
            #endregion

            ViewBag.SubmitUrl = this.GetUrl("code/save");
            ViewBag.DelUrl = this.GetUrl("code/del");
            ViewBag.TreeUrl = this.GetUrl("code/list");
            ViewBag.InfoUrl = this.GetUrl("code/info");
            ViewBag.OrderUrl = this.GetUrl("code/order");

            var model = new CodeModel()
            {
                CodeFlags = EnumHelper.ToList<CodeFlag>().Select(a => new JsonData<int>() { id = (int)a, text = a.GetDescription() }).ToList(),
                MenuId = mid,
                Menu = menu,
                SortId = sort.GetValueOrDefault()
            };
            return View(model);
        }
        #endregion

        #region 获取代码信息
        [Route("code/info")]
        public IActionResult GetCodeInfo(int id)
        {
            using (var db = this.GetMongodb())
            {
                var code = db.Codes.FirstOrDefault(a => a.Id == id);
                return this.GetJsonResult(new { code.Id, code.Name, code.NameEN, code.Icon, code.ParentId, code.Sort, code.Data, code.Flags });
            }
        }
        #endregion

        #region 代码列表
        [Route("code/list")]
        public IActionResult GetCodeList(long menuId, bool open)
        {
            var treeList = new List<Tree<int>>();
            using (var db = this.GetMongodb())
            {
                var list = db.Codes.Where(a => a.MenuId == menuId).OrderBy(a => a.ByOrder).ToList();
                var rootCodes = list.Where(a => a.ParentId == 0).OrderBy(a => a.ByOrder).ToList();
                foreach (var data in rootCodes)
                {
                    var tree = new Tree<int>()
                    {
                        text = data.Name,
                        id = data.Id,
                        icon = data.Icon,
                        expand = open
                    };
                    treeList.Add(tree);
                    this.getCodeList(list, tree, open);
                }
                return this.GetTree(treeList, "数据字典");
            }
        }
        private void getCodeList(List<Code> list, Tree<int> tree, bool open)
        {
            var children = list.Where(a => a.ParentId == tree.id).OrderBy(a => a.ByOrder).ToList();
            tree.children = new List<Tree<int>>();
            foreach (var data in children)
            {
                var cTree = new Tree<int>()
                {
                    id = data.Id,
                    text = data.Name,
                    icon = data.Icon,
                    expand = open
                };
                tree.children.Add(cTree);
                getCodeList(list, cTree, open);
            }
        }
        #endregion

        #region 保存代码
        [Route("code/save")]
        public IActionResult SaveCode(int id)
        {
            using (var db = this.GetMongodb())
            {
                try
                {
                    Code code;
                    if (id > 0)
                    {
                        code = db.Codes.FirstOrDefault(a => a.Id == id);
                    }
                    else
                    {
                        code = new Code();
                        code.Id = (int)db.Codes.GetMaxId();
                        code.ByOrder = code.Id;
                    }
                    code.LastModify = DateTime.Now;
                    code.ModifyUser = this.LoginId;
                    FormHelper.FillTo(code, new DisableField("Id"));
                    if (code.Id == code.ParentId)
                        code.ParentId = 0;
                    db.Codes.Save(code);
                    this.RemoveCache($"#Codes_{code.MenuId}");
                    return this.GetResult(true);
                }
                catch (Exception ex)
                {
                    return this.GetResult(ex);
                }
            }
        }
        #endregion

        #region 删除代码
        [Route("code/del")]
        public IActionResult DelCode(long menuId, int sort, int id)
        {
            using (var db = this.GetMongodb())
            {
                var codeList = db.Codes.Where(a => a.MenuId == menuId && a.Sort == sort).ToList();
                List<int> ids = new List<int>();
                GetCodeDels(codeList, id, ref ids);

                #region 回收垃圾箱
                db.Trashs.Add(new Trash()
                {
                    LastModify = DateTime.Now,
                    ModifyUser = this.LoginId,
                    Type = (int)TrashType.Code,
                    TargetId = id.ToString(),
                    SortId = menuId,
                    Content = JsonHelper.ToJson(codeList.Where(a => ids.Contains(id)))
                });
                #endregion

                var result = db.Codes.Delete(a => ids.Contains(a.Id));
                this.RemoveCache($"#Codes_{menuId}");
                return this.GetResult(result.DeletedCount > 0);
            }
        }
        private void GetCodeDels(List<Code> codes, int id, ref List<int> ids)
        {
            ids.Add(id);
            var children = codes.Where(a => a.ParentId == id).ToList();
            foreach (var c in children)
            {
                GetCodeDels(codes, c.Id, ref ids);
            }
        }
        #endregion

        #region 代码排序
        [Route("code/order")]
        public IActionResult OrderCode(long[] ids)
        {
            using (var db = this.GetMongodb())
            {
                for (var i = 0; i < ids.Length; i++)
                {
                    db.Codes.UpdateByPK(ids[i], b => b.ByOrder, i);
                }
                return this.GetResult(true);
            }
        }
        #endregion
    }

    #region CodeModel
    public class CodeModel
    {
        public List<JsonData<string>> Languages { get; set; }
        public List<JsonData<int>> CodeFlags { get; set; }
        public Menu Menu { get; set; }
        public long MenuId { get; set; }
        public int SortId { get; set; }
    }
    #endregion
}