using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Daemon.Contracts;
using OpenTabletDriver.Daemon.Contracts.Persistence;
using OpenTabletDriver.Logging;

namespace OpenTabletDriver.Daemon.Reflection
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

        public static bool IsPlatformSupported(this Type type)
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

        public static IEnumerable<PluginSettingMetadata> GetSettingsMetadatas(this Type type, IServiceProvider serviceProvider)
        {
            foreach (var property in type.GetProperties())
            {
                var settingProperty = property.GetCustomAttribute<SettingAttribute>();
                if (settingProperty == null)
                    continue;

                var propertyName = property.Name;
                var friendlyName = settingProperty.DisplayName;
                var description = settingProperty.Description;
                var longDescription = property.GetCustomAttribute<ToolTipAttribute>()?.ToolTip;
                var settingType = GetSettingType(property);

                var pluginSettingsMetadata = new PluginSettingMetadata(propertyName, friendlyName, description, longDescription, settingType);

                if (property.GetCustomAttribute<MemberValidatedAttribute>() is MemberValidatedAttribute validated)
                {
                    // TODO: Move this check during plugin loading
                    if (property.PropertyType != typeof(string))
                        throw new InvalidOperationException($"{nameof(MemberValidatedAttribute)} can only be applied to string properties.");

                    const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static;
                    var targetMemberName = validated.MemberName;

                    var validatorCandidates = type.GetMember(targetMemberName, bindingFlags);
                    if (validatorCandidates.Length == 0)
                        throw new InvalidOperationException($"Static target member '{targetMemberName}' does not exist.");

                    var validator = validatorCandidates[0];

                    var validValues = validator.MemberType switch
                    {
                        MemberTypes.Field => (validator as FieldInfo)?.GetValue(null) as IEnumerable<string>,
                        MemberTypes.Property => (validator as PropertyInfo)?.GetValue(null) as IEnumerable<string>,
                        MemberTypes.Method => invokeMethod((validator as MethodInfo)!, serviceProvider),
                        _ => throw new InvalidOperationException($"Static target member '{targetMemberName}' is not a field, property or method.")
                    };

                    pluginSettingsMetadata.Enum(validValues?.ToArray() ?? Array.Empty<string>());

                    static IEnumerable<string>? invokeMethod(MethodInfo validator, IServiceProvider serviceProvider)
                    {
                        var parameters = validator.GetParameters();
                        if (parameters.Length == 0)
                            return validator.Invoke(null, null) as IEnumerable<string>;
                        if (parameters.Length == 1 && parameters[0].ParameterType == typeof(IServiceProvider))
                            return validator.Invoke(null, new object[] { serviceProvider }) as IEnumerable<string>;

                        throw new InvalidOperationException($"Static target method '{validator.Name}' has invalid parameters.");
                    }
                }
                else if (property.PropertyType.IsEnum)
                {
                    pluginSettingsMetadata.Enum(Enum.GetNames(property.PropertyType));
                }

                if (property.GetCustomAttribute<RangeSettingAttribute>() is RangeSettingAttribute range)
                {
                    pluginSettingsMetadata.Range(range.Min, range.Max, range.Step);
                }

                if (property.GetCustomAttribute<DefaultValueAttribute>() is DefaultValueAttribute defaultValue)
                {
                    pluginSettingsMetadata.Default(defaultValue.Value);
                }

                if (property.GetCustomAttribute<MemberSourcedDefaultsAttribute>() is MemberSourcedDefaultsAttribute memberSourcedDefaults)
                {
                    const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static;
                    var targetMemberName = memberSourcedDefaults.TargetMemberName;

                    if (type.GetProperty(targetMemberName, bindingFlags) is PropertyInfo defaultProperty)
                    {
                        pluginSettingsMetadata.Default(defaultProperty.GetValue(null));
                    }
                    else if (type.GetField(targetMemberName, bindingFlags) is FieldInfo defaultField)
                    {
                        pluginSettingsMetadata.Default(defaultField.GetValue(null));
                    }
                }

                if (property.GetCustomAttribute<UnitAttribute>() is UnitAttribute unit)
                {
                    pluginSettingsMetadata.Unit(unit.Unit);
                }

                yield return pluginSettingsMetadata;
            }
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

        private static SettingType GetSettingType(PropertyInfo propertyInfo)
        {
            var propertyType = propertyInfo.PropertyType;

            if (propertyType == typeof(bool))
                return SettingType.Boolean;
            if (propertyType == typeof(int) || propertyType == typeof(uint))
                return SettingType.Integer;
            if (propertyType == typeof(float) || propertyType == typeof(double))
                return SettingType.Number;
            if (propertyType == typeof(string))
                return SettingType.String;

            return SettingType.Unknown;
        }
    }
}
