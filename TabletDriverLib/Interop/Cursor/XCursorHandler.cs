using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TabletDriverLib.Component;

namespace TabletDriverLib.Interop.Cursor
{
    using static Native.Linux;
    
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

        private XButtonEvent ParseXButtonEvent(MouseButton button)
        {
            var xButtonEvent = new XButtonEvent()
            {
                display = Display,
                window = RootWindow
            };
            switch (button)
            {
                case MouseButton.Left:
                    xButtonEvent.button = Button.LEFT;
                    break;
                case MouseButton.Middle:
                    xButtonEvent.button = Button.MIDDLE;
                    break;
                case MouseButton.Right:
                    xButtonEvent.button = Button.RIGHT;
                    break;
                default:
                    xButtonEvent.button = 0;
                    break;
            }
            return xButtonEvent;
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
            if (button != MouseButton.None)
            {
                var xevent = ParseXButtonEvent(button);
                if (XSendEvent(Display, RootWindow, true, (long)EventMask.ButtonPressMask, xevent) != 0)
                    UpdateButtonState(button, true);
                else
                    Log.Error($"Failed to send XButtonEvent for {button}");
                
                XFlush(Display);
            }
        }

        public void MouseUp(MouseButton button)
        {
            if (button != MouseButton.None)
            {
                var xevent = ParseXButtonEvent(button);
                if (XSendEvent(Display, RootWindow, true, (long)EventMask.ButtonReleaseMask, xevent) != 0)
                    UpdateButtonState(button, false);
                else
                    Log.Error($"Failed to send XButtonEvent for {button}");
                
                XFlush(Display);
            }
        }

        public bool GetMouseButtonState(MouseButton button)
        {
            return ButtonStateDictionary.TryGetValue(button, out var value) ? value : false;
        }
    }
}