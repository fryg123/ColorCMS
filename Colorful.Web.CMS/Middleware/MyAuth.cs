using Colorful.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Colorful.Web.CMS
{
    #region 自定义验证Attr
    /// <summary>
    /// 自定义验证
    /// </summary>
    public class MyAuth : AuthorizeAttribute
    {
        /// <summary>
        /// 自定义验证
        /// </summary>
        /// <param name="permissions">子权限枚举</param>
        public MyAuth(params PermissionEnum[] permissions)
            : base()
        {
            this.Roles = string.Join(",", permissions.Select(a => (long)a).ToArray());
        }
        public MyAuth(params long[] permissions) : base()
        {
            this.Roles = string.Join(",", permissions);
        }
    }
    #endregion

    #region MyAuthorizeFilter
    public class MyAuthorizeFilter : AuthorizeFilter
    {
        public MyAuthorizeFilter(AuthorizationPolicy policy) : base(policy)
        {
        }

        public override Task OnAuthorizationAsync(Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext context)
        {
            // If there is another authorize filter, do nothing
            if (context.Filters.Any(item => item is IAsyncAuthorizationFilter && item != this))
            {
                return Task.FromResult(0);
            }

            //Otherwise apply this policy
            return base.OnAuthorizationAsync(context);
        }
    }
    #endregion

    #region UserExtension
    public static class UserExtension
    {
        #region 当前登录用户是否为管理员
        /// <summary>
        /// 当前登录用户是否为管理员
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static bool IsAdmin(this ClaimsPrincipal user)
        {
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
                return false;
            var flagClaim = user.Claims.FirstOrDefault(a => a.Type == "Flag");
            if (flagClaim == null || flagClaim.Value != "admin")
                return false;
            var loginIdClaim = user.Claims.FirstOrDefault(a => a.Type == "LoginId");
            return loginIdClaim != null && loginIdClaim.Value == "admin";
        }
        #endregion

        #region 获取用户Id
        /// <summary>
        /// 获取当前登录用户Id
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string GetId(this ClaimsPrincipal user)
        {
            if (user != null && user.Identity.IsAuthenticated)
            {
                var claim = user.Claims.FirstOrDefault(a => a.Type == "Id");
                if (claim != null)
                    return claim.Value;
            }
            return "";
        }
        #endregion

        #region 获取用户登录Id
        /// <summary>
        /// 获取当前登录用户的登录Id
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string GetLoginId(this ClaimsPrincipal user)
        {
            if (user != null && user.Identity.IsAuthenticated)
            {
                var claim = user.Claims.FirstOrDefault(a => a.Type == "LoginId");
                if (claim != null)
                    return claim.Value;
            }
            return "";
        }
        #endregion

        #region 获取用户NumberId
        /// <summary>
        /// 获取当前登录用户的登录Id
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string GetNumberId(this ClaimsPrincipal user)
        {
            if (user != null && user.Identity.IsAuthenticated)
            {
                var claim = user.Claims.FirstOrDefault(a => a.Type == "NumberId");
                if (claim != null)
                    return claim.Value;
            }
            return "";
        }
        #endregion

        #region 获取用户标识
        /// <summary>
        /// 获取当前登录用户的标识
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string GetFlag(this ClaimsPrincipal user)
        {
            if (user != null && user.Identity.IsAuthenticated)
            {
                var claim = user.Claims.FirstOrDefault(a => a.Type == "Flag");
                if (claim != null)
                    return claim.Value;
            }
            return "";
        }
        #endregion

        #region 验证用户权限
        /// <summary>
        /// 当前登录用户是否有指定的权限
        /// </summary>
        /// <param name="user"></param>
        /// <param name="permissions">PermissionEnum权限枚举</param>
        /// <returns></returns>
        public static bool HasPermissions(this ClaimsPrincipal user, params PermissionEnum[] permissions)
        {
            return user.HasPermissions(permissions.Select(a => (long)a).ToArray());
        }
        /// <summary>
        /// 当前登录用户是否有指定的权限
        /// </summary>
        /// <param name="user"></param>
        /// <param name="permissions">权限列表</param>
        /// <returns></returns>
        public static bool HasPermissions(this ClaimsPrincipal user, params long[] permissions)
        {
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
                return false;
            if (user.IsAdmin()) return true;
            var list = permissions.Select(a => a.ToString());
            return user.Claims.Count(a => a.Type == ClaimTypes.Role && list.Contains(a.Value)) == permissions.Length;
        }
        #endregion
    }
    #endregion
}
