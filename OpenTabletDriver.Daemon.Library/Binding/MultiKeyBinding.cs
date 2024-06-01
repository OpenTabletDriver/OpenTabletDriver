using System;
using System.Collections.Generic;
using System.Linq;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Platform.Keyboard;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Daemon.Binding
{
    [PluginName(PLUGIN_NAME)]
    public class MultiKeyBinding : IStateBinding
    {
        private readonly IVirtualKeyboard _keyboard;

        public MultiKeyBinding(IVirtualKeyboard keyboard, ISettingsProvider settingsProvider)
        {
            _keyboard = keyboard;

            settingsProvider.Inject(this);
        }

        private const string PLUGIN_NAME = "Multi-Key Binding";

        private readonly List<BindableKey> _keys = new(4);
        private string _keysString = null!;

        [Setting("Keys")]
        public string Keys
        {
            get => _keysString;
            set => ParseKeys(_keysString = value);
        }

        public void Press(IDeviceReport report)
        {
            if (_keys.Count > 0)
                _keyboard.Press(_keys);
        }

        public void Release(IDeviceReport report)
        {
            if (_keys.Count > 0)
                _keyboard.Release(_keys);
        }

        private void ParseKeys(string str)
        {
            _keys.Clear();
            var newKeys = str.Split('+', StringSplitOptions.TrimEntries)
                .Select(s => BindableKeyExtensions.TryParse(s, out var key)
                    ? key
                    : throw new InvalidOperationException($"Invalid key: {s}"));
            _keys.AddRange(newKeys);
        }

        public override string ToString() => $"{PLUGIN_NAME}: {Keys}";
    }
}
