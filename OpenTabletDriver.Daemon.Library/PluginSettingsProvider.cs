using System.Reflection;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Daemon.Contracts.Persistence;

namespace OpenTabletDriver.Daemon
{
    public class PluginSettingsProvider : ISettingsProvider
    {
        private readonly PluginSettings _store;

        public PluginSettingsProvider(PluginSettings store)
        {
            _store = store;
        }

        public void Inject(object? obj)
        {
            if (obj == null)
                return;

            var type = obj.GetType();
            var group = type.Name;

            foreach (var property in type.GetProperties())
            {
                var attr = property.GetCustomAttribute<SettingAttribute>();
                if (attr != null)
                {
                    var setting = _store[property.Name];
                    var value = setting.GetValue(property.PropertyType);
                    property.SetValue(obj, value);

                    if (value != null)
                        Log.Debug(group, $"{property.Name}: {value}");
                }
            }
        }
    }
}
