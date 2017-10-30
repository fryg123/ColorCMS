using Microsoft.AspNetCore.Http;
using SkiaSharp;
using System;
using System.Drawing;
using System.IO;

namespace Colorful.Image
{
    /// <summary>
    /// 图像处理帮助类
    /// </summary>
    public class ImageHelper
    {
        #region 压缩
        /// <summary>
        /// 压缩图片
        /// </summary>
        /// <param name="filePath">要压缩的图片路径</param>
        /// <param name="maxSize">图片最大宽高</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="quality">压缩率</param>
        /// <param name="imageFormat">要保存的图片格式</param>
        public ImageHelper Compress(string filePath, Size? maxSize = null, string savePath = null, int quality = 80, ImageFormat imageFormat = ImageFormat.Jpeg)
        {
            if (string.IsNullOrEmpty(savePath))
                savePath = filePath;
            using (var stream = new MemoryStream(File.ReadAllBytes(filePath)))
            {
                return Compress(stream, ref savePath, maxSize, quality, imageFormat);
            }
        }
        /// <summary>
        /// 压缩图片
        /// </summary>
        /// <param name="formFile">上传的文件对象</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="maxSize">图片最大宽高</param>
        /// <param name="quality">压缩率</param>
        /// <param name="imageFormat">要保存的图片格式</param>
        public ImageHelper Compress(IFormFile formFile, string savePath, Size? maxSize = null, int quality = 80, ImageFormat imageFormat = ImageFormat.Jpeg)
        {
            using (var stream = new MemoryStream())
            {
                formFile.CopyTo(stream);
                return Compress(stream, ref savePath, maxSize, quality, imageFormat);
            }
        }

        /// <summary>
        /// 压缩图片
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="maxSize">图片最大宽高</param>
        /// <param name="quality">压缩率</param>
        /// <param name="imageFormat">要保存的图片格式</param>
        public ImageHelper Compress(Stream stream, ref string savePath, Size? maxSize = null, int quality = 80, ImageFormat? imageFormat = null)
        {
            if (savePath.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
            {
                using (var inputStream = new SKManagedStream(stream))
                {
                    var buffer = new byte[inputStream.Length];
                    inputStream.Read(buffer, buffer.Length);
                    System.IO.File.WriteAllBytes(savePath, buffer);
                }
                return this;
            }
            using (var inputStream = new SKManagedStream(stream))
            {
                using (var original = SKBitmap.Decode(inputStream))
                {
                    if (imageFormat == null)
                    {
                        if (original.Info.IsOpaque)
                        {
                            imageFormat = ImageFormat.Jpeg;
                            savePath = savePath.Replace(".png", ".jpg");
                        }
                        else
                        {
                            imageFormat = ImageFormat.Png;
                            savePath = savePath.Replace(".jpg", ".png");
                        }
                    }
                    SKBitmap bitmap;
                    if (maxSize != null && !maxSize.Value.IsEmpty && (original.Width> maxSize.Value.Width || original.Height>maxSize.Value.Height))
                    {
                        int width, height;
                        if (original.Width > original.Height)
                        {
                            var size = maxSize.Value.Width;
                            width = size;
                            height = original.Height * size / original.Width;
                        }
                        else
                        {
                            var size = maxSize.Value.Height;
                            width = original.Width * size / original.Height;
                            height = size;
                        }
                        bitmap = original.Resize(new SKImageInfo(width, height), SKBitmapResizeMethod.Lanczos3);
                    }
                    else
                    {
                        bitmap = original;
                    }
                    using (var image = SKImage.FromBitmap(bitmap))
                    {
                        using (var output = File.OpenWrite(savePath))
                        {
                            image.Encode((SKEncodedImageFormat)imageFormat.Value, quality)
                                .SaveTo(output);
                        }
                    }
                }
            }
            return this;
        }
        #endregion
    }
}