using OpenTabletDriver.Native.Windows;
using OpenTabletDriver.Native.Windows.Input;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Interop.Input.Mouse
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

            inputs[0].U.mi.dwFlags = MOUSEEVENTF.MOVE;
            inputs[0].U.mi.dx = (int)dX;
            inputs[0].U.mi.dy = (int)dY;
            SendInput(1, inputs, INPUT.Size);
        }
    }
}