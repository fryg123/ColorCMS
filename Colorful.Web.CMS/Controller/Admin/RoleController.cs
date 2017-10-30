using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using Colorful.Models;
using Microsoft.AspNetCore.Mvc;

namespace Colorful.Web.CMS.Controllers.Admin
{ 
    [MyAuth(PermissionEnum.WebSetting)]
    public class RoleController : AdminBaseController
    {
        #region 首页
        [Cache(10)]
        public IActionResult Index(long mid)
        {
            if (!this.HasMenu(mid))
                return this.GetResult(false, "无访问权限！");
            //权限
            ViewBag.DelUrl = this.GetUrl("permission/del");
            ViewBag.DataUrl = this.GetUrl("permission/list");
            ViewBag.SaveUrl = this.GetUrl("permission/save");
            //角色
            ViewBag.RoleDelUrl = this.GetUrl("role/del");
            ViewBag.RoleDataUrl = this.GetUrl("role/list");
            ViewBag.RoleSaveUrl = this.GetUrl("role/save");

            ViewBag.TreeUrl = this.GetUrl("permission/tree");
            ViewBag.MenuUrl = this.GetUrl("role/menus");
            ViewBag.SetPermissionUrl = this.GetUrl("role/setPermissions");

            return View();
        }
        #endregion

        #region 角色管理

        #region 获取角色列表
        [Route("role/list")]
        public IActionResult RoleList(string keyword)
        {
            using (var db = this.GetMongodb())
            {
                var query = db.Roles.GetQuery();
                if (!string.IsNullOrEmpty(keyword))
                    query = query.Where(a => a.Name.Contains(keyword));
                return this.GetDataGrid(query.OrderBy(a => a.Id).Select(a => new { a.Id, a.Name, a.Menus, a.Permissions, a.LastModify }));
            }
        }
        #endregion

        #region 保存角色
        [Route("role/save")]
        public IActionResult SaveRole(long id)
        {
            if (id == 0)
                return this.GetResult(false, "Id不能为空！");
            using (var db = this.GetMongodb())
            {
                var role = db.Roles.FirstOrDefault(a => a.Id == id);
                if (role == null)
                {
                    role = new Role()
                    {
                        Permissions = new List<long>(),
                        Menus = new List<long>()
                    };
                }
                FormHelper.FillTo(role);
                role.LastModify = DateTime.Now;
                role.ModifyUser = this.LoginId;
                db.Roles.Save(role);
                return this.GetResult(true);
            }
        }
        #endregion

        #region 删除角色
        [Route("role/del")]
        public IActionResult Del(long[] ids)
        {
            using (var db = this.GetMongodb())
            {
                var result = db.Roles.Delete(a => ids.Contains(a.Id));
                return this.GetResult(result.DeletedCount > 0);
            }
        }
        #endregion

        #region 获取菜单列表
        [Route("role/menus")]
        public IActionResult GetMenus(bool? open)
        {
            return this.GetMyMenus(open);
        }
        #endregion

        #region 设置角色权限
        [Route("role/setPermissions")]
        public IActionResult SetPermissions(long id, long[] permissions, long[] menus)
        {
            using (var db = this.GetMongodb())
            {
                var result = db.Roles.Update(a => a.Id == id, db.Roles.GetUpdate(b => b.Permissions, permissions.ToList())
                    .Set(b=>b.Menus, menus.ToList())
                    .Set(b => b.LastModify, DateTime.Now)
                    .Set(b => b.ModifyUser, this.LoginId));
                return this.GetResult(result.ModifiedCount > 0);
            }
        }
        #endregion

        #endregion

        #region 权限管理

        #region 获取权限列表
        [Route("permission/tree")]
        public IActionResult GetPermission()
        {
            using (var db = this.GetMongodb())
            {
                var userPermission = this.GetUserPermission();
                var permissions = userPermission.Permissions.Select(a => (long)a).ToArray();
                var query = db.Permissions.GetQuery();
                if (!this.IsAdmin)
                    query = query.Where(a => permissions.Contains(a.Id));
                var list = query.Select(a => new Tree<long>() { id = a.Id, text = a.Name }).ToList();
                return this.GetTree<long>(list);
            }
        }
        [Route("permission/list")]
        public IActionResult PermissionList(string keyword)
        {
            if (!this.HasPermission(PermissionEnum.SysSetting))
                return this.GetResult(false, "无操作权限！");
            using (var db = this.GetMongodb())
            {
                var query = db.Permissions.GetQuery();
                if (!string.IsNullOrEmpty(keyword))
                    query = query.Where(a => a.Name.Contains(keyword));
                return this.GetDataGrid(query.OrderBy(a => a.Id).Select(a => new { a.Id, a.Name, a.LastModify }));
            }
        }
        #endregion

        #region 保存权限
        [Route("permission/save")]
        public IActionResult PermissionSave(long id)
        {
            if (!this.HasPermission(PermissionEnum.SysSetting))
                return this.GetResult(false, "无操作权限！");
            using (var db = this.GetMongodb())
            {
                if (id == 0)
                    id = db.Permissions.GetMaxId();
                var permission = db.Permissions.FirstOrDefault(a => a.Id == id);
                if (permission == null)
                    permission = new Permission();
                FormHelper.FillTo(permission);
                permission.Id = id;
                permission.LastModify = DateTime.Now;
                permission.ModifyUser = this.LoginId;
                db.Permissions.Save(permission);
                return this.GetResult(true);
            }
        }
        #endregion

        #region 删除权限
        [Route("permission/del")]
        public IActionResult PermissionDel(long[] ids)
        {
            if (!this.HasPermission(PermissionEnum.SysSetting))
                return this.GetResult(false, "无操作权限！");
            using (var mongodb = this.GetMongodb())
            {
                var result = mongodb.Permissions.Delete(a => ids.Contains(a.Id));
                return this.GetResult(result.DeletedCount > 0);
            }
        }
        #endregion

        #endregion
    }
}