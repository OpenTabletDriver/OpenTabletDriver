using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using TabletDriverLib.Component;
using TabletDriverLib.Interop.Converters;
using NativeLib.Linux;

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
            _offsetX = (int)Platform.Display.Displays.Min(d => d.Position.X);
            _offsetY = (int)Platform.Display.Displays.Max(d => d.Position.Y);
        }

        public void Dispose()
        {
            XCloseDisplay(Display);
            InputDictionary = null;
        }

        private int _offsetX, _offsetY;

        private Display Display;
        private Window RootWindow;
        private InputDictionary InputDictionary = new InputDictionary();
        private static XButtonConverter Converter = new XButtonConverter();

        public Point GetCursorPosition()
        {
            XQueryPointer(Display, RootWindow, out var root, out var child, out var x, out var y, out var winX, out var winY, out var mask);
                return new Point((int)x + _offsetX, (int)y + _offsetY);
        }

        public void SetCursorPosition(Point pos)
        {
            XQueryPointer(Display, RootWindow, out var root, out var child, out var x, out var y, out var winX, out var winY, out var mask);
            XWarpPointer(Display, RootWindow, new IntPtr(0), 0, 0, 0, 0, (int)pos.X - x + _offsetX, (int)pos.Y - y + _offsetY);
            XFlush(Display);
        }

        private void UpdateMouseButtonState(MouseButton button, bool isPressed)
        {
            var xButton = Converter.Convert(button);
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