using System;
using System.Collections.Generic;
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

        public KeyBinding(InputDevice device, IVirtualKeyboard keyboard, ISettingsProvider settingsProvider)
        {
            _device = device;
            _keyboard = keyboard;

            settingsProvider.Inject(this);
        }

        [Setting("Key"), MemberValidated(nameof(GetValidKeys))]
        public string Key { set; get; } = string.Empty;

        public void Press(IDeviceReport report)
        {
            if (!string.IsNullOrWhiteSpace(Key))
                _keyboard.Press(Key);
        }

        public void Release(IDeviceReport report)
        {
            if (!string.IsNullOrWhiteSpace(Key))
                _keyboard.Release(Key);
        }

        public static IEnumerable<string> GetValidKeys(IServiceProvider serviceProvider)
        {
            var keysProvider = serviceProvider.GetRequiredService<IKeysProvider>();
            return keysProvider.EtoToNative.Keys;
        }

        public override string ToString() => $"{PLUGIN_NAME}: {Key}";
    }
}
