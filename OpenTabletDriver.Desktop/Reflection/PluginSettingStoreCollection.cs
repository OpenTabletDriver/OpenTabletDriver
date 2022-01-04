using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace OpenTabletDriver.Desktop.Reflection
{
    public class PluginSettingStoreCollection : ObservableCollection<PluginSettingStore>
    {
        public PluginSettingStoreCollection()
        {
        }

        public PluginSettingStoreCollection(IEnumerable<PluginSettingStore> collection)
            : base(collection)
        {
        }

        public PluginSettingStoreCollection Trim()
        {
            while (true)
            {
                if (!Remove(null))
                    break;
            }

            return this;
        }

        public PluginSettingStoreCollection SetExpectedCount(int expectedCount)
        {
            while (Count < expectedCount)
                Add(null);

            return this;
        }

        public PluginSettingStore FromType(TypeInfo type)
        {
            if (type == null)
                return null;

            var store = this.FirstOrDefault(s => s.Path == type.FullName) ?? new PluginSettingStore(type, false);
            if (!this.Contains(store))
                this.Add(store);
            return store;
        }
    }
}
