using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Colorful.Models;
using MongoDB.Driver;

namespace Colorful.Web.CMS.Controllers.Admin
{
    [Area("Admin")]
    [MyAuth(PermissionEnum.Admin)]
    public class AdminBaseController : WebBaseController
    {
        private List<Menu> _menus;
        private bool? _isAdmin = null;

        #region 属性
        /// <summary>
        /// 菜单列表
        /// </summary>
        protected List<Menu> Menus
        {
            get
            {
                if (_menus == null)
                {
                    using (var db = this.GetMongodb())
                        _menus = db.Menus.OrderBy(a => a.ByOrder).ToList();
                }
                return _menus;
            }
        }
        /// <summary>
        /// 是否为管理员
        /// </summary>
        public bool IsAdmin
        {
            get
            {
                if (_isAdmin == null)
                {
                    _isAdmin = this.User.IsAdmin();
                }
                return _isAdmin.Value;
            }
        }
        #endregion

        #region 方法
        /// <summary>
        /// 当前登陆用户是否有菜单访问权限
        /// </summary>
        /// <param name="menuId">菜单Id</param>
        /// <returns></returns>
        protected bool HasMenu(long menuId)
        {
            if (menuId == 0)
                return false;
            if (this.IsAdmin) return true;
            var menu = this.GetMenu(menuId);
            if (menu == null)
                return false;
            return this.AppUser.Menus.Contains(menuId);
        }
        /// <summary>
        /// 当前登陆用户是否有指定的权限
        /// </summary>
        /// <param name="permissions"></param>
        /// <returns></returns>
        protected bool HasPermission(params PermissionEnum[] permissions)
        {
            return this.User.HasPermissions(permissions);
        }
        /// <summary>
        /// 获取后台地址
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        protected virtual string GetUrl(string url)
        {
            if (url.StartsWith("/"))
                url = url.Substring(1);
            return $"/{MyWebConfig.AdminRoutePrefix}/{url}";
        }
        /// <summary>
        /// 获取菜单名称
        /// </summary>
        /// <param name="id">菜单Id</param>
        /// <returns></returns>
        protected Menu GetMenu(long id)
        {
            var url = Request.Path.HasValue ? Request.Path.Value : "";
            url = url.Replace($"/{MyWebConfig.AdminRoutePrefix}/", "");
            if (url.Contains("/"))
                url = url.Substring(0, url.IndexOf('/')) + "/";
            var menu = this.Menus.FirstOrDefault(a =>a.Id == id && a.Url != null && url.StartsWith(a.Url, StringComparison.OrdinalIgnoreCase));
            return menu;
        }
        #endregion

        #region 获取菜单访问接口
        protected IActionResult GetMyMenus(bool? open, int type = 1)
        {
            var treeList = new List<Tree<long>>();
            var menulist = this.Menus;
            var userMenus = this.AppUser.Menus;
            var rootMenus = menulist.Where(a =>a.Type == type && a.ParentId == 0).ToList();
            foreach (var menu in rootMenus)
            {
                if (!this.IsAdmin && !userMenus.Contains(menu.Id)) continue;
                var tree = new Tree<long>()
                {
                    text = menu.Name,
                    id = menu.Id,
                    icon = menu.Icon,
                    expand = open.GetValueOrDefault()
                };
                treeList.Add(tree);
                this.getMenuList(menulist, tree, open.GetValueOrDefault(), userMenus);
            }
            return this.GetTree(treeList, "根菜单");
        }
        private void getMenuList(List<Menu> menulist, Tree<long> tree, bool open, List<long> userMenus = null)
        {
            var children = menulist.Where(a => a.ParentId == tree.id).ToList();
            tree.children = new List<Tree<long>>();
            foreach (var menu in children)
            {
                if (!this.IsAdmin && userMenus != null && !userMenus.Contains(menu.Id))
                    continue;
                var cTree = new Tree<long>()
                {
                    id = menu.Id,
                    text = menu.Name,
                    icon = menu.Icon
                };
                tree.children.Add(cTree);
                getMenuList(menulist, cTree, open, userMenus);
            }
        }
        #endregion
    }
}
