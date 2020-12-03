using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Reflection;

namespace OpenTabletDriver.Desktop.Reflection
{
    public class PluginSettingStore
    {
        public PluginSettingStore(object source)
            : this(source.GetType(), source)
        {
            if (source is MemberInfo)
                throw new InvalidOperationException($"Invalid store type '{source.GetType().FullName}'. It is likely this was unintentional.");
        }

        public PluginSettingStore(Type targetType, object source)
        {
            Path = targetType.FullName;

            var settings = from property in targetType.GetProperties()
                where property.GetCustomAttribute<PropertyAttribute>() is PropertyAttribute
                select new PluginSetting(property, source == null ? null : property.GetValue(source));

            Settings = new ObservableCollection<PluginSetting>(settings);
        }

        [JsonConstructor]
        private PluginSettingStore()
        {
        }

        public string Path { set; get; }

        public ObservableCollection<PluginSetting> Settings { set; get; }

        public PluginReference GetPluginReference() => new StoredPluginReference(AppInfo.PluginManager, this);

        public T Construct<T>() where T : class => this.GetPluginReference().Construct<T>();
        public T Construct<T>(params object[] args) where T : class => this.GetPluginReference().Construct<T>(args);

        public void ApplySettings(object target)
        {
            if (target == null)
                return;

            var properties = from property in target.GetType().GetProperties()
                let attrs = property.GetCustomAttributes(true)
                where attrs.Any(attr => attr is PropertyAttribute)
                select property;

            foreach (var setting in Settings)
                if (properties.FirstOrDefault(d => d.Name == setting.PropertyName) is PropertyInfo property)
                    property.SetValue(target, setting.Value);
        }

        public PluginSetting this[string propertyName] => Settings.FirstOrDefault(s => s.PropertyName == propertyName);
        public PluginSetting this[PropertyInfo property] => this[property.Name];
    }
}