using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using Colorful.Models;

namespace MongoDB.Driver
{
    public partial class MongodbContext
    {
        #region 表集
        /// <summary>
        /// 资源表
        /// </summary>
        public IMongoCollection<Resource> Resources
        {
            get
            {
                return _db.GetCollection<Resource>("Resource");
            }
        }
        /// <summary>
        /// 邮件
        /// </summary>
        public IMongoCollection<MailTemplate> MailTemplates
        {
            get
            {
                return _db.GetCollection<MailTemplate>("MailTemplate");
            }
        }
        /// <summary>
        /// 权限
        /// </summary>
        public IMongoCollection<Role> Roles
        {
            get
            {
                return _db.GetCollection<Role>("Roles");
            }
        }
        /// <summary>
        /// 文章
        /// </summary>
        public IMongoCollection<Article> Articles
        {
            get
            {
                return _db.GetCollection<Article>("Article");
            }
        }
        /// <summary>
        /// 标签表
        /// </summary>
        public IMongoCollection<Label> Labels
        {
            get
            {
                return _db.GetCollection<Label>("Label");
            }
        }
        /// <summary>
        /// 友情链接
        /// </summary>
        public IMongoCollection<Link> Links
        {
            get
            {
                return _db.GetCollection<Link>("Link");
            }
        }
        /// <summary>
        /// 菜单
        /// </summary>
        public IMongoCollection<Menu> Menus
        {
            get
            {
                return _db.GetCollection<Menu>("Menus");
            }
        }
        /// 菜单模块
        /// </summary>
        public IMongoCollection<MenuModule> MenuModules
        {
            get
            {
                return _db.GetCollection<MenuModule>("MenuModule");
            }
        }
        /// <summary>
        /// 模板
        /// </summary>
        public IMongoCollection<Template> Templates
        {
            get
            {
                return _db.GetCollection<Template>("Template");
            }
        }
        /// <summary>
        /// 管理员表
        /// </summary>
        public IMongoCollection<Administrator> Users
        {
            get
            {
                return _db.GetCollection<Administrator>("Administrator");
            }
        }
        /// <summary>
        /// 网站设置
        /// </summary>
        public IMongoCollection<WebSetting> WebSettings
        {
            get
            {
                return _db.GetCollection<WebSetting>("WebSettings");
            }
        }
        /// <summary>
        /// 权限
        /// </summary>
        public IMongoCollection<Permission> Permissions
        {
            get
            {
                return _db.GetCollection<Permission>("Permissions");
            }
        }
        /// <summary>
        /// 代码
        /// </summary>
        public IMongoCollection<Code> Codes
        {
            get
            {
                return _db.GetCollection<Code>("Code");
            }
        }
        /// <summary>
        /// 验证表
        /// </summary>
        public IMongoCollection<Verification> Verifications
        {
            get
            {
                return _db.GetCollection<Verification>("Verification");
            }
        }
        /// <summary>
        /// 国家列表
        /// </summary>
        public IMongoCollection<Country> Countries
        {
            get
            {
                return _db.GetCollection<Country>("Country");
            }
        }
        /// <summary>
        /// 国家Json数据
        /// </summary>
        public IMongoCollection<CountryJson> CountryJson
        {
            get
            {
                return _db.GetCollection<CountryJson>("CountryJson");
            }
        }
        /// <summary>
        /// 系统设置
        /// </summary>
        public IMongoCollection<SysSetting> SysSettings
        {
            get
            {
                return _db.GetCollection<SysSetting>("SysSetting");
            }
        }
        /// <summary>
        /// 垃圾箱
        /// </summary>
        public IMongoCollection<Trash> Trashs
        {
            get
            {
                return _db.GetCollection<Trash>("Trash");
            }
        }
        /// <summary>
        /// 操作历史
        /// </summary>
        public IMongoCollection<ActionHistory> ActionHistories
        {
            get
            {
                return _db.GetCollection<ActionHistory>("ActionHistory");
            }
        }
        /// <summary>
        /// 短消息
        /// </summary>
        public IMongoCollection<Message> Messages
        {
            get
            {
                return _db.GetCollection<Message>("Message");
            }
        }
        /// <summary>
        /// 公告
        /// </summary>
        public IMongoCollection<Notice> Notices
        {
            get
            {
                return _db.GetCollection<Notice>("Notice");
            }
        }
        /// <summary>
        /// 访客日志
        /// </summary>
        public IMongoCollection<VisitorLog> VisitorLogs
        {
            get
            {
                return _db.GetCollection<VisitorLog>("VisitorLog");
            }
        }
        /// <summary>
        /// 统计
        /// </summary>
        public IMongoCollection<Statistic> Statistics
        {
            get
            {
                return _db.GetCollection<Statistic>("Statistic");
            }
        }
        #endregion
    }
}
