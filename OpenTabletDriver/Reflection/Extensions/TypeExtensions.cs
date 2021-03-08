using System;
using System.Reflection;
using OpenTabletDriver.Plugin.Attributes;

namespace OpenTabletDriver.Reflection.Extensions
{
    public static class TypeExtension
    {
        public static bool IsIgnoredPlugin(this Type type)
        {
            return type.GetCustomAttribute<PluginIgnoreAttribute>() is PluginIgnoreAttribute;
        }

        public static bool IsPlatformSupported(this Type type)
        {
            var attr = type.GetCustomAttribute<SupportedPlatformAttribute>();
            return attr?.IsCurrentPlatform ?? true;
        }
    }
}