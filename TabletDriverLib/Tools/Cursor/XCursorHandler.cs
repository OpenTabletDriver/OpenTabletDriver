using System;
using TabletDriverLib.Class;

namespace TabletDriverLib.Tools.Cursor
{
    using Display = IntPtr;
    
    public class XCursorHandler : ICursorHandler, IDisposable
    {
        public unsafe XCursorHandler()
        {
            Display = Native.Linux.XOpenDisplay(null);
        }

        public void Dispose()
        {
            if (Display is Display dp)
                Native.Linux.XCloseDisplay(dp);
            Display = null;
        }

        public Point GetCursorPosition()
        {
            if (Display is Display dp)
            {
                Native.Linux.XQueryPointer(dp, (IntPtr) 0, out var root, out var child, out var x, out var y, out var winX, out var winY, out var mask);
                return new Point((int)x, (int)y);
            }
            else
                return null;
        }

        public void SetCursorPosition(Point pos)
        {
            if (Display is Display dp)
            {
                Native.Linux.XWarpPointer(dp, (IntPtr) 0, new IntPtr(0), 0, 0, 0, 0, (int)pos.X, (int)pos.Y);
                Native.Linux.XFlush(dp);
            }
        }

        Display? Display;
    }
}