using NativeLib.Linux.Evdev;
using System;
using TabletDriverPlugin.Attributes;
using TabletDriverPlugin.Platform.Pointer;

namespace TabletDriverLib.Interop
{
    [PluginIgnore]
    public abstract class EvdevVirtualPointer : IVirtualPointer, IDisposable
    {
        protected EvdevDevice Device { set; get; }

        public void MouseDown(MouseButton button)
        {
            if (button != MouseButton.None)
            {
                Device.Write(EventType.EV_KEY, GetCode(button), 1);
                Device.Sync();
            }
        }

        public void MouseUp(MouseButton button)
        {
            if (button != MouseButton.None)
            {
                Device.Write(EventType.EV_KEY, GetCode(button), 0);
                Device.Sync();
            }
        }

        protected EventCode GetCode(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left:
                    return EventCode.BTN_LEFT;
                case MouseButton.Middle:
                    return EventCode.BTN_MIDDLE;
                case MouseButton.Right:
                    return EventCode.BTN_RIGHT;
                case MouseButton.Forward:
                    return EventCode.BTN_FORWARD;
                case MouseButton.Backward:
                    return EventCode.BTN_BACK;
                default:
                    return 0;
            }
        }

        public void Dispose()
        {
            Device?.Dispose();
        }
    }
}