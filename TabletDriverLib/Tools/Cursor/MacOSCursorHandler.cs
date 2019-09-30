using System;
using TabletDriverLib.Class;

namespace TabletDriverLib.Tools.Cursor
{
    public class MacOSCursorHandler : ICursorHandler
    {
        public Point GetCursorPosition()
        {
            throw new NotImplementedException();
        }

        public void SetCursorPosition(Point pos)
        {
            Native.MacOSX.CGWarpMouseCursorPosition((Native.MacOSX.CGPoint)pos);
        }
    }
}