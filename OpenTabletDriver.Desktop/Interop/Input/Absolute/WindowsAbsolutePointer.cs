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
        public void SetPosition(Vector2 pos)
        {
            inputs[0].U.mi.dwFlags = MOUSEEVENTF.ABSOLUTE | MOUSEEVENTF.MOVE | MOUSEEVENTF.VIRTUALDESK;
            inputs[0].U.mi.dx = (int)(pos.X / Platform.VirtualScreen.Width * 65535);
            inputs[0].U.mi.dy = (int)(pos.Y / Platform.VirtualScreen.Height * 65535);
            SendInput(1, inputs, INPUT.Size);
        }
    }
}
