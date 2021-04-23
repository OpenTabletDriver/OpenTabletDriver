using System;
using System.Numerics;
using OpenTabletDriver.Native.Windows;
using OpenTabletDriver.Native.Windows.Input;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Interop.Input.Absolute
{
    using static Windows;

    public class WindowsAbsolutePointer : WindowsVirtualMouse, IAbsolutePointer
    {
        static WindowsAbsolutePointer()
        {
            timer = new System.Threading.Timer(
                state => ScreenToVirtualDesktop = new Vector2(
                    DesktopInterop.VirtualScreen.Width,
                    DesktopInterop.VirtualScreen.Height
                ) / 65535,
                null,
                TimeSpan.Zero,
                TimeSpan.FromSeconds(1)
            );
        }

        private static System.Threading.Timer timer;
        private static Vector2 ScreenToVirtualDesktop = new Vector2(DesktopInterop.VirtualScreen.Width, DesktopInterop.VirtualScreen.Height) / 65535;

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
