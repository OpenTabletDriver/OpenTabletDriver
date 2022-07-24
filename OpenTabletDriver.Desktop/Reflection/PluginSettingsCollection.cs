using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace OpenTabletDriver.Desktop.Reflection
{
    public class PluginSettingsCollection : ObservableCollection<PluginSettings>
    {
        public PluginSettingsCollection()
        {
        }

        public PluginSettingsCollection(IEnumerable<PluginSettings> collection)
            : base(collection)
        {
        }

        public PluginSettingsCollection SetExpectedCount(int expectedCount)
        {
            while (Count < expectedCount)
                Add(new PluginSettings(typeof(object)));

            return this;
        }

        public PluginSettings FromType(TypeInfo type)
        {
            var store = this.FirstOrDefault(s => s.Path == type.FullName) ?? new PluginSettings(type, false);
            if (!Contains(store))
                Add(store);
            return store;
        }
    }
}
