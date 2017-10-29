using System;
using System.Collections.Generic;
using System.Linq;

namespace Colorful.Models
{
    #region FormBuilderModel
    public class FormBuilderModel
    {
        public MenuConfig MenuConfig { get; set; }
        public string Prefix { get; set; }

        public FormBuilderModel(MenuConfig menuConfig, string prefiex)
        {
            this.Prefix = prefiex;
            this.MenuConfig = menuConfig;
        }
    }
    #endregion
}