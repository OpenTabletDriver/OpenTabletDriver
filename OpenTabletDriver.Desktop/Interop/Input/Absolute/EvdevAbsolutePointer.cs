using System;
using System.Numerics;
using OpenTabletDriver.Native.Linux;
using OpenTabletDriver.Native.Linux.Evdev;
using OpenTabletDriver.Native.Linux.Evdev.Structs;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Interop.Input.Absolute
{
    public class EvdevAbsolutePointer : EvdevVirtualMouse, IAbsolutePointer
    {
        public unsafe EvdevAbsolutePointer()
        {
            Device = new EvdevDevice("OpenTabletDriver Virtual Tablet");

            Device.EnableType(EventType.EV_ABS);
            Device.EnableType(EventType.EV_REL);

            var xAbs = new input_absinfo
            {
                maximum = (int)DesktopInterop.VirtualScreen.Width
            };
            input_absinfo* xPtr = &xAbs;
            Device.EnableCustomCode(EventType.EV_ABS, EventCode.ABS_X, (IntPtr)xPtr);

            var yAbs = new input_absinfo
            {
                maximum = (int)DesktopInterop.VirtualScreen.Height
            };
            input_absinfo* yPtr = &yAbs;
            Device.EnableCustomCode(EventType.EV_ABS, EventCode.ABS_Y, (IntPtr)yPtr);

            Device.EnableTypeCodes(
                EventType.EV_KEY,
                EventCode.BTN_LEFT,
                EventCode.BTN_MIDDLE,
                EventCode.BTN_RIGHT,
                EventCode.BTN_SIDE,
                EventCode.BTN_EXTRA
            );

            Device.EnableTypeCodes(
                EventType.EV_REL,
                EventCode.REL_WHEEL,
                EventCode.REL_WHEEL_HI_RES,
                EventCode.REL_HWHEEL,
                EventCode.REL_HWHEEL_HI_RES
            );

            var result = Device.Initialize();
            switch (result)
            {
                case ERRNO.NONE:
                    Log.Debug("Evdev", $"Successfully initialized virtual tablet. (code {result})");
                    break;
                default:
                    Log.WriteNotify("Evdev", $"Failed to initialize virtual tablet. (error code {result})", LogLevel.Error);
                    break;
            }
        }

        public void SetPosition(Vector2 pos)
        {
            Device.Write(EventType.EV_ABS, EventCode.ABS_X, (int)pos.X);
            Device.Write(EventType.EV_ABS, EventCode.ABS_Y, (int)pos.Y);
        }
    }
}
