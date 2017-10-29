using System;
using System.Collections.Generic;
using System.Linq;

namespace Colorful.Models
{
    /// <summary>
    /// 数据字典
    /// </summary>
    public class Code : BaseIntId
    {
        /// <summary>
        /// 菜单编号
        /// </summary>
        public long MenuId { get; set; }
        /// <summary>
        /// 字典分类
        /// </summary>
        [BindField("类别", MenuFieldType.Select)]
        public int Sort { get; set; }
        /// <summary>
        /// 字典标识
        /// </summary>
        [BindField("字典标识", MenuFieldType.Select)]
        public CodeFlag Flag { get; set; }
        /// <summary>
        /// 父Id
        /// </summary>
        public int ParentId { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        [BindField("名称")]
        public string Name { get; set; }
        /// <summary>
        /// 英文名称
        /// </summary>
        [BindField("英文名称")]
        public string NameEN { get; set; }
        /// <summary>
        /// 图标
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int ByOrder { get; set; }
        /// <summary>
        /// 特殊数据
        /// </summary>
        [BindField("配置")]
        public string Data { get; set; }
        /// <summary>
        /// 标记
        /// </summary>
        [BindField("特殊标识")]
        public List<int> Flags { get; set; }
        [BindField("语言")]
        public string Lang { get; set; }
    }
}
