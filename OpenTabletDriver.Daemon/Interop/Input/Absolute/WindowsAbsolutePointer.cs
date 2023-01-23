using System;
using System.Numerics;
using OpenTabletDriver.Native.Windows;
using OpenTabletDriver.Native.Windows.Input;
using OpenTabletDriver.Platform.Display;
using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Daemon.Interop.Input.Absolute
{
    using static Windows;

    public class WindowsAbsolutePointer : WindowsVirtualMouse, IAbsolutePointer, IDisposable
    {
        public WindowsAbsolutePointer(IVirtualScreen virtualScreen)
        {
            timer = new System.Threading.Timer(
                UpdateVirtualDesktop,
                virtualScreen,
                TimeSpan.Zero,
                TimeSpan.FromSeconds(5)
            );
        }

        private System.Threading.Timer timer;
        private Vector2 screenToVirtualDesktop;

        private void UpdateVirtualDesktop(object? state)
        {
            var virtualScreen = (IVirtualScreen)state!;

            screenToVirtualDesktop = new Vector2(
                virtualScreen.Width,
                virtualScreen.Height
            ) / 65535;
        }

        public void SetPosition(Vector2 pos)
        {
            var virtualDesktopCoords = pos / screenToVirtualDesktop;

            inputs[0].U.mi.dwFlags = MOUSEEVENTF.ABSOLUTE | MOUSEEVENTF.MOVE | MOUSEEVENTF.VIRTUALDESK;
            inputs[0].U.mi.dx = (int)virtualDesktopCoords.X;
            inputs[0].U.mi.dy = (int)virtualDesktopCoords.Y;
            SendInput(1, inputs, INPUT.Size);
        }

        public void Dispose()
        {
            timer.Dispose();
        }
    }
}
