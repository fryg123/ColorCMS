using Colorful.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Colorful.Web.CMS
{
    #region MyActionFilter
    public class MyActionFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = context.Controller as BaseController;
            if (controller != null)
            {
                #region 语言设置
                var lang = context.HttpContext.Request.Query["lang"].ToString();
                if (!string.IsNullOrEmpty(lang) && lang.ToLower() == "en")
                {
                    lang = LangEnum.EN.ToString();
                }
                else if (context.HttpContext.Request.Path.HasValue && context.HttpContext.Request.Path.Value.ToLower().StartsWith("/en"))
                {
                    lang = LangEnum.EN.ToString();
                }
                else
                {
                    lang = LangEnum.CN.ToString();
                }
                controller.ViewData["Lang"] = lang;
                #endregion
                //IsDebug
                controller.ViewData["IsDebug"] = MyWebConfig.IsDebug;
                controller.ViewData["IsMobile"] = controller.Client.IsMobile;
                controller.ViewData["IsWeXin"] = controller.Client.IsWeiXin;
                controller.ViewData["Title"] = MyWebConfig.WebSetting.Title;
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            //if (context.Exception != null)
            //{
            //    throw context.Exception;
            //}
            // do something after the action executes
        }
    }
    #endregion
}
