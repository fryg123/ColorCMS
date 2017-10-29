using System;
using System.Collections.Generic;
using System.Linq;

namespace Colorful.Models
{
    /// <summary>
    /// 菜单模块
    /// </summary>
    public class MenuModule : BaseLongId
    {
        public MenuModule()
        {
        }
        public MenuModule(params MenuFlag[] flags)
        {
            this.Flags = flags.ToList();
        }

        public List<MenuFlag> Flags { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 配置（Json格式）
        /// </summary>
        public string Config { get; set; }
        /// <summary>
        /// 链接地址
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 模块
        /// </summary>
        public string Model { get; set; }
    }
}