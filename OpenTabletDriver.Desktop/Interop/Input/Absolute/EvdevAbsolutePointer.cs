using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using OpenTabletDriver.Native.Linux.Evdev;
using OpenTabletDriver.Native.Linux.Evdev.Structs;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Interop.Input.Absolute
{
    [SupportedPlatform(PluginPlatform.Linux)]
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public class EvdevAbsolutePointer : EvdevVirtualMouse, IAbsolutePointer
    {
        [SetsRequiredMembers]
        public unsafe EvdevAbsolutePointer(IVirtualScreen virtualScreen, TabletReference tabletReference)
        {
            var tabletName = tabletReference.Properties.Name;
            var deviceName = $"OpenTabletDriver {tabletName} Absolute Pointer";
            Device = new EvdevDevice(deviceName);

            Device.EnableType(EventType.EV_ABS);
            Device.EnableType(EventType.EV_REL);

            var xAbs = new input_absinfo
            {
                maximum = (int)virtualScreen.Width,
            };
            input_absinfo* xPtr = &xAbs;
            Device.EnableCustomCode(EventType.EV_ABS, EventCode.ABS_X, (IntPtr)xPtr);

            var yAbs = new input_absinfo
            {
                maximum = (int)virtualScreen.Height,
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

            Device.InitializeAndLog();
        }

        public void SetPosition(Vector2 pos)
        {
            Device.Write(EventType.EV_ABS, EventCode.ABS_X, (int)pos.X);
            Device.Write(EventType.EV_ABS, EventCode.ABS_Y, (int)pos.Y);
        }
    }
}
