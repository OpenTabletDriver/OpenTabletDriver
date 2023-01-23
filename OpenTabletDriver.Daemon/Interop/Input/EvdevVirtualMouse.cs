using System;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Native.Linux.Evdev;
using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Daemon.Interop.Input
{
    [PluginIgnore]
    public abstract class EvdevVirtualMouse : IMouseButtonHandler, IDisposable
    {
        protected EvdevDevice Device { init; get; } = null!;

        public void MouseDown(MouseButton button)
        {
            if (GetCode(button) is EventCode code)
            {
                Device!.Write(EventType.EV_KEY, code, 1);
                Device.Sync();
            }
        }

        public void MouseUp(MouseButton button)
        {
            if (GetCode(button) is EventCode code)
            {
                Device.Write(EventType.EV_KEY, code, 0);
                Device.Sync();
            }
        }

        protected virtual EventCode? GetCode(MouseButton button) => button switch
        {
            MouseButton.Left => EventCode.BTN_LEFT,
            MouseButton.Middle => EventCode.BTN_MIDDLE,
            MouseButton.Right => EventCode.BTN_RIGHT,
            MouseButton.Backward => EventCode.BTN_SIDE,
            MouseButton.Forward => EventCode.BTN_EXTRA,
            _ => null
        };

        public virtual void Dispose()
        {
            Device.Dispose();
        }
    }
}
