using System;
using TabletDriverLib.Component;

namespace TabletDriverLib.Interop.Cursor
{
    using static Native.Windows;

    public class WindowsCursorHandler : ICursorHandler
    {
        public Point GetCursorPosition()
        {
            GetCursorPos(out POINT pt);
            return (Point)pt;
        }

        public void SetCursorPosition(Point pos)
        {
            SetCursorPos((int)pos.X, (int)pos.Y);
        }

        private void MouseEvent(MOUSEEVENTF arg)
        {
            var pos = GetCursorPosition();
            mouse_event((uint)arg, (uint)pos.X, (uint)pos.Y, 0, 0);
        }

        public void MouseDown(MouseButton button)
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
                default:
                    return;
            }
        }

        public void MouseUp(MouseButton button)
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
            }
        }

        public bool GetMouseButtonState(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left:
                    return Convert.ToBoolean(GetKeyState(VirtualKeyStates.VK_LBUTTON) & KEY_PRESSED);
                case MouseButton.Middle:
                    return Convert.ToBoolean(GetKeyState(VirtualKeyStates.VK_MBUTTON) & KEY_PRESSED);
                case MouseButton.Right:
                    return Convert.ToBoolean(GetKeyState(VirtualKeyStates.VK_RBUTTON) & KEY_PRESSED);
                default:
                    return false;
            }
        }
    }
}