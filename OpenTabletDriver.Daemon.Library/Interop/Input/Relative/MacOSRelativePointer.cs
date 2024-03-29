using System;
using System.Linq;
using System.Numerics;
using OpenTabletDriver.Native.MacOS;
using OpenTabletDriver.Native.MacOS.Input;
using OpenTabletDriver.Platform.Display;
using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Daemon.Interop.Input.Relative
{
    using static MacOS;

    public class MacOSRelativePointer : MacOSVirtualMouse, IRelativePointer
    {
        private CGPoint offset;

        public MacOSRelativePointer(IVirtualScreen virtualScreen)
        {
            var primary = virtualScreen.Displays.First();
            offset = new CGPoint(primary.Position.X, primary.Position.Y);
        }

        public void SetPosition(Vector2 delta)
        {
            QueuePendingPosition(delta.X, delta.Y);
        }

        protected override void SetPendingPosition(IntPtr mouseEvent, float x, float y)
        {
            CGEventSetLocation(mouseEvent, GetCursorPosition() + new CGPoint(x, y));
            CGEventSetDoubleValueField(mouseEvent, CGEventField.mouseEventDeltaX, x);
            CGEventSetDoubleValueField(mouseEvent, CGEventField.mouseEventDeltaY, y);
        }

        protected override void ResetPendingPosition(IntPtr mouseEvent)
        {
            CGEventSetDoubleValueField(mouseEvent, CGEventField.mouseEventDeltaX, 0);
            CGEventSetDoubleValueField(mouseEvent, CGEventField.mouseEventDeltaY, 0);
        }

        private CGPoint GetCursorPosition()
        {
            var eventRef = CGEventCreate(IntPtr.Zero);
            var pos = CGEventGetLocation(eventRef) + offset;
            CFRelease(eventRef);
            return pos;
        }
    }
}
