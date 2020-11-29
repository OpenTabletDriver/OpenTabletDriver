using System;
using OpenTabletDriver.Native.OSX;
using OpenTabletDriver.Native.OSX.Generic;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Interop.Input.Mouse
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
