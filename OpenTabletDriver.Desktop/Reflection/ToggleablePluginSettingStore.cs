using System;

namespace OpenTabletDriver.Desktop.Reflection
{
    public class ToggleablePluginSettingStore : PluginSettingStore
    {
        public ToggleablePluginSettingStore(object source)
            : base(source)
        {
        }

        public ToggleablePluginSettingStore(Type targetType, object source)
            : base(targetType, source)
        {
        }

        public bool Enable { set; get; }
    }
}