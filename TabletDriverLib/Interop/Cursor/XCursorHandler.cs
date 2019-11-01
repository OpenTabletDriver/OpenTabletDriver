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
            ButtonStateDictionary = null;
        }

        private Display Display;
        private Window RootWindow;

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

        private Dictionary<MouseButton, bool> ButtonStateDictionary { set; get; } = new Dictionary<MouseButton, bool>();

        private void UpdateButtonState(MouseButton button, bool isPressed)
        {
            if (ButtonStateDictionary.Keys.Contains(button))
                ButtonStateDictionary[button] = isPressed;
            else
                ButtonStateDictionary.Add(button, isPressed);
        }

        public void MouseDown(MouseButton button)
        {   
            if (!ButtonStateDictionary.TryGetValue(button, out var isPressed))
                isPressed = false;
            
            if (button != MouseButton.None && !isPressed)
            {
                var xButton = ParseButton(button);
                XTestFakeButtonEvent(Display, xButton, true, 0L);
                UpdateButtonState(button, true);
                XFlush(Display);
            }
        }

        public void MouseUp(MouseButton button)
        {
            if (!ButtonStateDictionary.TryGetValue(button, out var isPressed))
                isPressed = false;

            if (button != MouseButton.None && isPressed)
            {
                var xButton = ParseButton(button);
                XTestFakeButtonEvent(Display, xButton, false, 0L);
                UpdateButtonState(button, false);
                XFlush(Display);
            }
        }

        public bool GetMouseButtonState(MouseButton button)
        {
            return ButtonStateDictionary.TryGetValue(button, out var value) ? value : false;
        }
    }
}