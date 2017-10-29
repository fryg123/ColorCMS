using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Colorful.Models
{
    /// <summary>
    /// 用户权限对象
    /// </summary>
    public class UserPermission
    {
        private bool? _isAdmin = null;

        public UserPermission()
        {
            this.Permissions = new List<long>();
            this.Menus = new List<long>();
        }
        /// <summary>
        /// 用户权限
        /// </summary>
        public List<long> Permissions { get; set; }
        /// <summary>
        /// 用户菜单
        /// </summary>
        public List<long> Menus { get; set; }
        /// <summary>
        /// 是否为管理员
        /// </summary>
        public bool IsAdmin
        {
            get
            {
                if (_isAdmin == null)
                {
                    _isAdmin = this.Permissions.Contains((long)PermissionEnum.Admin);
                }
                return _isAdmin.Value;
            }
        }
    }
}
