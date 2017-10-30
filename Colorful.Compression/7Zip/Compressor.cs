using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using SevenZipNET;
using System.IO;

namespace Colorful.Compression
{
    /// <summary>
    /// 压缩/解压
    /// </summary>
    public class Package
    {
        private string _archivePath;
        private string _password;

        public Package(string archivePath, string password = null)
        {
            _password = password;
            _archivePath = archivePath;
        }
        /// <summary>
        /// 压缩
        /// </summary>
        /// <param name="targets">要压缩的文件或文件夹</param>
        public void Packing(params string[] targets)
        {
            Packing(CompressionLevel.Normal, targets);
        }
        /// <summary>
        /// 压缩
        /// </summary>
        /// <param name="level">压缩级别</param>
        /// <param name="targets">要压缩的文件或文件夹</param>
        public void Packing(CompressionLevel level, params string[] targets)
        {
            var zipFile = new SevenZipCompressor(_archivePath, _password);
            zipFile.CompressDirectory(targets, level);
        }
        /// <summary>
        /// 解压
        /// </summary>
        /// <param name="destination">解压路径</param>
        /// <param name="overwrite">是否覆盖</param>
        /// <param name="keepStructure">是否保持目录结构</param>
        public void Unpacking(string destination, bool overwrite = false, bool keepStructure = true)
        {
            var extractor = new SevenZipExtractor(_archivePath, _password);
            extractor.ExtractAll(destination, overwrite, keepStructure);
        }
    }
}
