using System;
using System.Numerics;
using OpenTabletDriver.Native.Linux;
using OpenTabletDriver.Native.Linux.Evdev;
using OpenTabletDriver.Native.Linux.Evdev.Structs;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Interop.Input.Absolute
{
    public class EvdevVirtualTablet : IPenActionHandler, IAbsolutePointer, IPressureHandler, ITiltHandler, IEraserHandler, IHoverDistanceHandler, IConfidenceHandler, IToolHandler, ISynchronousPointer, IDisposable
    {
        private const int RESOLUTION = 1000; // subpixels per screen pixel

        private bool isEraser, useConfidenceHandling, isConfident, isToolReport;
        private int toolID, toolSerial, lastToolSerial;

        private EvdevDevice Device { set; get; }

        private EventCode[] supportedEventCodes =
        [
            EventCode.BTN_TOUCH,
            EventCode.BTN_STYLUS,
            EventCode.BTN_STYLUS2,
            EventCode.BTN_STYLUS3,
            EventCode.BTN_TOOL_PEN,
            EventCode.BTN_TOOL_RUBBER,
        ];

        public unsafe EvdevVirtualTablet()
        {
            Device = new EvdevDevice("OpenTabletDriver Virtual Artist Tablet");

            Device.EnableProperty(InputProperty.INPUT_PROP_DIRECT);
            Device.EnableProperty(InputProperty.INPUT_PROP_POINTER);

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

            // enable tool ID/serial reporting
            var toolId = new input_absinfo { minimum = 0, maximum = int.MaxValue };
            input_absinfo* toolIdPtr = &toolId;
            Device.EnableCustomCode(EventType.EV_ABS, EventCode.ABS_MISC, (IntPtr)toolIdPtr);
            Device.EnableType(EventType.EV_MSC);
            Device.EnableCode(EventType.EV_MSC, EventCode.MSC_SERIAL);

            Device.EnableTypeCodes(
                EventType.EV_KEY,
                supportedEventCodes
            );

            var result = Device.Initialize();
            switch (result)
            {
                case ERRNO.NONE:
                    Log.Debug("Evdev", $"Successfully initialized virtual pressure sensitive tablet. (code {result})");
                    break;
                default:
                    Log.WriteNotify("Evdev", $"Failed to initialize virtual pressure sensitive tablet. (error code {result})", LogLevel.Error);
                    break;
            }
        }

        private const int MaxPressure = ushort.MaxValue;

        private EventCode currentTool => isEraser ? EventCode.BTN_TOOL_RUBBER : EventCode.BTN_TOOL_PEN;

        public void SetPosition(Vector2 pos)
        {
            if (useConfidenceHandling && !isConfident) return;
            if (!useConfidenceHandling)
                Device.Write(EventType.EV_KEY, currentTool, 1);
            Device.Write(EventType.EV_ABS, EventCode.ABS_X, (int)(pos.X * RESOLUTION));
            Device.Write(EventType.EV_ABS, EventCode.ABS_Y, (int)(pos.Y * RESOLUTION));
        }

        public void SetPressure(float percentage)
        {
            if (useConfidenceHandling && !isConfident) return;
            Device.Write(EventType.EV_KEY, EventCode.BTN_TOUCH, percentage > 0 ? 1 : 0);
            Device.Write(EventType.EV_ABS, EventCode.ABS_PRESSURE, (int)(MaxPressure * percentage));
        }

        public void SetTilt(Vector2 tilt)
        {
            if (useConfidenceHandling && !isConfident) return;
            Device.Write(EventType.EV_ABS, EventCode.ABS_TILT_X, (int)tilt.X);
            Device.Write(EventType.EV_ABS, EventCode.ABS_TILT_Y, (int)tilt.Y);
        }

        public void SetEraser(bool isEraser)
        {
            if (useConfidenceHandling && !isConfident) return;
            if (this.isEraser == isEraser)
                return; // do nothing if no state change

            // unset opposite tool (in case tablet never sends us Reset/OutOfRange)
            Device.Write(EventType.EV_KEY, currentTool, 0);
            Flush();

            this.isEraser = isEraser;
        }

        public void SetHoverDistance(uint distance)
        {
            if (useConfidenceHandling && !isConfident) return;
            Device.Write(EventType.EV_ABS, EventCode.ABS_DISTANCE, (int)distance);
        }

        public void SetKeyState(EventCode eventCode, bool state)
        {
            Device.Write(EventType.EV_KEY, eventCode, state ? 1 : 0);
        }

        public void Reset()
        {
            // Zero out everything except position and tilt
            foreach (var eventCode in supportedEventCodes)
                Device.Write(EventType.EV_KEY, eventCode, 0);

            Device.Write(EventType.EV_ABS, EventCode.ABS_PRESSURE, 0);

            isEraser = false;
            isConfident = false;
            lastToolSerial = toolSerial;
            toolID = toolSerial = 0;
        }

        private static EventCode? GetCode(PenAction button) => button switch
        {
            PenAction.Tip => null, // tip is handled via pressure
            PenAction.Eraser => null, // eraser is handled via pressure
            PenAction.BarrelButton1 => EventCode.BTN_STYLUS2, // STYLUS2 = right click
            PenAction.BarrelButton2 => EventCode.BTN_STYLUS,
            PenAction.BarrelButton3 => EventCode.BTN_STYLUS3,
            _ => null,
        };

        public void Dispose()
        {
            Device?.Dispose();
            GC.SuppressFinalize(this);
        }

        public void Flush()
        {
            if (isToolReport)
            {
                // drop packet for tool reports to avoid jumping cursor
                isToolReport = false;
                return;
            }

            if (useConfidenceHandling)
            {
                Device.Write(EventType.EV_ABS, EventCode.ABS_MISC, toolID);
                if (isConfident)
                {
                    Device.Write(EventType.EV_KEY, currentTool, 1);
                    if (toolSerial > 0)
                    {
                        Device.Write(EventType.EV_MSC, EventCode.MSC_SERIAL, toolSerial);
                    }
                }
                else if (lastToolSerial != 0) // we must report serial on last out report
                {
                    Device.Write(EventType.EV_MSC, EventCode.MSC_SERIAL, lastToolSerial);
                    lastToolSerial = 0;
                }
            }
            Device.Sync();
        }

        public void Activate(PenAction action)
        {
            if (GetCode(action) is { } code)
                SetKeyState(code, true);
        }

        public void Deactivate(PenAction action)
        {
            if (GetCode(action) is { } code)
                SetKeyState(code, false);
        }

        public void SetConfidence(bool isConfident) => this.isConfident = isConfident;

        public void SetConfidenceHandling(bool enableConfidenceHandling) => useConfidenceHandling = enableConfidenceHandling;

        public void RegisterTool(uint toolID, ulong toolSerial)
        {
            this.toolID = (int)toolID;
            this.toolSerial = (int)toolSerial;
            isToolReport = true;
        }
    }
}
