using System.Collections.Generic;
using System.Collections.ObjectModel;

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
    }
}