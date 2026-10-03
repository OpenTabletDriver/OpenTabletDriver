using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Desktop.Interop.Input.Keyboard;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Keyboard;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    [PluginName(PLUGIN_NAME)]
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public class KeyBinding(IVirtualKeyboard virtualKeyboard) : IStateBinding
    {
        private const string PLUGIN_NAME = "Key Binding";

        [Property("Key"), PropertyValidated(nameof(ValidKeys))]
        public string? Key { set; get; }

        public void Press(TabletReference tablet, IDeviceReport report)
        {
            if (!string.IsNullOrWhiteSpace(Key))
                virtualKeyboard.Press(Key);
        }

        public void Release(TabletReference tablet, IDeviceReport report)
        {
            if (!string.IsNullOrWhiteSpace(Key))
                virtualKeyboard.Release(Key);
        }

        private static IEnumerable<string>? validKeys;
        public static IEnumerable<string>? ValidKeys
        {
            get => validKeys ??= SystemInterop.CurrentPlatform switch
            {
                PluginPlatform.Windows => WindowsVirtualKeyboard.EtoKeysymToVK.Keys,
                PluginPlatform.Linux => EvdevVirtualKeyboard.EtoKeysymToEventCode.Keys,
                PluginPlatform.MacOS => MacOSVirtualKeyboard.EtoKeysymToVK.Keys,
                _ => null
            };
        }

        public override string ToString() => $"{PLUGIN_NAME}: {Key}";
    }
}
