using System;
using System.Collections.Generic;
using System.Linq;
using OpenTabletDriver.Native.Linux;
using OpenTabletDriver.Native.Linux.Evdev;
using OpenTabletDriver.Native.Linux.Evdev.Structs;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Platform.Keyboard;

namespace OpenTabletDriver.Desktop.Interop.Input.Exotic;

public class EvdevVirtualPad : IVirtualPad, IDisposable
{
    // the ABS_MISC event code sent with button presses on the stock Wacom Linux driver(?)
    private static int _wacomMagicNumber = 15;

    public static readonly Dictionary<string, EventCode> ValidButtons = new()
    {
        { "Pad Button 1", EventCode.BTN_1 },
        { "Pad Button 2", EventCode.BTN_2 },
        { "Pad Button 3", EventCode.BTN_3 },
        { "Pad Button 4", EventCode.BTN_4 },
        { "Pad Button 5", EventCode.BTN_5 },
        { "Pad Button 6", EventCode.BTN_6 },
        { "Pad Button 7", EventCode.BTN_7 },
        { "Pad Button 8", EventCode.BTN_8 },
        { "Pad Button 9", EventCode.BTN_9 },
        { "Pad Button 0", EventCode.BTN_0 },
    };

    private static EventCode[] supportedEventCodes = ValidButtons.Values.ToArray();

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

        Device.EnableTypeCodes(EventType.EV_KEY, supportedEventCodes);

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

    public void KeyEvent(string key, bool isPress)
    {
        var eventCode = ValidButtons[key];

        Device.Write(EventType.EV_KEY, eventCode, isPress ? 1 : 0);
        Device.Write(EventType.EV_ABS, EventCode.ABS_MISC, isPress ? _wacomMagicNumber : 0);
        Device.Sync();
    }

    public void Dispose()
    {
        Device?.Dispose();
    }
}
