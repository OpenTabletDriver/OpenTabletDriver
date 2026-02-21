using System;
using System.Collections.Generic;
using System.Linq;
using OpenTabletDriver.Desktop.Interop.Input.Absolute;
using OpenTabletDriver.Native.Linux.Evdev;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding.LinuxArtistMode
{
    [PluginName("Linux Artist Mode"), SupportedPlatform(PluginPlatform.Linux)]
    public class LinuxArtistModeButtonBinding : IStateBinding
    {
        private EvdevVirtualTablet? virtualTablet;

        [Resolved]
        public IPressureHandler? PressureHandler
        {
            set => virtualTablet = value as EvdevVirtualTablet
                                   ?? throw new InvalidOperationException($"Only {nameof(EvdevVirtualTablet)} is supported for {nameof(LinuxArtistModeButtonBinding)}");
        }

        public static Dictionary<string, EventCode> SupportedButtons { get; } = new() {
            { "Pen Button 1", EventCode.BTN_STYLUS },
            { "Pen Button 2", EventCode.BTN_STYLUS2 },
            { "Pen Button 3", EventCode.BTN_STYLUS3 },
        };

        public static string[] ValidButtons { get; } = SupportedButtons.Keys.ToArray();

        [Property("Button"), PropertyValidated(nameof(ValidButtons))]
        public string? Button { get; set; }

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
            if (Button == null)
                throw new InvalidOperationException("Button unset");

            if (!SupportedButtons.TryGetValue(Button, out var eventCode))
                throw new InvalidOperationException($"Invalid Button '{Button}'");

            if (virtualTablet == null)
                throw new InvalidOperationException($"{nameof(virtualTablet)} was never injected");

            virtualTablet.SetKeyState(eventCode, state);
        }

        public override string ToString() => $"{nameof(LinuxArtistModeButtonBinding)}: {Button}";
    }
}
