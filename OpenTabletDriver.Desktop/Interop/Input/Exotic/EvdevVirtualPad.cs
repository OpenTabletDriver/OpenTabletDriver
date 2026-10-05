using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using OpenTabletDriver.Native.Linux.Evdev;
using OpenTabletDriver.Native.Linux.Evdev.Structs;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Keyboard;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Interop.Input.Exotic;

[SupportedPlatform(PluginPlatform.Linux)]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
public sealed class EvdevVirtualPad : IVirtualPad, IDisposable
{
    private static readonly Dictionary<TabletPadEvent, EventCode> s_ValidButtons = new()
    {
        { TabletPadEvent.BUTTON_1, EventCode.BTN_0 },
        { TabletPadEvent.BUTTON_2, EventCode.BTN_1 },
        { TabletPadEvent.BUTTON_3, EventCode.BTN_2 },
        { TabletPadEvent.BUTTON_4, EventCode.BTN_3 },
        { TabletPadEvent.BUTTON_5, EventCode.BTN_4 },
        { TabletPadEvent.BUTTON_6, EventCode.BTN_5 },
        { TabletPadEvent.BUTTON_7, EventCode.BTN_6 },
        { TabletPadEvent.BUTTON_8, EventCode.BTN_7 },
        { TabletPadEvent.BUTTON_9, EventCode.BTN_8 },
        { TabletPadEvent.BUTTON_10, EventCode.BTN_9 },
    };

    private static readonly EventCode[] s_SupportedEventCodes = s_ValidButtons.Values.ToArray();

    public unsafe EvdevVirtualPad(TabletReference tabletReference)
    {
        var tabletName = tabletReference.Properties.Name;
        var deviceName = $"OpenTabletDriver {tabletName} Pad";
        Device = new EvdevDevice(deviceName);

        // we want to send ABS_MISC on button presses to match wacom driver behavior
        var miscInfo = new input_absinfo(); // intentionally empty
        var miscInfoPtr = &miscInfo;
        Device.EnableCustomCode(EventType.EV_ABS, EventCode.ABS_MISC, (IntPtr)miscInfoPtr);

        // device needs to enable BTN_STYLUS, ABS_X and ABS_Y otherwise libinput won't pick it up as a tablet pad
        Device.EnableCode(EventType.EV_KEY, EventCode.BTN_STYLUS);

        var xAbs = new input_absinfo
        {
            minimum = 0,
            maximum = 1,
        };
        input_absinfo* xPtr = &xAbs;
        Device.EnableCustomCode(EventType.EV_ABS, EventCode.ABS_X, (IntPtr)xPtr);

        var yAbs = new input_absinfo
        {
            minimum = 0,
            maximum = 1,
        };
        input_absinfo* yPtr = &yAbs;
        Device.EnableCustomCode(EventType.EV_ABS, EventCode.ABS_Y, (IntPtr)yPtr);

        Device.EnableTypeCodes(EventType.EV_KEY, s_SupportedEventCodes);

        Device.InitializeAndLog();
    }

    private EvdevDevice Device { set; get; }

    public void KeyEvent(TabletPadEvent key, bool isPress)
    {
        var eventCode = s_ValidButtons[key];

        Device.Write(EventType.EV_KEY, eventCode, isPress ? 1 : 0);
        // Linux' Wacom driver reports 0x0F for pad devices, let's just report 1 to simplify
        Device.Write(EventType.EV_ABS, EventCode.ABS_MISC, isPress ? 1 : 0);
        Device.Sync();
    }

    public void Dispose()
    {
        Device?.Dispose();
    }
}
