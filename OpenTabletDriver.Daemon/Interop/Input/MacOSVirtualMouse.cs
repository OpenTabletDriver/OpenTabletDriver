using System;
using System.Linq;
using System.Numerics;
using OpenTabletDriver.Native.MacOS;
using OpenTabletDriver.Native.MacOS.Generic;
using OpenTabletDriver.Native.MacOS.Input;
using OpenTabletDriver.Platform.Display;
using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Daemon.Interop.Input
{
    using static MacOS;

    public abstract class MacOSVirtualMouse : IMouseButtonHandler
    {
        protected MacOSVirtualMouse(IVirtualScreen virtualScreen)
        {
            var primary = virtualScreen.Displays.First();
            Offset = new CGPoint(primary.Position.X, primary.Position.Y);
        }

        private readonly InputDictionary _inputDictionary = new InputDictionary();
        protected CGEventType MoveEvent = CGEventType.kCGEventMouseMoved;
        protected CGPoint Offset;
        protected CGMouseButton PressedButtons;

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
            return _inputDictionary.TryGetValue(button, out var state) ? state : false;
        }

        private void SetMouseButtonState(MouseButton button, bool newState)
        {
            _inputDictionary.UpdateState(button, newState);
            MoveEvent = GetMoveEventType();
            PressedButtons = GetPressedCgButtons();
        }

        protected Vector2 GetPosition()
        {
            var eventRef = CGEventCreate(IntPtr.Zero);
            CGPoint cursor = CGEventGetLocation(eventRef) + Offset;
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

        private CGMouseButton GetPressedCgButtons()
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
            var cgPos = new CGPoint(curPos.X, curPos.Y) - Offset;
            var mouseEventRef = CGEventCreateMouseEvent(IntPtr.Zero, type, cgPos, cgButton);
            CGEventPost(CGEventTapLocation.kCGHIDEventTap, mouseEventRef);
            CFRelease(mouseEventRef);
        }
    }
}
