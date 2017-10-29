using System;
using System.Collections.Generic;
using System.Linq;

namespace Colorful.Models
{
    /// <summary>
    /// 通用友情链接类表
    /// </summary>
    public partial class Link : BaseIntId
    {
        /// <summary>
        /// 菜单编号
        /// </summary>
        public long MenuId
        {
            get;
            set;
        }
        /// <summary>
        /// 用户编号
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 分类Id
        /// </summary>
        [BindField("分类", MenuFieldType.Select)]
        public long SortId
        {
            get;
            set;
        }
        /// <summary>
        /// 标题
        /// </summary>
        [BindField("标题")]
        public string Title
        {
            get;
            set;
        }
        /// <summary>
        /// 图片
        /// </summary>
        [BindField("图片", MenuFieldType.Image)]
        public string Photo
        {
            get;
            set;
        }
        /// <summary>
        /// 附件
        /// </summary>
        [BindField("附件", MenuFieldType.File)]
        public string File
        {
            get; set;
        }
        /// <summary>
        /// 链接地址
        /// </summary>
        [BindField("链接")]
        public string Url
        {
            get;
            set;
        }
        /// <summary>
        /// 介绍
        /// </summary>
        [BindField("介绍", MenuFieldType.SmallEditor)]
        public string Intro
        {
            get;
            set;
        }
        /// <summary>
        /// 排序
        /// </summary>
        public long ByOrder
        {
            get;
            set;
        }
        /// <summary>
        /// 语言
        /// </summary>
        [BindField("语言", MenuFieldType.Select)]
        public string Lang
        {
            get;
            set;
        }
        /// <summary>
        /// 特殊字段
        /// </summary>
        [BindField("特殊字段")]
        public string Data
        {
            get; set;
        }
        /// <summary>
        /// 特殊标识
        /// </summary>
        [BindField("标识", MenuFieldType.Select)]
        public List<int> Flags { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        [BindField("标签", MenuFieldType.Tag)]
        public List<string> Tags { get; set; }
    }
}
