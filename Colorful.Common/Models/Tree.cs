using System;
using System.Collections.Generic;
using System.Linq;

namespace Colorful.Models
{
    public class Tree<T>
    {
        /// <summary>
        /// Id
        /// </summary>
        public T id { get; set; }
        /// <summary>
        /// 显示的文本
        /// </summary>
        public string text { get; set; }
        /// <summary>
        /// 图标
        /// </summary>
        public string icon { get; set; }
        /// <summary>
        /// 是否选中
        /// </summary>
        public bool Checked { get; set; }
        /// <summary>
        /// 是否禁用Checkbox
        /// </summary>
        public bool disableCheckbox { get; set; }
        /// <summary>
        /// 是否展开
        /// </summary>
        public bool expand { get; set; }
        /// <summary>
        /// 是否屏蔽
        /// </summary>
        public bool disabled { get; set; }
        /// <summary>
        /// 子对象
        /// </summary>
        public List<Tree<T>> children { get; set; }
    }
}