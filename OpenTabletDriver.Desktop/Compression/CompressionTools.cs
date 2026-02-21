using System;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;

namespace OpenTabletDriver.Desktop.Compression
{
    public static class CompressionTools
    {
        public static void Decompress(this Stream stream, string outputDir, string? format)
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
                case null:
                    throw new ArgumentNullException(nameof(format), "null formats are not supported");
                default:
                    throw new NotSupportedException($"{format} is not supported.");
            }
        }

        public static void Decompress(this ZipInputStream zipStream, string outputDir)
        {
            if (zipStream.Available == 0) return;

            Directory.CreateDirectory(outputDir);

            var buffer = new byte[0x1000];

            while (zipStream.GetNextEntry() is ZipEntry entry)
            {
                if (entry.IsDirectory)
                    continue;

                using (var streamWriter = File.Create(Path.Combine(outputDir, entry.Name)))
                    StreamUtils.Copy(zipStream, streamWriter, buffer);
            }
        }

        public static void Decompress(this GZipInputStream gzipStream, string outputDir)
        {
            using (var archive = TarArchive.CreateInputTarArchive(gzipStream, null))
                archive.ExtractContents(outputDir);
        }
    }
}
