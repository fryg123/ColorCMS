using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Colorful.Web.CMS
{
    public static class ColorCms
    {
        public static IWebHost Run<TStartup>(string[] args) where TStartup : ColorCmsStartup
        {
            WebConfig.RootPath = Directory.GetCurrentDirectory();
            var host = WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    config.AddJsonFile($"appsettings.json", optional: true, reloadOnChange: true);
                    builderContext.Configuration.GetReloadToken().RegisterChangeCallback(_ =>
                    {
                        ColorCmsStartup.Current.LoadSettings(true);
                    }, null);
                })
                .UseKestrel(c =>
                {
                    c.AddServerHeader = false;
                })
                .UseContentRoot(WebConfig.RootPath)
                .UseStartup<TStartup>()
                .UseApplicationInsights()
                .Build();
            return host;
        }
    }
}
