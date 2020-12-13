using System;

namespace OpenTabletDriver.Desktop.Reflection
{
    public struct PluginMetadata
    {
        /// <summary>
        /// The name of the plugin.
        /// </summary>
        public string Name { set; get; }

        /// <summary>
        /// The author of the plugin.
        /// </summary>
        public string Author { set; get; }

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
        public Uri RepositoryUrl { set; get; }

        /// <summary>
        /// The plugin's binary download URL.
        /// </summary>
        public Uri DownloadUrl { set; get; }

        /// <summary>
        /// The plugin's wiki URL.
        /// </summary>
        public Uri WikiUrl { set; get; }

        /// <summary>
        /// The plugin license name.
        /// </summary>
        public string LicenseName { set; get; }
    }
}