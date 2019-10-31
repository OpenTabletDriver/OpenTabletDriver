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

        private XButtonEvent ParseXButtonEvent(MouseButton button)
        {
            var xButtonEvent = new XButtonEvent()
            {
                display = Display,
                window = RootWindow,
                same_screen = true
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
                // Marshal XEvent to IntPtr
                var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(xevent));
                Marshal.StructureToPtr(xevent, ptr, false);
                
                XGrabKeyboard(Display, RootWindow, false, 1, 1, 0);
                if (XSendEvent(Display, RootWindow, true, (long)EventMask.ButtonPressMask, ptr) != 0)
                    UpdateButtonState(button, true);
                else
                    Log.Error($"Failed to send XButtonEvent for {button}");
                
                XFlush(Display);
                XUngrabKeyboard(Display, 0);
                Marshal.FreeHGlobal(ptr);
            }
        }

        public void MouseUp(MouseButton button)
        {
            if (button != MouseButton.None)
            {
                var xevent = ParseXButtonEvent(button);
                // Marshal XEvent to IntPtr
                var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(xevent));
                Marshal.StructureToPtr(xevent, ptr, false);

                XGrabKeyboard(Display, RootWindow, false, 1, 1, 0);
                if (XSendEvent(Display, RootWindow, true, (long)EventMask.ButtonReleaseMask, ptr) != 0)
                    UpdateButtonState(button, false);
                else
                    Log.Error($"Failed to send XButtonEvent for {button}");
                
                XFlush(Display);
                XUngrabKeyboard(Display, 0);
                Marshal.FreeHGlobal(ptr);
            }
        }

        public bool GetMouseButtonState(MouseButton button)
        {
            return ButtonStateDictionary.TryGetValue(button, out var value) ? value : false;
        }
    }
}