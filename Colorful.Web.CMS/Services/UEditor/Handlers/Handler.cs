using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

namespace Colorful.Web.CMS
{
    /// <summary>
    /// Handler 的摘要说明
    /// </summary>
    public abstract class Handler
    {
        private string _userid;
        private string _userPath;

        /// <summary>
        /// 当前登陆用户Id
        /// </summary>
        protected string UserId
        {
            get
            {
                if (string.IsNullOrEmpty(_userid))
                {
                    _userid = this.Context.User.GetId();
                }
                return _userid;
            }
        }
        /// <summary>
        /// 当前登陆用户路径
        /// </summary>
        protected string UserPath
        {
            get
            {
                if (string.IsNullOrEmpty(_userPath))
                {
                    var flag = this.Context.User.GetFlag();
                    if (!string.IsNullOrEmpty(flag))
                        _userPath = $"{flag}/{this.UserId}";
                    else
                        _userPath = this.UserId;
                }
                return _userPath;
            }
        }

        public Handler(HttpContext context)
        {
            this.Request = context.Request;
            this.Response = context.Response;
            this.Context = context;
            //this.Server = context.Server;
        }

        public abstract void Process();

        protected void WriteJson(object response)
        {
            string jsonpCallback = Request.Get("callback"),
                json = JsonConvert.SerializeObject(response);
            if (String.IsNullOrWhiteSpace(jsonpCallback))
            {
                Response.Headers.Add("Content-Type", "text/plain");
                Response.WriteAsync(json);
            }
            else
            {
                Response.Headers.Add("Content-Type", "application/javascript");
                Response.WriteAsync(String.Format("{0}({1});", jsonpCallback, json));
            }
        }

        public HttpRequest Request { get; private set; }
        public HttpResponse Response { get; private set; }
        public HttpContext Context { get; private set; }
        //public HttpServerUtility Server { get; private set; }
    }
}