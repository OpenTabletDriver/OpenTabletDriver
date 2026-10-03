using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Keyboard;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    [PluginName(PLUGIN_NAME)]
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public class MultiKeyBinding(IVirtualKeyboard virtualKeyboard) : IStateBinding
    {
        private const string PLUGIN_NAME = "Multi-Key Binding";
        private const char KEYS_SPLITTER = '+';

        private string[]? keys;
        private string? keysString;

        [Property("Keys")]
        public string? Keys
        {
            set
            {
                this.keysString = value;
                this.keys = ParseKeys(Keys);
            }
            get => this.keysString;
        }

        public void Press(TabletReference tablet, IDeviceReport report)
        {
            if (keys?.Length > 0)
                virtualKeyboard.Press(this.keys);
        }

        public void Release(TabletReference tablet, IDeviceReport report)
        {
            if (keys?.Length > 0)
                virtualKeyboard.Release(this.keys);
        }

        private string[] ParseKeys(string? str)
        {
            if (str == null) return [];
            var newKeys = str.Split(KEYS_SPLITTER, StringSplitOptions.TrimEntries);
            return newKeys.All(k => virtualKeyboard.SupportedKeys.Contains(k)) ? newKeys : [];
        }

        public override string ToString() => $"{PLUGIN_NAME}: {Keys}";
    }
}
