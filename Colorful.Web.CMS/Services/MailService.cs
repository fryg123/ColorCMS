using Colorful.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using MailKit.Net.Smtp;
using MailKit;
using MimeKit;
using System.Collections.Specialized;

namespace Colorful.Web.CMS
{
    #region MailService
    public class MailService : IDisposable
    {
        #region 私有变量
        private SmtpClient _smtpClient = null;
        #endregion

        #region 构造函数
        public MailService(string smtp, string username, string password, int port = 25, bool enableSSL = false)
        {
            this.Connection = new SmtpInfo()
            {
                SMTP = smtp,
                Username = username,
                Password = password,
                Port = port,
                SSL = enableSSL
            };
            _smtpClient = new SmtpClient();
        }
        #endregion

        #region 属性
        public SmtpInfo Connection { get; set; }
        #endregion

        #region 发送邮件
        /// <summary>
        /// 同步方式发送邮件
        /// </summary>
        /// <param name="toEmail">要主送的邮件地址</param>
        /// <param name="subject">邮件标题</param>
        /// <param name="body">邮件内容</param>
        /// <param name="from">发件人Email</param>
        /// <param name="fromName">发件人名称</param>
        public void SendMail(string toEmail, string subject, string body, string from, string fromName)
        {
            this.SendMail(this.GetMail(toEmail, subject, body, from, fromName));
        }
        /// <summary>
        /// 同步发送Email
        /// </summary>
        /// <param name="mail">Email对象</param>
        public void SendMail(MimeMessage mail)
        {
            if (!_smtpClient.IsConnected)
            {
                _smtpClient.Connect(this.Connection.SMTP, this.Connection.Port, this.Connection.SSL);
                _smtpClient.AuthenticationMechanisms.Remove("XOAUTH2");
                _smtpClient.Authenticate(this.Connection.Username, this.Connection.Password);
            }
            _smtpClient.Send(mail);
        }
        /// <summary>
        /// 异步发送Email
        /// </summary>
        /// <param name="mail">Email对象</param>
        public async Task<bool> SendMailAsync(string toEmail, string subject, string body, string from, string fromName)
        {
            return await this.SendMailAsync(this.GetMail(toEmail, subject, body, from, fromName));
        }
        public async Task<bool> SendMailAsync(MimeMessage mail)
        {
            if (!_smtpClient.IsConnected)
            {
                await _smtpClient.ConnectAsync(this.Connection.SMTP, this.Connection.Port, this.Connection.SSL);
                _smtpClient.AuthenticationMechanisms.Remove("XOAUTH2");
                _smtpClient.Authenticate(this.Connection.Username, this.Connection.Password);
            }
            await _smtpClient.SendAsync(mail);
            return true;
        }
        /// <summary>
        /// 获取Email对象
        /// </summary>
        /// <param name="toEmail">要发送到的Email地址</param>
        /// <param name="subject">Email标题</param>
        /// <param name="body">Email内容</param>
        /// <param name="from">发件人地址</param>
        /// <param name="fromName">发件人名称</param>
        /// <param name="headers">Headers</param>
        /// <returns></returns>
        public MimeMessage GetMail(string toEmail, string subject, string body, string from, string fromName, NameValueCollection headers = null)
        {
            var mail = new MimeMessage()
            {
                Subject = subject,
                Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body }
            };
            mail.From.Add(new MailboxAddress(from, fromName));
            mail.To.Add(new MailboxAddress(fromName, toEmail));
            if (headers != null)
            {
                foreach (var key in headers.AllKeys)
                    mail.Headers.Add(key, headers[key]);
            }
            return mail;
        }

        public void Dispose()
        {
            _smtpClient.Disconnect(true);
            _smtpClient.Dispose();
        }
        #endregion
    }
    #endregion

    #region MailClient
    public class SmtpInfo
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string SMTP { get; set; }
        public int Port { get; set; }
        public bool SSL { get; set; }
    }
    #endregion
}
