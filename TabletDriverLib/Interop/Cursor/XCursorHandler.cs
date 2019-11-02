using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using TabletDriverLib.Component;

namespace TabletDriverLib.Interop.Cursor
{
    using static Native.Linux;

    using IntPtr = IntPtr;
    using Display = IntPtr;
    using Window = IntPtr;

    public class XCursorHandler : ICursorHandler, IDisposable
    {
        public unsafe XCursorHandler()
        {
            Display = XOpenDisplay(null);
            RootWindow = XDefaultRootWindow(Display);
        }

        public void Dispose()
        {
            XCloseDisplay(Display);
            InputDictionary = null;
        }

        private Display Display;
        private Window RootWindow;
        private InputDictionary InputDictionary { set; get; } = new InputDictionary();

        private Button ParseButton(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left:
                    return Button.LEFT;
                case MouseButton.Middle:
                    return Button.MIDDLE;
                case MouseButton.Right:
                    return Button.RIGHT;
                default:
                    return 0;
            }
        }

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
            var xButton = ParseButton(button);
            InputDictionary.UpdateState(button, isPressed);
            XTestFakeButtonEvent(Display, xButton, isPressed, 0L);
            XFlush(Display);
        }

        public void MouseDown(MouseButton button)
        {   
            if (button != MouseButton.None && !GetMouseButtonState(button))
                UpdateMouseButtonState(button, true);
        }

        public void MouseUp(MouseButton button)
        {
            if (button != MouseButton.None && GetMouseButtonState(button))
                UpdateMouseButtonState(button, false);
        }

        public bool GetMouseButtonState(MouseButton button)
        {
            return InputDictionary.TryGetValue(button, out var state) ? state : false;
        }
    }
}