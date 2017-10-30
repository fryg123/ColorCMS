using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Colorful.Web.CMS
{
    public static class MiddlewareExtensions
    {
        public static void UseRoutePrefix(this MvcOptions opts)
        {
            opts.Conventions.Insert(0, new RouteConvention());
        }
    }
}
