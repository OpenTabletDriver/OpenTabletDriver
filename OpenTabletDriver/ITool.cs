using System;
using JetBrains.Annotations;
using OpenTabletDriver.Attributes;

namespace OpenTabletDriver
{
    /// <summary>
    /// A plugin that will be started up and kept running until OpenTabletDriver is closed.
    /// </summary>
    [PublicAPI]
    [PluginInterface]
    public interface ITool : IDisposable
    {
        /// <summary>
        /// Initializes the tool.
        /// </summary>
        /// <returns>
        /// Whether the tool successfully initialized.
        /// </returns>
        bool Initialize();
    }
}
