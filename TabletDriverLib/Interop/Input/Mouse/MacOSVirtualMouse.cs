using System;
using NativeLib.OSX;
using NativeLib.OSX.Generic;
using TabletDriverPlugin.Platform.Pointer;

namespace TabletDriverLib.Interop.Input.Mouse
{
    using static OSX;
    
    public class MacOSVirtualMouse : MacOSVirtualPointer, IVirtualMouse
    {
        public void Move(float dX, float dY)
        {
            var lastPos = base.GetPosition();
            double x = lastPos.X + dX;
            double y = lastPos.Y + dY;
            var newPos = new CGPoint(x, y) - offset;
            var mouseEventRef = CGEventCreateMouseEvent(IntPtr.Zero, moveEvent, newPos, pressedButtons);
            CGEventPost(CGEventTapLocation.kCGHIDEventTap, mouseEventRef);
            CFRelease(mouseEventRef);
        }
    }
}