using System;

namespace NativeLib.Linux.Evdev
{
    using static Evdev;

    public class EvdevDevice : IDisposable
    {
        public EvdevDevice(string deviceName)
        {
            _device = libevdev_new();
            libevdev_set_name(_device, deviceName);
        }

        private IntPtr _device;
        private IntPtr _uidev;

        public bool Initialize()
        {
            var err = libevdev_uinput_create_from_device(_device, LIBEVDEV_UINPUT_OPEN_MANAGED, out _uidev);
            return err != 0;
        }

        public void Dispose()
        {
            if (_uidev != null)
            {
                libevdev_uinput_destroy(_uidev);
                _uidev = IntPtr.Zero;
                _device = IntPtr.Zero;
            }
        }

        public void EnableType(EventType type) => libevdev_enable_event_type(_device, (uint)type);

        public void EnableCode(EventType type, EventCode code) => libevdev_enable_event_code(_device, (uint)type, (uint)code, IntPtr.Zero);
        public void EnableCodes(EventType type, params EventCode[] codes)
        {
            foreach (var code in codes)
                EnableCode(type, code);
        }

        public void EnableCustomCode(EventType type, EventCode code, IntPtr ptr) => libevdev_enable_event_code(_device, (uint)type, (uint)code, ptr);

        public void EnableTypeCodes(EventType type, params EventCode[] codes)
        {
            EnableType(type);
            EnableCodes(type, codes);
        }

        public void Write(EventType type, EventCode code, int value) => libevdev_uinput_write_event(_uidev, (uint)type, (uint)code, value);

        public void Sync() => Write(EventType.EV_SYN, EventCode.SYN_REPORT, 0);
    }
}
