using System;
using System.Linq;
using System.Numerics;
using OpenTabletDriver.Native.OSX;
using OpenTabletDriver.Native.OSX.Input;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Interop.Input.Absolute
{
    using static OSX;

    public class MacOSAbsolutePointer : MacOSVirtualMouse, IAbsolutePointer
    {
        private Vector2 _offset;
        private Vector2? _lastPos;
        private Vector2? _delta;

        public MacOSAbsolutePointer()
        {
            var primary = DesktopInterop.VirtualScreen.Displays.First();
            _offset = primary.Position;
        }

        public void SetPosition(Vector2 pos)
        {
            var newPos = pos - _offset;
            _delta = newPos - _lastPos;
            _lastPos = newPos;

            QueuePendingPosition(newPos.X, newPos.Y);
        }

        protected override void SetPendingPosition(IntPtr mouseEvent, float x, float y)
        {
            CGEventSetLocation(mouseEvent, new CGPoint(x, y));
            if (_delta is not null)
            {
                CGEventSetDoubleValueField(mouseEvent, CGEventField.mouseEventDeltaX, _delta.Value.X);
                CGEventSetDoubleValueField(mouseEvent, CGEventField.mouseEventDeltaY, _delta.Value.Y);
            }
        }

        protected override void ResetPendingPosition(IntPtr mouseEvent)
        {
            _lastPos = null;
            _delta = null;
        }
    }
}
