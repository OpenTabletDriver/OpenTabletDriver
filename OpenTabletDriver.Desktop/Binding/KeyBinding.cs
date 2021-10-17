using System.Collections.Generic;
using OpenTabletDriver.Desktop.Interop.Input.Keyboard;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Keyboard;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    [PluginName(PLUGIN_NAME)]
    public class KeyBinding : IStateBinding
    {
        public KeyBinding(IVirtualKeyboard keyboard)
        {
            Keyboard = keyboard;
        }

        private const string PLUGIN_NAME = "Key Binding";

        public IVirtualKeyboard Keyboard { set; get; }

        [Property("Key"), PropertyValidated(nameof(ValidKeys))]
        public string Key { set; get; }

        public void Press(TabletReference tablet, IDeviceReport report)
        {
            if (!string.IsNullOrWhiteSpace(Key))
                Keyboard.Press(Key);
        }

        public void Release(TabletReference tablet, IDeviceReport report)
        {
            if (!string.IsNullOrWhiteSpace(Key))
                Keyboard.Release(Key);
        }

        private static IEnumerable<string> validKeys;
        public static IEnumerable<string> ValidKeys
        {
            get => validKeys ??= SystemInterop.CurrentPlatform switch
            {
                PluginPlatform.Windows => WindowsVirtualKeyboard.EtoKeysymToVK.Keys,
                PluginPlatform.Linux   => EvdevVirtualKeyboard.EtoKeysymToEventCode.Keys,
                PluginPlatform.MacOS   => MacOSVirtualKeyboard.EtoKeysymToVK.Keys,
                _                      => null
            };
        }

        public override string ToString() => $"{PLUGIN_NAME}: {Key}";
    }
}
