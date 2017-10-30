using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Colorful.Web.CMS
{
    public class RouteConvention : IApplicationModelConvention
    {
        private readonly AttributeRouteModel _languagePrefix;
        private readonly AttributeRouteModel _adminPrefix;

        public RouteConvention()
        {
            if (WebConfig.Languages.Count > 1)
                _languagePrefix = new AttributeRouteModel(new RouteAttribute("{lang:regex(^[cCnNeE]{{2}}$)}")); //{lang:regex(^[[a-z]]{{2}}-[[A-Z]]{{2}}$)}
            _adminPrefix = new AttributeRouteModel(new RouteAttribute(WebConfig.AdminRoutePrefix));
        }

        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                var controllerName = controller.ControllerName;
                if (controllerName == "Base" || controllerName == "WebBase"
                    || controllerName == "AdminBase" || controllerName == "Error" || controllerName == "UEditor")
                    continue;
                var isAdminController = controller.ControllerType.BaseType.Equals(typeof(Web.Controllers.Admin.AdminBaseController));
                #region ControllerRoutes
                var matchedSelectors = controller.Selectors.Where(x => x.AttributeRouteModel != null).ToList();
                if (matchedSelectors.Any())
                {
                    foreach (var selectorModel in matchedSelectors)
                    {
                        if (isAdminController)
                            selectorModel.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(_adminPrefix, selectorModel.AttributeRouteModel);
                        else if (_languagePrefix != null)
                            selectorModel.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(_languagePrefix, selectorModel.AttributeRouteModel);
                    }
                }
                var unmatchedSelectors = controller.Selectors.Where(x => x.AttributeRouteModel == null).ToList();
                if (unmatchedSelectors.Any())
                {
                    foreach (var selectorModel in unmatchedSelectors)
                    {
                        if (isAdminController)
                        {
                            selectorModel.AttributeRouteModel = _adminPrefix;
                        }
                        else if (_languagePrefix != null)
                            selectorModel.AttributeRouteModel = _languagePrefix;
                    }
                }
                #endregion

                #region ActionRoutes
                foreach (var action in controller.Actions)
                {
                    var selectors = action.Selectors.Where(x => x.AttributeRouteModel == null).ToList();
                    if (selectors.Any())
                    {
                        foreach (var selector in selectors)
                        {
                            if (!isAdminController && action.ActionName.ToLower() == "index")
                                continue;
                            if (controllerName != "Home")
                            {
                                string url;
                                if (action.ActionName.ToLower() == "index")
                                    url = controllerName;
                                else
                                    url = $"{controllerName}/{action.ActionName}";
                                selector.AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(url));
                            }
                            else
                            {
                                selector.AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(action.ActionName));
                            }
                        }
                    }
                }
                #endregion
            }
        }
    }
}