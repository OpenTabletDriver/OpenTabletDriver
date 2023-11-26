using System;
using System.Linq;
using System.Numerics;
using OpenTabletDriver.Native.OSX;
using OpenTabletDriver.Native.OSX.Input;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Interop.Input.Absolute
{
    using static OSX;

    public class MacOSAbsolutePointer : MacOSVirtualMouse, IAbsolutePointer
    {
        private Vector2 offset;
        private Vector2? lastPos;
        private Vector2? delta;

        public MacOSAbsolutePointer()
        {
            var primary = DesktopInterop.VirtualScreen.Displays.First();
            offset = primary.Position;
        }

        public void SetPosition(Vector2 pos)
        {
            var newPos = pos - offset;
            delta = newPos - lastPos;
            lastPos = newPos;

            QueuePendingPosition(newPos.X, newPos.Y);
        }

        protected override void SetPendingPosition(IntPtr mouseEvent, float x, float y)
        {
            CGEventSetLocation(mouseEvent, new CGPoint(x, y));
            if (delta is not null)
            {
                CGEventSetDoubleValueField(mouseEvent, CGEventField.mouseEventDeltaX, delta.Value.X);
                CGEventSetDoubleValueField(mouseEvent, CGEventField.mouseEventDeltaY, delta.Value.Y);
            }
        }

        protected override void ResetPendingPosition(IntPtr mouseEvent)
        {
            lastPos = null;
            delta = null;
        }
    }
}
