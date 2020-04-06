using System;
using System.Runtime.InteropServices;
using NativeLib.Linux.Evdev;
using NativeLib.Linux.Evdev.Structs;
using TabletDriverPlugin;

namespace TabletDriverLib.Interop.Cursor
{
    public class EvdevCursorHandler : ICursorHandler, IDisposable
    {
        public unsafe EvdevCursorHandler()
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
            Device.Initialize();
        }

        public void Dispose()
        {
            Device?.Dispose();
        }

        private EvdevDevice Device { set; get; }
        private Point _last;

        public Point GetCursorPosition()
        {
            return _last;
        }

        public void SetCursorPosition(Point pos)
        {
            _last = pos;
            Device.Write(EventType.EV_ABS, EventCode.ABS_X, (int)pos.X);
            Device.Write(EventType.EV_ABS, EventCode.ABS_Y, (int)pos.Y);
            Sync();
        }

        public void MouseDown(MouseButton button)
        {
            if (button != MouseButton.None)
            {
                Device.Write(EventType.EV_KEY, GetCode(button), 1);
                Sync();
            }
        }

        public void MouseUp(MouseButton button)
        {
            if (button != MouseButton.None)
            {
                Device.Write(EventType.EV_KEY, GetCode(button), 0);
                Sync();
            }
        }

        private void Sync()
        {
            Device.Write(EventType.EV_SYN, EventCode.SYN_REPORT, 0);
        }

        private EventCode GetCode(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left:
                    return  EventCode.BTN_LEFT;
                case MouseButton.Middle:
                    return  EventCode.BTN_MIDDLE;
                case MouseButton.Right:
                    return  EventCode.BTN_RIGHT;
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