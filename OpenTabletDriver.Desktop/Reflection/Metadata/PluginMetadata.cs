using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using OpenTabletDriver.Desktop.Compression;

namespace OpenTabletDriver.Desktop.Reflection.Metadata
{
    public class PluginMetadata
    {
        /// <summary>
        /// The name of the plugin.
        /// </summary>
        public string Name { set; get; }

        /// <summary>
        /// The owner of the plugin's source code repository.
        /// </summary>
        public string Owner { set; get; }

        /// <summary>
        /// The plugin's long description.
        /// </summary>
        public string Description { set; get; }

        /// <summary>
        /// The plugins' version.
        /// Newer supported versions will be preferred by default.
        /// </summary>
        public Version PluginVersion { set; get; }

        /// <summary>
        /// The plugin's minimum supported OpenTabletDriver version,
        /// </summary>
        public Version SupportedDriverVersion { set; get; }

        /// <summary>
        /// The plugin's source code repository URL.
        /// </summary>
        public string RepositoryUrl { set; get; }

        /// <summary>
        /// The plugin's binary download URL.
        /// </summary>
        public string DownloadUrl { set; get; }

        /// <summary>
        /// The compression format used in the binary download from <see cref="DownloadUrl"/>.
        /// </summary>
        public string CompressionFormat { set; get; }

        /// <summary>
        /// The SHA256 hash of the file at <see cref="DownloadUrl"/>, used for verifying file integrity.
        /// </summary>
        public string SHA256 { set; get; }

        /// <summary>
        /// The plugin's wiki URL.
        /// </summary>
        public string WikiUrl { set; get; }

        /// <summary>
        /// The SPDX license identifier expression.
        /// </summary>
        public string LicenseIdentifier { set; get; }

        public static string GetSHA256(Stream stream)
        {
            using (var sha256 = SHA256Managed.Create())
            {
                var hashData = sha256.ComputeHash(stream);
                stream.Position = 0;
                return string.Concat(hashData.Select(b => b.ToString("x2")));
            }
        }

        public bool VerifySHA256(Stream stream)
        {
            return GetSHA256(stream) == SHA256;
        }

        public async Task<Stream> GetDownloadStream()
        {
            using (var client = PluginMetadataCollection.GetClient())
            {
                return string.IsNullOrWhiteSpace(DownloadUrl) ? null : await client.GetStreamAsync(DownloadUrl);
            }
        }

        public async Task DownloadAsync(string outputDirectory)
        {
            using (var httpStream = await GetDownloadStream())
            using (var stream = new MemoryStream())
            {
                // Download into memory
                await httpStream.CopyToAsync(stream);
                stream.Position = 0;

                // Verify SHA256 hash
                if (SHA256 == null || VerifySHA256(stream))
                {
                    stream.Decompress(outputDirectory, this.CompressionFormat);
                }
                else
                {
                    throw new CryptographicException("The SHA256 cryptographic hashes of the downloaded content and the metadata do not match.");
                }
            }
        }

        public static bool Match(PluginMetadata primary, PluginMetadata secondary)
        {
            if (primary == null || secondary == null)
                return false;

            return primary.Name == secondary.Name &&
                primary.Owner == secondary.Owner &&
                primary.RepositoryUrl == secondary.RepositoryUrl;
        }
    }
}