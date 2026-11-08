using System;
using System.Collections.Generic;
using OpenTabletDriver.Desktop.Interop.Input.Keyboard;
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

        [OnDependencyLoad]
        public void VerifyInitialization()
        {
            if (Keyboard == null)
                Log.Write(nameof(KeyBinding),
                    $"{nameof(IVirtualKeyboard)} unavailable. {PLUGIN_NAME} will not work", LogLevel.Error);
        }

        public void Press(TabletReference tablet, IDeviceReport report)
        {
            if (!string.IsNullOrWhiteSpace(Key))
                Keyboard?.Press(Key);
        }

        public void Release(TabletReference tablet, IDeviceReport report)
        {
            if (!string.IsNullOrWhiteSpace(Key))
                Keyboard?.Release(Key);
        }

        private static IEnumerable<string>? validKeys;
        public static IEnumerable<string>? ValidKeys
        {
            get => validKeys ??= true switch
            {
                _ when OperatingSystem.IsWindows() => WindowsVirtualKeyboard.EtoKeysymToVK.Keys,
                _ when OperatingSystem.IsLinux() => EvdevVirtualKeyboard.EtoKeysymToEventCode.Keys,
                _ when OperatingSystem.IsMacOS() => MacOSVirtualKeyboard.EtoKeysymToVK.Keys,
                _ => null
            };
        }

        public override string ToString() => $"{PLUGIN_NAME}: {Key}";
    }
}
