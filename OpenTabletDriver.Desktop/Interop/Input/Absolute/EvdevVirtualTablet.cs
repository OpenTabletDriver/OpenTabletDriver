using System;
using System.Numerics;
using OpenTabletDriver.Native.Linux;
using OpenTabletDriver.Native.Linux.Evdev;
using OpenTabletDriver.Native.Linux.Evdev.Structs;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Interop.Input.Absolute
{
    public class EvdevVirtualTablet : EvdevVirtualMouse, IAbsolutePointer, IPressureHandler, ITiltHandler, IEraserHandler, IHoverDistanceHandler, IProximityHandler, ISynchronousPointer
    {
        private const int RESOLUTION = 1000; // subpixels per screen pixel

        private bool isEraser;
        private bool proximity = true;

        public unsafe EvdevVirtualTablet()
        {
            Device = new EvdevDevice("OpenTabletDriver Virtual Artist Tablet");

            Device.EnableType(EventType.INPUT_PROP_DIRECT);
            Device.EnableType(EventType.EV_ABS);

            var xAbs = new input_absinfo
            {
                maximum = (int)(DesktopInterop.VirtualScreen.Width * RESOLUTION),
                resolution = 100000
            };
            input_absinfo* xPtr = &xAbs;
            Device.EnableCustomCode(EventType.EV_ABS, EventCode.ABS_X, (IntPtr)xPtr);

            var yAbs = new input_absinfo
            {
                maximum = (int)(DesktopInterop.VirtualScreen.Height * RESOLUTION),
                resolution = 100000
            };
            input_absinfo* yPtr = &yAbs;
            Device.EnableCustomCode(EventType.EV_ABS, EventCode.ABS_Y, (IntPtr)yPtr);

            var pressure = new input_absinfo
            {
                maximum = MaxPressure
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
                EventCode.BTN_TOUCH,
                EventCode.BTN_STYLUS,
                EventCode.BTN_TOOL_PEN,
                EventCode.BTN_TOOL_RUBBER,
                EventCode.BTN_STYLUS2,
                EventCode.BTN_STYLUS3
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

        private const int MaxPressure = ushort.MaxValue;

        public void SetPosition(Vector2 pos)
        {
            Device.Write(EventType.EV_KEY, isEraser ? EventCode.BTN_TOOL_RUBBER : EventCode.BTN_TOOL_PEN, proximity ? 1 : 0);
            Device.Write(EventType.EV_ABS, EventCode.ABS_X, (int)(pos.X * RESOLUTION));
            Device.Write(EventType.EV_ABS, EventCode.ABS_Y, (int)(pos.Y * RESOLUTION));
        }

        public void SetPressure(float percentage)
        {
            Device.Write(EventType.EV_KEY, EventCode.BTN_TOUCH, percentage > 0 ? 1 : 0);
            Device.Write(EventType.EV_ABS, EventCode.ABS_PRESSURE, (int)(MaxPressure * percentage));
        }

        public void SetTilt(Vector2 tilt)
        {
            Device.Write(EventType.EV_ABS, EventCode.ABS_TILT_X, (int)tilt.X);
            Device.Write(EventType.EV_ABS, EventCode.ABS_TILT_Y, (int)tilt.Y);
        }

        public void SetEraser(bool isEraser)
        {
            this.isEraser = isEraser;
        }

        public void SetProximity(bool proximity)
        {
            this.proximity = proximity;
        }

        public void SetHoverDistance(uint distance)
        {
            Device.Write(EventType.EV_ABS, EventCode.ABS_DISTANCE, (int)distance);
        }

        public void SetKeyState(EventCode eventCode, bool state)
        {
            Device.Write(EventType.EV_KEY, eventCode, state ? 1 : 0);
        }

        public void Reset()
        {
            // Zero out everything except position and tilt
            Device.Write(EventType.EV_KEY, EventCode.BTN_TOOL_RUBBER, 0);
            Device.Write(EventType.EV_KEY, EventCode.BTN_TOOL_PEN, 0);
            Device.Write(EventType.EV_KEY, EventCode.BTN_TOUCH, 0);
            Device.Write(EventType.EV_ABS, EventCode.ABS_PRESSURE, 0);
            Device.Write(EventType.EV_KEY, EventCode.BTN_STYLUS, 0);
            Device.Write(EventType.EV_KEY, EventCode.BTN_STYLUS2, 0);
            Device.Write(EventType.EV_KEY, EventCode.BTN_STYLUS3, 0);

            isEraser = false;
            proximity = true; // we counterintuitively set this to true since its the initial state
        }

        public void Flush()
        {
            Device.Sync();
        }

        protected override EventCode? GetCode(MouseButton button) => null;
    }
}
