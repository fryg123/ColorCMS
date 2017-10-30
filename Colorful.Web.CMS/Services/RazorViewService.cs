using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace Colorful.Web.CMS
{
    public class RazorViewService
    {
        private IRazorViewEngine _viewEngine;
        private ITempDataProvider _tempDataProvider;
        private IServiceProvider _serviceProvider;

        public RazorViewService(
            IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

        public string ToHtml(string name, object model)
        {
            var viewData = new ViewDataDictionary(
                        metadataProvider: new EmptyModelMetadataProvider(),
                        modelState: new ModelStateDictionary())
            {
                Model = model
            };
            return this.ToHtml(name, viewData);
        }
        public string ToHtml(string name, ViewDataDictionary viewData)
        {
            var actionContext = GetActionContext();

            var viewEngineResult = _viewEngine.FindView(actionContext, name, false);

            if (!viewEngineResult.Success)
            {
                throw new InvalidOperationException(string.Format("Couldn't find view '{0}'", name));
            }

            var view = viewEngineResult.View;

            using (var output = new StringWriter())
            {
                var viewContext = new ViewContext(
                    actionContext,
                    view,
                    viewData,
                    new TempDataDictionary(
                        actionContext.HttpContext,
                        _tempDataProvider),
                    output,
                    new HtmlHelperOptions());

                view.RenderAsync(viewContext).GetAwaiter().GetResult();

                return output.ToString();
            }
        }

        private ActionContext GetActionContext()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = _serviceProvider;
            return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        }
    }
}