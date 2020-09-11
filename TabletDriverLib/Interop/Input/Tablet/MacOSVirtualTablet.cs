using System;
using System.Linq;
using System.Numerics;
using NativeLib.OSX;
using NativeLib.OSX.Generic;
using NativeLib.OSX.Input;
using TabletDriverLib.Interop.Input;
using TabletDriverPlugin;
using TabletDriverPlugin.Platform.Pointer;

namespace TabletDriverLib.Interop.Input.Tablet
{
    using static OSX;

    public class MacOSVirtualTablet : MacOSVirtualPointer, IVirtualTablet
    {
        public void SetPosition(Vector2 pos)
        {
            var newPos = new CGPoint(pos.X, pos.Y) - offset;
            var mouseEventRef = CGEventCreateMouseEvent(IntPtr.Zero, moveEvent, newPos, pressedButtons);
            CGEventPost(CGEventTapLocation.kCGHIDEventTap, mouseEventRef);
            CFRelease(mouseEventRef);
        }
    }
}