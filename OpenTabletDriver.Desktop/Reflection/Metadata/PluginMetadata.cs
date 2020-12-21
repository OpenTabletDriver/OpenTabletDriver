using System;
using System.IO;
using System.Threading.Tasks;

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
        /// The plugin's wiki URL.
        /// </summary>
        public string WikiUrl { set; get; }

        /// <summary>
        /// The SPDX license identifier expression.
        /// </summary>
        public string LicenseIdentifier { set; get; }

        public async Task<Stream> GetDownloadStream()
        {
            using (var client = PluginMetadataCollection.GetClient())
            {
                return string.IsNullOrWhiteSpace(DownloadUrl) ? null : await client.GetStreamAsync(DownloadUrl);
            }
        }
    }
}