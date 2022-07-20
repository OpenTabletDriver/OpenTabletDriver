using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using JetBrains.Annotations;
using OpenTabletDriver.Desktop.Compression;

#nullable enable

namespace OpenTabletDriver.Desktop.Reflection.Metadata
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public class PluginMetadata
    {
        /// <summary>
        /// The name of the plugin.
        /// </summary>
        public string Name { set; get; } = null!;

        /// <summary>
        /// The owner of the plugin's source code repository.
        /// </summary>
        public string? Owner { set; get; }

        /// <summary>
        /// The plugin's long description.
        /// </summary>
        public string? Description { set; get; }

        /// <summary>
        /// The plugins' version.
        /// Newer supported versions will be preferred by default.
        /// </summary>
        [DisplayName("Plugin Version")]
        public Version? PluginVersion { set; get; }

        /// <summary>
        /// The plugin's minimum supported OpenTabletDriver version,
        /// </summary>
        [DisplayName("Supported Driver Version")]
        public Version? SupportedDriverVersion { set; get; }

        /// <summary>
        /// The plugin's source code repository URL.
        /// </summary>
        [DisplayName("Source Code Repository"), Url]
        public string? RepositoryUrl { set; get; }

        /// <summary>
        /// The plugin's binary download URL.
        /// </summary>
        [DisplayName("Download"), Url]
        public string? DownloadUrl { set; get; }

        /// <summary>
        /// The compression format used in the binary download from <see cref="DownloadUrl"/>.
        /// </summary>
        public string? CompressionFormat { set; get; }

        /// <summary>
        /// The SHA256 hash of the file at <see cref="DownloadUrl"/>, used for verifying file integrity.
        /// </summary>
        public string? SHA256 { set; get; }

        /// <summary>
        /// The plugin's wiki URL.
        /// </summary>
        [DisplayName("Wiki"), Url]
        public string? WikiUrl { set; get; }

        /// <summary>
        /// The SPDX license identifier expression.
        /// </summary>
        [DisplayName("License")]
        public string? LicenseIdentifier { set; get; }

        public static string GetSHA256(Stream stream)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
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

        public async Task<Stream?> GetDownloadStream()
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
                    stream.Decompress(outputDirectory, CompressionFormat);
                }
                else
                {
                    throw new CryptographicException("The SHA256 cryptographic hashes of the downloaded content and the metadata do not match.");
                }
            }
        }

        public bool IsSupportedBy(Version appVersion)
        {
            // Always return false when major and minor is not equal (x.y.0.0).
            if (SupportedDriverVersion!.Major != appVersion.Major)
                return false;
            if (SupportedDriverVersion.Minor != appVersion.Minor)
                return false;

            // Always return false when driver's version is older than plugin's declared support version (0.0.x.0).
            // We do this because the driver will bump build version when a non-breaking feature is introduced.
            // Newer plugins may start using these new features not available in older drivers.
            if (SupportedDriverVersion.Build > appVersion.Build)
                return false;

            return true;
        }

        public bool Match(PluginMetadata secondary)
        {
            return Name == secondary.Name &&
                Owner == secondary.Owner &&
                RepositoryUrl == secondary.RepositoryUrl;
        }
    }
}
