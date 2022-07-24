using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpenTabletDriver.Attributes;

namespace OpenTabletDriver.Desktop.Reflection
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
            var type = GetPluginType(path);
            if (type != null)
            {
                try
                {
                    return _serviceProvider.CreateInstance(type, args) as T;
                }
                catch (TargetInvocationException e) when (e.Message == "Exception has been thrown by the target of an invocation.")
                {
                    Log.Write("Plugin", "Object construction has thrown an exception", LogLevel.Error);
                    Log.Exception(e.GetInnermostException());
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

        public Type? GetPluginType(string path)
        {
            return _pluginManager.PluginTypes.FirstOrDefault(t => t.FullName == path);
        }

        public IEnumerable<Type> GetMatchingTypes(Type baseType)
        {
            return from type in _pluginManager.PluginTypes
                where type.IsAssignableTo(baseType)
                where !type.IsAbstract
                where IsPlatformSupported(type)
                where !IsPluginIgnored(type)
                select type;
        }

        public string? GetFriendlyName(string path)
        {
            var type = GetPluginType(path);
            var name = type?.GetCustomAttribute<PluginNameAttribute>()?.Name;
            return name;
        }

        private static bool IsPlatformSupported(Type type)
        {
            var attr = type.GetCustomAttribute(typeof(SupportedPlatformAttribute), false) as SupportedPlatformAttribute;
            return attr?.IsCurrentPlatform ?? true;
        }

        private static bool IsPluginIgnored(Type type)
        {
            return type.GetCustomAttributes(false).Any(a => a.GetType() == typeof(PluginIgnoreAttribute));
        }
    }
}
