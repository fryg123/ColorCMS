using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SevenZipNET.Arguments;
using Colorful.Compression;

namespace SevenZipNET
{
    /// <summary>
    /// A class to handle compressing files into an archive.
    /// </summary>
    public class SevenZipCompressor : SevenZipBase
    {
        private readonly string archive;
        private readonly string password;
        private readonly bool encryptHeaders;

        /// <param name="filename">The path to the file to be created.</param>
        public SevenZipCompressor(string filename)
        {
            archive = filename;
        }

        /// <param name="filename">The path to the file to be created.</param>
        /// <param name="password">The password used in encryption.</param>
        /// <param name="encryptHeaders">If set to true, encrypts file headers</param>
        public SevenZipCompressor(string filename, string password, bool encryptHeaders = false)
        {
            archive = filename;
            this.password = password;
            this.encryptHeaders = encryptHeaders;
        }


        /// <summary>
        /// Raised when progress of an operation has been updated.
        /// </summary>
        public event ProgressUpdatedEventArgs ProgressUpdated;

        /// <summary>
        /// Fires the ProgressUpdated event.
        /// </summary>
        /// <param name="progress">The current progress.</param>
        protected void UpdateProgress(int progress)
        {
            ProgressUpdatedEventArgs invoke = ProgressUpdated;
            if (invoke != null)
            {
                invoke(progress);
            }
        }

        /// <summary>
        /// Adds a directory to the archive, compressing it in the process.
        /// </summary>
        /// <param name="folders">The directory to compress.</param>
        /// <param name="level">The amount of compression to use.</param>
        public void CompressDirectory(string[] folders, CompressionLevel level = CompressionLevel.Normal)
        {
            CompressDirectory(folders, null, level);
        }

        /// <summary>
        /// Adds a directory to the archive, compressing it in the process.
        /// </summary>
        /// <param name="dir">The directory to compress.</param>
        /// <param name="workingdir">The working directory to set.</param>
        /// <param name="level">The amount of compression to use.</param>

        public void CompressDirectory(string[] folders, string workingdir, CompressionLevel level = CompressionLevel.Normal)
        {
            ArgumentBuilder argumentBuilder = GetCompressionArguments(level);

            // Adding password switch only if password has been given in constructor
            if (!string.IsNullOrEmpty(password))
            {
                argumentBuilder.AddSwitch(new SwitchPassword(password));

                if (encryptHeaders)
                {
                    argumentBuilder.Add(new SwitchEncryptHeaders());
                }
            }
            
            argumentBuilder.AddTargets(folders);

            WProcess p = new WProcess(argumentBuilder);

            if (workingdir != null)
            {
                p.BaseProcess.StartInfo.WorkingDirectory = workingdir;
            }

            p.ProgressUpdated += ProgressUpdated;

            p.Execute();
        }

        /// <summary>
        /// Returns default arguments for compression.
        /// </summary>
        /// <param name="level">The amount of compression to use.</param>
        /// <returns>ArgumentBuilder with default compression parameters.</returns>
        private ArgumentBuilder GetCompressionArguments(CompressionLevel level)
        {
            return new ArgumentBuilder()
                .AddCommand(SevenZipCommands.Add, archive)
                .AddSwitch(new SwitchMultithread())
                .AddSwitch(new SwitchOutputStream(OutputStream.Progress, OutputStreamState.Stdout))
                .AddSwitch(new SwitchCompression(level));
        }
    }
}
