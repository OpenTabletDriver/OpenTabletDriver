using System;
using System.Linq;
using System.Numerics;
using OpenTabletDriver.Native.OSX;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Interop.Input.Absolute
{
    using static OSX;

    public class MacOSAbsolutePointer : MacOSVirtualMouse, IAbsolutePointer
    {
        private Vector2 offset;

        public MacOSAbsolutePointer()
        {
            var primary = DesktopInterop.VirtualScreen.Displays.First();
            offset = primary.Position;
        }

        public void SetPosition(Vector2 pos)
        {
            var newPos = pos - offset;
            QueuePendingPosition(newPos.X, newPos.Y);
        }

        protected override void SetPendingPosition(IntPtr mouseEvent, float x, float y)
        {
            CGEventSetLocation(mouseEvent, new CGPoint(x, y));
        }

        protected override void ResetPendingPosition(IntPtr mouseEvent)
        {
        }
    }
}
