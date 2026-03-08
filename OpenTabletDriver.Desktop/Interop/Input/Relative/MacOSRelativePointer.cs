using System;
using System.Linq;
using System.Numerics;
using OpenTabletDriver.Native.OSX;
using OpenTabletDriver.Native.OSX.Input;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Interop.Input.Relative
{
    using static OSX;

    public class MacOSRelativePointer : MacOSVirtualMouse, IRelativePointer
    {
        private Vector2 error;

        public void SetPosition(Vector2 delta)
        {
            QueuePendingPosition(delta.X, delta.Y);
        }

        protected override void SetPendingPosition(IntPtr mouseEvent, float x, float y)
        {
            var pos = GetCursorPosition();
            if (CGCursorIsVisible())
                CGEventSetLocation(mouseEvent, pos + new CGPoint(x, y));
            else
                CGEventSetLocation(mouseEvent, pos);

            Vector2 delta = new Vector2(x, y) + error;
            error = new Vector2(delta.X % 1, delta.Y % 1);

            CGEventSetDoubleValueField(mouseEvent, CGEventField.mouseEventDeltaX, Math.Truncate(delta.X));
            CGEventSetDoubleValueField(mouseEvent, CGEventField.mouseEventDeltaY, Math.Truncate(delta.Y));
        }

        protected override void ResetPendingPosition(IntPtr mouseEvent)
        {
            CGEventSetDoubleValueField(mouseEvent, CGEventField.mouseEventDeltaX, 0);
            CGEventSetDoubleValueField(mouseEvent, CGEventField.mouseEventDeltaY, 0);
        }

        protected override void QueuePendingPositionFromSystem()
        {
            QueuePendingPosition(0, 0);
        }

        private static CGPoint GetCursorPosition()
        {
            var eventRef = CGEventCreate(IntPtr.Zero);
            var pos = CGEventGetLocation(eventRef);
            CFRelease(eventRef);
            return pos;
        }
    }
}
