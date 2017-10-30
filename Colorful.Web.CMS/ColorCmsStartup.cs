using Colorful.Cache.Web;
using Colorful.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Colorful.Web.CMS
{
    public abstract class ColorCmsStartup
    {
        public IConfiguration Configuration { get; }
        public static ColorCmsStartup Current;

        public ColorCmsStartup(IConfiguration configuration)
        {
            Configuration = configuration;
            Current = this;
        }

        #region ConfigureServices
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //加载设置
            LoadSettings();

            // HttpContext.Current
            services.AddHttpContextAccessor();

            #region Redis缓存服务
            services.AddCacheService(opts =>
            {
                opts.Prefix = WebConfig.ProjectId;
                var appSetting = Configuration.GetSection("AppSetting");
                var databaseSetting = Configuration.GetSection("Database");
                if (appSetting.GetValue<bool>("UseRedis"))
                {
                    opts.ConnectionString = databaseSetting.GetValue<string>("Redis");
                }
            });
            #endregion

            #region CookieAuthentication 安全认证
            IdentityService.AuthenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            services.AddAuthentication(opts =>
            {
                opts.DefaultScheme = IdentityService.AuthenticationScheme;
            }).AddCookie(IdentityService.AuthenticationScheme, opts =>
            {
                opts.AccessDeniedPath = "/";
                opts.LoginPath = "/";
                opts.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToAccessDenied = ctx => //访问被拒绝
                    {
                        if (ctx.Request.Path.Value.StartsWith("/adm") || ctx.Request.Path.Value.StartsWith("/admin"))
                            ctx.RedirectUri = "/error/401";
                        ctx.Response.Redirect(ctx.RedirectUri);
                        return Task.FromResult(0);
                    },
                    OnRedirectToLogin = ctx => //未登陆跳转
                    {
                        if (ctx.Request.Path.Value.StartsWith("/adm") || ctx.Request.Path.Value.StartsWith("/admin"))
                            ctx.RedirectUri = "/";
                        ctx.Response.Redirect(ctx.RedirectUri);
                        return Task.FromResult(0);
                    }
                };
            });
            #endregion

            //UEditor
            services.AddUEditorService();
            //Session
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(20);
                options.Cookie.HttpOnly = true;
            });
            //Mvc设置
            services.AddMvc(opts =>
            {
                opts.Filters.Add(new MyActionFilter());
                opts.UseRoutePrefix();
            });

            #region RazorViewToStringRenderer
            services.AddSingleton<RazorViewService>();
            #endregion
        }
        #endregion

        #region Configure App
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //app.UseRequestLocalization(new RequestLocalizationOptions());

            #region 日志
            WebConfig.LoggerFactory = loggerFactory;
            env.ConfigureNLog("nlog.config");
            loggerFactory.AddNLog();
            #endregion

            #region 中间件
            //自定义Http头
            app.UseWebCache();
            #endregion

            if (env.IsDevelopment())
            {
                WebConfig.IsDevelopment = true;

                #region 调试日志
                loggerFactory.AddConsole(Configuration.GetSection("Logging"));
                loggerFactory.AddDebug();
                #endregion

                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                WebConfig.IsDevelopment = false;
                app.UseExceptionHandler("/error");
            }
            app.UseStatusCodePagesWithRedirects("/error/{0}");

            //HttpContext.Current
            app.UseStaticHttpContext();
            //安装认证
            app.UseAuthentication();
            //启用Session
            app.UseSession();
            //开启静态文件支持
            app.UseStaticFiles();

            #region 路由配置
            app.UseMvcWithDefaultRoute();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "areas",
                    template: "{area:exists}/{controller=Home}/{action=Login}");

                routes.MapRoute(
                    name: "default",
                    template: "Home/Index",
                    defaults: new { controller = "Home", action = "Index" });
            });

            #endregion

            #region 初始化安装
            using (var setup = new SetupService())
            {
                setup.Setup("liguo1987", "上海七彩网络科技有限公司", "网站管理").InitCache();
            }
            #endregion

            //动态解析Razor视图服务
            WebConfig.RazorViewService = app.ApplicationServices.GetService<RazorViewService>();
            //缓存服务
            WebConfig.CacheService = app.ApplicationServices.GetService<CacheService>().GetCacheService();
        }
        #endregion

        #region 加载设置 LoadAppSettings
        public void LoadSettings(bool reload = false)
        {
            //if (reload)
            //    Configuration.Reload();

            #region AppSetting
            var appSetting = Configuration.GetSection("AppSetting");
            WebConfig.ProjectId = appSetting.GetValue<string>("ProjectId");
            WebConfig.AdminRoutePrefix = appSetting.GetValue<string>("AdminRoutePrefix");
            if (string.IsNullOrEmpty(WebConfig.AdminRoutePrefix))
                WebConfig.AdminRoutePrefix = "admcolorful";
            WebConfig.ResUrl = appSetting.GetValue<string>("ResUrl");
            var languages = appSetting.GetValue<string>("Languages");
            if (string.IsNullOrEmpty(languages))
            {
                WebConfig.Languages = EnumHelper.ToList<LangEnum>().Select(a => new JsonData<string>() { id = a.ToString(), text = a.GetDescription() }).ToList();

            }
            else
            {
                WebConfig.Languages = EnumHelper.ToList<LangEnum>().Where(a => languages.Contains(a.ToString().ToLower())).Select(a => new JsonData<string>() { id = a.ToString(), text = a.GetDescription() }).ToList();
            }
            //网站静态目录
            var staticFolders = appSetting.GetValue<string>("StaticFolders");
            if (!string.IsNullOrEmpty(staticFolders))
                WebConfig.WebStaticFolders = staticFolders.Split(',').ToList();
            else
                WebConfig.WebStaticFolders = new List<string>();
            #endregion

            #region 数据库
            var databaseSetting = Configuration.GetSection("Database");
            //Mongodb
            var mongodbSetting = databaseSetting.GetSection("Mongodb");
            WebConfig.Database = mongodbSetting.GetValue<string>("DatabaseName");
            WebConfig.ConnectionString = mongodbSetting.GetValue<string>("ConnectionString");
            #endregion
        }
        #endregion
    }
}
