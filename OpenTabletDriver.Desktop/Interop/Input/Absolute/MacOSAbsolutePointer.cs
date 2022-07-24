using System;
using System.Numerics;
using OpenTabletDriver.Native.OSX;
using OpenTabletDriver.Native.OSX.Generic;
using OpenTabletDriver.Platform.Display;
using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Interop.Input.Absolute
{
    using static OSX;

    public class MacOSAbsolutePointer : MacOSVirtualMouse, IAbsolutePointer
    {
        public MacOSAbsolutePointer(IVirtualScreen virtualScreen) : base(virtualScreen)
        {
        }

        public void SetPosition(Vector2 pos)
        {
            var newPos = new CGPoint(pos.X, pos.Y) - Offset;
            var mouseEventRef = CGEventCreateMouseEvent(IntPtr.Zero, MoveEvent, newPos, PressedButtons);
            CGEventPost(CGEventTapLocation.kCGHIDEventTap, mouseEventRef);
            CFRelease(mouseEventRef);
        }
    }
}
