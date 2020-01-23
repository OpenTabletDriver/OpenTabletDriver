using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using TabletDriverLib.Interop.Converters;
using NativeLib.Linux;
using TabletDriverPlugin;

namespace TabletDriverLib.Interop.Cursor
{
    using static XLib;

    using IntPtr = IntPtr;
    using Display = IntPtr;
    using Window = IntPtr;

    public class XCursorHandler : ICursorHandler, IDisposable
    {
        public unsafe XCursorHandler()
        {
            Display = XOpenDisplay(null);
            RootWindow = XDefaultRootWindow(Display);
            _offsetX = (int)Platform.VirtualScreen.Position.X;
            _offsetY = (int)Platform.VirtualScreen.Position.Y;
        }

        public void Dispose()
        {
            XCloseDisplay(Display);
        }

        private int _offsetX, _offsetY;

        private Display Display;
        private Window RootWindow;
        private static XButtonConverter Converter = new XButtonConverter();

        public Point GetCursorPosition()
        {
            XQueryPointer(Display, RootWindow, out var root, out var child, out var x, out var y, out var winX, out var winY, out var mask);
                return new Point((int)x, (int)y);
        }

        public void SetCursorPosition(Point pos)
        {
            XQueryPointer(Display, RootWindow, out var root, out var child, out var x, out var y, out var winX, out var winY, out var mask);
            XWarpPointer(Display, RootWindow, new IntPtr(0), 0, 0, 0, 0, (int)pos.X - x, (int)pos.Y - y);
            XFlush(Display);
        }

        private void UpdateMouseButtonState(MouseButton button, bool isPressed)
        {
            var xButton = Converter.Convert(button);
            XTestFakeButtonEvent(Display, xButton, isPressed, 0L);
            XFlush(Display);
        }

        public void MouseDown(MouseButton button)
        {   
            if (button != MouseButton.None)
                UpdateMouseButtonState(button, true);
        }

        public void MouseUp(MouseButton button)
        {
            if (button != MouseButton.None)
                UpdateMouseButtonState(button, false);
        }
    }
}