using System;
using System.Numerics;
using OpenTabletDriver.Native.Windows.Input;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Interop.Input.Absolute
{
    public class WindowsAbsolutePointer : WindowsVirtualMouse, IAbsolutePointer
    {
        public WindowsAbsolutePointer()
        {
            var virtualScreen = DesktopInterop.VirtualScreen ??
                                throw new InvalidOperationException("Could not get virtual screen");

            ScreenToVirtualDesktop = new Vector2(virtualScreen.Width, virtualScreen.Height) / 65535;
        }

        private readonly Vector2 ScreenToVirtualDesktop;

        public void SetPosition(Vector2 pos)
        {
            SetDirty();

            var virtualDesktopCoords = pos / ScreenToVirtualDesktop;

            inputs[0].U.mi.dwFlags |= MOUSEEVENTF.ABSOLUTE | MOUSEEVENTF.MOVE | MOUSEEVENTF.VIRTUALDESK;
            inputs[0].U.mi.dx = (int)virtualDesktopCoords.X;
            inputs[0].U.mi.dy = (int)virtualDesktopCoords.Y;
        }
    }
}
