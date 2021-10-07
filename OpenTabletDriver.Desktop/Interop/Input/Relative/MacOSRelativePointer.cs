using System;
using System.Numerics;
using OpenTabletDriver.Native.OSX;
using OpenTabletDriver.Native.OSX.Generic;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Interop.Input.Relative
{
    using static OSX;
    
    public class MacOSRelativePointer : MacOSVirtualMouse, IRelativePointer
    {
        public MacOSRelativePointer(IVirtualScreen virtualScreen)
            : base(virtualScreen)
        {
        }

        public void Translate(Vector2 delta)
        {
            var lastPos = base.GetPosition();
            var newPos = lastPos + delta;
            var cgPt = new CGPoint(newPos.X, newPos.Y) - offset;
            var mouseEventRef = CGEventCreateMouseEvent(IntPtr.Zero, moveEvent, cgPt, pressedButtons);
            CGEventPost(CGEventTapLocation.kCGHIDEventTap, mouseEventRef);
            CFRelease(mouseEventRef);
        }
    }
}
