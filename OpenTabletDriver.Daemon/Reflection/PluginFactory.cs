using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Daemon.Contracts;

namespace OpenTabletDriver.Daemon.Reflection
{
    public class PluginFactory : IPluginFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IPluginManager _pluginManager;

        public PluginFactory(IServiceProvider serviceProvider, IPluginManager pluginManager)
        {
            _serviceProvider = serviceProvider;
            _pluginManager = pluginManager;
        }

        public T? Construct<T>(PluginSettings? settings, params object[] args) where T : class
        {
            if (settings == null)
                return null;

            var settingsProvider = new PluginSettingsProvider(settings);
            var newArgs = args.Append(settingsProvider).ToArray();

            return Construct<T>(settings.Path, newArgs);
        }

        public T? Construct<T>(string path, params object[] args) where T : class
        {
            var type = GetPlugin(path);
            if (type != null)
            {
                try
                {
                    return _serviceProvider.CreateInstance(type, args) as T;
                }
                catch (TargetInvocationException e) when (e.Message == "Exception has been thrown by the target of an invocation.")
                {
                    Log.Write("Plugin", "Object construction has thrown an exception", LogLevel.Error);
                    Log.Exception(e.GetBaseException());
                }
                catch (Exception e)
                {
                    Log.Write("Plugin", $"Unable to construct object '{path}'", LogLevel.Error, e.StackTrace);
                    Log.Exception(e);
                }
            }
            else
            {
                Log.Write("Plugin", $"No constructor found for '{path}'", LogLevel.Warning);
            }

            return null;
        }

        public IEnumerable<Type> GetPlugins<T>()
        {
            return _pluginManager.GetPlugins<T>();
        }

        public IEnumerable<Type> GetPlugins(Type pluginInterfaceType)
        {
            return _pluginManager.GetPlugins(pluginInterfaceType);
        }

        public Type? GetPlugin(string path)
        {
            return _pluginManager.GetPlugin(path);
        }
    }
}
