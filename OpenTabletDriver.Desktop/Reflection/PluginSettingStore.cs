using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using OpenTabletDriver.Plugin.Attributes;

namespace OpenTabletDriver.Desktop.Reflection
{
    public class PluginSettingStore
    {
        public PluginSettingStore(Type type, bool enable = true)
        {
            Path = type.FullName;
            Settings = GetSettingsForType(type);
            Enable = enable;
        }

        public PluginSettingStore(object source, bool enable = true)
        {
            if (source != null)
            {
                var sourceType = source.GetType();
                Path = sourceType.FullName;
                Settings = GetSettingsForType(sourceType, source);
                Enable = enable;
            }
            else
            {
                throw new NullReferenceException("Creating a plugin setting store from a null object is not allowed.");
            }
        }

        [JsonConstructor]
        private PluginSettingStore()
        {
        }

        public string Path { set; get; }

        public ObservableCollection<PluginSetting> Settings { set; get; }

        public bool Enable { set; get; }

        public PluginReference GetPluginReference() => new StoredPluginReference(AppInfo.PluginManager, this);

        public T Construct<T>() where T : class => this.GetPluginReference().Construct<T>();
        public T Construct<T>(params object[] args) where T : class => this.GetPluginReference().Construct<T>(args);

        public static PluginSettingStore FromPath(string path)
        {
            var pathType = AppInfo.PluginManager.GetPluginReference(path).GetTypeReference();
            return pathType != null ? new PluginSettingStore(pathType) : null;
        }

        public void ApplySettings(object target)
        {
            if (target == null)
                return;

            var properties = from property in target.GetType().GetProperties()
                let attrs = property.GetCustomAttributes(true)
                where attrs.Any(attr => attr is PropertyAttribute)
                select property;

            foreach (var setting in Settings)
            {
                if (properties.FirstOrDefault(d => d.Name == setting.Property) is PropertyInfo property)
                {
                    if (setting.HasValue)
                        property.SetValue(target, setting.GetValue(property.PropertyType));
                    else if (property.GetCustomAttribute<DefaultPropertyValueAttribute>() is DefaultPropertyValueAttribute defaults)
                        property.SetValue(target, defaults.Value);
                }
            }
        }

        private static ObservableCollection<PluginSetting> GetSettingsForType(Type targetType, object source = null)
        {
            var settings = from property in targetType.GetProperties()
                where property.GetCustomAttribute<PropertyAttribute>() is PropertyAttribute
                select new PluginSetting(property, source == null ? null : property.GetValue(source));
            return new ObservableCollection<PluginSetting>(settings);
        }

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
            get
            {
                var result = Settings.FirstOrDefault(s => s.Property == propertyName);
                if (result == null)
                {
                    var newSetting = new PluginSetting(propertyName, null);
                    Settings.Add(newSetting);
                    return newSetting;
                }
                return result;
            }
        }

        public PluginSetting this[PropertyInfo property]
        {
            set => this[property.Name] = value;
            get => this[property.Name];
        }

        public string GetHumanReadableString()
        {
            var name = this.GetPluginReference().Name;
            string settings = string.Join(", ", this.Settings.Select(s => $"({s.Property}: {s.Value})"));
            string suffix = Settings.Any() ? $": {settings}" : string.Empty;
            return name + suffix;
        }
    }
}