using System;
using OpenTabletDriver.Desktop.Interop;
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
        [Resolved]
        public readonly IPressureHandler virtualTablet;

        public static string[] ValidButtons { get; } = {
            "Pen Button 1",
            "Pen Button 2",
            "Pen Button 3"
        };

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
            var eventCode = Button switch
            {
                "Pen Button 1" => EventCode.BTN_STYLUS,
                "Pen Button 2" => EventCode.BTN_STYLUS2,
                "Pen Button 3" => EventCode.BTN_STYLUS3,
                _ => throw new InvalidOperationException($"Invalid Button '{Button}'")
            };

            if (virtualTablet is EvdevVirtualTablet evdevTablet)
                evdevTablet.SetKeyState(eventCode, state);
            else
                Log.Write(nameof(LinuxArtistModeButtonBinding), $"Attempted to output to invalid output mode", LogLevel.Error);
        }
    }
}
