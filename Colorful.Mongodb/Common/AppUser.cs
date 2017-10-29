using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Colorful.Models
{
    [Serializable]
    public partial class AppUser
    {
        private string _nickname;
        private string _userPhoto;

        /// <summary>
        /// 用户唯一Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 内部数字Id
        /// </summary>
        public long NumberId { get; set; }
        /// <summary>
        /// 用户登录Id
        /// </summary>
        public string LoginId { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// SessionId
        /// </summary>
        public string SessionId { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        public string Sex { get; set; }
        /// <summary>
        /// 职位
        /// </summary>
        public string Position { get; set; }
        /// <summary>
        /// 部门
        /// </summary>
        public string Department { get; set; }
        /// <summary>
        /// 用户标识
        /// </summary>
        public string Flag { get; set; }
        /// <summary>
        /// 头像
        /// </summary>
        public string Photo
        {
            get
            {
                if (string.IsNullOrEmpty(_userPhoto))
                    _userPhoto = "/images/photos/default.png";
                return _userPhoto;
            }
            set
            {
                _userPhoto = value;
            }
        }
        /// <summary>
        /// 角色列表
        /// </summary>
        public List<long> Roles { get; set; }
        /// <summary>
        /// 权限列表
        /// </summary>
        public List<long> Permissions { get; set; }
        /// <summary>
        /// 菜单权限
        /// </summary>
        public List<long> Menus { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public string Nickname
        {
            get
            {
                if (string.IsNullOrEmpty(_nickname))
                {
                    if (string.IsNullOrEmpty(this.Name))
                        _nickname = this.LoginId;
                    else
                        _nickname = this.Name;
                }
                return _nickname;
            }
            set
            {
                _nickname = value;
            }
        }

        public AppUser() { }
    }
}