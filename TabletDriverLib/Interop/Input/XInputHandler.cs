using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using TabletDriverLib.Component;
using TabletDriverLib.Interop.Converters;
using NativeLib.Linux;

namespace TabletDriverLib.Interop.Input
{
    using static Linux;

    using IntPtr = IntPtr;
    using Display = IntPtr;
    using Window = IntPtr;

    public class XInputHandler : IInputHandler, IDisposable
    {
        public unsafe XInputHandler()
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
        private InputDictionary InputDictionary = new InputDictionary();
        private static XButtonConverter ButtonConverter = new XButtonConverter();
        private static XKeyConverter KeyConverter = new XKeyConverter();

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
            var xButton = ButtonConverter.Convert(button);
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

        private void UpdateKeyState(Key key, bool isPressed)
        {
            var xkey = KeyConverter.Convert(key);
            if (xkey != default)
            {
                InputDictionary.UpdateState(key, isPressed);
                XTestFakeKeyEvent(Display, xkey, isPressed, 0);
                XFlush(Display);
            }
        }

        public void KeyDown(Key key)
        {
            if (!GetKeyState(key))
                UpdateKeyState(key, true);
        }

        public void KeyUp(Key key)
        {
            if (GetKeyState(key))
                UpdateKeyState(key, false);
        }

        public bool GetKeyState(Key key)
        {
            return InputDictionary.TryGetValue(key, out var state) ? state : false;
        }
    }
}