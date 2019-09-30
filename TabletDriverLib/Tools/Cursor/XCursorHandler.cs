using System;
using TabletDriverLib.Class;

namespace TabletDriverLib.Tools.Cursor
{
    public class XCursorHandler : ICursorHandler
    {
        public Point GetCursorPosition()
        {
            Native.Linux.XQueryPointer((IntPtr) 0, (IntPtr) 0, out var root, out var child, out var x, out var y, out var winX, out var winY, out var mask);
            return new Point((int)x, (int)y);
        }

        public void SetCursorPosition(Point pos)
        {
            Native.Linux.XWarpPointer((IntPtr) 0, (IntPtr) 0, new IntPtr(0), 0, 0, 0, 0, (int)pos.X, (int)pos.Y);
            Native.Linux.XFlush((IntPtr) 0);
        }
    }
}