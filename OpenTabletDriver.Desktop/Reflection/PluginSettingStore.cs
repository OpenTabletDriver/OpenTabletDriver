using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Reflection
{
    public class PluginSettingStore
    {
        private static readonly Type _tabletRefType = typeof(TabletReference);

        public PluginSettingStore(Type? type, bool enable = true)
        {
            Path = type?.FullName;
            Settings = type != null ? GetSettingsForType(type) : new ObservableCollection<PluginSetting>();
            Enable = enable;
        }

        public PluginSettingStore(object source, bool enable = true)
        {
            var sourceType = source.GetType();

            Path = sourceType.FullName;

            Settings = GetSettingsForType(sourceType, source);
            Enable = enable;
        }

        [JsonConstructor]
        public PluginSettingStore(string path, ObservableCollection<PluginSetting> settings)
        {
            Path = path;
            Settings = settings;
        }

        // TODO: make non-nullable or similar fix, since it never makes sense to have a null/empty path? -gonX
        public string? Path { set; get; }

        [JsonIgnore]
        public string? Name => Path != null ? AppInfo.PluginManager.GetFriendlyName(Path) : null;

        public ObservableCollection<PluginSetting> Settings
        {
            set => field = SanitizeSettings(value);
            get;
        }

        /// <summary>
        /// Accepts a missing collection ("Settings": null) and drops null entries
        /// ("Settings": [null] in a corrupt or hand-edited settings.json), which would
        /// otherwise NRE every consumer - <see cref="ApplySettings"/>, the indexer and
        /// <see cref="GetHumanReadableString"/> - and take the whole settings
        /// application down with them.
        /// </summary>
        private static ObservableCollection<PluginSetting> SanitizeSettings(ObservableCollection<PluginSetting>? settings)
        {
            if (settings == null)
                return new ObservableCollection<PluginSetting>();

            if (!settings.Any(setting => setting == null))
                return settings;

            return new ObservableCollection<PluginSetting>(settings.Where(setting => setting != null));
        }

        public bool Enable { set; get; }

        /// <summary>
        /// The service manager of the host this store belongs to (the profile's binding
        /// service manager when constructed through <see cref="Construct{T}(IServiceManager, TabletReference?)"/>).
        /// Services not present in <see cref="AppInfo.PluginManager"/> - most importantly the
        /// output mode's pointer handlers - are injected from here, and propagated to nested
        /// stores such as bindings stored in [BindingProperty] plugin options.
        /// </summary>
        [JsonIgnore]
        public IServiceManager? Provider { set; get; }

        public T? Construct<T>(TabletReference? tabletReference = null, bool trigger = true) where T : class
        {
            if (Path == null)
            {
                Log.Write($"Construct<T>", $"{nameof(Path)} is null, returning null", LogLevel.Debug);
                return null;
            }

            var obj = AppInfo.PluginManager.ConstructObject<T>(Path);
            ApplySettings(obj);
            if (obj != null && Provider != null)
            {
                PluginManager.Inject(Provider, obj);
                PropagateProvider(obj, Provider);
            }
            if (trigger)
                TriggerEventMethods(obj, tabletReference);
            return obj;
        }

        public T? Construct<T>(IServiceManager provider, TabletReference? tabletReference = null) where T : class
        {
            Provider = provider;
            return Construct<T>(tabletReference);
        }

        /// <summary>
        /// Passes this store's <see cref="Provider"/> to stores held in plugin properties
        /// (e.g. a binding stored in a [BindingProperty] option), so plugins can construct
        /// them with the parameterless Construct overload and still receive the host's services.
        /// </summary>
        private static void PropagateProvider(object target, IServiceManager provider)
        {
            foreach (var property in target.GetType().GetProperties())
            {
                if (property.GetIndexParameters().Length != 0)
                    continue;
                if (!typeof(PluginSettingStore).IsAssignableFrom(property.PropertyType))
                    continue;

                try
                {
                    if (property.GetValue(target) is PluginSettingStore nested)
                        nested.Provider = provider;
                }
                catch (Exception e)
                {
                    Log.Write(nameof(PluginSettingStore), $"Failed to propagate the service provider through '{property.Name}'.", LogLevel.Warning);
                    Log.Exception(e);
                }
            }
        }

        public static PluginSettingStore? FromPath(string? path)
        {
            var pathType = AppInfo.PluginManager.PluginTypes.FirstOrDefault(t => t.FullName == path);
            return pathType != null ? new PluginSettingStore(pathType) : null;
        }

        /// <summary>
        /// Apply <see cref="Settings"/> values for <see cref="PropertyAttribute"/> properties
        /// </summary>
        /// <param name="target">The target to apply settings for</param>
        public void ApplySettings(object? target)
        {
            if (target == null)
                return;

            var properties = from property in target.GetType().GetProperties()
                             let attrs = property.GetCustomAttributes(true)
                             where attrs.Any(attr => attr is PropertyAttribute)
                             select property;

            foreach (var setting in Settings)
            {
                if (properties.FirstOrDefault(d => d.Name == setting.Property) is not PropertyInfo property)
                    continue;

                try
                {
                    if (setting.HasValue)
                        property.SetValue(target, setting.GetValue(property.PropertyType));
                    else if (property.GetCustomAttribute<DefaultPropertyValueAttribute>() is DefaultPropertyValueAttribute defaults)
                        property.SetValue(target, defaults.Value);
                }
                catch (Exception e)
                {
                    // A stored value whose shape no longer matches the property (hand-edited or
                    // otherwise corrupt settings.json) must not escape: it would abort this
                    // store's Construct, and with it the daemon's entire SetSettings pass.
                    // Skip the setting and keep the property's current value - the same
                    // graceful handling the settings UI already applies when it reads one.
                    Log.Write(nameof(PluginSettingStore), $"Failed to apply '{setting.Property}' of '{Path}': {e.Message}", LogLevel.Warning);
                    Log.Exception(e);
                }
            }
        }

        private static ObservableCollection<PluginSetting> GetSettingsForType(Type targetType, object? source = null)
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
            var name = Name ?? Path ?? string.Empty;
            string settings = string.Join(", ", this.Settings.Select(s => $"({s.Property}: {FormatSettingValue(s.Value)})"));
            string suffix = Settings.Any() ? $": {settings}" : string.Empty;
            return name + suffix;
        }

        public string GetElidedHumanReadableString(int elideAt)
        {
            var baseString = this.GetHumanReadableString();
            if (baseString.Length > elideAt)
                return baseString[..elideAt] + "...";
            return baseString;
        }

        /// <summary>
        /// Renders a stored setting value for the summary. A value that holds another
        /// store (a [BindingProperty] option) is summarised through
        /// <see cref="GetHumanReadableString"/> instead of being dumped as the indented
        /// JSON that <see cref="JToken.ToString()"/> produces.
        /// </summary>
        private static string FormatSettingValue(JToken? value)
        {
            switch (value)
            {
                case null:
                    return string.Empty;
                case JValue jvalue:
                    return jvalue.ToString() ?? string.Empty;
                case JObject json when TryGetNestedStore(json, out var nested):
                    return nested.GetHumanReadableString();
                default:
                    // Any other JSON container: compact, without indentation.
                    return value.ToString(Formatting.None);
            }
        }

        private static bool TryGetNestedStore(JObject json, out PluginSettingStore nested)
        {
            nested = null!;

            if (json["Path"]?.Type != JTokenType.String)
                return false;

            try
            {
                if (json.ToObject<PluginSettingStore>() is PluginSettingStore store)
                {
                    nested = store;
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Write(nameof(PluginSettingStore), $"Failed to read the nested store of '{json["Path"]}'.", LogLevel.Warning);
                Log.Exception(e);
            }

            return false;
        }

        public TypeInfo? GetTypeInfo()
        {
            return AppInfo.PluginManager.PluginTypes.FirstOrDefault(t => t.FullName == Path);
        }

        public TypeInfo? GetTypeInfo<T>()
        {
            return AppInfo.PluginManager.GetChildTypes<T>().FirstOrDefault(t => t.FullName == Path);
        }

        private static void TriggerEventMethods(object? obj, TabletReference? tabletReference)
        {
            if (obj == null)
                return;

            var properties = from property in obj.GetType().GetProperties()
                             let attr = property.GetCustomAttribute<TabletReferenceAttribute>()
                             where attr != null && property.PropertyType == _tabletRefType
                             select property;

            foreach (var property in properties)
                property.SetValue(obj, tabletReference);

            var methods = from method in obj.GetType().GetMethods()
                          let attr = method.GetCustomAttribute<OnDependencyLoadAttribute>()
                          where attr != null
                          select method;

            foreach (var method in methods)
                method.Invoke(obj, Array.Empty<object>());
        }
    }
}
