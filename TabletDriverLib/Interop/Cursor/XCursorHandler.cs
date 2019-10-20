using System;
using TabletDriverLib.Component;

namespace TabletDriverLib.Interop.Cursor
{
    using Display = IntPtr;
    using Window = IntPtr;
    
    public class XCursorHandler : ICursorHandler, IDisposable
    {
        public unsafe XCursorHandler()
        {
            Display = Native.Linux.XOpenDisplay(null);
            RootWindow = Native.Linux.XDefaultRootWindow(Display);
        }

        public void Dispose()
        {
            Native.Linux.XCloseDisplay(Display);
        }

        private Display Display;
        private Window RootWindow;

        public Point GetCursorPosition()
        {
            Native.Linux.XQueryPointer(Display, RootWindow, out var root, out var child, out var x, out var y, out var winX, out var winY, out var mask);
                return new Point((int)x, (int)y);
        }

        public void SetCursorPosition(Point pos)
        {
            Native.Linux.XQueryPointer(Display, RootWindow, out var root, out var child, out var x, out var y, out var winX, out var winY, out var mask);
            Native.Linux.XWarpPointer(Display, RootWindow, new IntPtr(0), 0, 0, 0, 0, (int)pos.X - x, (int)pos.Y - y);
            Native.Linux.XFlush(Display);
        }
    }
}