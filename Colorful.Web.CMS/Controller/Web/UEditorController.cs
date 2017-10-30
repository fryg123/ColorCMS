using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Colorful.Web
{
    [Route("api/[controller]")]
    public class UEditorController : Controller
    {
        private UEditorService ue;
        public UEditorController(UEditorService ue)
        {
            this.ue = ue;
        }

        public void Do()
        {
            ue.DoAction(HttpContext);
        }
    }
}
