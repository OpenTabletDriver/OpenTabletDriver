using System;
using System.Collections.Generic;
using System.Linq;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Platform.Keyboard;
using OpenTabletDriver.Plugin.Tablet;

#nullable enable

namespace OpenTabletDriver.Desktop.Binding.LinuxArtistMode
{
    [PluginName(_PLUGIN_NAME), SupportedPlatform(PluginPlatform.Linux)]
    public class LinuxArtistModePadBinding : IStateBinding
    {
        private const string _PLUGIN_NAME = "Linux Artist Mode Pad Binding";

        [Resolved]
        public IVirtualPad? VirtualPad;

        [OnDependencyLoad]
        public void VerifyInitialization()
        {
            if (VirtualPad == null)
                Log.Write(_PLUGIN_NAME,
                    $"{nameof(IVirtualPad)} unavailable", LogLevel.Error);
        }

        private static readonly Dictionary<string, TabletPadEvent> s_ValidButtons = new()
        {
            { "Pad Button 1", TabletPadEvent.BUTTON_1 },
            { "Pad Button 2", TabletPadEvent.BUTTON_2 },
            { "Pad Button 3", TabletPadEvent.BUTTON_3 },
            { "Pad Button 4", TabletPadEvent.BUTTON_4 },
            { "Pad Button 5", TabletPadEvent.BUTTON_5 },
            { "Pad Button 6", TabletPadEvent.BUTTON_6 },
            { "Pad Button 7", TabletPadEvent.BUTTON_7 },
            { "Pad Button 8", TabletPadEvent.BUTTON_8 },
            { "Pad Button 9", TabletPadEvent.BUTTON_9 },
            { "Pad Button 10", TabletPadEvent.BUTTON_10 },
        };


        public static string[] ValidKeys => s_ValidButtons.Keys.ToArray();

        [Property("Button"), PropertyValidated(nameof(ValidKeys))]
        public string? Button
        {
            get => _button;
            set
            {
                _button = value;
                if (value != null)
                    _buttonPadEvent = s_ValidButtons.First(x => x.Key == value).Value;
                else
                    _buttonPadEvent = null;
            }
        }

        private string? _button;
        private TabletPadEvent? _buttonPadEvent;

        public void Press(TabletReference tablet, IDeviceReport report)
        {
            SetState(true);
        }

        public void Release(TabletReference tablet, IDeviceReport report)
        {
            SetState(false);
        }

        private void SetState(bool isPress)
        {
            if (_buttonPadEvent == null) throw new InvalidOperationException("Cannot send null event");

            VirtualPad?.KeyEvent(_buttonPadEvent.Value, isPress);
        }

        public override string ToString() => $"{nameof(LinuxArtistModePadBinding)}: {Button ?? "<button not set>"}";
    }
}
