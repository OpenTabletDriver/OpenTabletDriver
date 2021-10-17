using System;
using System.Numerics;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Native.Linux;
using OpenTabletDriver.Native.Linux.Evdev;
using OpenTabletDriver.Native.Linux.Evdev.Structs;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Interop.Input.Absolute
{
    public class EvdevVirtualTablet : EvdevVirtualMouse, IAbsolutePointer, IVirtualTablet
    {
        private const int Max = 1 << 28;
        private static readonly EventCode[] BUTTONS =
        {
            EventCode.BTN_STYLUS,
            EventCode.BTN_STYLUS2,
            EventCode.BTN_STYLUS3
        };

        private Vector2 screenScale;
        private bool IsEraser = false;
        private bool Proximity = true;

        public unsafe EvdevVirtualTablet(IVirtualScreen virtualScreen)
        {
            Device = new EvdevDevice("OpenTabletDriver Virtual Artist Tablet");

            Device.EnableType(EventType.INPUT_PROP_DIRECT);
            Device.EnableType(EventType.EV_ABS);

            screenScale = new Vector2(virtualScreen.Width, virtualScreen.Height);

            var xAbs = new input_absinfo
            {
                maximum = Max,
                resolution = 100
            };
            input_absinfo* xPtr = &xAbs;
            Device.EnableCustomCode(EventType.EV_ABS, EventCode.ABS_X, (IntPtr)xPtr);

            var yAbs = new input_absinfo
            {
                maximum = Max,
                resolution = 100
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
            var newPos = pos / screenScale * Max;
            Device.Write(EventType.EV_KEY, IsEraser ? EventCode.BTN_TOOL_RUBBER : EventCode.BTN_TOOL_PEN, Proximity ? 1 : 0);
            Device.Write(EventType.EV_ABS, EventCode.ABS_X, (int)newPos.X);
            Device.Write(EventType.EV_ABS, EventCode.ABS_Y, (int)newPos.Y);
            Device.Sync();
        }

        public void SetPressure(float percentage)
        {
            Device.Write(EventType.EV_KEY, EventCode.BTN_TOUCH, percentage > 0 ? 1 : 0);
            Device.Write(EventType.EV_ABS, EventCode.ABS_PRESSURE, (int)(MaxPressure * percentage));
            Device.Sync();
        }

        public void SetTilt(Vector2 tilt)
        {
            Device.Write(EventType.EV_ABS, EventCode.ABS_TILT_X, (int)tilt.X);
            Device.Write(EventType.EV_ABS, EventCode.ABS_TILT_Y, (int)tilt.Y);
            Device.Sync();
        }

        public void SetButtonState(uint button, bool active)
        {
            Device.Write(EventType.EV_KEY, BUTTONS[button], active ? 1 : 0);
            Device.Sync();
        }

        public void SetEraser(bool isEraser)
        {
            IsEraser = isEraser;
        }

        public void SetProximity(bool proximity, uint distance)
        {
            Proximity = proximity;
            Device.Write(EventType.EV_ABS, EventCode.ABS_DISTANCE, (int)distance);
            Device.Sync();
        }

        protected override EventCode? GetCode(MouseButton button) => null;
    }
}
