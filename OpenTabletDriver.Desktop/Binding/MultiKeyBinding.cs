using System;
using System.Collections.Generic;
using System.Linq;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Keyboard;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    [PluginName(PLUGIN_NAME)]
    public class MultiKeyBinding : IStateBinding
    {
        public MultiKeyBinding(IVirtualKeyboard keyboard)
        {
            Keyboard = keyboard;
        }

        private const string PLUGIN_NAME = "Multi-Key Binding";
        private const char KEYS_SPLITTER = '+';

        private IList<string> keys;
        private string keysString;

        public IVirtualKeyboard Keyboard { set; get; }

        [Property("Keys")]
        public string Keys
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
            if (keys.Count > 0)
                Keyboard.Press(this.keys);
        }

        public void Release(TabletReference tablet, IDeviceReport report)
        {
            if (keys.Count > 0)
                Keyboard.Release(this.keys);
        }

        private IList<string> ParseKeys(string str)
        {
            var newKeys = str.Split(KEYS_SPLITTER, StringSplitOptions.TrimEntries);
            return newKeys.All(k => Keyboard.SupportedKeys.Contains(k)) ? newKeys : Array.Empty<string>();
        }

        public override string ToString() => $"{PLUGIN_NAME}: {Keys}";
    }
}