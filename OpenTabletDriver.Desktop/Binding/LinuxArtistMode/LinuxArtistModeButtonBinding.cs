using System;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Desktop.Interop.Input.Absolute;
using OpenTabletDriver.Native.Linux.Evdev;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Desktop.Binding.LinuxArtistMode
{
    [PluginName("Linux Artist Mode"), SupportedPlatform(SystemPlatform.Linux)]
    public class LinuxArtistModeButtonBinding : IStateBinding
    {
        private readonly InputDevice _inputDevice;
        private readonly EvdevVirtualTablet _virtualTablet;

        public LinuxArtistModeButtonBinding(InputDevice inputDevice, EvdevVirtualTablet virtualTablet)
        {
            _inputDevice = inputDevice;
            _virtualTablet = virtualTablet;
        }

        public static string[] ValidButtons { get; } = {
            "Pen Button 1",
            "Pen Button 2",
            "Pen Button 3"
        };

        [Setting("Button"), MemberValidated(nameof(ValidButtons))]
        public string Button { set; get; } = string.Empty;

        public void Press(IDeviceReport report)
        {
            SetState(true);
        }

        public void Release(IDeviceReport report)
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

            _virtualTablet.SetKeyState(eventCode, state);
        }
    }
}
