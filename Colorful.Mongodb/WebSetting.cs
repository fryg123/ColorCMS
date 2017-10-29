using System;
using System.Collections.Generic;
using System.Linq;

namespace Colorful.Models
{
    /// <summary>
    /// 网站设置
    /// </summary>
    public partial class WebSetting : BaseLongId
    {
        private EmailSetting _emailSetting;

        public string AdminDefaultUrl { get; set; }
        /// <summary>
        /// 后台标题
        /// </summary>
        public string AdminTitle { get; set; }
        /// <summary>
        /// 后台小标题
        /// </summary>
        public string AdminSubTitle { get; set; }
        /// <summary>
        /// 网站备份下载路径
        /// </summary>
        public string AdminBackupPath { get; set; }
        /// <summary>
        /// 网站需要备份的文件夹
        /// </summary>
        public List<string> AdminBackupFolders { get; set; }
        /// <summary>
        /// 最多备份数量
        /// </summary>
        public int BackupLimit { get; set; }
        /// <summary>
        /// 网站首页标题
        /// </summary>
        public string Title
        {
            get;
            set;
        }
        /// <summary>
        /// 网站关键字
        /// </summary>
        public string Keyword
        {
            get;
            set;
        }
        /// <summary>
        /// 网站描述
        /// </summary>
        public string Description
        {
            get;
            set;
        }
        /// <summary>
        /// 页脚
        /// </summary>
        public string Footer
        {
            get;
            set;
        }
        /// <summary>
        /// 数据缓存时间
        /// </summary>
        public int DataCacheTime { get; set; }
        /// <summary>
        /// Email设置
        /// </summary>
        public EmailSetting EmailSetting
        {
            get
            {
                if (_emailSetting == null)
                {
                    _emailSetting = new EmailSetting();
                }
                return _emailSetting;
            }
            set
            {
                _emailSetting = value;
            }
        }
    }

    #region EmailSetting
    public class EmailSetting
    {
        /// <summary>
        /// SMTP地址
        /// </summary>
        public string SMTP { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }
        public int Port { get; set; }
        /// <summary>
        /// 发件人Email
        /// </summary>
        public string SenderEmail { get; set; }
        /// <summary>
        /// 发件人名称
        /// </summary>
        public string SenderName { get; set; }
        /// <summary>
        /// 发件人名称（英文）
        /// </summary>
        public string SenderNameEn { get; set; }
    }
    #endregion
}