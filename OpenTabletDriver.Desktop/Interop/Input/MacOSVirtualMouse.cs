using System;
using OpenTabletDriver.Native.OSX;
using OpenTabletDriver.Native.OSX.Input;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Interop.Input
{
    using static OSX;

    public abstract class MacOSVirtualMouse : IMouseButtonHandler, ISynchronousPointer
    {
        private int currButtonStates;
        private int prevButtonStates;
        private float? pendingX;
        private float? pendingY;
        private CGMouseButton lastButton;
        private IntPtr mouseEvent = CGEventCreate(IntPtr.Zero);

        public void MouseDown(MouseButton button)
        {
            SetButtonState(ref currButtonStates, ToCGMouseButton(button), true);
        }

        public void MouseUp(MouseButton button)
        {
            SetButtonState(ref currButtonStates, ToCGMouseButton(button), false);
        }

        public void Flush()
        {
            if (currButtonStates != prevButtonStates)
            {
                // keys here just changed, no drag event should be sent
                ProcessKeyStates(prevButtonStates, currButtonStates);
                prevButtonStates = currButtonStates;
            }
            else if (DrainPendingPosition())
            {
                // can send drag here
                var lastButtonSet = IsButtonSet(currButtonStates, lastButton);
                var cgEventType = ToDragCGEventType(lastButton, lastButtonSet);
                CGEventSetType(mouseEvent, cgEventType);

                if (lastButtonSet)
                    CGEventSetIntegerValueField(mouseEvent, CGEventField.mouseEventButtonNumber, (int)lastButton);
                CGEventPost(CGEventTapLocation.kCGHIDEventTap, mouseEvent);
            }
        }

        public void Reset()
        {
            // send a key up for all currently held keys
            if (currButtonStates > 0)
            {
                ProcessKeyStates(currButtonStates, 0);
                prevButtonStates = 0;
                currButtonStates = 0;
            }
        }

        protected abstract void SetPendingPosition(IntPtr mouseEvent, float x, float y);
        protected abstract void ResetPendingPosition(IntPtr mouseEvent);

        protected void QueuePendingPosition(float x, float y)
        {
            pendingX = x;
            pendingY = y;
        }

        private bool DrainPendingPosition()
        {
            if (pendingX.HasValue)
            {
                SetPendingPosition(mouseEvent, pendingX.Value, pendingY.Value);
                pendingX = null;
                pendingY = null;
                return true;
            }

            return false;
        }

        private void ProcessKeyStates(int prevButtonStates, int currButtonStates)
        {
            for (int i = 0; i < 5; i++)
            {
                var button = (CGMouseButton)i;
                var currState = IsButtonSet(currButtonStates, button);
                var prevState = IsButtonSet(prevButtonStates, button);

                if (currState != prevState)
                {
                    // prepare the mouse event, we reset it here in case
                    // it's a relative event
                    ResetPendingPosition(mouseEvent);

                    // propagate pending position to mouseEvent
                    DrainPendingPosition();
                    var cgEventType = ToNoDragCGEventType(button, currState);

                    CGEventSetType(mouseEvent, cgEventType);
                    CGEventSetIntegerValueField(mouseEvent, CGEventField.mouseEventButtonNumber, i);
                    CGEventPost(CGEventTapLocation.kCGHIDEventTap, mouseEvent);

                    lastButton = button;
                }
            }
        }

        private static CGMouseButton ToCGMouseButton(MouseButton button)
        {
            return button switch
            {
                MouseButton.Left => CGMouseButton.kCGMouseButtonLeft,
                MouseButton.Right => CGMouseButton.kCGMouseButtonRight,
                MouseButton.Middle => CGMouseButton.kCGMouseButtonCenter,
                MouseButton.Backward => CGMouseButton.kCGMouseButtonBackward,
                MouseButton.Forward => CGMouseButton.kCGMouseButtonForward,
                _ => throw new ArgumentException("Invalid mouse button", nameof(button))
            };
        }

        private static CGEventType ToNoDragCGEventType(CGMouseButton button, bool state)
        {
            return (button, state) switch
            {
                (CGMouseButton.kCGMouseButtonLeft, true) => CGEventType.kCGEventLeftMouseDown,
                (CGMouseButton.kCGMouseButtonLeft, false) => CGEventType.kCGEventLeftMouseUp,
                (CGMouseButton.kCGMouseButtonRight, true) => CGEventType.kCGEventRightMouseDown,
                (CGMouseButton.kCGMouseButtonRight, false) => CGEventType.kCGEventRightMouseUp,
                (_, true) => CGEventType.kCGEventOtherMouseDown,
                (_, false) => CGEventType.kCGEventOtherMouseUp,
            };
        }

        private static CGEventType ToDragCGEventType(CGMouseButton button, bool state)
        {
            return (button, state) switch
            {
                (CGMouseButton.kCGMouseButtonLeft, true) => CGEventType.kCGEventLeftMouseDragged,
                (CGMouseButton.kCGMouseButtonRight, true) => CGEventType.kCGEventRightMouseDragged,
                (_, true) => CGEventType.kCGEventOtherMouseDragged,
                (_, false) => CGEventType.kCGEventMouseMoved,
            };
        }

        private bool IsButtonSet(int buttonStates, CGMouseButton button)
        {
            return (buttonStates & (1 << (int)button)) != 0;
        }

        private void SetButtonState(ref int buttonStates, CGMouseButton button, bool state)
        {
            if (state)
                buttonStates |= 1 << (int)button;
            else
                buttonStates &= ~(1 << (int)button);
        }
    }
}
