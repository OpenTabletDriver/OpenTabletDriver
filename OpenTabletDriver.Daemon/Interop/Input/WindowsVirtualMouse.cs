using System;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Native.Windows;
using OpenTabletDriver.Native.Windows.Input;
using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Daemon.Interop.Input
{
    using static Windows;

    [PluginIgnore]
    public abstract class WindowsVirtualMouse : IMouseButtonHandler
    {
        protected INPUT[] inputs = new INPUT[]
        {
            new INPUT
            {
                type = INPUT_TYPE.MOUSE_INPUT,
                U = new InputUnion
                {
                    mi = new MOUSEINPUT
                    {
                        time = 0,
                        dwExtraInfo = UIntPtr.Zero
                    }
                }
            }
        };

        protected void MouseEvent(MOUSEEVENTF arg, uint dwData = 0)
        {
            inputs[0].U.mi.dwFlags = arg;
            inputs[0].U.mi.mouseData = dwData;
            SendInput(1, inputs, INPUT.Size);
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
