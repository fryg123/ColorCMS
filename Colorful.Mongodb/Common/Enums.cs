using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace Colorful.Models
{
    #region 资源枚举 ResourceType
    public enum ResourceType: int
    {
        [Description("图片")]
        Image = 1,
        [Description("视频")]
        Video = 2,
        [Description("文档")]
        Doc = 3,
        [Description("压缩文件")]
        Zip = 4,
        [Description("音频")]
        Audio = 5,
        [Description("其他")]
        Other = 6
    }
    #endregion

    #region 权限枚举 Permission
    public enum PermissionEnum : int
    {
        /// <summary>
        /// 后台管理
        /// </summary>
        [Description("后台管理")]
        Admin = 1,
        /// <summary>
        /// 编辑（组合权限，可用于用户编辑、新闻编辑等）
        /// </summary>
        [Description("编辑")]
        Edit = 2,
        /// <summary>
        /// 删除（组合权限，可用于用户删除、新闻删除等）
        /// </summary>
        [Description("删除")]
        Delete = 3,
        /// <summary>
        /// 网站设置
        /// </summary>
        [Description("网站设置")]
        WebSetting = 4,
        /// <summary>
        /// 用户管理
        /// </summary>
        [Description("用户管理")]
        UserAdmin = 5,
        /// <summary>
        /// 审核
        /// </summary>
        [Description("审核权限")]
        Audit = 6,
        /// <summary>
        /// 系统设置
        /// </summary>
        [Description("系统设置")]
        SysSetting = 7
    }
    #endregion

    #region 菜单标识 MenuFlag
    /// <summary>
    /// 菜单标识
    /// </summary>
    public enum MenuFlag : int
    {
        /// <summary>
        /// 多语言
        /// </summary>
        [Description("多语言")]
        Lang = 1,
        /// <summary>
        /// 不可删除
        /// </summary>
        [Description("不可删除")]
        NoDelete = 2,
        /// <summary>
        /// 删除保护
        /// </summary>
        [Description("删除保护")]
        DeleteProtect = 3,
        /// <summary>
        /// 记录操作历史
        /// </summary>
        [Description("记录操作历史")]
        RecordHistory = 4,
        /// <summary>
        /// 审核
        /// </summary>
        [Description("审核")]
        Audit = 6
    }
    #endregion

    #region 活动状态 EventStatus
    /// <summary>
    /// 活动状态
    /// </summary>
    public enum EventStatus
    {
        /// <summary>
        /// 即将开始
        /// </summary>
        [Description("即将开始")]
        Coming = 1,
        /// <summary>
        /// 活动进行中
        /// </summary>
        [Description("活动进行中")]
        Starting = 2,
        /// <summary>
        /// 活动已结束
        /// </summary>
        [Description("活动已结束")]
        End = 3
    }
    #endregion

    #region 字典类别 CodeFlag
    /// <summary>
    /// 字典类别
    /// </summary>
    public enum CodeFlag
    {
        [Description("无标记")]
        None = 0,
        [Description("单选")]
        Single = 1,
        [Description("多选")]
        Multiple = 2,
        [Description("单选输入框")]
        SingleText = 3,
        [Description("多选输入框")]
        MultipleText = 4
    }
    #endregion

    #region 支付状态 PaymentStatus
    /// <summary>
    /// 支付状态
    /// </summary>
    public enum PaymentStatus : int
    {
        /// <summary>
        /// 支付中
        /// </summary>
        Paying = 1,
        /// <summary>
        /// 成功
        /// </summary>
        Success = 2,
        /// <summary>
        /// 失败
        /// </summary>
        Faild = 3
    }
    #endregion

    #region 语言枚举 LangEnum
    /// <summary>
    /// 语言枚举
    /// </summary>
    public enum LangEnum
    {
        /// <summary>
        /// 中文
        /// </summary>
        [Description("中文")]
        CN,
        /// <summary>
        /// 英文
        /// </summary>
        [Description("英文")]
        EN,
        /// <summary>
        /// 手机中文
        /// </summary>
        [Description("手机中文")]
        WAPCN,
        /// <summary>
        /// 手机英文
        /// </summary>
        [Description("手机英文")]
        WAPEN
    }
    #endregion

    #region 审核状态 AuditingState
    /// <summary>
    /// 审核状态
    /// </summary>
    public enum AuditingStatus : int
    {
        /// <summary>
        /// 无审核状态
        /// </summary>
        [Description("无")]
        None = 0,
        /// <summary>
        /// 审核中
        /// </summary>
        [Description("审核中")]
        Waiting = 1,
        /// <summary>
        /// 审核未通过
        /// </summary>
        [Description("审核未通过")]
        Fail = 2,
        /// <summary>
        /// 审核成功
        /// </summary>
        [Description("审核成功")]
        Success = 3
    }
    #endregion

    #region 垃圾箱种类 TrashType
    public enum TrashType : int
    {
        /// <summary>
        /// 文章 
        /// </summary>
        Article = 1,
        /// <summary>
        /// 菜单
        /// </summary>
        Menu = 2,
        /// <summary>
        /// 用户
        /// </summary>
        User = 3,
        /// <summary>
        /// Email模板
        /// </summary>
        MailTemplate = 4,
        /// <summary>
        /// 角色
        /// </summary>
        Role = 5,
        /// <summary>
        /// 模板
        /// </summary>
        Template = 6,
        /// <summary>
        /// 代码
        /// </summary>
        Code = 7,
        /// <summary>
        /// 活动
        /// </summary>
        Event = 8,
        /// <summary>
        /// 会员
        /// </summary>
        Member = 9,
        /// <summary>
        /// Link表
        /// </summary>
        Link = 10,
        /// <summary>
        /// 用户消息
        /// </summary>
        Message = 11
    }
    #endregion

    #region 操作类型 ActionType
    public enum ActionType : int
    {
        /// <summary>
        /// 修改密码
        /// </summary>
        [Description("修改密码")]
        ModifyPassword = 1,
        /// <summary>
        /// 修改文章 
        /// </summary>
        [Description("修改文章")]
        Article = 2,
        /// <summary>
        /// 修改活动
        /// </summary>
        [Description("修改活动")]
        Event = 3,
        /// <summary>
        /// 修改链接
        /// </summary>
        [Description("修改链接")]
        Link = 4,
        /// <summary>
        /// 修改代码
        /// </summary>
        [Description("修改代码")]
        Code = 5,
        /// <summary>
        /// 修改网站设置
        /// </summary>
        [Description("修改网站设置")]
        WebSetting = 6,
        /// <summary>
        /// 修改角色
        /// </summary>
        [Description("修改角色")]
        Role = 7,
        /// <summary>
        /// 修改用户
        /// </summary>
        [Description("修改用户")]
        User = 8,
        /// <summary>
        /// 修改邮件
        /// </summary>
        [Description("修改邮件")]
        MailTemplate = 9
    }
    #endregion

    #region 菜单模块字段类型 MenuFieldType
    public enum MenuFieldType
    {
        [Description("输入框")]
        Text,
        [Description("图片")]
        Image,
        [Description("文件")]
        File,
        [Description("视频")]
        Video,
        [Description("简易编辑器")]
        SmallEditor,
        [Description("全功能编辑器")]
        Editor,
        [Description("日期")]
        Date,
        [Description("日期和时间")]
        DateTime,
        [Description("选择框")]
        Select,
        [Description("标签框")]
        Tag,
        [Description("文本框")]
        TextArea 
    }
    #endregion

    #region 发送对象 SendTarget
    public enum SendTarget
    {
        /// <summary>
        /// 所有人
        /// </summary>
        [Description("所有人")]
        All = 0,
        /// <summary>
        /// 普通用户
        /// </summary>
        [Description("普通用户")]
        User = 1
    }
    #endregion
}