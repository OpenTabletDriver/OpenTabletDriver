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
    [PluginName(_PLUGIN_NAME), SupportedPlatform(PluginPlatform.Linux)]
    public class LinuxArtistModeButtonBinding : IStateBinding
    {
        private const string _PLUGIN_NAME = "Linux Artist Mode Button Binding";

        [Resolved] public IPressureHandler? PressureHandler;

        private EvdevVirtualTablet? _virtualTablet;
        private EvdevVirtualTablet? VirtualTablet => _virtualTablet ??= PressureHandler as EvdevVirtualTablet;

        [OnDependencyLoad]
        public void VerifyInitialization()
        {
            if (VirtualTablet == null)
                Log.Write(_PLUGIN_NAME,
                    $"{nameof(EvdevVirtualTablet)} unavailable", LogLevel.Error);
        }

        public static Dictionary<string, EventCode> SupportedButtons { get; } = new() {
            { "Pen Button 1", EventCode.BTN_STYLUS },
            { "Pen Button 2", EventCode.BTN_STYLUS2 },
            { "Pen Button 3", EventCode.BTN_STYLUS3 },
        };

        public static string[] ValidButtons => SupportedButtons.Keys.ToArray();

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
            if (Button == null || !SupportedButtons.TryGetValue(Button, out var eventCode))
                throw new InvalidOperationException($"Invalid Button '{Button}'");

            VirtualTablet?.SetKeyState(eventCode, state);
        }

        public override string ToString() => $"{nameof(LinuxArtistModeButtonBinding)}: {Button}";
    }
}
