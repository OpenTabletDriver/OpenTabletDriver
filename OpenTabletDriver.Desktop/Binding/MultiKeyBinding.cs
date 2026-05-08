using System;
using System.Linq;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Platform.Keyboard;
using OpenTabletDriver.Plugin.Tablet;

#nullable enable

namespace OpenTabletDriver.Desktop.Binding
{
    [PluginName(PLUGIN_NAME)]
    public class MultiKeyBinding : IStateBinding
    {
        private const string PLUGIN_NAME = "Multi-Key Binding";
        private const char KEYS_SPLITTER = '+';

        private string[] keys = [];

        [Resolved]
        public IVirtualKeyboard? Keyboard { set; get; }

        [OnDependencyLoad]
        public void VerifyInitialization()
        {
            if (Keyboard == null)
                Log.Write(nameof(MultiKeyBinding),
                    $"{nameof(IVirtualKeyboard)} unavailable. Keyboard buttons will not work", LogLevel.Error);
        }

        [Property("Keys")]
        public string? Keys
        {
            set
            {
                field = value;
                this.keys = ParseKeys(Keys);
            }
            get;
        }

        public void Press(TabletReference tablet, IDeviceReport report)
        {
            if (keys.Length > 0)
                Keyboard?.Press(this.keys);
        }

        public void Release(TabletReference tablet, IDeviceReport report)
        {
            if (keys.Length > 0)
                Keyboard?.Release(this.keys);
        }

        private string[] ParseKeys(string? str)
        {
            if (str == null || Keyboard == null) return [];
            var newKeys = str.Split(KEYS_SPLITTER, StringSplitOptions.TrimEntries);
            return newKeys.All(k => Keyboard.SupportedKeys.Contains(k)) ? newKeys : [];
        }

        public override string ToString() => $"{PLUGIN_NAME}: {Keys}";
    }
}
