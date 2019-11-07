using System;
using TabletDriverLib.Component;
using NativeLib.Windows;
using NativeLib.Windows.Input;

namespace TabletDriverLib.Interop.Input
{
    using static Windows;

    public class WindowsInputHandler : IInputHandler
    {
        public Point GetCursorPosition()
        {
            GetCursorPos(out POINT pt);
            return new Point(pt.X, pt.Y);
        }

        public void SetCursorPosition(Point pos)
        {
            SetCursorPos((int)pos.X, (int)pos.Y);
        }

        private void MouseEvent(MOUSEEVENTF arg, uint dwData = 0)
        {
            var pos = GetCursorPosition();
            mouse_event((uint)arg, (uint)pos.X, (uint)pos.Y, dwData, 0);
        }

        public void MouseDown(MouseButton button)
        {
            if (!GetMouseButtonState(button))
            {
                switch (button)
                {
                    case MouseButton.Left:
                        MouseEvent(MOUSEEVENTF.LEFTDOWN);
                        return;
                    case MouseButton.Middle:
                        MouseEvent(MOUSEEVENTF.MIDDLEDOWN);
                        return;
                    case MouseButton.Right:
                        MouseEvent(MOUSEEVENTF.RIGHTDOWN);
                        return;
                    case MouseButton.Backward:
                        MouseEvent(MOUSEEVENTF.XDOWN, (uint)XBUTTON.XBUTTON1);
                        return;
                    case MouseButton.Forward:
                        MouseEvent(MOUSEEVENTF.XDOWN, (uint)XBUTTON.XBUTTON2);
                        return;
                }
            }
        }

        public void MouseUp(MouseButton button)
        {
            if (GetMouseButtonState(button))
            {
                switch (button)
                {
                    case MouseButton.Left:
                        MouseEvent(MOUSEEVENTF.LEFTUP);
                        return;
                    case MouseButton.Middle:
                        MouseEvent(MOUSEEVENTF.MIDDLEUP);
                        return;
                    case MouseButton.Right:
                        MouseEvent(MOUSEEVENTF.RIGHTUP);
                        return;
                    case MouseButton.Backward:
                        MouseEvent(MOUSEEVENTF.XUP, (uint)XBUTTON.XBUTTON1);
                        return;
                    case MouseButton.Forward:
                        MouseEvent(MOUSEEVENTF.XUP, (uint)XBUTTON.XBUTTON2);
                        return;
                }
            }
        }

        public bool GetMouseButtonState(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left:
                    return Convert.ToBoolean(Windows.GetKeyState((int)VirtualKeyStates.VK_LBUTTON) & (int)KEYSTATE.KEY_PRESSED);
                case MouseButton.Middle:
                    return Convert.ToBoolean(Windows.GetKeyState((int)VirtualKeyStates.VK_MBUTTON) & (int)KEYSTATE.KEY_PRESSED);
                case MouseButton.Right:
                    return Convert.ToBoolean(Windows.GetKeyState((int)VirtualKeyStates.VK_RBUTTON) & (int)KEYSTATE.KEY_PRESSED);
                case MouseButton.Backward:
                    return Convert.ToBoolean(Windows.GetKeyState((int)VirtualKeyStates.VK_XBUTTON1) & (int)KEYSTATE.KEY_PRESSED);
                case MouseButton.Forward:
                    return Convert.ToBoolean(Windows.GetKeyState((int)VirtualKeyStates.VK_XBUTTON2) & (int)KEYSTATE.KEY_PRESSED);
                default:
                    return false;
            }
        }
        
        private void KeyEvent(Key key, bool isPressed)
        {
            keybd_event((byte)key, 0, isPressed ? (uint)KEYSTATE.KEY_PRESSED : 0, 0);
        }

        public void KeyDown(Key key)
        {
            if (!GetKeyState(key))
                KeyEvent(key, true);
        }

        public void KeyUp(Key key)
        {
            if (GetKeyState(key))
                KeyEvent(key, false);
        }

        public bool GetKeyState(Key key)
        {
            return Convert.ToBoolean(Windows.GetKeyState((int)key) & (int)KEYSTATE.KEY_PRESSED);
        }
    }
}