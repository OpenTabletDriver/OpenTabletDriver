using System;
using System.Diagnostics;
using System.Numerics;
using OpenTabletDriver.Native.OSX;
using OpenTabletDriver.Native.OSX.Input;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Interop.Input
{
    using static OSX;

    public abstract class MacOSVirtualMouse : IMouseButtonHandler, ISynchronousPointer, ITiltHandler, IEraserHandler, IPressureHandler
    {
        private const int DoubleClickMoveTolerance = 8;
        private const int ProximityExpiresDurationInMs = 200;

        private int _currButtonStates;
        private int _prevButtonStates;
        private float? _pendingX;
        private float? _pendingY;
        private float? _pressure;
        private Vector2? _tilt;
        private bool? _isEraser;
        private CGMouseButton _lastButton;
        private Vector2 _lastMouseDownPosition;
        private bool _mouseMovedSinceLastDown;

        private int _clickState;
        private readonly Stopwatch _stopWatch;
        private readonly Stopwatch _doubleClickStopWatch;
        private readonly IntPtr _eventSource;
        private IntPtr _mouseEvent;
        private readonly double _doubleClickIntervalInMs;

        public MacOSVirtualMouse()
        {

            _doubleClickIntervalInMs = GetDoubleClickInterval() * 1000;
            _doubleClickStopWatch = new Stopwatch();
            _stopWatch = new Stopwatch();
            _stopWatch.Start();
            _eventSource = CGEventSourceCreate(CGEventSourceStatePrivate);
            _mouseEvent = CGEventCreate(_eventSource);
        }

        public void MouseDown(MouseButton button)
        {
            if (!_pendingX.HasValue)
                QueuePendingPositionFromSystem();
            SetButtonState(ref _currButtonStates, ToCGMouseButton(button), true);
        }

        public void MouseUp(MouseButton button)
        {
            if (!_pendingX.HasValue)
                QueuePendingPositionFromSystem();
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
            else if (DrainPendingPosition() is { } position)
            {
                // can send drag here
                var lastButtonSet = IsButtonSet(_currButtonStates, _lastButton);
                var cgEventType = ToDragCGEventType(_lastButton, lastButtonSet);
                CGEventSetType(_mouseEvent, cgEventType);
                SetPendingPosition(_mouseEvent, position.X, position.Y);
                ApplyTabletValues();
                PostEvent();
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

        // binding can be triggered by auxiliary buttons and cursor might be moved by other devices.
        // in such case we fetch the position from system.
        protected abstract void QueuePendingPositionFromSystem();

        protected void QueuePendingPosition(float x, float y)
        {
            _pendingX = x;
            _pendingY = y;
            if (Vector2.Distance(_lastMouseDownPosition, new Vector2(x, y)) > DoubleClickMoveTolerance)
            {
                _mouseMovedSinceLastDown = true;
            }
        }

        public void SetPressure(float percentage)
        {
            _pressure = percentage;
        }

        private Vector2? DrainPendingPosition()
        {
            if (_pendingX.HasValue)
            {
                var vector2 = new Vector2(_pendingX!.Value, _pendingY!.Value);
                _pendingX = null;
                _pendingY = null;
                return vector2;
            }

            return null;
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
                        if (_pendingX.HasValue && (_clickState == 0 || doubleClickInvalidated))
                        {
                            _doubleClickStopWatch.Restart();
                            _lastMouseDownPosition = new Vector2(_pendingX.Value, _pendingY!.Value);
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
                    ResetPendingPosition(_mouseEvent);

                    // propagate pending position to mouseEvent
                    var cgEventType = ToNoDragCGEventType(button, currState);
                    CGEventSetType(_mouseEvent, cgEventType);
                    if (DrainPendingPosition() is { } position)
                    {
                        SetPendingPosition(_mouseEvent, position.X, position.Y);
                    }
                    CGEventSetIntegerValueField(_mouseEvent, CGEventField.mouseEventButtonNumber, i);
                    CGEventSetIntegerValueField(_mouseEvent, CGEventField.mouseEventClickState, _clickState); // clickState should be set to 1 (or more) during up, down, and drag events
                    ApplyTabletValues();
                    PostEvent();
                    _lastButton = button;
                }
            }

            if (currButtonStates == 0)
            {
                // no buttons are pressed, reset button to 0
                CGEventSetIntegerValueField(_mouseEvent, CGEventField.mouseEventButtonNumber, 0);
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
            // The capabilityMask is essential for Adobe software to recognize tablet pressure sensitivity,
            // a feature specific to Wacom devices.
            // However, Adobe officially supports only Wacom tablets,
            // while most other software tends to disregard these masks.

            const WacomCapabilityMask capabilityMask = WacomCapabilityMask.absXBitMask |
                                                       WacomCapabilityMask.absYBitMask |
                                                       WacomCapabilityMask.buttonsBitMask |
                                                       WacomCapabilityMask.pressureBitMask |
                                                       WacomCapabilityMask.tiltXBitMask |
                                                       WacomCapabilityMask.tiltYBitMask |
                                                       WacomCapabilityMask.deviceIdBitMask;

            // A non-zero deviceId is essential for Adobe software to map tablet events to their
            // corresponding capabilities. Randomly generated and hopefully do not conflict with another driver.
            const long deviceId = 5303613955435230461;

            // send proximity events if there are no reports for a while.
            if (_currButtonStates == 0 && _prevButtonStates == 0)
            {
                var elapsed = _stopWatch.ElapsedMilliseconds;
                _stopWatch.Restart();
                if (elapsed > ProximityExpiresDurationInMs)
                {
                    var pointerType = _isEraser ?? false ?
                            NSPointingDeviceType.Eraser : NSPointingDeviceType.Pen;

                    var proximityEvent = CGEventCreate(_eventSource);
                    CGEventSetType(proximityEvent, CGEventType.kCGEventTabletProximity);
                    CGEventSetIntegerValueField(proximityEvent, CGEventField.tabletProximityEventEnterProximity, 1);
                    CGEventSetIntegerValueField(proximityEvent, CGEventField.tabletProximityEventPointerType, (long)(_isEraser ?? false ?
                            NSPointingDeviceType.Eraser : NSPointingDeviceType.Pen));
                    CGEventSetIntegerValueField(proximityEvent, CGEventField.tabletProximityEventCapabilityMask, (long)capabilityMask);
                    CGEventSetIntegerValueField(proximityEvent, CGEventField.tabletProximityEventDeviceID, deviceId);

                    CGEventPost(CGEventTapLocation.kCGHIDEventTap, proximityEvent);
                    CFRelease(proximityEvent);

                    CGEventSetIntegerValueField(_mouseEvent, CGEventField.mouseEventSubtype, (long)CGMouseEventSubtype.TabletProximity);
                    CGEventSetIntegerValueField(_mouseEvent, CGEventField.tabletProximityEventEnterProximity, 1);
                    CGEventSetIntegerValueField(_mouseEvent, CGEventField.tabletProximityEventPointerType, (long)pointerType);
                    CGEventSetIntegerValueField(_mouseEvent, CGEventField.tabletProximityEventCapabilityMask, (long)capabilityMask);
                    CGEventSetIntegerValueField(_mouseEvent, CGEventField.tabletProximityEventDeviceID, deviceId);

                    return;
                }
            }

            // Qt < 5.9 interprets kCGTabletEventPointButtons as left, right, middle mouse click.
            // see https://codereview.qt-project.org/c/qt/qtbase/+/180717
            int buttons = 0;
            if (IsButtonSet(_currButtonStates, CGMouseButton.kCGMouseButtonLeft))
                buttons |= 1;
            else if (IsButtonSet(_currButtonStates, CGMouseButton.kCGMouseButtonRight))
                buttons |= 2;
            else if (IsButtonSet(_currButtonStates, CGMouseButton.kCGMouseButtonCenter))
                buttons |= 4;

            CGEventSetDoubleValueField(_mouseEvent, CGEventField.mouseEventPressure, _pressure ?? 1.0);

            CGEventSetIntegerValueField(_mouseEvent, CGEventField.mouseEventSubtype, (long)CGMouseEventSubtype.TabletPoint);
            // The fields of tabletEvent can only be set after the subtype is defined.
            CGEventSetIntegerValueField(_mouseEvent, CGEventField.tabletEventPointButtons, buttons);
            CGEventSetIntegerValueField(_mouseEvent, CGEventField.tabletEventDeviceID, deviceId);
            CGEventSetDoubleValueField(_mouseEvent, CGEventField.tabletEventPointPressure, _pressure ?? 1.0);

            if (_tilt != null)
            {
                CGEventSetDoubleValueField(_mouseEvent, CGEventField.tabletEventTiltX, _tilt.Value.X / 90.0);
                // TiltY is inverted on MacOS
                // see https://github.com/chromium/chromium/blob/62f1a92b04c1172431a64d581be9e64742c81576/content/browser/renderer_host/input/web_input_event_builders_mac.mm#L431
                CGEventSetDoubleValueField(_mouseEvent, CGEventField.tabletEventTiltY, -_tilt.Value.Y / 90.0);
            }

            // set keyboard modifier and filter out `nonCoalesced` and 0x20000000 flags
            // see https://github.com/Hammerspoon/hammerspoon/blob/0ccc9d07641a660140d1d2f05b76f682b501a0e8/extensions/eventtap/libeventtap_event.m#L1558-L1560
            CGEventSetFlags(_mouseEvent, CGEventSourceFlagsState(CGEventSourceStateHIDSystemState) & (0xffffffff ^ 0x20000100));
        }

        private void PostEvent()
        {
            CGEventPost(CGEventTapLocation.kCGHIDEventTap, _mouseEvent);
            // Fields in a CGEvent are stored in a union determined by the event type,
            // and they cannot be safely reused.
            CFRelease(_mouseEvent);
            _mouseEvent = CGEventCreate(_eventSource);
        }

        ~MacOSVirtualMouse()
        {
            if (_eventSource != IntPtr.Zero)
            {
                CFRelease(_eventSource);
            }
            if (_mouseEvent != IntPtr.Zero)
            {
                CFRelease(_mouseEvent);
            }
        }
    }
}
