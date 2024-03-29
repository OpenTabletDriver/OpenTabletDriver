using System;
using System.Numerics;
using OpenTabletDriver.Native.Linux;
using OpenTabletDriver.Native.Linux.Evdev;
using OpenTabletDriver.Native.Linux.Evdev.Structs;
using OpenTabletDriver.Platform.Display;
using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Daemon.Interop.Input.Absolute
{
    public class EvdevVirtualTablet : EvdevVirtualMouse, IPressureHandler, ITiltHandler, IEraserHandler, IHoverDistanceHandler, ISynchronousPointer
    {
        // order seems important due to reset ordering (to satisfy libinput)
        // tools -> touch -> buttons
        private static readonly EventCode[] eventCodes =
        {
                EventCode.BTN_TOOL_PEN,
                EventCode.BTN_TOOL_RUBBER,
                EventCode.BTN_TOUCH,
                EventCode.BTN_STYLUS,
                EventCode.BTN_STYLUS2,
                EventCode.BTN_STYLUS3,
        };

        private const int RESOLUTION = 1000; // subpixels per screen pixel

        private bool _isEraser;

        public unsafe EvdevVirtualTablet(IVirtualScreen virtualScreen)
        {
            Device = new EvdevDevice("OpenTabletDriver Virtual Artist Tablet");

            Device.EnableProperty(InputProperty.INPUT_PROP_DIRECT);
            Device.EnableProperty(InputProperty.INPUT_PROP_POINTER);

            Device.EnableType(EventType.EV_ABS);

            var xAbs = new input_absinfo
            {
                maximum = (int)(virtualScreen.Width * RESOLUTION),
                resolution = 100000
            };
            input_absinfo* xPtr = &xAbs;
            Device.EnableCustomCode(EventType.EV_ABS, EventCode.ABS_X, (IntPtr)xPtr);

            var yAbs = new input_absinfo
            {
                maximum = (int)(virtualScreen.Height * RESOLUTION),
                resolution = 100000
            };
            input_absinfo* yPtr = &yAbs;
            Device.EnableCustomCode(EventType.EV_ABS, EventCode.ABS_Y, (IntPtr)yPtr);

            var pressure = new input_absinfo
            {
                maximum = MAX_PRESSURE
            };
            input_absinfo* pressurePtr = &pressure;
            Device.EnableCustomCode(EventType.EV_ABS, EventCode.ABS_PRESSURE, (IntPtr)pressurePtr);

            var xTilt = new input_absinfo
            {
                minimum = -64,
                maximum = 63,
                resolution = 57
            };
            input_absinfo* xTiltPtr = &xTilt;
            Device.EnableCustomCode(EventType.EV_ABS, EventCode.ABS_TILT_X, (IntPtr)xTiltPtr);

            var yTilt = new input_absinfo
            {
                minimum = -64,
                maximum = 63,
                resolution = 57
            };
            input_absinfo* yTiltPtr = &yTilt;
            Device.EnableCustomCode(EventType.EV_ABS, EventCode.ABS_TILT_Y, (IntPtr)yTiltPtr);

            Device.EnableTypeCodes(
                EventType.EV_KEY,
                eventCodes
            );

            var result = Device.Initialize();
            switch (result)
            {
                case ERRNO.NONE:
                    Log.Debug("Evdev", $"Successfully initialized virtual pressure sensitive tablet. (code {result})");
                    break;
                default:
                    Log.Write("Evdev", $"Failed to initialize virtual pressure sensitive tablet. (error code {result})", LogLevel.Error);
                    break;
            }
        }

        private const int MAX_PRESSURE = ushort.MaxValue;

        public void SetPosition(Vector2 pos)
        {
            Device.Write(EventType.EV_KEY, _isEraser ? EventCode.BTN_TOOL_RUBBER : EventCode.BTN_TOOL_PEN, 1);
            Device.Write(EventType.EV_ABS, EventCode.ABS_X, (int)(pos.X * RESOLUTION));
            Device.Write(EventType.EV_ABS, EventCode.ABS_Y, (int)(pos.Y * RESOLUTION));
        }

        public void SetPressure(float percentage)
        {
            Device.Write(EventType.EV_ABS, EventCode.ABS_PRESSURE, (int)(MAX_PRESSURE * percentage));
        }

        public void SetTilt(Vector2 tilt)
        {
            Device.Write(EventType.EV_ABS, EventCode.ABS_TILT_X, (int)tilt.X);
            Device.Write(EventType.EV_ABS, EventCode.ABS_TILT_Y, (int)tilt.Y);
        }

        public void SetEraser(bool isEraser)
        {
            this._isEraser = isEraser;
        }

        public void SetHoverDistance(uint distance)
        {
            Device.Write(EventType.EV_ABS, EventCode.ABS_DISTANCE, (int)distance);
        }

        public void SetKeyState(EventCode eventCode, bool state)
        {
            Device.Write(EventType.EV_KEY, eventCode, state ? 1 : 0);
        }

        public sealed override void Reset()
        {
            // Zero out everything except position and tilt
            foreach (var code in eventCodes)
                Device.Write(EventType.EV_KEY, code, 0);
            Device.Write(EventType.EV_ABS, EventCode.ABS_PRESSURE, 0);

            _isEraser = false;
        }
    }
}
