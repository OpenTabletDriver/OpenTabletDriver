using System;
using System.Linq;
using System.Numerics;
using OpenTabletDriver.Native.OSX;
using OpenTabletDriver.Native.OSX.Generic;
using OpenTabletDriver.Native.OSX.Input;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Interop.Input
{
    using static OSX;

    public abstract class MacOSVirtualMouse : IPressureHandler, ITiltHandler, IProximityHandler, IMouseButtonHandler
    {
        protected MacOSVirtualMouse()
        {
            var primary = DesktopInterop.VirtualScreen.Displays.FirstOrDefault();
            offset = new CGPoint(primary.Position.X, primary.Position.Y);
        }

        protected InputDictionary inputDictionary = new InputDictionary();
        protected CGEventType moveEvent = CGEventType.kCGEventMouseMoved;
        protected CGPoint offset;
        protected CGMouseButton pressedButtons;
        protected double pressure;
        protected Vector2 tilt;
        protected bool proximity;
        protected bool tabletFeaturesEnabled = false;

        public void MouseDown(MouseButton button)
        {
            if (!GetMouseButtonState(button))
            {
                CGEventType type;
                CGMouseButton cgButton;
                switch (button)
                {
                    case MouseButton.Left:
                        type = CGEventType.kCGEventLeftMouseDown;
                        cgButton = CGMouseButton.kCGMouseButtonLeft;
                        break;
                    case MouseButton.Middle:
                        type = CGEventType.kCGEventOtherMouseDown;
                        cgButton = CGMouseButton.kCGMouseButtonCenter;
                        break;
                    case MouseButton.Right:
                        type = CGEventType.kCGEventRightMouseDown;
                        cgButton = CGMouseButton.kCGMouseButtonRight;
                        break;
                    case MouseButton.Backward:
                        type = CGEventType.kCGEventOtherMouseDown;
                        cgButton = CGMouseButton.kCGMouseButtonBackward;
                        break;
                    case MouseButton.Forward:
                        type = CGEventType.kCGEventOtherMouseDown;
                        cgButton = CGMouseButton.kCGMouseButtonForward;
                        break;
                    default:
                        return;
                }
                PostMouseEvent(type, cgButton);
                SetMouseButtonState(button, true);
            }
        }

        public void MouseUp(MouseButton button)
        {
            if (GetMouseButtonState(button))
            {
                CGEventType type;
                CGMouseButton cgButton;
                switch (button)
                {
                    case MouseButton.Left:
                        type = CGEventType.kCGEventLeftMouseUp;
                        cgButton = CGMouseButton.kCGMouseButtonLeft;
                        break;
                    case MouseButton.Middle:
                        type = CGEventType.kCGEventOtherMouseUp;
                        cgButton = CGMouseButton.kCGMouseButtonCenter;
                        break;
                    case MouseButton.Right:
                        type = CGEventType.kCGEventRightMouseUp;
                        cgButton = CGMouseButton.kCGMouseButtonRight;
                        break;
                    case MouseButton.Backward:
                        type = CGEventType.kCGEventOtherMouseUp;
                        cgButton = CGMouseButton.kCGMouseButtonBackward;
                        break;
                    case MouseButton.Forward:
                        type = CGEventType.kCGEventOtherMouseUp;
                        cgButton = CGMouseButton.kCGMouseButtonForward;
                        break;
                    default:
                        return;
                }
                PostMouseEvent(type, cgButton);
                SetMouseButtonState(button, false);
            }
        }

        public void SetPressure(float percentage)
        {
            this.pressure = percentage;
        }

        public void SetTilt(Vector2 tilt)
        {
            this.tilt = tilt;
        }

        public void SetProximity(bool proximity)
        {
            this.proximity = proximity;
        }

        private bool GetMouseButtonState(MouseButton button)
        {
            return inputDictionary.TryGetValue(button, out var state) ? state : false;
        }

        private void SetMouseButtonState(MouseButton button, bool newState)
        {
            inputDictionary.UpdateState(button, newState);
            moveEvent = GetMoveEventType();
            pressedButtons = GetPressedCGButtons();
        }

        protected Vector2 GetPosition()
        {
            var eventRef = CGEventCreate(IntPtr.Zero);
            CGPoint cursor = CGEventGetLocation(eventRef) + offset;
            CFRelease(eventRef);
            return new Vector2((float)cursor.x, (float)cursor.y);
        }

        private CGEventType GetMoveEventType()
        {
            CGEventType eventType = 0;

            if (GetMouseButtonState(MouseButton.Left))
                eventType = CGEventType.kCGEventLeftMouseDragged;
            else if (GetMouseButtonState(MouseButton.Right))
                eventType = CGEventType.kCGEventRightMouseDragged;
            else if (GetMouseButtonState(MouseButton.Middle))
                eventType = CGEventType.kCGEventOtherMouseDragged;
            else if (GetMouseButtonState(MouseButton.Forward))
                eventType = CGEventType.kCGEventOtherMouseDragged;
            else if (GetMouseButtonState(MouseButton.Backward))
                eventType = CGEventType.kCGEventOtherMouseDragged;

            return eventType == 0 ? CGEventType.kCGEventMouseMoved : eventType;
        }

        protected CGMouseEventSubtype GetMouseEventSubtype()
        {
            if (!tabletFeaturesEnabled)
            {
                return CGMouseEventSubtype.kCGEventMouseSubtypeDefault;
            }
            else if (proximity)
            {
                return CGMouseEventSubtype.kCGEventMouseSubtypeTabletProximity;
            }
            else
            {
                return CGMouseEventSubtype.kCGEventMouseSubtypeTabletPoint;
            }
        }

        private CGMouseButton GetPressedCGButtons()
        {
            CGMouseButton pressedButtons = 0;

            if (GetMouseButtonState(MouseButton.Left))
                pressedButtons |= CGMouseButton.kCGMouseButtonLeft;
            if (GetMouseButtonState(MouseButton.Middle))
                pressedButtons |= CGMouseButton.kCGMouseButtonCenter;
            if (GetMouseButtonState(MouseButton.Right))
                pressedButtons |= CGMouseButton.kCGMouseButtonRight;
            if (GetMouseButtonState(MouseButton.Forward))
                pressedButtons |= CGMouseButton.kCGMouseButtonForward;
            if (GetMouseButtonState(MouseButton.Backward))
                pressedButtons |= CGMouseButton.kCGMouseButtonBackward;

            return pressedButtons;
        }

        private void PostMouseEvent(CGEventType type, CGMouseButton cgButton)
        {
            var curPos = GetPosition();
            var cgPos = new CGPoint(curPos.X, curPos.Y) - offset;
            var mouseEventRef = CGEventCreateMouseEvent(IntPtr.Zero, type, cgPos, cgButton);
            CGEventSetIntegerValueField(mouseEventRef, CGEventField.kCGMouseEventSubtype, (ulong) GetMouseEventSubtype());
            if (tabletFeaturesEnabled)
            {
                CGEventSetDoubleValueField(mouseEventRef, CGEventField.kCGMouseEventPressure, pressure);
                CGEventSetDoubleValueField(mouseEventRef, CGEventField.kCGTabletEventPointPressure, pressure);
                CGEventSetDoubleValueField(mouseEventRef, CGEventField.kCGTabletEventTiltX, (double) tilt.X);
                CGEventSetDoubleValueField(mouseEventRef, CGEventField.kCGTabletEventTiltY, (double) tilt.Y);
            }
            CGEventPost(CGEventTapLocation.kCGHIDEventTap, mouseEventRef);
            CFRelease(mouseEventRef);
        }
    }
}
