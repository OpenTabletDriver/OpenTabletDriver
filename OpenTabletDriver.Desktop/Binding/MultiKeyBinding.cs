using System;
using System.Linq;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Platform.Keyboard;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    [PluginName(PLUGIN_NAME)]
    public class MultiKeyBinding : IStateBinding
    {
        private const string PLUGIN_NAME = "Multi-Key Binding";
        private const char KEYS_SPLITTER = '+';

        private string[]? keys;
        private string? keysString;

        [Resolved]
        public IVirtualKeyboard? Keyboard { set; get; }

        [Property("Keys")]
        public string? Keys
        {
            set
            {
                this.keysString = value;
                this.keys = ParseKeys(value);
            }
            get => this.keysString;
        }

        public void Press(TabletReference tablet, IDeviceReport report)
        {
            if (this.Keyboard == null)
                throw new InvalidOperationException($"Resolved property '{nameof(Keyboard)}' has not been injected");

            if (keys?.Length > 0)
                Keyboard.Press(this.keys);
            else
                Log.Debug(nameof(MultiKeyBinding), "No keys to press");
        }

        public void Release(TabletReference tablet, IDeviceReport report)
        {
            if (this.Keyboard == null)
                throw new InvalidOperationException($"Resolved property '{nameof(Keyboard)}' has not been injected");

            if (keys?.Length > 0)
                Keyboard.Release(this.keys);
            else
                Log.Debug(nameof(MultiKeyBinding), "No keys to release");
        }

        private string[] ParseKeys(string? str)
        {
            if (str == null) return [];

            if (this.Keyboard == null)
                throw new InvalidOperationException($"Resolved property '{nameof(Keyboard)}' has not been injected");

            var newKeys = str.Split(KEYS_SPLITTER, StringSplitOptions.TrimEntries);
            var rv = newKeys.All(k => Keyboard.SupportedKeys.Contains(k)) ? newKeys : [];
            if (rv.Length != newKeys.Length)
                Log.Write(nameof(MultiKeyBinding), $"Only partially parsed input keys: {newKeys}", LogLevel.Warning);
            return rv;
        }

        public override string ToString() => $"{PLUGIN_NAME}: {Keys}";
    }
}
