using System;

namespace OpenTabletDriver.Daemon.Contracts
{
    public class PluginMetadata
    {
        /// <summary>
        /// The name of the plugin.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// The owner of the plugin's source code repository.
        /// </summary>
        public string? Owner { get; set; }

        /// <summary>
        /// The plugin's long description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// The plugins' version.
        /// Newer supported versions will be preferred by default.
        /// </summary>
        public Version? PluginVersion { get; set; }

        /// <summary>
        /// The plugin's minimum supported OpenTabletDriver version,
        /// </summary>
        public Version? SupportedDriverVersion { get; set; }

        /// <summary>
        /// The plugin's source code repository URL.
        /// </summary>
        public string? RepositoryUrl { get; set; }

        /// <summary>
        /// The plugin's binary download URL.
        /// </summary>
        public string? DownloadUrl { get; set; }

        /// <summary>
        /// The compression format used in the binary download from <see cref="DownloadUrl"/>.
        /// </summary>
        public string? CompressionFormat { get; set; }

        /// <summary>
        /// The SHA256 hash of the file at <see cref="DownloadUrl"/>, used for verifying file integrity.
        /// </summary>
        public string? SHA256 { get; set; }

        /// <summary>
        /// The plugin's wiki URL.
        /// </summary>
        public string? WikiUrl { get; set; }

        /// <summary>
        /// The SPDX license identifier expression.
        /// </summary>
        public string? LicenseIdentifier { get; set; }

        public static bool Match(PluginMetadata a, PluginMetadata b)
        {
            return a.Name == b.Name
                && a.Owner == b.Owner
                && a.RepositoryUrl == b.RepositoryUrl;
        }
    }
}
