using System;
using OpenTabletDriver.Native.MacOS;
using OpenTabletDriver.Native.MacOS.Input;
using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Daemon.Interop.Input
{
    using static MacOS;

    public abstract class MacOSVirtualMouse : IMouseButtonHandler, ISynchronousPointer
    {
        private int _currButtonStates;
        private int _prevButtonStates;
        private float? _pendingX;
        private float? _pendingY;
        private CGMouseButton _lastButton;
        private readonly IntPtr _mouseEvent = CGEventCreate(IntPtr.Zero);

        public void MouseDown(MouseButton button)
        {
            SetButtonState(ref _currButtonStates, ToCGMouseButton(button), true);
        }

        public void MouseUp(MouseButton button)
        {
            SetButtonState(ref _currButtonStates, ToCGMouseButton(button), false);
        }

        public void Flush()
        {
            if (_currButtonStates != _prevButtonStates)
            {
                // keys here just changed, no drag event should be sent
                ProcessKeyStates(_prevButtonStates, _currButtonStates);
                _prevButtonStates = _currButtonStates;
            }
            else if (DrainPendingPosition())
            {
                // can send drag here
                var lastButtonSet = IsButtonSet(_currButtonStates, _lastButton);
                var cgEventType = ToDragCGEventType(_lastButton, lastButtonSet);
                CGEventSetType(_mouseEvent, cgEventType);
                CGEventPost(CGEventTapLocation.kCGHIDEventTap, _mouseEvent);
            }
        }

        public void Reset()
        {
            // send a key up for all currently held keys
            if (_currButtonStates > 0)
            {
                ProcessKeyStates(_currButtonStates, 0);
                _prevButtonStates = 0;
                _currButtonStates = 0;
            }
        }

        protected abstract void SetPendingPosition(IntPtr mouseEvent, float x, float y);
        protected abstract void ResetPendingPosition(IntPtr mouseEvent);

        protected void QueuePendingPosition(float x, float y)
        {
            _pendingX = x;
            _pendingY = y;
        }

        private bool DrainPendingPosition()
        {
            if (_pendingX.HasValue)
            {
                SetPendingPosition(_mouseEvent, _pendingX.Value, _pendingY.Value);
                _pendingX = null;
                _pendingY = null;
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
                    ResetPendingPosition(_mouseEvent);

                    // propagate pending position to mouseEvent
                    DrainPendingPosition();
                    var cgEventType = ToNoDragCGEventType(button, currState);

                    CGEventSetType(_mouseEvent, cgEventType);
                    CGEventSetIntegerValueField(_mouseEvent, CGEventField.mouseEventButtonNumber, i);
                    CGEventSetIntegerValueField(_mouseEvent, CGEventField.mouseEventClickState, 1); // clickState should be set to 1 (or more) during up, down, and drag events

                    CGEventPost(CGEventTapLocation.kCGHIDEventTap, _mouseEvent);

                    _lastButton = button;
                }
            }

            if (currButtonStates == 0)
            {
                // no buttons are pressed, reset button and click state to 0
                CGEventSetIntegerValueField(_mouseEvent, CGEventField.mouseEventButtonNumber, 0);
                CGEventSetIntegerValueField(_mouseEvent, CGEventField.mouseEventClickState, 0);
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

        private static bool IsButtonSet(int buttonStates, CGMouseButton button)
        {
            return (buttonStates & (1 << (int)button)) != 0;
        }

        private static void SetButtonState(ref int buttonStates, CGMouseButton button, bool state)
        {
            if (state)
                buttonStates |= 1 << (int)button;
            else
                buttonStates &= ~(1 << (int)button);
        }
    }
}
