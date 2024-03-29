using System.Numerics;
using OpenTabletDriver.Native.Windows.Input;
using OpenTabletDriver.Platform.Display;
using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Daemon.Interop.Input.Absolute
{
    public class WindowsAbsolutePointer : WindowsVirtualMouse, IAbsolutePointer
    {
        public WindowsAbsolutePointer(IVirtualScreen virtualScreen)
        {
            screenToVirtualDesktop = new Vector2(
                virtualScreen.Width,
                virtualScreen.Height
            ) / 65535;
        }

        private Vector2 screenToVirtualDesktop;

        public void SetPosition(Vector2 pos)
        {
            SetDirty();

            var virtualDesktopCoords = pos / screenToVirtualDesktop;

            inputs[0].U.mi.dwFlags |= MOUSEEVENTF.ABSOLUTE | MOUSEEVENTF.MOVE | MOUSEEVENTF.VIRTUALDESK;
            inputs[0].U.mi.dx = (int)virtualDesktopCoords.X;
            inputs[0].U.mi.dy = (int)virtualDesktopCoords.Y;
        }
    }
}
