using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Colorful.Models
{
    public class Client
    {
        //private HttpRequest _request;

        #region 属性
        /// <summary>
        /// 客户是否为手机端访问
        /// </summary>
        public bool IsMobile { get; set; }
        /// <summary>
        /// 客户是否为微信访问
        /// </summary>
        public bool IsWeiXin { get; set; }
        #endregion

        public Client(HttpRequest request)
        {
            //_request = request;
            string userAgent = $"{request.Headers["User-Agent"]}".ToLower();
            this.IsWeiXin = userAgent.Contains("micromessenger");
            this.IsMobile = this.IsWeiXin;
            if (!this.IsMobile)
            {
                var regex = new Regex(@"(iemobile|iphone|ipod|android|nokia|sonyericsson|blackberry|samsung|sec\-|windows ce|motorola|mot\-|up.b|midp\-)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                this.IsMobile = regex.IsMatch(userAgent);
            }
        }
    }
}
