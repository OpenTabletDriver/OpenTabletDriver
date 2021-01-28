using System.Numerics;
using OpenTabletDriver.Native.Windows;
using OpenTabletDriver.Native.Windows.Input;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Interop.Input.Absolute
{
    using static Windows;

    public class WindowsAbsolutePointer : WindowsVirtualMouse, IAbsolutePointer
    {
        private Vector2 ScreenToVirtualDesktop = new Vector2(SystemInterop.VirtualScreen.Width, SystemInterop.VirtualScreen.Height) / 65535;

        public void SetPosition(Vector2 pos)
        {
            var virtualDesktopCoords = pos / ScreenToVirtualDesktop;
            inputs[0].U.mi.dwFlags = MOUSEEVENTF.ABSOLUTE | MOUSEEVENTF.MOVE | MOUSEEVENTF.VIRTUALDESK;
            inputs[0].U.mi.dx = (int)virtualDesktopCoords.X;
            inputs[0].U.mi.dy = (int)virtualDesktopCoords.Y;
            SendInput(1, inputs, INPUT.Size);
        }
    }
}
