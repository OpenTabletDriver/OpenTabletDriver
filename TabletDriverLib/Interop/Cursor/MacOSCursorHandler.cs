using System;
using TabletDriverLib.Component;

namespace TabletDriverLib.Interop.Cursor
{
    using static Native.MacOSX;

    public class MacOSCursorHandler : ICursorHandler
    {
        private InputDictionary InputDictionary = new InputDictionary();

        public Point GetCursorPosition()
        {
            IntPtr eventRef = CGEventCreate();
            CGPoint cursor = CGEventGetLocation(ref eventRef);
            CFRelease(eventRef);
            return (Point)cursor;
        }

        public void SetCursorPosition(Point pos)
        {
            CGWarpMouseCursorPosition((CGPoint)pos);
        }

        private void PostMouseEvent(CGEventType type, CGMouseButton cgButton)
        {
            var eventRef = CGEventCreate();
            var curPos = (CGPoint)GetCursorPosition();
            var mouseEventRef = CGEventCreateMouseEvent(ref eventRef, type, curPos, cgButton);
            CGEventPost(ref mouseEventRef, type, curPos, cgButton);
            CFRelease(eventRef);
            CFRelease(mouseEventRef);
        }
 
        public void MouseDown(MouseButton button)
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
                default:
                    return;
            }
            PostMouseEvent(type, cgButton);
            InputDictionary.UpdateState(button, true);    
        }

        public void MouseUp(MouseButton button)
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
                default:
                    return;
            }
            PostMouseEvent(type, cgButton);
            InputDictionary.UpdateState(button, false);
        }

        public bool GetMouseButtonState(MouseButton button)
        {
            return InputDictionary.TryGetValue(button, out var state) ? state : false;
        }
    }
}