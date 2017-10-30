using Colorful.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Colorful.Web.CMS
{
    public class IdentityService
    {
        private BaseController _baseController;
        public static string AuthenticationScheme = null;

        public IdentityService(BaseController baseController)
        {
            _baseController = baseController;
        }

        #region 手机验证
        public bool CheckMobile(string mobile, string code)
        {
            return true;
        }
        #endregion

        #region 根据AppUser对象返回ClaimsPrincipal对象
        /// <summary>
        /// 根据AppUser对象返回ClaimsPrincipal对象
        /// </summary>
        /// <param name="appUser"></param>
        /// <returns></returns>
        public static ClaimsPrincipal GetIdentityUser(AppUser appUser)
        {
            var ci = new ClaimsIdentity(AuthenticationScheme);
            ci.AddClaim(new Claim("Id", appUser.Id));
            ci.AddClaim(new Claim("LoginId", appUser.LoginId));
            ci.AddClaim(new Claim("NumberId", appUser.NumberId.ToString()));
            ci.AddClaim(new Claim("Flag", appUser.Flag));
            if (appUser.Permissions != null && appUser.Permissions.Count > 0)
            {
                foreach (var permission in appUser.Permissions)
                {
                    ci.AddClaim(new Claim(ci.RoleClaimType, permission.ToString()));
                }
                //ci.AddClaim(new Claim(ClaimTypes.UserData, string.Join(",", appUser.Roles)));
            }
            return new ClaimsPrincipal(ci);
        }
        #endregion
    }
}
