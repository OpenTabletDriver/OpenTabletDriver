using System;
using System.Numerics;
using OpenTabletDriver.Native.Windows;
using OpenTabletDriver.Native.Windows.Input;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Interop.Input.Absolute
{
    using static Windows;

    public class WindowsAbsolutePointer : WindowsVirtualMouse, IAbsolutePointer
    {
        public WindowsAbsolutePointer(IVirtualScreen virtualScreen)
        {
            timer = new System.Threading.Timer(
                state => screenToVirtualDesktop = new Vector2(
                    virtualScreen.Width,
                    virtualScreen.Height
                ) / 65535,
                null,
                TimeSpan.Zero,
                TimeSpan.FromSeconds(1)
            );
            screenToVirtualDesktop =
                new Vector2(virtualScreen.Width, virtualScreen.Height) / 65535;
        }

        private System.Threading.Timer timer;
        private Vector2 screenToVirtualDesktop;

        public void SetPosition(Vector2 pos)
        {
            var virtualDesktopCoords = pos / screenToVirtualDesktop;

            inputs[0].U.mi.dwFlags = MOUSEEVENTF.ABSOLUTE | MOUSEEVENTF.MOVE | MOUSEEVENTF.VIRTUALDESK;
            inputs[0].U.mi.dx = (int)virtualDesktopCoords.X;
            inputs[0].U.mi.dy = (int)virtualDesktopCoords.Y;
            SendInput(1, inputs, INPUT.Size);
        }
    }
}
