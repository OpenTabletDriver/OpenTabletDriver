using System;
using System.Collections.Generic;
using System.Linq;
using OpenTabletDriver.Native.Linux;
using OpenTabletDriver.Native.Linux.Evdev;
using OpenTabletDriver.Native.Linux.Evdev.Structs;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Platform.Keyboard;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Interop.Input.Exotic;

public sealed class EvdevVirtualPad : IVirtualPad, IDisposable
{
    // represents the report is coming from a pad (PAD_DEVICE_ID in wacom_wac.h)
    private const int _WACOM_MAGIC_NUMBER = 0x0F;

    private static readonly Dictionary<TabletPadEvent, EventCode> s_ValidButtons = new()
    {
        { TabletPadEvent.BUTTON_1, EventCode.BTN_1 },
        { TabletPadEvent.BUTTON_2, EventCode.BTN_2 },
        { TabletPadEvent.BUTTON_3, EventCode.BTN_3 },
        { TabletPadEvent.BUTTON_4, EventCode.BTN_4 },
        { TabletPadEvent.BUTTON_5, EventCode.BTN_5 },
        { TabletPadEvent.BUTTON_6, EventCode.BTN_6 },
        { TabletPadEvent.BUTTON_7, EventCode.BTN_7 },
        { TabletPadEvent.BUTTON_8, EventCode.BTN_8 },
        { TabletPadEvent.BUTTON_9, EventCode.BTN_9 },
        { TabletPadEvent.BUTTON_10, EventCode.BTN_0 },
    };

    private static readonly EventCode[] s_SupportedEventCodes = s_ValidButtons.Values.Append(EventCode.ABS_WHEEL).ToArray();

    public unsafe EvdevVirtualPad()
    {
        Device = new EvdevDevice("OpenTabletDriver Virtual Pad");

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

        var wheelAbs = new input_absinfo
        {
            minimum = 0,
            maximum = 359,
        };
        input_absinfo* wheelPtr = &wheelAbs;
        Device.EnableCustomCode(EventType.EV_ABS, EventCode.ABS_WHEEL, (IntPtr)wheelPtr);

        Device.EnableTypeCodes(EventType.EV_KEY, s_SupportedEventCodes);

        var result = Device.Initialize();
        switch (result)
        {
            case ERRNO.NONE:
                Log.Debug("Evdev", "Successfully initialized virtual pad");
                break;
            default:
                Log.Write("Evdev", $"Failed to initialize virtual pad. (error code {result})", LogLevel.Error);
                break;
        }
    }

    private EvdevDevice Device { set; get; }

    public void KeyEvent(TabletPadEvent key, bool isPress)
    {
        var eventCode = s_ValidButtons[key];

        Device.Write(EventType.EV_KEY, eventCode, isPress ? 1 : 0);
        Sync(isPress);
    }

    public void WheelEvent(uint? degrees)
    {
        Device.Write(EventType.EV_ABS, EventCode.ABS_WHEEL, (int)(degrees ?? 0));
        Sync(degrees.HasValue);
    }

    private void Sync(bool isPress)
    {
        Device.Write(EventType.EV_ABS, EventCode.ABS_MISC, isPress ? _WACOM_MAGIC_NUMBER : 0);
        Device.Sync();
    }

    public void Dispose()
    {
        Device?.Dispose();
    }
}
