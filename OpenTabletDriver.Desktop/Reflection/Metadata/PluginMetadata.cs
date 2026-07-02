using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Compression;

namespace OpenTabletDriver.Desktop.Reflection.Metadata
{
    public class PluginMetadata
    {
        /// <summary>
        /// The name of the plugin.
        /// </summary>
        [JsonRequired]
        [JsonProperty(nameof(Name))]
        public required string Name { set; get; }

        /// <summary>
        /// The owner of the plugin's source code repository.
        /// </summary>
        [JsonRequired]
        [JsonProperty(nameof(Owner))]
        public string? Owner { set; get; }

        /// <summary>
        /// The original creator of the plugin if it differs from the Owner.
        /// </summary>
        public string? Creator { set; get; }

        /// <summary>
        /// The plugin's long description.
        /// <para/>
        /// GUI currently expects this property to be filled with something useful in some manner
        /// </summary>
        [JsonRequired]
        [JsonProperty(nameof(Description))]
        public string? Description { set; get; }

        /// <summary>
        /// The plugins' version.
        /// Newer supported versions will be preferred by default.
        /// </summary>
        [JsonRequired]
        [JsonProperty(nameof(PluginVersion))]
        public Version? PluginVersion { set; get; }

        /// <summary>
        /// The plugin's minimum supported OpenTabletDriver version,
        /// </summary>
        [JsonRequired]
        [JsonProperty(nameof(SupportedDriverVersion))]
        public required Version SupportedDriverVersion { set; get; }

        /// <summary>
        /// The plugin's maximum supported OpenTabletDriver version.
        /// <para/>
        /// You probably want to leave this <c>null</c> unless a minor version bump from upstream breaks your plugin
        /// </summary>
        [JsonProperty(nameof(MaxSupportedDriverVersion))]
        public Version? MaxSupportedDriverVersion { set; get; }

        /// <summary>
        /// The plugin's source code repository URL.
        /// </summary>
        [JsonProperty(nameof(RepositoryUrl))]
        public string? RepositoryUrl { set; get; }

        /// <summary>
        /// The plugin's binary download URL.
        /// </summary>
        [JsonProperty(nameof(DownloadUrl))]
        public string? DownloadUrl { set; get; }

        /// <summary>
        /// The compression format used in the binary download from <see cref="DownloadUrl"/>.
        /// </summary>
        /// <remarks>Should be <c>null</c> if there is no DownloadURL</remarks>
        [JsonProperty(nameof(CompressionFormat))]
        public string? CompressionFormat { set; get; }

        /// <summary>
        /// The SHA256 hash of the file at <see cref="DownloadUrl"/>, used for verifying file integrity.
        /// </summary>
        /// <remarks>Should be <c>null</c> if there is no DownloadURL</remarks>
        [JsonProperty(nameof(SHA256))]
        public string? SHA256 { set; get; }

        /// <summary>
        /// The plugin's wiki URL.
        /// </summary>
        [JsonProperty(nameof(WikiUrl))]
        public string? WikiUrl { set; get; }

        /// <summary>
        /// Whether the plugin is installed.
        /// </summary>
        public bool Installed { set; get; }

        /// <summary>
        /// The SPDX license identifier expression.
        /// </summary>
        [JsonProperty(nameof(LicenseIdentifier))]
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

        public async Task<Stream> GetDownloadStream()
        {
            ArgumentException.ThrowIfNullOrEmpty(DownloadUrl);

            using var client = PluginMetadataCollection.GetClient();

            return await client.GetStreamAsync(DownloadUrl);
        }

        public async Task DownloadAsync(string outputDirectory)
        {
            await using (var httpStream = await GetDownloadStream())
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

        public bool IsSupportedBy(Version appVersion)
        {
            // Always return false when major and minor is not equal (x.y.0.0).
            if (SupportedDriverVersion.Major != appVersion.Major)
                return false;
            if (SupportedDriverVersion.Minor != appVersion.Minor)
                return false;

            // Always return false when driver's version is older than plugin's declared support version (0.0.x.0).
            // We do this because the driver will bump build version when a non-breaking feature is introduced.
            // Newer plugins may start using these new features not available in older drivers.
            if (SupportedDriverVersion.Build > appVersion.Build)
                return false;

            // Always return false when driver's version is newer than plugin's declared upper support version.
            // We do this as some plugins may be integrated into the driver in the future.
            // We also do this as breaking changes may be introduced.
            if (MaxSupportedDriverVersion != null && MaxSupportedDriverVersion < appVersion)
                return false;

            return true;
        }

        public static bool Match(PluginMetadata? primary, PluginMetadata? secondary)
        {
            if (primary == null || secondary == null)
                return false;

            return primary.Name == secondary.Name &&
                primary.Owner == secondary.Owner &&
                primary.RepositoryUrl == secondary.RepositoryUrl;
        }
    }
}
