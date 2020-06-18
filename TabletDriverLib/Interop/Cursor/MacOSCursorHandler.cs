using System;
using System.Linq;
using NativeLib.OSX;
using NativeLib.OSX.Generic;
using NativeLib.OSX.Input;
using TabletDriverPlugin;
using TabletDriverPlugin.Platform.Pointer;

namespace TabletDriverLib.Interop.Cursor
{
    using static OSX;

    public class MacOSCursorHandler : ICursorHandler
    {
        public MacOSCursorHandler()
        {
            var primary = Platform.VirtualScreen.Displays.FirstOrDefault();
            offset = new CGPoint(primary.Position.X, primary.Position.Y);
        }

        private InputDictionary InputDictionary = new InputDictionary();
        private CGPoint offset;

        public Point GetCursorPosition()
        {
            var eventRef = CGEventCreate(IntPtr.Zero);
            CGPoint cursor = CGEventGetLocation(eventRef);
            CFRelease(eventRef);
            return new Point((float)cursor.x, (float)cursor.y);
        }

        public void SetCursorPosition(Point pos)
        {
            var newPos = new CGPoint(pos.X, pos.Y) - offset;
            CGWarpMouseCursorPosition(newPos);
        }

        private void PostMouseEvent(CGEventType type, CGMouseButton cgButton)
        {
            var eventRef = CGEventCreate(IntPtr.Zero);
            var curPos = GetCursorPosition();
            var cgPos = new CGPoint(curPos.X, curPos.Y);
            var mouseEventRef = CGEventCreateMouseEvent(IntPtr.Zero, type, cgPos, cgButton);
            CGEventPost(CGEventTapLocation.kCGHIDEventTap, mouseEventRef);
            CFRelease(eventRef);
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
                InputDictionary.UpdateState(button, true);
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
                InputDictionary.UpdateState(button, false);
            }
        }

        public bool GetMouseButtonState(MouseButton button)
        {
            return InputDictionary.TryGetValue(button, out var state) ? state : false;
        }
    }
}