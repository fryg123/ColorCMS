using Colorful.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Colorful.Web.CMS
{
    public static class ControllerExtensions
    {
        #region Json输出
        /// <summary>
        /// 输出为Json格式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="controller"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static IActionResult GetJsonResult<T>(this ControllerBase controller, T data)
        {
            var mediaType = new MediaTypeHeaderValue("application/json");
            mediaType.Encoding = Encoding.UTF8;
            return new ContentResult() { Content = JsonHelper.ToJson(data), ContentType = mediaType.ToString() };
        }
        #endregion

        #region DataGrid输出
        /// <summary>
        /// 输入为DataGrid格式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="controller"></param>
        /// <param name="q"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static IActionResult GetDataGrid<T>(this ControllerBase controller, IQueryable<T> q, Func<List<T>, dynamic> filter = null)
        {
            if (q == null)
                return controller.GetJsonResult(new { count = 0, data = new List<T>() });
            var page = controller.Request.Form["page"].ToString().ParseInt() ?? 1;
            if (page < 1) page = 1;
            page--;
            var pagesize = controller.Request.Form["pagesize"].ToString().ParseInt() ?? 10;
            List<T> list;
            if (pagesize > 0)
                list = q.Skip(page * pagesize).Take(pagesize).ToList();
            else
                list = q.ToList();
            if (typeof(T).Name == "BsonDocument")
            {
                var json = new StringBuilder("{\"count\":" + q.Count() + ",\"data\":[");
                foreach (var doc in (list as List<MongoDB.Bson.BsonDocument>))
                {
                    json.Append("{");
                    foreach (var item in doc)
                    {
                        if (item.Value == null || item.Value == BsonNull.Value) continue;

                        json.Append($"\"{(item.Name == "_id" ? "Id" : item.Name)}\":\"{(item.Value.IsValidDateTime ? item.Value.ToLocalTime().ToString("yyyy/MM/dd HH:mm:ss") : item.Value)}\",");
                    }
                    json.Remove(json.Length - 1, 1);
                    json.Append("},");
                }
                if (json.ToString().EndsWith(","))
                    json.Remove(json.Length - 1, 1);
                json.Append("]}");
                var mediaType = new MediaTypeHeaderValue("application/json");
                mediaType.Encoding = Encoding.UTF8;
                return new ContentResult() { Content = json.ToString(), ContentType = mediaType.ToString() };
            }
            else
            {
                return controller.GetJsonResult(new
                {
                    count = q.Count(),
                    data = filter == null ? list : filter(list)
                });
            }
        }
        #endregion

        #region 数据库访问接口

        #region Mongodb
        /// <summary>
        /// 获取Mongodb数据访问接口
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="database">要访问的数据库</param>
        /// <returns></returns>
        public static MongodbContext GetMongodb(this BaseController controller, string database = "")
        {
            return WebConfig.GetMongodb(database);
        }
        #endregion

        #endregion

        #region 生成数据源
        /// <summary>
        /// 生成数据源
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="database">要访问的数据库</param>
        /// <returns></returns>
        public static void SetDataSource(this BaseController controller, List<MenuConfigField> fields)
        {
            foreach (var field in fields)
            {
                if (field.Name == "Lang")
                {
                    controller.ViewData["Lang"] = WebConfig.Languages;
                    continue;
                }
                if (field.DataSource == null || string.IsNullOrEmpty(field.DataSource.Data))
                    continue;
                switch (field.DataSource.DataType)
                {
                    case "Enum":
                        controller.ViewData[field.Name] = EnumHelper.ToJsonData(field.DataSource.Data).Select(a => new JsonData<string>() { id = a.id.ToString(), text = a.text }).ToList();
                        break;
                    case "Data":
                        var assembly = typeof(WebSetting).GetTypeInfo().Assembly;
                        var target = (IDataSource)assembly.CreateInstance($"Colorful.Models.{field.DataSource.Data}");
                        controller.ViewData[field.Name] = target.Datas;
                        break;
                    default:
                        controller.ViewData[field.Name] = JsonHelper.Parse<List<JsonData<int>>>(field.DataSource.Data).Select(a => new JsonData<string>() { id = a.id.ToString(), text = a.text }).ToList();
                        break;
                }
            }
        }
        #endregion

        #region 访问统计

        #endregion
    }
}
