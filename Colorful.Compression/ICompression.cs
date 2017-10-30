using System;
using System.Collections.Generic;
using System.Text;

namespace Colorful.Compression
{
    #region CompressionLevel
    /// <summary>
    /// The compression level to use when compressing an archive.
    /// </summary>
    public enum CompressionLevel : int
    {
        /// <summary>
        /// Stores files, performs no compression.
        /// </summary>
        Store = 0,
        /// <summary>
        /// The lowest and fastest amount of compression.
        /// </summary>
        Low = 1,
        /// <summary>
        /// The second lowest and fastest amount of compression.
        /// </summary>
        Fast = 3,
        /// <summary>
        /// The default amount of compression.
        /// </summary>
        Normal = 5,
        /// <summary>
        /// The second highest and slowest amount of compression.
        /// </summary>
        High = 7,
        /// <summary>
        /// The highest and slowest amount of compression.
        /// </summary>
        Ultra = 9
    }
    #endregion

    public interface ICompression
    {
        void Compress(string fileOrFolder, string savePath, CompressionLevel level = CompressionLevel.Normal);
        void UnCompress(string filePath);
    }
}
