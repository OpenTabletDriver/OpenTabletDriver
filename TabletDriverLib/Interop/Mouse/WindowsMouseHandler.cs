using System;
using NativeLib.Windows;
using NativeLib.Windows.Input;
using TabletDriverPlugin;
using TabletDriverPlugin.Platform.Pointer;

namespace TabletDriverLib.Interop.Mouse
{
    using static Windows;

    public class WindowsMouseHandler : IMouseHandler
    {
        private Point _last;

        public Point GetPosition()
        {
            return _last;
        }

        public void SetPosition(Point pos)
        {
            var input = new INPUT
            {
                type = INPUT_TYPE.MOUSE_INPUT,
                U = new InputUnion
                {
                    mi = new MOUSEINPUT
                    {
                        dx = (int)(pos.X / Platform.VirtualScreen.Width * 65535),
                        dy = (int)(pos.Y / Platform.VirtualScreen.Height * 65535),
                        dwFlags = MOUSEEVENTF.ABSOLUTE | MOUSEEVENTF.MOVE | MOUSEEVENTF.VIRTUALDESK,
                        time = 0,
                        dwExtraInfo = UIntPtr.Zero
                    }
                }
            };
            var inputs = new INPUT[] { input };
            SendInput((uint)inputs.Length, inputs, INPUT.Size);
            _last = pos;
        }

        private void MouseEvent(MOUSEEVENTF arg, uint dwData = 0)
        {
            var pos = GetPosition();
            mouse_event((uint)arg, (uint)pos.X, (uint)pos.Y, dwData, 0);
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
                case MouseButton.Backward:
                    MouseEvent(MOUSEEVENTF.XDOWN, (uint)XBUTTON.XBUTTON1);
                    return;
                case MouseButton.Forward:
                    MouseEvent(MOUSEEVENTF.XDOWN, (uint)XBUTTON.XBUTTON2);
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
                case MouseButton.Backward:
                    MouseEvent(MOUSEEVENTF.XUP, (uint)XBUTTON.XBUTTON1);
                    return;
                case MouseButton.Forward:
                    MouseEvent(MOUSEEVENTF.XUP, (uint)XBUTTON.XBUTTON2);
                    return;
            }
        }
    }
}