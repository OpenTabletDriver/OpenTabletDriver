using OpenTabletDriver.Native.Windows;
using OpenTabletDriver.Native.Windows.Input;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Interop.Input
{
    using static Windows;

    [PluginIgnore]
    public abstract class WindowsVirtualMouse : IVirtualMouse
    {
        protected readonly unsafe INPUT* input;
        protected readonly unsafe MOUSEINPUT* mouseInput;

        public unsafe WindowsVirtualMouse()
        {
            *input = new INPUT() { type = INPUT_TYPE.MOUSE_INPUT };
            mouseInput = INPUT.GetMouseInputPtr(input);
        }

        protected unsafe void MouseEvent(MOUSEEVENTF arg, uint dwData = 0)
        {
            mouseInput->dwFlags = arg;
            mouseInput->mouseData = dwData;
            SendInput(1, input, INPUT.Size);
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
