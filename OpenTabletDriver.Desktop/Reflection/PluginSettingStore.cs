using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;

namespace OpenTabletDriver.Desktop.Reflection
{
    public class PluginSettingStore
    {
        public PluginSettingStore(Type type, bool enable = true)
        {
            Path = type?.FullName;
            Settings = type != null ? GetSettingsForType(type) : new ObservableCollection<PluginSetting>();
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

        [JsonIgnore]
        public string Name => AppInfo.PluginManager.GetFriendlyName(Path);

        public ObservableCollection<PluginSetting> Settings { set; get; }

        public bool Enable { set; get; }

        public T Construct<T>() where T : class => AppInfo.PluginManager.ConstructObject<T>(Path);
        public T Construct<T>(IServiceManager provider) where T : class
        {
            var obj = Construct<T>();
            PluginManager.Inject(provider, obj);
            ApplySettings(obj);
            return obj;
        }

        public static PluginSettingStore FromPath(string path)
        {
            var pathType = AppInfo.PluginManager.PluginTypes.FirstOrDefault(t => t.FullName == path);
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
            var name = Name;
            string settings = string.Join(", ", this.Settings.Select(s => $"({s.Property}: {s.Value})"));
            string suffix = Settings.Any() ? $": {settings}" : string.Empty;
            return name + suffix;
        }

        public TypeInfo GetTypeInfo()
        {
            return AppInfo.PluginManager.PluginTypes.FirstOrDefault(t => t.FullName == Path);
        }

        public TypeInfo GetTypeInfo<T>()
        {
            return AppInfo.PluginManager.GetChildTypes<T>().FirstOrDefault(t => t.FullName == Path);
        }
    }
}