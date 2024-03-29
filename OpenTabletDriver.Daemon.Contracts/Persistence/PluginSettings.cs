using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;
using OpenTabletDriver.Attributes;

namespace OpenTabletDriver.Daemon.Contracts.Persistence
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public class PluginSettings
    {
        public string Path { set; get; } = string.Empty;

        public Collection<PluginSetting> Settings { get; } = new Collection<PluginSetting>();

        public bool Enable { set; get; }

        public PluginSetting this[string propertyName]
        {
            set
            {
                if (Settings.FirstOrDefault(t => t.Property == propertyName) is PluginSetting setting)
                {
                    Settings.Remove(setting);
                    Settings.Add(value);
                }
                else
                {
                    Settings.Add(value);
                }
            }
            get => GetOrDefault(propertyName, null);
        }

        public PluginSettings(Type type, bool enable = true)
        {
            Path = type.FullName!;
            Settings = GetSettingsForType(type);
            Enable = enable;
        }

        public PluginSettings(Type type, IEnumerable<PluginSetting> settings, bool enable = true)
        {
            Path = type.FullName!;
            Settings = new Collection<PluginSetting>(settings.ToArray());
            Enable = enable;
        }

        public PluginSettings(Type type, object settings, bool enable = true)
        {
            Path = type.FullName!;
            Settings = GetSettingsFromObject(settings);
            Enable = enable;
        }

        [JsonConstructor]
        public PluginSettings()
        {
        }

        public override string ToString()
        {
            return base.ToString() + ": " + Path;
        }

        public PluginSetting GetOrDefault(string propertyName, object? defaultValue)
        {
            var result = Settings.FirstOrDefault(s => s.Property == propertyName);
            if (result == null)
            {
                var newSetting = new PluginSetting(propertyName, defaultValue);
                Settings!.Add(newSetting);
                return newSetting;
            }

            return result;
        }

        private static Collection<PluginSetting> GetSettingsForType(Type targetType, object? source = null)
        {
            var settings = from property in targetType.GetProperties()
                where property.GetCustomAttribute<SettingAttribute>() != null
                select new PluginSetting(property, source == null ? null : property.GetValue(source));

            return new Collection<PluginSetting>(settings.ToArray());
        }

        private static Collection<PluginSetting> GetSettingsFromObject(object obj)
        {
            var type = obj.GetType();
            if (type.IsAssignableTo(typeof(PluginSettings)))
            {
                throw new ArgumentException(
                    $"Attempted to generate settings from a {nameof(PluginSettings)}.",
                    nameof(obj)
                );
            }

            var settings = from property in type.GetProperties()
                let name = property.Name
                let value = property.GetValue(obj)
                select new PluginSetting(name, value);

            return new Collection<PluginSetting>(settings.ToArray());
        }
    }
}
