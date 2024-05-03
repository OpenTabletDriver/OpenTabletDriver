using System;
using System.Diagnostics;
using System.Numerics;
using OpenTabletDriver.Native.OSX;
using OpenTabletDriver.Native.OSX.Input;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Interop.Input
{
    using static OSX;

    public abstract class MacOSVirtualMouse : IMouseButtonHandler, ISynchronousPointer, ITiltHandler, IEraserHandler
    {
        private const int _DoubleClickMoveTolerance = 8;
        private const int _ProximityExpiresDurationInMs = 200;

        private int currButtonStates;
        private int prevButtonStates;
        private float? pendingX;
        private float? pendingY;
        private float? _pressure;
        private Vector2? _tilt;
        private bool? _isEraser;
        private CGMouseButton lastButton;
        private Vector2 _lastMouseDownPosition;
        private bool _mouseMovedSinceLastDown;

        private int _clickState;
        private readonly Stopwatch _stopWatch;
        private readonly Stopwatch _doubleClickStopWatch;
        private readonly IntPtr _eventSource;
        private readonly IntPtr mouseEvent;
        private readonly double _doubleClickIntervalInMs;

        public MacOSVirtualMouse()
        {

            _doubleClickIntervalInMs = GetDoubleClickInterval() * 1000;
            _doubleClickStopWatch = new Stopwatch();
            _stopWatch = new Stopwatch();
            _stopWatch.Start();
            _eventSource = CGEventSourceCreate(CGEventSourceStatePrivate);
            mouseEvent = CGEventCreate(_eventSource);
        }

        public void MouseDown(MouseButton button)
        {
            QueuePendingPositionFromSystem();
            SetButtonState(ref currButtonStates, ToCGMouseButton(button), true);
        }

        public void MouseUp(MouseButton button)
        {
            QueuePendingPositionFromSystem();
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
                ApplyTabletValues();
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

        public void SetEraser(bool isEraser)
        {
            _isEraser = isEraser;
        }

        public void SetTilt(Vector2 tilt)
        {
            _tilt = tilt;
        }

        protected abstract void SetPendingPosition(IntPtr mouseEvent, float x, float y);
        protected abstract void ResetPendingPosition(IntPtr mouseEvent);

        protected void QueuePendingPosition(float x, float y)
        {
            pendingX = x;
            pendingY = y;
            if (Vector2.Distance(_lastMouseDownPosition, new Vector2(x, y)) > _DoubleClickMoveTolerance)
            {
                _mouseMovedSinceLastDown = true;
            }
        }

        protected void setPressure(float percentage)
        {
            _pressure = percentage;
        }

        private bool DrainPendingPosition()
        {
            if (pendingX.HasValue)
            {
                SetPendingPosition(mouseEvent, pendingX.Value, pendingY!.Value);
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
                    var doubleClickInvalidated = _mouseMovedSinceLastDown || _doubleClickStopWatch.ElapsedMilliseconds > _doubleClickIntervalInMs;

                    if (currState)
                    {
                        if (pendingX.HasValue && (_clickState == 0 || doubleClickInvalidated))
                        {
                            _doubleClickStopWatch.Reset();
                            _lastMouseDownPosition = new Vector2(pendingX.Value, pendingY!.Value);
                            _mouseMovedSinceLastDown = false;
                            _clickState = 1;
                        }
                        else
                        {
                            _clickState++;
                        }
                    }
                    else
                    {
                        if (doubleClickInvalidated)
                        {
                            _clickState = 0;
                        }
                    }

                    // prepare the mouse event, we reset it here in case
                    // it's a relative event
                    ResetPendingPosition(mouseEvent);

                    // propagate pending position to mouseEvent
                    DrainPendingPosition();
                    var cgEventType = ToNoDragCGEventType(button, currState);

                    CGEventSetType(mouseEvent, cgEventType);
                    CGEventSetIntegerValueField(mouseEvent, CGEventField.mouseEventButtonNumber, i);
                    CGEventSetIntegerValueField(mouseEvent, CGEventField.mouseEventClickState, _clickState); // clickState should be set to 1 (or more) during up, down, and drag events
                    ApplyTabletValues();
                    CGEventPost(CGEventTapLocation.kCGHIDEventTap, mouseEvent);

                    lastButton = button;
                }
            }

            if (currButtonStates == 0)
            {
                // no buttons are pressed, reset button to 0
                CGEventSetIntegerValueField(mouseEvent, CGEventField.mouseEventButtonNumber, 0);
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

        private void ApplyTabletValues()
        {
            // send proximity events if there are no reports for a while.
            if (currButtonStates == 0)
            {
                var elapsed = _stopWatch.ElapsedMilliseconds;
                _stopWatch.Restart();
                if (elapsed > _ProximityExpiresDurationInMs)
                {
                    var pointerType = _isEraser ?? false ?
                            NSPointingDeviceType.Eraser : NSPointingDeviceType.Pen;

                    var proximityEvent = CGEventCreate(_eventSource);
                    CGEventSetType(proximityEvent, CGEventType.kCGEventTabletProximity);
                    CGEventSetIntegerValueField(proximityEvent, CGEventField.tabletProximityEventEnterProximity, 1);
                    CGEventSetIntegerValueField(proximityEvent, CGEventField.tabletProximityEventPointerType, (long)(_isEraser ?? false ?
                            NSPointingDeviceType.Eraser : NSPointingDeviceType.Pen));
                    CGEventPost(CGEventTapLocation.kCGHIDEventTap, proximityEvent);
                    CFRelease(proximityEvent);

                    CGEventSetIntegerValueField(mouseEvent, CGEventField.mouseEventSubtype, (long)CGMouseEventSubtype.TabletProximity);
                    CGEventSetIntegerValueField(mouseEvent, CGEventField.tabletProximityEventEnterProximity, 1);
                    CGEventSetIntegerValueField(mouseEvent, CGEventField.tabletProximityEventPointerType, (long)pointerType);
                    return;
                }
            }

            // Qt < 5.9 interprets kCGTabletEventPointButtons as left, right, middle mouse click.
            // see https://codereview.qt-project.org/c/qt/qtbase/+/180717
            int buttons = 0;
            if (IsButtonSet(currButtonStates, CGMouseButton.kCGMouseButtonLeft))
                buttons |= 1;
            else if (IsButtonSet(currButtonStates, CGMouseButton.kCGMouseButtonRight))
                buttons |= 2;
            else if (IsButtonSet(currButtonStates, CGMouseButton.kCGMouseButtonCenter))
                buttons |= 4;
            CGEventSetIntegerValueField(mouseEvent, CGEventField.tabletEventPointButtons, buttons);

            CGEventSetIntegerValueField(mouseEvent, CGEventField.mouseEventSubtype, (long)CGMouseEventSubtype.TabletPoint);
            CGEventSetDoubleValueField(mouseEvent, CGEventField.mouseEventPressure, _pressure ?? 1.0);
            CGEventSetDoubleValueField(mouseEvent, CGEventField.tabletEventPointPressure, _pressure ?? 1.0);

            if (_tilt != null)
            {
                CGEventSetDoubleValueField(mouseEvent, CGEventField.tabletEventTiltX, _tilt.Value.X / 90.0);
                // TiltY is inverted on MacOS
                // see https://github.com/chromium/chromium/blob/62f1a92b04c1172431a64d581be9e64742c81576/content/browser/renderer_host/input/web_input_event_builders_mac.mm#L431
                CGEventSetDoubleValueField(mouseEvent, CGEventField.tabletEventTiltY, -_tilt.Value.Y / 90.0);
            }

            // set keyboard modifier and filter out `nonCoalesced` and 0x20000000 flags
            // see https://github.com/Hammerspoon/hammerspoon/blob/0ccc9d07641a660140d1d2f05b76f682b501a0e8/extensions/eventtap/libeventtap_event.m#L1558-L1560
            CGEventSetFlags(mouseEvent, CGEventSourceFlagsState(CGEventSourceStateHIDSystemState) & (0xffffffff ^ 0x20000100));
        }

        // binding can be triggered by auxiliary buttons and cursor might be moved by other devices.
        // in such case we fetch the position from system.
        private void QueuePendingPositionFromSystem()
        {
            if (!pendingX.HasValue)
            {
                var eventRef = CGEventCreate(IntPtr.Zero);
                var pos = CGEventGetLocation(eventRef);
                CFRelease(eventRef);
                QueuePendingPosition((float)pos.x, (float)pos.y);
            }
        }

        ~MacOSVirtualMouse()
        {
            if (_eventSource != IntPtr.Zero)
            {
                CFRelease(_eventSource);
            }
            if (mouseEvent != IntPtr.Zero)
            {
                CFRelease(mouseEvent);
            }
        }
    }
}
