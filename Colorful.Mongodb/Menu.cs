using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Colorful.Models
{
    /// <summary>
    /// 菜单表
    /// </summary>
    public class Menu : BaseLongId
    {
        public Menu()
        {
        }
        /// <summary>
        /// 父编号
        /// </summary>
        public long ParentId
        {
            get;
            set;
        }
        /// <summary>
        /// 菜单类型
        /// </summary>
        public int Type {
            get;
            set;
        }
        /// <summary>
        /// 菜单名称
        /// </summary>
        public string Name
        {
            get;
            set;
        }
        /// <summary>
        /// 菜单英文名称
        /// </summary>
        public string NameEN
        {
            get;
            set;
        }
        /// <summary>
        /// 链接地址
        /// </summary>
        public string Url
        {
            get;
            set;
        }
        /// <summary>
        /// 参数配置
        /// </summary>
        public MenuConfig Config { get; set; }
        /// <summary>
        /// 菜单图标
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public long ByOrder
        {
            get;
            set;
        }
        /// <summary>
        /// 菜单Token
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// 标识
        /// </summary>
        public List<int> Flags
        {
            get;
            set;
        }

        #region Ignore
        [BsonIgnore]
        public List<Menu> Children { get; set; }
        /// <summary>
        /// 获取参数配置列表
        /// </summary>
        /// <returns></returns>
        public List<string> GetConfigs()
        {
            return null;
        }
        /// <summary>
        /// 菜单是否包含指定标识
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        public bool HasFlag(MenuFlag flag)
        {
            if (this.Flags == null) return false;
            return this.Flags.Contains((int)flag);
        }
        #endregion
    }

    #region MenuJson
    public class MenuJson
    {
        public MenuJson(Menu menu)
        {
            this.id = menu.Id;
            this.text = menu.Name;
            this.url = menu.Url;
            this.icon = menu.Icon;
        }
        public long id { get; set; }
        public string text { get; set; }
        public string url { get; set; }
        public string icon { get; set; }
        public List<MenuJson> children { get; set; }
    }
    #endregion

    #region MenuConfig
    public class MenuConfig
    {
        /// <summary>
        /// 栏目字段列表
        /// </summary>
        public List<MenuConfigField> Fields { get; set; }
    }
    public class MenuConfigMaxSize
    {
        public int Width { get; set; }
        public int? Height { get; set; }
    }
    public class MenuConfigDataSource
    {
        /// <summary>
        /// 数据类型：Enum（枚举）、Data（IDataSource数据源）、Json（自定义Json数据）
        /// </summary>
        public string DataType { get; set; }
        /// <summary>
        /// 绑定的数据
        /// </summary>
        public string Data { get; set; }
    }
    public class MenuConfigField
    {
        /// <summary>
        /// 字段名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 字段标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 是否为必填项
        /// </summary>
        public bool Required { get; set; }
        /// <summary>
        /// 字段类型
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 长度限制
        /// </summary>
        public int MaxLength { get; set; }
        /// <summary>
        /// 最大数量
        /// </summary>
        public int MaxCount { get; set; }
        /// <summary>
        /// 最少数量
        /// </summary>
        public int MinCount { get; set; }
        /// <summary>
        /// 限制文件格式
        /// </summary>
        public string Format { get; set; }
        /// <summary>
        /// 是否为多选
        /// </summary>
        public bool Multiple { get; set; }
        /// <summary>
        /// 大小限制
        /// </summary>
        public MenuConfigMaxSize MaxSize { get; set; }
        /// <summary>
        /// 数据源
        /// </summary>
        public MenuConfigDataSource DataSource { get; set; }
    }
    #endregion
}