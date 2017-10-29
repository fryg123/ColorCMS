using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Colorful.Models
{
    /// <summary>
    /// 通用文章表
    /// </summary>
    public partial class Article : Base
    {
        private string _id = null;
        public string Id
        {
            get
            {
                if (_id == null)
                    _id = ObjectId.GenerateNewId().ToString();
                return _id;
            }
            set
            {
                _id = value;
            }
        }
        /// <summary>
        /// 数字Id
        /// </summary>
        public long NumberId { get; set; }
        /// <summary>
        /// 栏目编号
        /// </summary>
        public long MenuId { get; set; }
        /// <summary>
        /// 用户Id
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 是否发布
        /// </summary>
        public bool Publish { get; set; }
        /// <summary>
        /// 分类编号
        /// </summary>
        [BindField("分类", MenuFieldType.Select)]
        public int SortId { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        [BindField("标题")]
        public string Title { get; set; }
        /// <summary>
        /// 图片
        /// </summary>
        [BindField("缩略图", MenuFieldType.Image)]
        public string Photo { get; set; }
        /// <summary>
        /// 视频
        /// </summary>
        [BindField("视频", MenuFieldType.Video)]
        public string Video { get; set; }
        /// <summary>
        /// 文章来源
        /// </summary>
        [BindField("文章来源")]
        public string Source { get; set; }
        /// <summary>
        /// 作者
        /// </summary>
        [BindField("作者")]
        public string Author { get; set; }
        /// <summary>
        /// 简介
        /// </summary>
        [BindField("简介", MenuFieldType.SmallEditor)]
        public string Intro { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        [BindField("内容", MenuFieldType.Editor)]
        public string Content { get; set; }
        /// <summary>
        /// 查看次数
        /// </summary>
        public int ViewTimes { get; set; }
        /// <summary>
        /// 文章标识（如推荐、首页显示等）
        /// </summary>
        [BindField("文章标识", MenuFieldType.Select)]
        public List<int> Flags { get; set; }
        /// <summary>
        /// 审核状态
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 文章标签
        /// </summary>
        [BindField("标签", MenuFieldType.Tag)]
        public List<string> Tags { get; set; }
        /// <summary>
        /// 关键词
        /// </summary>
        [BindField("关键词")]
        public string Keyword { get; set; }
        /// <summary>
        /// 语言
        /// </summary>
        [BindField("语言", MenuFieldType.Select)]
        public string Lang { get; set; }
        /// <summary>
        /// 发布时间
        /// </summary>
        [BindField("发布时间", MenuFieldType.DateTime)]
        public DateTime AddDate { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public long ByOrder { get; set; }
        /// <summary>
        /// 置顶排序
        /// </summary>
        public int Top { get; set; }
    }
}