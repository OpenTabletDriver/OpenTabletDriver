using System;
using TabletDriverLib.Component;

namespace TabletDriverLib.Interop.Cursor
{
    using static Native.MacOSX;

    public class MacOSCursorHandler : ICursorHandler
    {
        public Point GetCursorPosition()
        {
            IntPtr eventRef = CGEventCreate();
            CGPoint cursor = CGEventGetLocation(ref eventRef);
            CFRelease(eventRef);
            return (Point)cursor;
        }

        public void SetCursorPosition(Point pos)
        {
            CGWarpMouseCursorPosition((CGPoint)pos);
        }
 
        public void MouseDown(MouseButton button)
        {
            throw new NotImplementedException();
        }

        public void MouseUp(MouseButton button)
        {
            throw new NotImplementedException();
        }

        public bool GetMouseButtonState(MouseButton button)
        {
            throw new NotImplementedException();
        }
    }
}