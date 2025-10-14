using System.Collections.Generic;
using System.Collections.ObjectModel;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.Desktop.Profiles
{
    public class GenericSettingCollection<T> : ObservableCollection<T> where T : new()
    {
        public GenericSettingCollection()
        {
        }

        public GenericSettingCollection(IEnumerable<T> collection)
            : base(collection)
        {
        }

        public GenericSettingCollection<T> SetExpectedCount(int expectedCount)
        {
            int trimmed = 0;

            while (Count < expectedCount)
                Add(new T());

            while (Count > expectedCount)
            {
                var me = this[Count - 1];
                Log.Write(nameof(T), $"Removing instance n°{me}", LogLevel.Debug);
                trimmed++;
                RemoveAt(Count - 1);
            }

            if (trimmed > 0)
                Log.WriteNotify(nameof(T), $"Too many objects configured for associated device - trimmed {trimmed} settings for consistency. Some settings may have been lost.", LogLevel.Warning);

            return this;
        }
    }
}
