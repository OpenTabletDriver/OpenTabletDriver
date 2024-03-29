using System;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Daemon.Interop.Input.Absolute;
using OpenTabletDriver.Native.Linux.Evdev;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Daemon.Binding.LinuxArtistMode
{
    [PluginName(PLUGIN_NAME), SupportedPlatform(SystemPlatform.Linux)]
    public class LinuxArtistModeButtonBinding : IStateBinding
    {
        private const string PLUGIN_NAME = "Linux Artist Mode Binding";

        private readonly InputDevice _inputDevice;
        private readonly EvdevVirtualTablet _virtualTablet;

        public LinuxArtistModeButtonBinding(InputDevice inputDevice, ISettingsProvider settingsProvider, EvdevVirtualTablet? virtualTablet = null)
        {
            _inputDevice = inputDevice;
            _virtualTablet = virtualTablet ?? throw new Exception("The selected output mode does not support artist mode bindings");

            settingsProvider.Inject(this);
        }

        public static string[] ValidButtons { get; } = {
            "Pen Tip",
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
                "Pen Tip" => EventCode.BTN_TOUCH,
                "Pen Button 1" => EventCode.BTN_STYLUS,
                "Pen Button 2" => EventCode.BTN_STYLUS2,
                "Pen Button 3" => EventCode.BTN_STYLUS3,
                _ => throw new InvalidOperationException($"Invalid Button '{Button}'")
            };

            _virtualTablet.SetKeyState(eventCode, state);
        }
    }
}
