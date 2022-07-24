using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using OpenTabletDriver.Desktop.Reflection.Metadata;

namespace OpenTabletDriver.Desktop.Reflection
{
    public interface IPluginManager
    {
        event EventHandler? AssembliesChanged;
        IEnumerable<Assembly> Assemblies { get; }
        IReadOnlyList<DesktopPluginContext> Plugins { get; }
        IEnumerable<Type> ExportedTypes { get; }
        IEnumerable<Type> LibraryTypes { get; }
        IEnumerable<Type> PluginTypes { get; }

        void Clean();
        void Load();
        bool InstallPlugin(string filePath);
        Task<bool> DownloadPlugin(PluginMetadata metadata);
        bool InstallPlugin(DirectoryInfo target, DirectoryInfo source);
        bool UninstallPlugin(DesktopPluginContext plugin);
        bool UpdatePlugin(DesktopPluginContext plugin, DirectoryInfo source);
        bool UnloadPlugin(DesktopPluginContext context);
        string GetStateHash();
    }
}
