using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Platform.Keyboard;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Daemon.Binding
{
    [PluginName(PLUGIN_NAME)]
    public class KeyBinding : IStateBinding
    {
        private const string PLUGIN_NAME = "Key Binding";

        private readonly InputDevice _device;
        private readonly IVirtualKeyboard _keyboard;

        private BindableKey _key;

        public KeyBinding(InputDevice device, IVirtualKeyboard keyboard, ISettingsProvider settingsProvider)
        {
            _device = device;
            _keyboard = keyboard;

            settingsProvider.Inject(this);
        }

        [Setting("Key"), MemberValidated(nameof(GetValidKeys))]
        public string Key
        {
            get => _key.ToStringFast();
            set => _key = BindableKeyExtensions.TryParse(value, out var key)
                        ? key
                        : throw new InvalidOperationException($"Invalid key: {value}");
        }

        public void Press(IDeviceReport report)
        {
            _keyboard.Press(_key);
        }

        public void Release(IDeviceReport report)
        {
            _keyboard.Release(_key);
        }

        public static IEnumerable<string> GetValidKeys(IServiceProvider serviceProvider)
        {
            var keysProvider = serviceProvider.GetRequiredService<IKeyMapper>();
            return keysProvider.GetBindableKeys()
                .Select(key => key.ToStringFast())
                .ToList();
        }

        public override string ToString() => $"{PLUGIN_NAME}: {Key}";
    }
}
