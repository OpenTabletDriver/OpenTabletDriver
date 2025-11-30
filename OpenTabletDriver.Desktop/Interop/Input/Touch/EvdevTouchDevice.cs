using System;
using OpenTabletDriver.Native.Linux;
using OpenTabletDriver.Native.Linux.Evdev;
using OpenTabletDriver.Native.Linux.Evdev.Structs;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet.Touch;

namespace OpenTabletDriver.Desktop.Interop.Input.Touch
{
    public class EvdevTouchDevice : ITouchPointer, ISynchronousPointer, IDisposable
    {
        // a lot of implementation details gathered from:
        // https://www.kernel.org/doc/html/latest/input/multi-touch-protocol.html
        //
        // Unsupported until later, as it needs further additions to the OpenTabletDriver core stack:
        // - ABS_MT_TOUCH_MAJOR
        // - ABS_MT_TOUCH_MINOR
        // - ABS_MT_PRESSURE
        //
        // Unsupported in OpenTabletDriver core and not yet seen in the wild:
        // - ABS_MT_WIDTH_MAJOR
        // - ABS_MT_WIDTH_MINOR
        // - ABS_MT_DISTANCE
        // - ABS_MT_ORIENTATION (value is signed quarter rotation of a revolution around the touch center)
        //
        // some notes on wacom driver behavior:
        // - first finger always updates ABS_X/ABS_Y
        // - ABS_MT_TRACKING_ID corresponds to a unique finger position that increments monotonically for every new finger
        //   - this must be precluded by an ABS_MT_SLOT event to indicate the slot being modified
        // - sends ABS_MT_TRACKING_ID -1 on last finger out and sets tools (TOUCH + TOOL_FINGER) to 0
        // - incrementing finger counts releases the last tool type specified

        public const int DeviceDimension = ushort.MaxValue;

        public static EventCode? FingerCountToTool(int fingerCount) => fingerCount switch
        {
            0 => null,
            1 => EventCode.BTN_TOOL_FINGER,
            2 => EventCode.BTN_TOOL_DOUBLETAP,
            3 => EventCode.BTN_TOOL_TRIPLETAP,
            4 => EventCode.BTN_TOOL_QUADTAP,
            < 32 and >= 5 => EventCode.BTN_TOOL_QUINTTAP, // cap at 32 in case things go weird
            _ => throw new ArgumentOutOfRangeException(nameof(fingerCount), fingerCount, "An unexpected amount of fingers was requested") // overly failsafe
        };

        private EventCode? _lastToolSent = null;

        private EvdevDevice Device { set; get; }

        public unsafe EvdevTouchDevice()
        {
            Device = new EvdevDevice("OpenTabletDriver Virtual Touch");

            // INPUT_PROP_DIRECT intentionally not enabled as docs says that touchpads shouldn't use this

            // TODO: may not need a pointer if used tablet device is a touch screen
            //   Currently not possible due to TabletReference not being available from here, since we're a singleton
            //   See https://github.com/OpenTabletDriver/OpenTabletDriver/issues/4036
            Device.EnableProperty(InputProperty.INPUT_PROP_POINTER);

            Device.EnableType(EventType.EV_ABS);

            var xAbs = new input_absinfo
            {
                maximum = DeviceDimension,
                resolution = 1000
            };
            input_absinfo* xPtr = &xAbs;
            Device.EnableCustomCode(EventType.EV_ABS, EventCode.ABS_X, (IntPtr)xPtr);

            var yAbs = new input_absinfo
            {
                maximum = DeviceDimension,
                resolution = 1000
            };
            input_absinfo* yPtr = &yAbs;
            Device.EnableCustomCode(EventType.EV_ABS, EventCode.ABS_Y, (IntPtr)yPtr);

            Device.EnableTypeCodes(
                EventType.EV_KEY,
                EventCode.BTN_TOUCH,
                EventCode.BTN_TOOL_FINGER
                // TODO: when multitouch gets ready
                //EventCode.BTN_TOOL_DOUBLETAP,
                //EventCode.BTN_TOOL_TRIPLETAP,
                //EventCode.BTN_TOOL_QUADTAP,
                //EventCode.BTN_TOOL_QUINTTAP
            );

            var result = Device.Initialize();
            switch (result)
            {
                case ERRNO.NONE:
                    Log.Debug("Evdev", $"Successfully initialized virtual touch tablet. (code {result})");
                    break;
                default:
                    Log.Write("Evdev", $"Failed to initialize virtual touch tablet. (error code {result})", LogLevel.Error);
                    break;
            }
        }

        public void SetPositions(ReadOnlySpan<TouchPoint> positions, int maxX, int maxY)
        {
            // TODO: handle remaining fingers
            bool firstIteration = true;
            int fingersTouched = 0;
            int firstPositionX = 0, firstPositionY = 0;
            foreach (var touchPoint in positions)
            {
                if (touchPoint == null) continue;
                fingersTouched++;

                float normalizedPosX = touchPoint.Position.X / maxX;
                float normalizedPosY = touchPoint.Position.Y / maxY;

                if (firstIteration)
                {
                    firstPositionX = (int)(normalizedPosX * DeviceDimension);
                    firstPositionY = (int)(normalizedPosY * DeviceDimension);
                    firstIteration = false;
                }

                // TODO: send ABS_MT_POSITION_X etc here
            }

            if (fingersTouched > 0)
                Device.Write(EventType.EV_KEY, EventCode.BTN_TOUCH, 1);

            // TODO: use this line instead when multi-touch is ready
            //var toolToSend = FingerCountToTool(fingersTouched);
            EventCode? toolToSend = fingersTouched > 0 ? EventCode.BTN_TOOL_FINGER : null;
            if (_lastToolSent.HasValue && (!toolToSend.HasValue || _lastToolSent.Value != toolToSend.Value))
                Device.Write(EventType.EV_KEY, _lastToolSent.Value, 0);
            if (toolToSend.HasValue)
                Device.Write(EventType.EV_KEY, toolToSend.Value, 1);
            _lastToolSent = toolToSend;

            if (fingersTouched > 0)
            {
                Device.Write(EventType.EV_ABS, EventCode.ABS_X, firstPositionX);
                Device.Write(EventType.EV_ABS, EventCode.ABS_Y, firstPositionY);
            }
        }

        public void Reset()
        {
            Device.Write(EventType.EV_KEY, EventCode.BTN_TOUCH, 0);
            if (_lastToolSent.HasValue)
                Device.Write(EventType.EV_KEY, _lastToolSent.Value, 0);
            // TODO: reset multi-touch slots
        }

        public void Flush() => Device.Sync();

        public void Dispose()
        {
            Device?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
