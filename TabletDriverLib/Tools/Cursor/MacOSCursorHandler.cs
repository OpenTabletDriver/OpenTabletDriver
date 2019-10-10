using TabletDriverLib.Class;

namespace TabletDriverLib.Tools.Cursor
{
    public class MacOSCursorHandler : ICursorHandler
    {
        public Point GetCursorPosition()
        {
            var eventRef = Native.MacOSX.CGEventCreate();
            var cursor = Native.MacOSX.CGEventGetLocation(ref eventRef);
            Native.MacOSX.CFRelease(eventRef);
            return (Point)cursor;
        }

        public void SetCursorPosition(Point pos)
        {
            Native.MacOSX.CGWarpMouseCursorPosition((Native.MacOSX.CGPoint)pos);
        }
    }
}