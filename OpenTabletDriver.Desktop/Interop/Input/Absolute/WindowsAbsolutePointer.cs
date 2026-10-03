using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using OpenTabletDriver.Native.Windows.Input;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Interop.Input.Absolute
{
    [SupportedPlatform(PluginPlatform.Windows)]
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public class WindowsAbsolutePointer(IVirtualScreen virtualScreen) : WindowsVirtualMouse, IAbsolutePointer
    {
        private readonly Vector2 ScreenToVirtualDesktop = new Vector2(virtualScreen.Width, virtualScreen.Height) / 65535;

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
