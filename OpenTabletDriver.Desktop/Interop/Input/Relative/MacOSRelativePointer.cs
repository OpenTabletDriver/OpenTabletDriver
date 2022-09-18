using System;
using System.Numerics;
using OpenTabletDriver.Native.OSX;
using OpenTabletDriver.Native.OSX.Generic;
using OpenTabletDriver.Platform.Display;
using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Interop.Input.Relative
{
    using static OSX;

    public class MacOSRelativePointer : MacOSVirtualMouse, IRelativePointer
    {
        public MacOSRelativePointer(IVirtualScreen virtualScreen) : base(virtualScreen)
        {
        }

        public void SetPosition(Vector2 delta)
        {
            var lastPos = GetPosition();
            var newPos = lastPos + delta;
            var cgPt = new CGPoint(newPos.X, newPos.Y) - Offset;
            var mouseEventRef = CGEventCreateMouseEvent(IntPtr.Zero, MoveEvent, cgPt, PressedButtons);
            CGEventPost(CGEventTapLocation.kCGHIDEventTap, mouseEventRef);
            CFRelease(mouseEventRef);
        }
    }
}
