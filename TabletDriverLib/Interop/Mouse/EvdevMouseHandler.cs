using NativeLib.Linux;
using NativeLib.Linux.Evdev;
using NativeLib.Linux.Evdev.Structs;
using System;
using TabletDriverPlugin;
using TabletDriverPlugin.Platform.Pointer;

namespace TabletDriverLib.Interop.Mouse
{
    public class EvdevMouseHandler : IMouseHandler, IDisposable
    {
        public unsafe EvdevMouseHandler()
        {
            Device = new EvdevDevice("OpenTabletDriver Virtual Pointer");

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
                    Log.Debug("Evdev", $"Successfully initialized virtual pointer. (code {result})");
                    break;
                default:
                    Log.Write("Evdev", $"Failed to initialize virtual pointer. (error code {result})", LogLevel.Error);
                    break;
            }
        }

        public void Dispose()
        {
            Device?.Dispose();
        }

        private EvdevDevice Device { set; get; }
        private Point _last;

        public Point GetPosition()
        {
            return _last;
        }

        public void SetPosition(Point pos)
        {
            _last = pos;
            Device.Write(EventType.EV_ABS, EventCode.ABS_X, (int)pos.X);
            Device.Write(EventType.EV_ABS, EventCode.ABS_Y, (int)pos.Y);
            Device.Sync();
        }

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

        private EventCode GetCode(MouseButton button)
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
    }
}
