using System;
using TabletDriverLib.Class;

namespace TabletDriverLib.Tools.Cursor
{
    public class XCursorHandler : ICursorHandler
    {
        public void SetCursorPosition(Point pos)
        {
            Native.Linux.XWarpPointer((IntPtr) 0, (IntPtr) 0, new IntPtr(0), 0, 0, 0, 0, (int)pos.X, (int)pos.Y);
            Native.Linux.XFlush((IntPtr) 0);
        }
    }
}