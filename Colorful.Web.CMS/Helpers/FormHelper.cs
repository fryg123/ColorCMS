using Ganss.XSS;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Colorful.Web.CMS
{
    public static class FormHelper
    {
        private static string[] _allowFiles = new string[] { ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".flv", ".swf", ".mkv", ".avi", ".rm", ".rmvb", ".mpeg", ".mpg", ".ogg", ".ogv", ".mov", ".wmv", ".mp4", ".webm", ".mp3", ".wav", ".mid", ".rar", ".zip", ".tar", ".gz", ".7z", ".bz2", ".cab", ".iso", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".pdf", ".txt", ".md", ".xml" };
        private static string[] _allowImages = new string[] { ".png", ".jpg", ".gif", ".webp" };
        /// <summary>
        /// 以安全模式自动填充表单内容到指定对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">要填充的对象</param>
        /// <param name="fields">要填充的字段></param>
        public static void SafeFill<T>(T target, params FormField[] fields)
        {
            FillTo(target, "CN", "", true, fields);
        }
        /// <summary>
        /// 以安全模式自动填充表单内容到指定对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">要填充的对象</param>
        /// <param name="lang">当前语言</param>
        /// <param name="fields">要填充的字段</param>
        public static void SafeFill<T>(T target, string lang, params FormField[] fields)
        {
            FillTo(target, lang, "", true, fields);
        }
        /// <summary>
        /// 自动填充表单内容到指定对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">要填充的对象</param>
        /// <param name="fields">要校验的字段</param>
        public static void FillTo<T>(T target, params FormField[] fields)
        {
            FillTo(target, "CN", "", false, fields);
        }
        /// <summary>
        /// 自动填充表单内容到指定对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">要填充的对象</param>
        /// <param name="lang">当前语言</param>
        /// <param name="fields">要校验的字段</param>
        public static void FillTo<T>(T target, string lang, params FormField[] fields)
        {
            FillTo(target, lang, "", false, fields);
        }
        /// <summary>
        /// 自动填充表单内容到指定对象
        /// </summary>
        /// <typeparam name="T">要填充的对象类型</typeparam>
        /// <param name="target">要填充的对象</param>
        /// <param name="lang">当前语言</param>
        /// <param name="targetName">反射类型名称</param>
        /// <param name="safeMode">是否启用安全模式（没在fieldFilters中指定的字段不予填充）</param>
        /// <param name="fields">要校验的字段</param>
        private static void FillTo<T>(T target, string lang, string targetName, bool safeMode, params FormField[] fields)
        {
            if (target == null)
                return;
            var type = target.GetType();
            var pList = target.GetType().GetProperties();
            List<string> allKeys;
            if (!string.IsNullOrEmpty(targetName))
            {
                allKeys = HttpContext.Current.Request.Form.Where(a => a.Key.ToLower().StartsWith(targetName.ToLower())).Select(a => a.Key.ToLower()).ToList();
                allKeys.AddRange(HttpContext.Current.Request.Form.Files.Where(a => a.Name.ToLower().StartsWith(targetName.ToLower())).Select(a => a.Name.ToLower()));
            }
            else
            {
                allKeys = System.Web.HttpContext.Current.Request.Form.Select(a => a.Key.ToLower()).ToList();
                allKeys.AddRange(HttpContext.Current.Request.Form.Files.Select(a => a.Name.ToLower()));
            }
            foreach (var pInfo in pList)
            {
                string pName = targetName + pInfo.Name;
                try
                {
                    if (!allKeys.Contains(pName.ToLower()))
                    {
                        if (allKeys.Any(a => a.StartsWith(pName.ToLower() + ".")))
                        {
                            var pValue = pInfo.GetValue(target);
                            FillTo(pValue, lang, pName + ".", safeMode, fields);
                        }
                        if (!safeMode)
                            continue;
                    }
                    Type targetType;
                    if (pInfo.PropertyType.Name == "Nullable`1")
                        targetType = pInfo.PropertyType.GetProperty("Value").PropertyType;
                    else
                        targetType = pInfo.PropertyType;
                    if (fields.Any(a => a.Name == pName && a.Disabled))
                        continue;
                    if (safeMode && !fields.Any(a => a.Name == pName)) //安全模式下若没指定字段则自动过滤
                        continue;

                    string value = System.Web.HttpContext.Current.Request.Form[pName];
                    var field = fields.FirstOrDefault(a => a.Name == pName);
                    var fileField = field as FileField;
                    var isFile = fileField != null;
                    var isEditor = field != null && field.Type == FieldType.Editor;
                    var htmlXXS = new HtmlSanitizer();
                    //if (!string.IsNullOrEmpty(value) && !isFile && !isEditor)
                    //{
                    //    value = HttpUtility.HtmlEncode(value);
                    //}
                    if (isEditor && (field as EditorField).XXSFilter)
                    {
                        value = htmlXXS.Sanitize(value);
                    }

                    #region 处理文件类型
                    if (isFile)
                    {
                        var fileList = new List<string>();
                        var files = HttpContext.Current.Request.Form.Files.GetFiles(field.Name);
                        foreach (var file in files)
                        {
                            //上传文件
                            var fileName = UploadFile(fileField, file, lang);
                            if (fileName == null) continue;

                            #region 删除源文件
                            //如果为多个文件
                            if (fileField.MaxCount > 1)
                            {
                                var list = (List<string>)pInfo.GetValue(target, null);
                                if (list != null && list.Count > 0)
                                {
                                    var reqFiles = HttpContext.Current.Request.Form.Files.GetFiles(pName + "_file");
                                    List<string> editFiles;
                                    if (reqFiles != null && reqFiles.Count > 0)
                                        editFiles = reqFiles.Select(a => a.FileName).ToList();
                                    else
                                        editFiles = new List<string>();
                                    foreach (var item in list)
                                    {
                                        if (editFiles.Contains(item))
                                        {
                                            fileList.Add(item);
                                            continue;
                                        }
                                        var fPath = MapPath(item);
                                        if (!fileField.KeepSourceFile && File.Exists(fPath))
                                            File.Delete(fPath);
                                    }

                                }
                            }
                            else
                            {
                                var sourceFilePath = string.Format("{0}", pInfo.GetValue(target, null));
                                if (!string.IsNullOrEmpty(sourceFilePath))
                                {
                                    sourceFilePath = MapPath(sourceFilePath);
                                    if (!fileField.KeepSourceFile && File.Exists(sourceFilePath))
                                        File.Delete(sourceFilePath);
                                }
                            }
                            #endregion

                            fileList.Add(fileName);
                        }
                        if (fileField.MinCount > 0)
                        {
                            if (fileList.Count < fileField.MinCount && field.Required)
                            {
                                var sourceValue = pInfo.GetValue(target, null);
                                if (sourceValue == null)
                                    throw new InvalidException($"{field.Text}至少选择{fileField.MinCount}个文件！");
                            }
                            else if (fileList.Count > fileField.MaxCount)
                                throw new InvalidException($"{field.Text}最多只能上传{fileField.MaxCount}个文件！");
                        }
                        else if (field.Required && fileList.Count == 0)
                        {
                            var sourceValue = pInfo.GetValue(target, null);
                            if (sourceValue == null)
                                throw new InvalidException(lang == "CN" ? $"{field.Text}不能为空！" : $"{field.Text}Can not be empty!");
                        }
                        if (fileList.Count == 1)
                            value = fileList[0];
                        else
                            value = string.Join(",", fileList);
                        if (string.IsNullOrEmpty(value))
                            continue;
                    }
                    #endregion
                    #region 过滤特殊字符
                    else
                    {
                        value = FilterInput(value);
                    }
                    #endregion
                    var oValue = value.ConvertTo(targetType);

                    #region 验证
                    if (field != null)
                    {
                        #region 必填项验证
                        if (field.Required && string.IsNullOrEmpty(value) && field.Type != FieldType.Image && field.Type != FieldType.Video && field.Type != FieldType.File)
                        {
                            throw new InvalidException(field.Message ?? (lang == "CN" ? $"{field.Text}不能为空！" : $"{field.Text}Can not be empty!"));
                        }
                        #endregion
                        if (!isFile)
                        {
                            #region 最少字符数验证
                            if (field.MinLength > 0 && (string.IsNullOrEmpty(value) || value.Length < field.MinLength))
                                throw new InvalidException(field.Message ?? (lang == "CN" ? $"{field.Text}至少输入{field.MinLength}个字符！" : $"{field.Text} Enter at least {field.MinLength} characters!"));
                            #endregion
                            #region 最多字符数验证
                            if (field.MaxLength > 0 && (string.IsNullOrEmpty(value) || value.Length > field.MaxLength))
                                throw new InvalidException(field.Message ?? (lang == "CN" ? $"{field.Text}不能大于{field.MaxLength}个字符！" : $"{field.Text} Enter up to {field.MinLength} characters!"));
                            #endregion
                        }
                        #region Email
                        if (field.Type == FieldType.Email && (string.IsNullOrEmpty(value) || !IsEmail(value)))
                            throw new InvalidException(field.Message ?? (lang == "CN" ? "Email格式不正确！" : "Email format is incorrect!"));
                        #endregion
                        #region Mobile
                        if (field.Type == FieldType.Mobile && (string.IsNullOrEmpty(value) || !Regex.IsMatch(value, @"^1[0-9]{10,10}$")))
                            throw new InvalidException(field.Message ?? "手机格式不正确！");
                        #endregion
                        #region 整数
                        if (field.Type == FieldType.Integer && (string.IsNullOrEmpty(value) || !Regex.IsMatch(value, @"^[0-9]{1,}$")))
                            throw new InvalidException(field.Message ?? $"字段【{field.Text}】只能为整数！");
                        #endregion
                        #region 整数
                        if (field.Type == FieldType.Number && (string.IsNullOrEmpty(value) || !Regex.IsMatch(value, @"^[\d]{1,}$")))
                            throw new InvalidException(field.Message ?? $"字段【{field.Text}】只能为数字类型！");
                        #endregion
                    }
                    #endregion

                    #region 处理编辑器中的图片
                    if (field != null && field is EditorField)
                    {
                        var editorField = field as EditorField;
                        if (editorField.AutoDownloadImages)
                        {
                            string savePath;
                            if (field.Data != null && field.Data.Path != null)
                                savePath = field.Data.Path;
                            else
                                savePath = $"/upFiles/article/remote/{DateTime.Now.ToString("yyyyMMdd")}";

                            #region 下载编辑器中的图片
                            value = string.Format("{0}", oValue);
                            var imageResult = Regex.Matches(value, "<img.*?src=[\"'](http[s]?://[^\"|']+)", RegexOptions.IgnoreCase);
                            if (imageResult.Count > 0)
                            {
                                string dictionary = MapPath(savePath) + "\\";
                                CreateDirectory(dictionary);
                                foreach (Match imageMatch in imageResult)
                                {
                                    string imageUrl = imageMatch.Groups[1].Value;
                                    if (imageUrl.ToLower().IndexOf("http:") == 0)
                                    {
                                        try
                                        {
                                            var fileName = $"{Path.GetRandomFileName()}.jpg";
                                            HttpHelper.DownloadFile(imageUrl, Path.Combine(dictionary, fileName));
                                            value = value.Replace(imageUrl, savePath + "/" + fileName);
                                        }
                                        catch
                                        {
                                            imageUrl = string.Empty;
                                        }
                                    }
                                }
                            }
                            oValue = value;
                            #endregion
                        }
                    }
                    #endregion

                    if (field == null || !(field.Required && string.IsNullOrEmpty(value)))
                        pInfo.SetValue(target, oValue, null);
                }
                catch (Exception ex)
                {
                    if (ex is InvalidException || ex is InvalidException)
                        throw ex;
                    else
                        throw new Exception($"字段：{pName}，值：{System.Web.HttpContext.Current.Request.Form[pName]}发生错误：", ex);
                }
            }
        }
        /// <summary>
        /// 填充对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">要填充的对象</param>
        /// <param name="fieldName">要填充的属性名称</param>
        /// <param name="value">要填充的值</param>
        public static void SetData<T>(T target, string fieldName, string value)
        {
            var pInfo = target.GetType().GetProperty(fieldName);
            if (pInfo == null) return;
            Type targetType;
            if (pInfo.PropertyType.Name == "Nullable`1")
                targetType = pInfo.PropertyType.GetProperty("Value").PropertyType;
            else
                targetType = pInfo.PropertyType;
            var oValue = value.ConvertTo(targetType);
            pInfo.SetValue(target, oValue, null);
        }
        /// <summary>
        /// 根据指定属于获取对象值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static string GetData<T>(T target, string fieldName)
        {
            var pInfo = target.GetType().GetProperty(fieldName);
            if (pInfo == null) return null;
            return $"{pInfo.GetValue(target)}";
        }
        #region 验证文件安全
        /// <summary>
        /// 是否为有效的文件
        /// </summary>
        /// <param name="fileName">用户上传的文件名</param>
        /// <returns></returns>
        public static bool IsFile(string fileName)
        {
            return !Path.GetInvalidFileNameChars().Any(a => fileName.Contains(a.ToString()));
        }
        /// <summary>
        /// 指定文件是否为图片格式
        /// </summary>
        /// <param name="fileName">用户上传的文件名</param>
        /// <returns></returns>
        public static bool IsImage(string fileName)
        {
            if (!IsFile(fileName))
                return false;
            var ext = Path.GetExtension(fileName).ToLower();
            return _allowImages.Contains(ext);
        }
        /// <summary>
        /// 指定文件是否为视频格式
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool IsVideo(string fileName)
        {
            if (!IsFile(fileName))
                return false;
            var allowExt = new string[] { ".mp4", ".flv", ".ogg", ".webm", ".wav", ".rtmp" };
            var ext = Path.GetExtension(fileName).ToLower();
            return allowExt.Contains(ext);
        }
        /// <summary>
        /// 是否为音频文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool IsAudio(string fileName)
        {
            if (!IsFile(fileName))
                return false;
            var allowExt = new string[] { "mp3", "wav", "wma", "ogg", "aac", "flac" };
            var ext = Path.GetExtension(fileName).ToLower();
            return allowExt.Contains(ext);
        }
        /// <summary>
        /// 指定的文件是否为文档格式
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool IsDoc(string fileName)
        {
            if (!IsFile(fileName))
                return false;
            var allowExt = new string[] { ".doc", ".docx", ".ppt", ".pptx", ".pdf", ".txt", ".xls", ".xlsx" };
            var ext = Path.GetExtension(fileName).ToLower();
            return allowExt.Contains(ext);
        }
        /// <summary>
        /// 指定的文件是否为压缩文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool IsZip(string fileName)
        {
            if (!IsFile(fileName))
                return false;
            var allowExt = new string[] { "zip", "rar", "7z" };
            var ext = Path.GetExtension(fileName).ToLower();
            return allowExt.Contains(ext);
        }
        /// <summary>
        /// 是否为安全扩展名
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool IsSecurityFile(string fileName)
        {
            if (!IsFile(fileName))
                return false;
            var ext = Path.GetExtension(fileName).ToLower();
            return _allowFiles.Contains(ext);
        }
        #endregion

        /// <summary>
        /// 是否为Email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool IsEmail(string email)
        {
            return Regex.IsMatch(email, @"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
        }

        /// <summary>
        /// 是否为手机号码
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public static bool IsMobile(string mobile)
        {
            return Regex.IsMatch(mobile, @"^1[3458][0-9]{9}$");
        }
        /// <summary>
        /// 根据最大文件大小获取说明
        /// </summary>
        /// <param name="maxsize"></param>
        /// <returns></returns>
        public static string GetSize(long maxsize)
        {
            if (maxsize > 1024 * 1024)
                return $"{maxsize / 1024 / 1024}M";
            else
                return $"{maxsize / 1024}KB";
        }
        /// <summary>
        /// 根据指定地址获取本地路径，若不存在则自动创建
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetFolder(string url)
        {
            var folder = MapPath(url);
            CreateDirectory(folder);
            return folder;
        }
        /// <summary>
        /// 压缩图片
        /// </summary>
        /// <param name="filePath">图片路径</param>
        /// <param name="maxSize">最大宽高</param>
        /// <param name="quality">压缩率</param>
        /// <param name="savePath">保存路径</param>
        public static void CompressImage(string filePath, Size maxSize, int quality = 80, string savePath = null)
        {
            if (savePath == null)
                savePath = filePath;
            new ImageHelper().Compress(filePath, maxSize, savePath, quality);
        }

        #region 安全方法

        #region Server.MapPath
        public static string MapPath(string url)
        {
            if (!url.StartsWith("/"))
                throw new InvalidException("无效的路径！");
            var staticFolders = WebConfig.WebStaticFolders;
            if (staticFolders.Any(a => url.StartsWith("/" + a, StringComparison.OrdinalIgnoreCase)))
                url = "/wwwroot" + url;
            var rootPath = WebConfig.RootPath;
            var path = rootPath + url.Replace("/", "\\");
            return path;
        }
        #endregion

        #region 删除文件
        /// <summary>
        /// 删除指定的文件
        /// </summary>
        /// <param name="files">要删除的文件</param>
        public static void DeleteFiles(params string[] files)
        {
            if (files.Length == 0) return;
            foreach (var fileUrl in files)
            {
                if (!fileUrl.StartsWith("/upFiles") && fileUrl.Length < 10)
                    throw new InvalidException("无效的路径！");
                var filePath = MapPath(fileUrl);
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }
        }
        #endregion

        #region 删除指定目录
        /// <summary>
        /// 删除指定目录
        /// </summary>
        /// <param name="url">要删除的目录Url</param>
        public static void DeleteDirectory(string url)
        {
            if (!url.StartsWith("/upFiles/") && url.Length < 10)
                throw new InvalidException("无效的路径！");
            var path = MapPath(url);
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }
        #endregion

        #region 创建目录
        public static void CreateDirectory(string path)
        {
            if (!path.Contains(@"\upFiles\") && !path.Contains(@"\backup_"))
                throw new InvalidException("目录无效！");
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
        }
        #endregion

        #endregion

        #region 过滤字符
        /// <summary>
        /// 过滤表单中的特殊字符
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string FilterInput(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            var dangerStrings = new string[] { "<script", "javascript:" };
            foreach (var item in dangerStrings)
            {
                s = Regex.Replace(s, item, "", RegexOptions.IgnoreCase);
            }
            var output = new StringBuilder();
            int runIndex = -1;
            int l = s.Length;
            for (var index = 0; index < l; ++index)
            {
                var c = s[index];
                if (c != '\t' && c != '\b' && c != '\f' && !((c >= 0 && c <= 31) || c == 127))
                {
                    if (runIndex == -1)
                        runIndex = index;
                    continue;
                }
                if (runIndex != -1)
                {
                    output.Append(s, runIndex, index - runIndex);
                    runIndex = -1;
                }
                switch (c)
                {
                    case '\b':
                        output.Append("\\b"); break;
                    case '\f':
                        output.Append("\\f"); break;
                    case '\t': output.Append("\\t"); break;
                    default:
                        if (!((c >= 0 && c <= 31) || c == 127)) //在ASCⅡ码中，第0～31号及第127号(共33个)是控制字符或通讯专用字符
                        {
                            output.Append(c);
                        }
                        break;
                }
            }
            if (runIndex != -1)
                output.Append(s, runIndex, s.Length - runIndex);
            return output.ToString();
        }
        #endregion

        #region 上传文件
        public static string UploadFile(string fieldName)
        {
            if (HttpContext.Current.Request.Method != "POST")
                return null;
            var fileField = new FileField(fieldName);
            var files = HttpContext.Current.Request.Form.Files.GetFiles(fileField.Name);
            if (files.Count == 0)
                throw new InvalidException("请选择文件！");
            var fileName = files[0].FileName;
            if (IsImage(fileName))
                fileField.Type = FieldType.Image;
            else if (IsVideo(fileName))
                fileField.Type = FieldType.Video;
            else if (IsAudio(fileName))
                fileField.Type = FieldType.Audio;
            else if (IsDoc(fileName))
                fileField.Type = FieldType.DocFile;
            else if (IsZip(fileName))
                fileField.Type = FieldType.ZipFile;
            var list = new List<string>();
            foreach (var file in files)
                list.Add(UploadFile(fileField, file));
            return string.Join(",", list);
        }
        public static string UploadFile(FileField fileField, Microsoft.AspNetCore.Http.IFormFile formFile = null, string lang = "CN")
        {
            if (HttpContext.Current.Request.Method != "POST")
                return null;
            if (formFile == null)
            {
                if (HttpContext.Current.Request.Form.Files.Count == 0)
                    return null;
                if (!string.IsNullOrEmpty(fileField.Name))
                    formFile = HttpContext.Current.Request.Form.Files[fileField.Name];
                else
                    formFile = HttpContext.Current.Request.Form.Files[0];
            }
            if (formFile.Length == 0) return null;
            if (fileField.MaxLength > 0 && formFile.Length > (fileField.MaxLength * 1024))
                throw new InvalidException(lang == "CN" ? $"{fileField.Text}({formFile.FileName})大小不能超过{GetSize(fileField.MaxLength * 1024)}！"
                    : $"{fileField.Text} ({formFile.FileName}) limit in {GetSize(fileField.MaxLength * 1024)}!");

            #region 检查文件名是否正常
            if (Path.GetInvalidFileNameChars().Any(a => formFile.FileName.Contains(a.ToString())))
            {
                throw new InvalidException(lang == "CN" ? $"无效的文件！" : "Invalid file!");
            }
            #endregion

            #region 检查文件扩展名
            switch (fileField.Type)
            {
                case FieldType.File:
                    if (!IsSecurityFile(formFile.FileName))
                        throw new InvalidException(lang == "CN" ? $"附件【{fileField.Text}】不被允许的文件！" : $"Attachments ({fileField.Text}) extensions that are not allowed!");
                    break;
                case FieldType.DocFile:
                    if (!IsDoc(formFile.FileName))
                        throw new InvalidException(lang == "CN" ? $"附件【{fileField.Text}】必需为文档格式！" : $"Attachments \"{fileField.Text}\" must be in docment format!");
                    break;
                case FieldType.Image:
                    if (!IsImage(formFile.FileName))
                        throw new InvalidException(lang == "CN" ? $"附件【{fileField.Text}】必需为图片格式！" : $"Attachments \"{fileField.Text}\" must be in image format!");
                    break;
                case FieldType.Video:
                    if (!IsVideo(formFile.FileName))
                        throw new InvalidException(lang == "CN" ? $"附件【{fileField.Text}】必需为视频格式！" : $"Attachments \"{fileField.Text}\" must be in video format!");
                    break;
                case FieldType.Audio:
                    if (!IsVideo(formFile.FileName))
                        throw new InvalidException(lang == "CN" ? $"附件【{fileField.Text}】必需为音频格式！" : $"Attachments \"{fileField.Text}\" must be in audio format!");
                    break;
                case FieldType.ZipFile:
                    if (!IsVideo(formFile.FileName))
                        throw new InvalidException(lang == "CN" ? $"附件【{fileField.Text}】必需为压缩文件！" : $"Attachments \"{fileField.Text}\" must be in zip format!");
                    break;
            }
            if (fileField.AllowExtensions != null && fileField.AllowExtensions.Length > 0)
            {
                var ext = Path.GetExtension(formFile.FileName).ToLower();
                if (!fileField.AllowExtensions.Contains(ext))
                {
                    throw new InvalidException(lang == "CN" ? $"附件【{fileField.Text}】的扩展名必需为：{string.Join("、", fileField.AllowExtensions)}格式！"
                        : $"The extension of the attachment \"{fileField.Text}\" must be: {string.Join("、", fileField.AllowExtensions)} format!");
                }
            }
            #endregion

            var fileName = GetFileName(fileField, formFile.FileName);
            var savePath = fileName.Substring(0, fileName.LastIndexOf("/"));
            var filePath = MapPath(fileName);
            formFile.SaveAs(filePath);

            var imageField = fileField as ImageField;
            #region 图片处理
            if (imageField != null)
            {
                #region 生成缩略图
                if (!imageField.ThumbSize.IsEmpty)
                {
                    string path;
                    try
                    {
                        if (!string.IsNullOrEmpty(imageField.ThumbPath))
                            path = fileField.Data.ThumbPath;
                        else
                            path = savePath + "/thumb";
                    }
                    catch
                    {
                        path = savePath + "/thumb";
                    }
                    var saveFolder = MapPath(path);
                    CreateDirectory(saveFolder);
                    var thumbPath = Path.Combine(saveFolder, Path.GetFileName(filePath));
                    CompressImage(filePath, imageField.ThumbSize, imageField.CompressQuality, thumbPath);
                }
                #endregion

                #region 压缩图片
                if (imageField.Compress)
                {
                    var fName = Path.GetFileNameWithoutExtension(filePath);
                    string compressSavePath;
                    if (imageField.KeepSourceFile)
                    {
                        compressSavePath = filePath.Replace(fName, $"{fName}-compress");
                        fileName = fileName.Replace(fName, $"{fName}-compress");
                    }
                    else
                    {
                        compressSavePath = filePath;
                    }
                    CompressImage(filePath, imageField.CompressSize, 80, compressSavePath);
                }
                #endregion
            }
            #endregion

            return fileName;
        }
        #endregion

        #region 获取文件名
        /// <summary>
        /// 自动创建目录并还回文件名
        /// </summary>
        /// <param name="fileField"></param>
        /// <param name="sourceFileName"></param>
        /// <returns></returns>
        public static string GetFileName(FileField fileField, string sourceFileName)
        {
            string savePath = fileField.SavePath;
            if (string.IsNullOrEmpty(savePath))
            {
                switch (fileField.Type)
                {
                    case FieldType.File:
                        savePath = "/upFiles/files";
                        break;
                    case FieldType.Image:
                        savePath = "/upFiles/photo";
                        break;
                    case FieldType.Video:
                        savePath = "/upFiles/video";
                        break;
                    case FieldType.DocFile:
                        savePath = "/upFiles/doc";
                        break;
                }
                savePath += "/" + DateTime.Now.ToString("yyyyMMdd");
            }
            if (!savePath.EndsWith("/") && !Path.HasExtension(savePath))
                savePath += "/";
            var folder = System.IO.Path.GetDirectoryName(MapPath(savePath));
            CreateDirectory(folder);

            string fileName;
            var ext = Path.GetExtension(sourceFileName);
            if (savePath.Contains("."))
            {
                if (ext == ".webp")
                    fileName = savePath.Replace(Path.GetExtension(savePath), ext);
                else
                    fileName = savePath;
            }
            else
            {
                if (ext == ".webp")
                    ext = ".jpg";
                fileName = string.Format("{0}{1}{2}", savePath, Path.GetRandomFileName(), ext);
            }
            if (!fileName.StartsWith("/upFiles"))
                throw new InvalidException("无效目录！");
            return fileName;

        }
        public static string GetFileName(string sourceFileName)
        {
            return $"{Path.GetRandomFileName()}.{sourceFileName.Substring(sourceFileName.LastIndexOf('.'))}";
        }
        #endregion

        #region 获取Post数据
        public static string GetPost()
        {
            var req = HttpContext.Current.Request;
            var q = new StringBuilder();
            foreach (var form in req.Form)
                q.AppendFormat("{0}:{1}, ", form.Key, form.Value);
            if (q.Length > 0)
            {
                q.AppendLine();
            }
            q.AppendFormat("Files:");
            q.AppendLine();
            foreach (var file in req.Form.Files)
                q.AppendFormat("{0}:{1}, size:{2}, ", file.Name, file.FileName, file.Length);
            if (q.ToString().EndsWith(", "))
                q.Remove(q.Length - 1, 1);
            return q.ToString();
        }
        #endregion
    }

    #region 字段类型
    public enum FieldType
    {
        None = 0,
        /// <summary>
        /// 文件型：如需指定保存路径可设置Data为：new { Path = "/保存路径" }，默认保存到：/upFiles/files
        /// </summary>
        File = 1,
        /// <summary>
        /// 图片型：如需指定保存路径可设置Data为：new { Path = "/保存路径" }，默认保存到：/upFiles/photo
        /// </summary>
        Image = 2,
        /// <summary>
        /// 视频型：如需指定保存路径可设置Data为：new { Path = "/保存路径" }，默认保存到：/upFiles/video
        /// </summary>
        Video = 3,
        /// <summary>
        /// 编辑器：如需指定保存路径可设置Data为：new { Path = "/保存路径" }，默认保存到：/upFiles/article
        /// </summary>
        Editor = 4,
        /// <summary>
        /// Email
        /// </summary>
        Email = 5,
        /// <summary>
        /// 手机
        /// </summary>
        Mobile = 6,
        /// <summary>
        /// 整数
        /// </summary>
        Integer = 7,
        /// <summary>
        /// 数字
        /// </summary>
        Number = 8,
        /// <summary>
        /// 文档
        /// </summary>
        DocFile = 9,
        /// <summary>
        /// 音频文件
        /// </summary>
        Audio = 10,
        /// <summary>
        /// 压缩文件
        /// </summary>
        ZipFile = 11
    }
    public enum FieldFlags
    {
        /// <summary>
        /// 生成缩略图，需要指定Data属性的Width和Height（缩略后的宽高），路径可设置：ThumbPath字段
        /// </summary>
        CreateThumbnail
    }
    #endregion

    #region 字段对象
    public class FormField
    {
        /// <summary>
        /// 字段名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 字段说明
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// 提示信息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 字段类型
        /// </summary>
        public FieldType Type { get; set; }
        /// <summary>
        /// 字段标记
        /// </summary>
        public FieldFlags[] Flags { get; set; }
        /// <summary>
        /// 动态数据
        /// </summary>
        public dynamic Data { get; set; }
        /// <summary>
        /// 是否为必填
        /// </summary>
        public bool Required { get; set; }
        /// <summary>
        /// 最小长度（多个文件必需设置最小值）
        /// </summary>
        public long MinLength { get; set; }
        /// <summary>
        /// 最大长度（如果是文件则为最大大小，单位（KB））
        /// </summary>
        public long MaxLength { get; set; }
        /// <summary>
        /// 是否为屏蔽
        /// </summary>
        public bool Disabled { get; set; }
    }

    public class TextField : FormField
    {
        public TextField(string name)
        {
            this.Name = name;
        }
    }

    public class DisableField : FormField
    {
        public DisableField(string name)
        {
            this.Name = name;
            this.Disabled = true;
        }
    }

    public class FileField : FormField
    {
        /// <summary>
        /// 保持原始文件
        /// </summary>
        public bool KeepSourceFile { get; set; }
        /// <summary>
        /// 最多上传文件数
        /// </summary>
        public int MaxCount { get; set; }
        /// <summary>
        /// 最小上传文件数
        /// </summary>
        public int MinCount { get; set; }
        /// <summary>
        /// 允许的扩展
        /// </summary>
        public string[] AllowExtensions { get; set; }
        /// <summary>
        /// 指定文件保存路径（如果不设置该字段则自动保存）
        /// </summary>
        public string SavePath { get; set; }
        public FileField(string name)
        {
            this.Name = name;
            this.Type = FieldType.File;
        }
    }

    public class ImageField : FileField
    {
        /// <summary>
        /// 是否自动压缩图片
        /// </summary>
        public bool Compress { get; set; }
        /// <summary>
        /// 缩略图最大宽高（未设置则不生成缩略图）
        /// </summary>
        public Size ThumbSize { get; set; }
        /// <summary>
        /// 缩略图路径
        /// </summary>
        public string ThumbPath { get; set; }
        /// <summary>
        /// 压缩最大宽高
        /// </summary>
        public Size CompressSize { get; set; }
        /// <summary>
        /// 图片压缩质量
        /// </summary>
        public int CompressQuality { get; set; }
        /// <summary>
        /// 图片要求最小宽高
        /// </summary>
        public Size MinSize { get; set; }
        /// <summary>
        /// 图片要求最大宽高
        /// </summary>
        public Size MaxSize { get; set; }

        public ImageField(string name, bool compress = true)
            : base(name)
        {
            this.CompressQuality = 80;
            this.CompressSize = Size.Empty;
            this.ThumbSize = Size.Empty;
            this.MinSize = Size.Empty;
            this.MaxSize = Size.Empty;
            this.Type = FieldType.Image;
            this.Compress = compress;
            this.CompressSize = new Size(2000, 2000);
        }
    }

    public class VideoField : FileField
    {
        public VideoField(string name)
            : base(name)
        {
            this.Type = FieldType.Video;
        }
    }

    public class DocField : FileField
    {
        public DocField(string name) : base(name)
        {
            this.Type = FieldType.DocFile;
        }
    }

    public class EditorField : FormField
    {
        /// <summary>
        /// 是否过滤XXS
        /// </summary>
        public bool XXSFilter { get; set; }
        /// <summary>
        /// 是否自动抓取编辑器中的图片
        /// </summary>
        public bool AutoDownloadImages { get; set; }
        public EditorField(string name, bool xxsFilter = true, bool autoDownloadImages = false)
        {
            this.XXSFilter = xxsFilter;
            this.Name = name;
            this.AutoDownloadImages = autoDownloadImages;
            this.Type = FieldType.Editor;
        }
    }

    public class EmailField : FormField
    {
        public EmailField(string name, string text = "Email", bool required = true)
        {
            this.Name = name;
            this.Text = text;
            this.Type = FieldType.Email;
            this.Required = required;
        }
    }

    public class MobileField : FormField
    {
        public MobileField(string name, string text = "手机号", bool required = true)
        {
            this.Name = name;
            this.Text = text;
            this.Type = FieldType.Mobile;
            this.Required = required;
        }
    }

    public class RequireField : FormField
    {
        public RequireField(string name, string text)
        {
            this.Name = name;
            this.Text = text;
            this.Required = true;
        }
    }
    #endregion

    #region 验证错误
    public class InvalidException : Exception
    {
        public InvalidException(string error) : base(error)
        {

        }
    }
    #endregion
}