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
    public class AdminMenuModuleController : AdminBaseController
    {
       
    }
}