using System.Numerics;
using OpenTabletDriver.Native.Windows;
using OpenTabletDriver.Native.Windows.Input;
using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Daemon.Interop.Input.Relative
{
    using static Windows;

    public class WindowsRelativePointer : WindowsVirtualMouse, IRelativePointer
    {
        private Vector2 error;

        public void SetPosition(Vector2 delta)
        {
            delta += error;
            error = new Vector2(delta.X % 1, delta.Y % 1);

            inputs[0].U.mi.dwFlags = MOUSEEVENTF.MOVE;
            inputs[0].U.mi.dx = (int)delta.X;
            inputs[0].U.mi.dy = (int)delta.Y;
            SendInput(1, inputs, INPUT.Size);
        }
    }
}
