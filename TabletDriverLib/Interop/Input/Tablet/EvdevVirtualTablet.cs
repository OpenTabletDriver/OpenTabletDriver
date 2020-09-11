using NativeLib.Linux;
using NativeLib.Linux.Evdev;
using NativeLib.Linux.Evdev.Structs;
using System;
using System.Numerics;
using TabletDriverPlugin;
using TabletDriverPlugin.Platform.Pointer;

namespace TabletDriverLib.Interop.Input.Tablet
{
    public class EvdevVirtualTablet : EvdevVirtualPointer, IVirtualTablet
    {
        public unsafe EvdevVirtualTablet()
        {
            Device = new EvdevDevice("OpenTabletDriver Virtual Tablet");

            Device.EnableType(EventType.EV_ABS);

            var xAbs = new input_absinfo
            {
                maximum = (int)Platform.VirtualScreen.Width
            };
            input_absinfo* xPtr = &xAbs;
            Device.EnableCustomCode(EventType.EV_ABS, EventCode.ABS_X, (IntPtr)xPtr);

            var yAbs = new input_absinfo
            {
                maximum = (int)Platform.VirtualScreen.Height
            };
            input_absinfo* yPtr = &yAbs;
            Device.EnableCustomCode(EventType.EV_ABS, EventCode.ABS_Y, (IntPtr)yPtr);

            Device.EnableTypeCodes(
                EventType.EV_KEY,
                EventCode.BTN_LEFT,
                EventCode.BTN_MIDDLE,
                EventCode.BTN_RIGHT,
                EventCode.BTN_FORWARD,
                EventCode.BTN_BACK);

            var result = Device.Initialize();
            switch (result)
            {
                case ERRNO.NONE:
                    Log.Debug("Evdev", $"Successfully initialized virtual tablet. (code {result})");
                    break;
                default:
                    Log.Write("Evdev", $"Failed to initialize virtual tablet. (error code {result})", LogLevel.Error);
                    break;
            }
        }

        public void SetPosition(Vector2 pos)
        {
            Device.Write(EventType.EV_ABS, EventCode.ABS_X, (int)pos.X);
            Device.Write(EventType.EV_ABS, EventCode.ABS_Y, (int)pos.Y);
            Device.Sync();
        }
    }
}
