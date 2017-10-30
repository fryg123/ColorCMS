using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using System.Text;

using Colorful.Models;
using MongoDB.Bson;
using System.Reflection;
using Colorful;
using Microsoft.AspNetCore.Mvc;

namespace Colorful.Web.CMS.Controllers.Admin
{ 
    [MyAuth(PermissionEnum.Admin)]
    public class AdminTableController : AdminBaseController
    {
        #region 首页
        [Cache(10)]
        [Route("table")]
        public IActionResult Table(string t, long mid)
        {
            ViewBag.DelUrl = this.GetUrl("table/del");
            ViewBag.DataUrl = this.GetUrl("table/list");
            ViewBag.SaveUrl = this.GetUrl("table/save");
            ViewBag.Table = t;

            using (var db = this.GetMongodb())
            {
                var menu = db.Menus.FirstOrDefault(a => a.Id == mid);
                ViewBag.Title = menu.Name;
            }

            return View($"../admin/tables/{t}");
        }
        #endregion

        #region 获取列表
        [Route("table/list")]
        public IActionResult TableList(string t, string keyword, int date = 0)
        {
            using (var db = this.GetMongodb())
            {
                var q = db.Database.GetCollection<BsonDocument>(t).AsQueryable();
                return this.GetDataGrid(q);
            }
        }
        #endregion

        #region 保存
        [Route("table/save")]
        public IActionResult SaveRole(string t, long id)
        {
            if (id == 0)
                return this.GetResult(false, "Id不能为空！");
            using (var db = this.GetMongodb())
            {
                var table = db.Database.GetCollection<BsonDocument>(t);
                var assembly = typeof(WebSetting).GetTypeInfo().Assembly;
                var target = assembly.CreateInstance($"Colorful.Models.{t}");
                FormHelper.FillTo(target);
                var bsonDoc = target.ToBsonDocument();
                if (bsonDoc.Contains("_t"))
                    bsonDoc.Remove("_t");
                table.Save(bsonDoc);
                return this.GetResult(true);
            }
        }
        #endregion

        #region 删除
        [Route("table/del")]
        public IActionResult Del(string t, long[] ids)
        {
            using (var db = this.GetMongodb())
            {
                var table = db.Database.GetCollection<BsonDocument>(t);
                foreach (var id in ids)
                    table.FindOneAndDelete(table.GetFilter().Eq("_id", id));
                return this.GetResult(true);
            }
        }
        #endregion
    }
}