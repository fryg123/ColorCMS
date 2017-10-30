using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Colorful.Web.CMS
{
    public static class UEditorServiceExtension
    {
        public static UEditorActionCollection AddUEditorService(
            this IServiceCollection services, 
            string configFile= "ueditorconfig.json", 
            bool isCache = false)
        {
            Config.ConfigFile = configFile;
            Config.noCache = !isCache;

            var actions = new UEditorActionCollection();
            services.AddSingleton(actions);
            services.AddSingleton<UEditorService>();

            return actions;
        }
    }
}
