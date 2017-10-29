using System;
using System.Collections.Generic;
using System.Linq;

namespace Colorful.Models
{
    /// <summary>
    /// 操作历史
    /// </summary>
    public class ActionHistory : BaseId
    {
        /// <summary>
        /// 操作类型：见ActionType枚举（1-100）
        /// </summary>
        public ActionType Type { get; set; }
        /// <summary>
        /// 目标Id
        /// </summary>
        public string TargetId { get; set; }
        /// <summary>
        /// 菜单Id
        /// </summary>
        public long MenuId { get; set; }
        /// <summary>
        /// 分类Id
        /// </summary>
        public long SortId { get; set; }
        /// <summary>
        /// 操作前内容
        /// </summary>
        public string Before { get; set; }
        /// <summary>
        /// 操作后内容
        /// </summary>
        public string After
        {
            get;
            set;
        }
        /// <summary>
        /// 操作IP
        /// </summary>
        public string IP { get; set; }

        #region Ignore
        /// <summary>
        /// 创建ActionHistory
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">操作类型</param>
        /// <param name="targetId"></param>
        /// <param name="before"></param>
        /// <param name="after"></param>
        /// <param name="modifyUser"></param>
        /// <returns></returns>
        public static ActionHistory Create<T>(ActionType type, T targetId, string before, string after, string modifyUser, string ip)
        {
            var actionHis = new ActionHistory();
            actionHis.LastModify = DateTime.Now;
            actionHis.ModifyUser = modifyUser;
            actionHis.Before = before;
            actionHis.After = after;
            actionHis.TargetId = targetId.ToString();
            actionHis.IP = ip;
            return actionHis;
        }
        #endregion
    }
}