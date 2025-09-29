using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using OpenTabletDriver.Plugin;

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
            int trimmed = 0;

            while (Count < expectedCount)
                Add(null);

            while (Count > expectedCount)
            {
                var me = this[Count - 1];
                Log.Write(nameof(PluginSettingStoreCollection), $"Removing '{me.GetHumanReadableString()}'", LogLevel.Debug);
                trimmed++;
                RemoveAt(Count - 1);
            }

            if (trimmed > 0)
                Log.WriteNotify(nameof(PluginSettingStoreCollection), $"Too many buttons configured for associated device - trimmed {trimmed} plugin settings for consistency. Some settings may have been lost.", LogLevel.Warning);

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
