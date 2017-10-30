using Microsoft.AspNetCore.Http;
using SkiaSharp;
using System;
using System.Drawing;
using System.IO;

namespace Colorful.Image
{
    /// <summary>
    /// ͼ���������
    /// </summary>
    public class ImageHelper
    {
        #region ѹ��
        /// <summary>
        /// ѹ��ͼƬ
        /// </summary>
        /// <param name="filePath">Ҫѹ����ͼƬ·��</param>
        /// <param name="maxSize">ͼƬ�����</param>
        /// <param name="savePath">����·��</param>
        /// <param name="quality">ѹ����</param>
        /// <param name="imageFormat">Ҫ�����ͼƬ��ʽ</param>
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
        /// ѹ��ͼƬ
        /// </summary>
        /// <param name="formFile">�ϴ����ļ�����</param>
        /// <param name="savePath">����·��</param>
        /// <param name="maxSize">ͼƬ�����</param>
        /// <param name="quality">ѹ����</param>
        /// <param name="imageFormat">Ҫ�����ͼƬ��ʽ</param>
        public ImageHelper Compress(IFormFile formFile, string savePath, Size? maxSize = null, int quality = 80, ImageFormat imageFormat = ImageFormat.Jpeg)
        {
            using (var stream = new MemoryStream())
            {
                formFile.CopyTo(stream);
                return Compress(stream, ref savePath, maxSize, quality, imageFormat);
            }
        }

        /// <summary>
        /// ѹ��ͼƬ
        /// </summary>
        /// <param name="stream">�ļ���</param>
        /// <param name="savePath">����·��</param>
        /// <param name="maxSize">ͼƬ�����</param>
        /// <param name="quality">ѹ����</param>
        /// <param name="imageFormat">Ҫ�����ͼƬ��ʽ</param>
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