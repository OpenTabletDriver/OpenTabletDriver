using System;
using TabletDriverLib.Class;

namespace TabletDriverLib.Tools.Cursor
{
    public class MacOSCursorHandler : ICursorHandler
    {
        public void SetCursorPosition(Point pos)
        {
            var point = new Native.MacOSX.CGPoint
            {
                X = (Single)pos.X,
                Y = (Single)pos.Y
            };
            Native.MacOSX.CGWarpMouseCursorPosition(point);
        }
    }
}