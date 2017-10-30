using MongoDB.Driver;
using System;
using System.Linq;

namespace Colorful.Web.CMS.Controllers
{
    public class WebBaseController : BaseController
    {
        #region 属性
        /// <summary>
        /// Email解析Url
        /// </summary>
        protected string MailUrl
        {
            get
            {
                var url = this.ServerUrl;
                if (url.Contains("localhost") || url.Contains("192.168") || url.Contains("local.7csoft"))
                    url = "http://autoshanghai.org";
                return url;
            }
        }
        #endregion

        #region 发送邮件
        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="email">Email</param>
        /// <param name="title">标题</param>
        /// <param name="body">内容</param>
        /// <param name="sendUser">发件人</param>
        protected void SendMail(string email, string title, string body, string sendUser = null)
        {
            var mailSetting = WebConfig.WebSetting.EmailSetting;
            if (sendUser == null)
                sendUser = this.GetLang(mailSetting.SenderName, mailSetting.SenderNameEn);
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(delegate (object state)
            {
                using (var mailService = new MailService(mailSetting.SMTP, mailSetting.Username, mailSetting.Password, mailSetting.Port))
                {
                    try
                    {
                        mailService.SendMail(email, title, body, mailSetting.SenderEmail, sendUser);
                    }
                    catch (Exception ex)
                    {
                        WriteError(ex, "Email {0} send fail!", email);
                    }
                }
            }), null);
        }
        /// <summary>
        /// 发送Email
        /// </summary>
        /// <param name="email">要发送的Email地址</param>
        /// <param name="mailId">Email模板Id</param>
        /// <param name="replaceCallback">模板内容替换回调</param>
        protected void SendMail(string email, int mailId, Func<string, string> replaceCallback, string sendUser = null)
        {
            var key = $"#mail_{mailId}";
            var mailTemplate = this.GetCache(key, () =>
            {
                using (var db = this.GetMongodb())
                {
                    var mail = db.MailTemplates.FirstOrDefault(a => a.Id == mailId);
                    mail.Content = ParseMailContent(mail.Content);
                    return mail;
                }
            }, TimeSpan.FromSeconds(WebConfig.WebSetting.DataCacheTime));
            mailTemplate.Content = replaceCallback(mailTemplate.Content);
            this.SendMail(email, mailTemplate.Title, mailTemplate.Content, sendUser);
        }
        /// <summary>
        /// 解析Email模板
        /// </summary>
        /// <param name="mailContent"></param>
        /// <returns></returns>
        protected string ParseMailContent(string mailContent)
        {
            var url = this.MailUrl;
            return mailContent.Replace("src=\"/", $"src=\"{url}/").Replace("src='/", $"src='{url}/")
                    .Replace("href=\"/", $"href=\"{url}/").Replace("#Url#", url, StringComparison.OrdinalIgnoreCase);
        }
        #endregion
    }
}
