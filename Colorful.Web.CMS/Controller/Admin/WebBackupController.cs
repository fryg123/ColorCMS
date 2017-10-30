using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Driver;
using Colorful.Models;
using System.Text;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using SevenZipNET;

namespace Colorful.Web.CMS.Controllers.Admin
{
    [MyAuth(PermissionEnum.WebSetting)]
    public class BackupController : AdminBaseController
    {
        #region 首页
        [Cache(10)]
        public IActionResult Index(long mid)
        {
            if (mid == 0)
                return this.GetTextResult("菜单不能为空！");
            if (!this.HasMenu(mid))
                return this.GetTextResult("无菜单访问权限！");

            var menu = this.Menus.FirstOrDefault(a => a.Id == mid);

            ViewBag.DelUrl = this.GetUrl("backup/del");
            ViewBag.DoUrl = this.GetUrl("backup/start");
            ViewBag.GoUrl = this.GetUrl($"backup?mid={mid}");
            ViewBag.Limit = this.WebSetting.BackupLimit;
            ViewBag.ShowBackup = true;
            List<BackupModel> list;
            if (!string.IsNullOrEmpty(this.WebSetting.AdminBackupPath))
            {
                if (!this.WebSetting.AdminBackupPath.StartsWith("/"))
                    this.WebSetting.AdminBackupPath = "/" + this.WebSetting.AdminBackupPath;
                var folder = FormHelper.MapPath(this.WebSetting.AdminBackupPath);
                FormHelper.CreateDirectory(folder);
                var files = System.IO.Directory.GetFiles(folder);
                list = files.Select(a => new BackupModel() { FileName = Path.GetFileName(a), FileUrl = $"{this.WebSetting.AdminBackupPath}/", Date = System.IO.File.GetCreationTime(a).ToString("yyyy/MM/dd HH:mm:ss") }).ToList();
                if (this.WebSetting.BackupLimit>0 && list.Count>=this.WebSetting.BackupLimit)
                {
                    ViewBag.ShowBackup = false;
                }
            }else
            {
                list = new List<BackupModel>();
            }
            return View(list);
        }
        #endregion

        #region 备份数据
        [Route("backup/start")]
        public IActionResult StartBackup()
        {
            if (!this.WebSetting.AdminBackupPath.StartsWith("/"))
                this.WebSetting.AdminBackupPath = "/" + this.WebSetting.AdminBackupPath;
            List<string> bakFolders;
            if (this.WebSetting.AdminBackupFolders == null || this.WebSetting.AdminBackupFolders.Count == 0)
                bakFolders = new string[] { "upFiles", "dbbak" }.ToList();
            else
                bakFolders = this.WebSetting.AdminBackupFolders;
            if (!bakFolders.Contains("img/uploadfiles"))
                bakFolders.Add("img/uploadfiles");
            var fileUrl = $"{this.WebSetting.AdminBackupPath}/{DateTime.Now.ToString("yyyyMMddHHmmss")}.zip";
            var filePath = FormHelper.MapPath(fileUrl);
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
            var backupFolder = FormHelper.MapPath(this.WebSetting.AdminBackupPath);
            FormHelper.CreateDirectory(backupFolder);
            var zipFile = new SevenZipCompressor(filePath);
            var filter = new string[] { "bin", "views", "templates", "js", "css", "fonts", "img" };
            foreach (var folder in bakFolders)
            {
                var f = folder.ToLower().Trim();
                if (filter.Contains(f) && f != "img/uploadfiles") continue;
                var folderPath = FormHelper.MapPath($"/{f}");
                if (!Directory.Exists(folderPath)) continue;
                zipFile.CompressDirectory(folderPath, CompressionLevel.Fast);
            }
            return this.GetResult(true, fileUrl);
        }
        #endregion

        #region 删除备份
        [Route("backup/del")]
        public IActionResult DelBackup(string fileName)
        {
            if (!this.WebSetting.AdminBackupPath.StartsWith("/"))
                this.WebSetting.AdminBackupPath = "/" + this.WebSetting.AdminBackupPath;
            var folder = FormHelper.MapPath(this.WebSetting.AdminBackupPath);
            var path = Path.Combine(folder, fileName);
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
            return this.GetResult(true);
        }
        #endregion
    }

    public class BackupModel
    {
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string Date { get; set; }
    }
}