using System;
using System.Collections.Generic;
using System.Linq;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Desktop.Interop.Input.Absolute;
using OpenTabletDriver.Native.Linux.Evdev;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding.LinuxArtistMode
{
    [PluginName("Linux Artist Mode"), SupportedPlatform(PluginPlatform.Linux)]
    public class LinuxArtistModeButtonBinding : IStateBinding
    {
        private readonly EvdevVirtualTablet virtualTablet = (EvdevVirtualTablet)DesktopInterop.VirtualTablet;

        public static Dictionary<string, EventCode> SupportedButtons { get; } = new() {
            { "Pen Button 1", EventCode.BTN_STYLUS },
            { "Pen Button 2", EventCode.BTN_STYLUS2 },
            { "Pen Button 3", EventCode.BTN_STYLUS3 },
        };

        public static string[] ValidButtons => SupportedButtons.Keys.ToArray();

        [Property("Button"), PropertyValidated(nameof(ValidButtons))]
        public string Button { get; set; }

        public void Press(TabletReference tablet, IDeviceReport report)
        {
            SetState(true);
        }

        public void Release(TabletReference tablet, IDeviceReport report)
        {
            SetState(false);
        }

        private void SetState(bool state)
        {
            if (!SupportedButtons.TryGetValue(Button, out var eventCode))
                throw new InvalidOperationException($"Invalid Button '{Button}'");

            virtualTablet.SetKeyState(eventCode, state);
        }

        public override string ToString() => $"{nameof(LinuxArtistModeButtonBinding)}: {Button}";
    }
}
