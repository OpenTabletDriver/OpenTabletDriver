using System;
using System.Collections.Generic;

namespace OpenTabletDriver.Native.Linux.Evdev
{
    using static Evdev;

    public class EvdevDevice : IDisposable
    {
        public EvdevDevice(string deviceName)
        {
            this.device = libevdev_new();
            libevdev_set_name(this.device, deviceName);
        }

        public bool CanWrite { private set; get; }

        private IntPtr device, uidev;

        public ERRNO Initialize()
        {
            var err = libevdev_uinput_create_from_device(this.device, LIBEVDEV_UINPUT_OPEN_MANAGED, out this.uidev);
            CanWrite = err == 0;
            return (ERRNO) (-err);
        }

        public void Dispose()
        {
            CanWrite = false;
            if (this.uidev != IntPtr.Zero)
            {
                libevdev_uinput_destroy(this.uidev);
                this.uidev = IntPtr.Zero;
                this.device = IntPtr.Zero;
            }
        }

        public void EnableProperty(InputProperty prop) => libevdev_enable_property(this.device, (uint) prop);

        public void EnableType(EventType type) => libevdev_enable_event_type(this.device, (uint) type);

        public void EnableCode(EventType type, EventCode code) =>
            libevdev_enable_event_code(this.device, (uint) type, (uint) code, IntPtr.Zero);

        public void EnableCodes(EventType type, params EventCode[] codes) =>
            EnableCodes(type, (IEnumerable<EventCode>) codes);

        public void EnableCodes(EventType type, IEnumerable<EventCode> codes)
        {
            foreach (var code in codes)
                EnableCode(type, code);
        }

        public void EnableCustomCode(EventType type, EventCode code, IntPtr ptr) =>
            libevdev_enable_event_code(this.device, (uint) type, (uint) code, ptr);

        public void EnableTypeCodes(EventType type, params EventCode[] codes) =>
            EnableTypeCodes(type, (IEnumerable<EventCode>) codes);

        public void EnableTypeCodes(EventType type, IEnumerable<EventCode> codes)
        {
            EnableType(type);
            EnableCodes(type, codes);
        }

        public int Write(EventType type, EventCode code, int value)
        {
            return CanWrite ? libevdev_uinput_write_event(this.uidev, (uint) type, (uint) code, value) : int.MinValue;
        }

        public bool Sync()
        {
            var err = Write(EventType.EV_SYN, EventCode.SYN_REPORT, 0);
            return err == 0;
        }
    }
}
