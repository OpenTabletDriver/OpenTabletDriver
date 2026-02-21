using System;
using System.Collections.Generic;
using OpenTabletDriver.Desktop.Interop.Input.Keyboard;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Platform.Keyboard;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    [PluginName(PLUGIN_NAME)]
    public class KeyBinding : IStateBinding
    {
        private const string PLUGIN_NAME = "Key Binding";

        [Resolved]
        public IVirtualKeyboard? Keyboard { set; get; }

        [Property("Key"), PropertyValidated(nameof(ValidKeys))]
        public string? Key { set; get; }

        public void Press(TabletReference tablet, IDeviceReport report)
        {
            if (Keyboard == null)
                throw new InvalidOperationException($"{nameof(KeyBinding)} can only be used if {nameof(Keyboard)} is set.");

            if (!string.IsNullOrWhiteSpace(Key))
                Keyboard.Press(Key);
        }

        public void Release(TabletReference tablet, IDeviceReport report)
        {
            if (Keyboard == null)
                throw new InvalidOperationException($"{nameof(KeyBinding)} can only be used if {nameof(Keyboard)} is set.");

            if (!string.IsNullOrWhiteSpace(Key))
                Keyboard.Release(Key);
        }

        private static IEnumerable<string> validKeys;
        public static IEnumerable<string> ValidKeys =>
            validKeys ??= SystemInterop.CurrentPlatform switch
            {
                PluginPlatform.Windows => WindowsVirtualKeyboard.EtoKeysymToVK.Keys,
                PluginPlatform.Linux => EvdevVirtualKeyboard.EtoKeysymToEventCode.Keys,
                PluginPlatform.MacOS => MacOSVirtualKeyboard.EtoKeysymToVK.Keys,
                _ => throw new InvalidOperationException($"Unknown platform {SystemInterop.CurrentPlatform}"),
            };

        public override string ToString() => $"{PLUGIN_NAME}: {Key}";
    }
}
