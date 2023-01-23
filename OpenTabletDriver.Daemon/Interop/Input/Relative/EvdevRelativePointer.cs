using System.Numerics;
using OpenTabletDriver.Native.Linux;
using OpenTabletDriver.Native.Linux.Evdev;
using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Daemon.Interop.Input.Relative
{
    public class EvdevRelativePointer : EvdevVirtualMouse, IRelativePointer
    {
        public EvdevRelativePointer()
        {
            Device = new EvdevDevice("OpenTabletDriver Virtual Mouse");

            Device.EnableTypeCodes(
                EventType.EV_REL,
                EventCode.REL_X,
                EventCode.REL_Y
            );

            Device.EnableTypeCodes(
                EventType.EV_KEY,
                EventCode.BTN_LEFT,
                EventCode.BTN_MIDDLE,
                EventCode.BTN_RIGHT,
                EventCode.BTN_SIDE,
                EventCode.BTN_EXTRA);

            var result = Device.Initialize();
            switch (result)
            {
                case ERRNO.NONE:
                    Log.Debug("Evdev", $"Successfully initialized virtual mouse. (code {result})");
                    break;
                default:
                    Log.Write("Evdev", $"Failed to initialize virtual mouse. (error code {result})", LogLevel.Error);
                    break;
            }
        }

        private Vector2 _error;

        public void SetPosition(Vector2 delta)
        {
            delta += _error;
            _error = new Vector2(delta.X % 1, delta.Y % 1);

            Device.Write(EventType.EV_REL, EventCode.REL_X, (int)delta.X);
            Device.Write(EventType.EV_REL, EventCode.REL_Y, (int)delta.Y);
            Device.Sync();
        }
    }
}
