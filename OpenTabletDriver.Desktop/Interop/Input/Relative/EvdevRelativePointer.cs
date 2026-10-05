using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using OpenTabletDriver.Native.Linux.Evdev;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Interop.Input.Relative
{
    [SupportedPlatform(PluginPlatform.Linux)]
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public class EvdevRelativePointer : EvdevVirtualMouse, IRelativePointer
    {
        [SetsRequiredMembers]
        public EvdevRelativePointer(TabletReference tabletReference)
        {
            var tabletName = tabletReference.Properties.Name;
            var deviceName = $"OpenTabletDriver {tabletName} Mouse";
            Device = new EvdevDevice(deviceName);

            Device.EnableTypeCodes(
                EventType.EV_REL,
                EventCode.REL_X,
                EventCode.REL_Y,
                EventCode.REL_WHEEL,
                EventCode.REL_WHEEL_HI_RES,
                EventCode.REL_HWHEEL,
                EventCode.REL_HWHEEL_HI_RES
            );

            Device.EnableTypeCodes(
                EventType.EV_KEY,
                EventCode.BTN_LEFT,
                EventCode.BTN_MIDDLE,
                EventCode.BTN_RIGHT,
                EventCode.BTN_SIDE,
                EventCode.BTN_EXTRA
            );

            Device.InitializeAndLog();
        }

        private Vector2 error;

        public void SetPosition(Vector2 delta)
        {
            delta += error;
            error = new Vector2(delta.X % 1, delta.Y % 1);

            Device.Write(EventType.EV_REL, EventCode.REL_X, (int)delta.X);
            Device.Write(EventType.EV_REL, EventCode.REL_Y, (int)delta.Y);
        }
    }
}
