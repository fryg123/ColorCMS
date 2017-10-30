using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace Colorful.Web.CMS
{
    public class UEditorService
    {
        private UEditorActionCollection actionList;

        public UEditorService(UEditorActionCollection actions)
        {
            actionList = actions;
        }

        public void DoAction(HttpContext context)
        {
            var action = context.Request.Query["action"];
            if (actionList.ContainsKey(action))
                actionList[action].Invoke(context);
            else
                new NotSupportedHandler(context).Process();
        }
    }
}
