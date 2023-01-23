using System;
using System.Collections.Generic;
using OpenTabletDriver.Daemon.Contracts;

namespace OpenTabletDriver.Daemon.Reflection
{
    public interface IPluginFactory
    {
        T? Construct<T>(PluginSettings settings, params object[] args) where T : class;
        T? Construct<T>(string fullPath, params object[] args) where T : class;
        IEnumerable<Type> GetPlugins<T>();
        IEnumerable<Type> GetPlugins(Type pluginInterfaceType);
        Type? GetPlugin(string path);
    }
}
