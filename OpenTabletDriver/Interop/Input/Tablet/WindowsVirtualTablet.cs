using System;
using System.Numerics;
using OpenTabletDriver.Native.Windows;
using OpenTabletDriver.Native.Windows.Input;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Interop.Input.Tablet
{
    using static Windows;

    public class WindowsVirtualTablet : WindowsVirtualPointer, IVirtualTablet
    {
        public void SetPosition(Vector2 pos)
        {
            var input = new INPUT
            {
                type = INPUT_TYPE.MOUSE_INPUT,
                U = new InputUnion
                {
                    mi = new MOUSEINPUT
                    {
                        dx = (int)(pos.X / Platform.VirtualScreen.Width * 65535),
                        dy = (int)(pos.Y / Platform.VirtualScreen.Height * 65535),
                        dwFlags = MOUSEEVENTF.ABSOLUTE | MOUSEEVENTF.MOVE | MOUSEEVENTF.VIRTUALDESK,
                        time = 0,
                        dwExtraInfo = UIntPtr.Zero
                    }
                }
            };
            var inputs = new INPUT[] { input };
            SendInput((uint)inputs.Length, inputs, INPUT.Size);
            _last = pos;
        }
    }
}