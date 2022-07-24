using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Logging;

namespace OpenTabletDriver.Desktop.Reflection
{
    public static class Extensions
    {
        public static string GetFullyQualifiedName(this Type type)
        {
            if (type.GenericTypeArguments.Any())
            {
                var name = type.Name;
                var index = name.IndexOf('`');
                var query = type.GenericTypeArguments.Select(t => t.GetFullyQualifiedName());
                return type.Namespace + "." + name[..index] + "<" + string.Join(", ", query) + ">";
            }

            return type.GetPath();
        }

        public static string GetPath(this Type type)
        {
            return type.Namespace + "." + type.Name;
        }

        public static string? GetFriendlyName(this Type type)
        {
            return type.GetCustomAttribute<PluginNameAttribute>()?.Name;
        }

        public static bool IsPlatformSupported(this TypeInfo type)
        {
            var attr = type.GetCustomAttribute<SupportedPlatformAttribute>();
            return attr?.IsCurrentPlatform ?? true;
        }

        public static bool IsLoadable(this Assembly asm)
        {
            try
            {
                _ = asm.DefinedTypes;
                return true;
            }
            catch (Exception ex)
            {
                var asmName = asm.GetName();
                var hResultHex = ex.HResult.ToString("X");
                var message = new LogMessage
                {
                    Group = "Plugin",
                    Level = LogLevel.Warning,
                    Message = $"Plugin '{asmName.Name}, Version={asmName.Version}' can't be loaded and is likely out of date. (HResult: 0x{hResultHex})",
                    StackTrace = ex.Message + Environment.NewLine + ex.StackTrace
                };
                Log.Write(message);
                return false;
            }
        }

        public static PluginSettings GetDefaultSettings(
            this IServiceProvider serviceProvider,
            Type type,
            params object[] additionalDeps
        )
        {
            var settings = EnumerateDefaultSettings(type, serviceProvider, additionalDeps);
            return new PluginSettings(type, settings);
        }

        public static Exception GetInnermostException(this Exception e)
        {
            var ex = e;
            while (ex.InnerException != null)
                ex = ex.InnerException;
            return ex;
        }

        private static IEnumerable<PluginSetting> EnumerateDefaultSettings(this Type type, IServiceProvider serviceProvider,
            object[] additionalDeps)
        {
            var settingProperties = from property in type.GetProperties()
                where property.GetCustomAttribute<SettingAttribute>() != null
                select property;

            foreach (var property in settingProperties)
            {
                var defaultValue = GetDefaultValue(type, property, serviceProvider, additionalDeps);
                if (defaultValue != null)
                {
                    yield return new PluginSetting(property, defaultValue);
                }
            }
        }

        private static object? GetDefaultValue(
            this Type type,
            PropertyInfo property,
            IServiceProvider serviceProvider,
            object[] additionalServices
        )
        {
            var value = property.GetCustomAttribute<DefaultValueAttribute>()?.Value;
            return value ?? property.GetCustomAttribute<MemberSourcedDefaultsAttribute>()
                ?.GetDefaultValue(serviceProvider, type, additionalServices);
        }

        private static object? GetDefaultValue(
            this MemberSourcedDefaultsAttribute attribute,
            IServiceProvider serviceProvider,
            Type sourceType,
            object[] additionalServices
        )
        {
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;

            var targetMemberName = attribute.TargetMemberName;

            if (sourceType.GetMethod(targetMemberName, bindingFlags) is MethodInfo defaultMethod)
                return defaultMethod.InvokeWithProvider(serviceProvider, additionalServices);

            if (sourceType.GetProperty(targetMemberName, bindingFlags) is PropertyInfo defaultProperty)
                return defaultProperty.GetValue(null);

            if (sourceType.GetField(targetMemberName, bindingFlags) is FieldInfo defaultField)
                return defaultField.GetValue(null);

            return null;
        }

        private static object? InvokeWithProvider(this MethodInfo method, IServiceProvider provider, object[] additionalServices)
        {
            var args = from parameter in method.GetParameters()
                let type = parameter.ParameterType
                select provider.GetService(type) ?? additionalServices.First(s => s.GetType().IsAssignableTo(type));

            return method.Invoke(null, args.ToArray());
        }
    }
}
