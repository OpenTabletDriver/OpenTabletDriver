using System;
using System.Linq;
using NativeLib.OSX;
using NativeLib.OSX.Generic;
using NativeLib.OSX.Input;
using TabletDriverPlugin;
using TabletDriverPlugin.Platform.Pointer;

namespace TabletDriverLib.Interop.Mouse
{
    using static OSX;

    public class MacOSMouseHandler : IMouseHandler
    {
        public MacOSMouseHandler()
        {
            var primary = Platform.VirtualScreen.Displays.FirstOrDefault();
            offset = new CGPoint(primary.Position.X, primary.Position.Y);
        }

        private InputDictionary InputDictionary = new InputDictionary();

        private CGPoint offset;
        private CGEventType moveEvent = CGEventType.kCGEventMouseMoved;
        private CGMouseButton pressedButtons;

        public Point GetPosition()
        {
            var eventRef = CGEventCreate(IntPtr.Zero);
            CGPoint cursor = CGEventGetLocation(eventRef) + offset;
            CFRelease(eventRef);
            return new Point((float)cursor.x, (float)cursor.y);
        }

        public void SetPosition(Point pos)
        {
            var newPos = new CGPoint(pos.X, pos.Y) - offset;
            var mouseEventRef = CGEventCreateMouseEvent(IntPtr.Zero, moveEvent, newPos, pressedButtons);
            CGEventPost(CGEventTapLocation.kCGHIDEventTap, mouseEventRef);
            CFRelease(mouseEventRef);
        }

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

        private bool GetMouseButtonState(MouseButton button)
        {
            return InputDictionary.TryGetValue(button, out var state) ? state : false;
        }

        private void SetMouseButtonState(MouseButton button, bool newState)
        {
            InputDictionary.UpdateState(button, newState);
            moveEvent = GetMoveEventType();
            pressedButtons = GetPressedCGButtons();
        }

        private CGEventType GetMoveEventType()
        {
            CGEventType eventType = 0;

            if (GetMouseButtonState(MouseButton.Left))
                eventType |= CGEventType.kCGEventLeftMouseDragged;
            if (GetMouseButtonState(MouseButton.Middle))
                eventType |= CGEventType.kCGEventOtherMouseDragged;
            if (GetMouseButtonState(MouseButton.Right))
                eventType |= CGEventType.kCGEventRightMouseDragged;
            if (GetMouseButtonState(MouseButton.Forward))
                eventType |= CGEventType.kCGEventOtherMouseDragged;
            if (GetMouseButtonState(MouseButton.Backward))
                eventType |= CGEventType.kCGEventOtherMouseDragged;

            return eventType == 0 ? CGEventType.kCGEventMouseMoved : eventType;
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
            CGEventPost(CGEventTapLocation.kCGHIDEventTap, mouseEventRef);
            CFRelease(mouseEventRef);
        }
    }
}