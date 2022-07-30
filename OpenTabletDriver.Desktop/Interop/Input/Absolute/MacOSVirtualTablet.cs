using System;
using System.Numerics;
using OpenTabletDriver.Native.OSX;
using OpenTabletDriver.Native.OSX.Generic;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Interop.Input.Absolute
{
    using static OSX;

    public class MacOSVirtualTablet : MacOSVirtualMouse, IAbsolutePointer, ISynchronousPointer
    {
        protected Vector2 position;

        public MacOSVirtualTablet() : base() {
            // The MacOSVirtualMouse base class sends pressure data with mouse events, but not by default.
            tabletFeaturesEnabled = true;
        }

        public void SetPosition(Vector2 pos)
        {
            this.position = pos;
        }

        public void Reset()
        {
            this.position = new Vector2(0.0f, 0.0f);
            this.tilt = new Vector2(0.0f, 0.0f);
            this.proximity = true;
            this.pressure = 0.0;
        }

        public void Flush()
        {
            var newPos = new CGPoint(position.X, position.Y) - offset;
            var eventRef = CGEventCreateMouseEvent(IntPtr.Zero, moveEvent, newPos, pressedButtons);

            CGEventSetIntegerValueField(eventRef, CGEventField.kCGMouseEventSubtype, (ulong) GetMouseEventSubtype());
            CGEventSetDoubleValueField(eventRef, CGEventField.kCGMouseEventPressure, this.pressure);
            CGEventSetDoubleValueField(eventRef, CGEventField.kCGTabletEventPointPressure, this.pressure);
            CGEventSetDoubleValueField(eventRef, CGEventField.kCGTabletEventTiltX, (double) this.tilt.X);
            CGEventSetDoubleValueField(eventRef, CGEventField.kCGTabletEventTiltY, (double) this.tilt.Y);

            CGEventPost(CGEventTapLocation.kCGHIDEventTap, eventRef);
            CFRelease(eventRef);
        }
    }
}
