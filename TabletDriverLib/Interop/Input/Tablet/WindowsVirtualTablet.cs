using System;
using NativeLib.Windows;
using NativeLib.Windows.Input;
using TabletDriverPlugin;
using TabletDriverPlugin.Platform.Pointer;

namespace TabletDriverLib.Interop.Input.Tablet
{
    using static Windows;

    public class WindowsVirtualTablet : WindowsVirtualPointer, IVirtualTablet
    {
        public void SetPosition(Point pos)
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