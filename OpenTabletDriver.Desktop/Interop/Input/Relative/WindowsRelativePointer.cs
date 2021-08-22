using System.Numerics;
using OpenTabletDriver.Native.Windows;
using OpenTabletDriver.Native.Windows.Input;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Interop.Input.Relative
{
    using static Windows;

    public class WindowsRelativePointer : Input.WindowsVirtualMouse, IRelativePointer
    {
        private Vector2 error;

        public unsafe void Translate(Vector2 delta)
        {
            delta += error;
            error = new Vector2(delta.X % 1, delta.Y % 1);

            mouseInput->dwFlags = MOUSEEVENTF.MOVE;
            mouseInput->dx = (int)delta.X;
            mouseInput->dy = (int)delta.Y;
            SendInput(1, input, INPUT.Size);
        }
    }
}
