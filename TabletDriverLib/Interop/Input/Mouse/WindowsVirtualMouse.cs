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
        private float xError, yError;

        public void Move(float dX, float dY)
        {
            dX += xError;
            dY += yError;
            xError = dX % 1;
            yError = dY % 1;

            var input = new INPUT
            {
                type = INPUT_TYPE.MOUSE_INPUT,
                U = new InputUnion
                {
                    mi = new MOUSEINPUT
                    {
                        dx = (int)dX,
                        dy = (int)dY,
                        dwFlags = MOUSEEVENTF.MOVE,
                        time = 0,
                        dwExtraInfo = UIntPtr.Zero
                    }
                }
            };
            var inputs = new INPUT[] { input };
            SendInput((uint)inputs.Length, inputs, INPUT.Size);
            _last = new Point(dX, dY);
        }
    }
}