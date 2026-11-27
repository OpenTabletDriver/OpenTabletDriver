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
            var virtualScreen = DesktopInterop.VirtualScreen
                                ?? throw new InvalidOperationException("Could not get virtual screen");
            var primary = virtualScreen.Displays.First();
            _offset = primary.Position;
        }

        public void SetPosition(Vector2 pos)
        {
            lock (SyncRoot)
            {
                var newPos = pos - _offset;
                _delta = newPos - _lastPos;
                _lastPos = newPos;

                QueuePendingPosition(newPos.X, newPos.Y);
            }
        }

        protected override void SetPendingPosition(IntPtr mouseEvent, float x, float y)
        {
            lock (SyncRoot)
            {
                CGEventSetLocation(mouseEvent, new CGPoint(x, y));
                if (_delta is { } delta)
                {
                    CGEventSetDoubleValueField(mouseEvent, CGEventField.mouseEventDeltaX, delta.X);
                    CGEventSetDoubleValueField(mouseEvent, CGEventField.mouseEventDeltaY, delta.Y);
                }
            }
        }

        protected override void QueuePendingPositionFromSystem()
        {
            lock (SyncRoot)
            {
                var eventRef = CGEventCreate(IntPtr.Zero);
                var pos = CGEventGetLocation(eventRef);
                CFRelease(eventRef);
                QueuePendingPosition((float)pos.x, (float)pos.y);
            }
        }

        protected override void ResetPendingPosition(IntPtr mouseEvent)
        {
            lock (SyncRoot)
            {
                _lastPos = null;
                _delta = null;
            }
        }
    }
}
