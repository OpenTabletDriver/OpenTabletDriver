using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using OpenTabletDriver.Daemon.Contracts;

namespace OpenTabletDriver.Daemon.Reflection
{
    public interface IPluginManager
    {
        event EventHandler<PluginContext>? PluginAdded;
        event EventHandler<PluginContext>? PluginRemoved;

        ImmutableArray<Type> PluginInterfaces { get; }
        ImmutableArray<Type> InternalPluginTypes { get; }
        ImmutableArray<PluginContext> Plugins { get; }

        void Load();
        Task<IEnumerable<PluginMetadata>> GetInstallablePlugins(string owner, string name, string gitRef);
        Task<bool> InstallFromRemote(PluginMetadata pluginMetadata);
        Task<bool> InstallFromLocal(string filePath);
        bool UninstallPlugin(PluginContext plugin);

        bool IsLoadablePluginType(Type type);
        IEnumerable<Type> GetPlugins<T>();
        IEnumerable<Type> GetPlugins(Type interfaceType);
        Type? GetPlugin(string type);
        IEnumerable<Type> GetImplementedInterfaces(Type pluginType);
    }
}
