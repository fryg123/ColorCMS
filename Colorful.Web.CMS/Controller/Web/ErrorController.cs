using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Colorful;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Net.Http.Headers;
using System.Text;

namespace Colorful.Web.CMS.Controllers
{
    public class ErrorController : WebBaseController
    {
        [Route("error")]
        [Route("error/{statusCode:length(3,3)}")]
        public IActionResult Index(int statusCode)
        {
            if (statusCode == 0)
                statusCode = Response.StatusCode;
            ViewData["ErrorCode"] = statusCode;
            switch (statusCode)
            {
                case 404:
                    ViewData["ErrMessage"] = "您访问的页面看起来不存哦！";
                    break;
                case 500:
                    ViewData["ErrMessage"] = "哎呀，系统出故障了！";
                    break;
                case 401:
                    ViewData["ErrMessage"] = "您没有权限访问哦！";
                    break;
            }
            return View("Error");
        }
    }
}
