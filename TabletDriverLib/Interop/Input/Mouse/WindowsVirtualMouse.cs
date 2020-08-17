using System;
using NativeLib.Windows;
using NativeLib.Windows.Input;
using TabletDriverPlugin;
using TabletDriverPlugin.Platform.Pointer;

namespace TabletDriverLib.Interop.Input.Mouse
{
    using static Windows;
    
    public class WindowsVirtualMouse : WindowsVirtualPointer, IVirtualMouse
    {
        public void Move(float dX, float dY)
        {
            var input = new INPUT
            {
                type = INPUT_TYPE.MOUSE_INPUT,
                U = new InputUnion
                {
                    mi = new MOUSEINPUT
                    {
                        dx = (int)dX,
                        dy = (int)dY,
                        dwFlags = MOUSEEVENTF.ABSOLUTE | MOUSEEVENTF.MOVE | MOUSEEVENTF.VIRTUALDESK,
                        time = 0,
                        dwExtraInfo = UIntPtr.Zero
                    }
                }
            };
            var inputs = new INPUT[] { input };
            SendInput((uint)inputs.Length, inputs, INPUT.Size);
        }
    }
}