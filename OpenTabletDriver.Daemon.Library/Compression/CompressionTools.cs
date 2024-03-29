using System;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;

namespace OpenTabletDriver.Daemon.Compression
{
    public static class CompressionTools
    {
        public static void Decompress(this Stream stream, string outputDir, string format)
        {
            switch (format)
            {
                case "gzip":
                {
                    using (var gzipStream = new GZipInputStream(stream))
                        gzipStream.Decompress(outputDir);
                    break;
                }
                case "zip":
                {
                    using (var zipStream = new ZipInputStream(stream))
                        zipStream.Decompress(outputDir);
                    break;
                }
                default:
                    throw new NotSupportedException($"{format} is not supported.");
            }
        }

        private static void Decompress(this ZipInputStream zipStream, string outputDir)
        {
            while (zipStream.GetNextEntry() is ZipEntry entry)
            {
                var entryFileName = entry.Name;
                var buffer = new byte[0x1000];

                // Manipulate the output filename here as desired.
                var zipPath = Path.Combine(outputDir, entryFileName);
                var directoryName = Path.GetDirectoryName(zipPath)!;
                if (directoryName.Length > 0)
                    Directory.CreateDirectory(directoryName);

                // Skip directory entry
                if (Path.GetFileName(zipPath).Length == 0)
                {
                    continue;
                }

                using (var streamWriter = File.Create(zipPath))
                    StreamUtils.Copy(zipStream, streamWriter, buffer);
            }
        }

        private static void Decompress(this GZipInputStream gzipStream, string outputDir)
        {
            using (var archive = TarArchive.CreateInputTarArchive(gzipStream, null))
                archive.ExtractContents(outputDir);
        }
    }
}
