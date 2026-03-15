using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Native.Linux.Evdev.Structs;

#nullable enable

namespace OpenTabletDriver.Native.Linux.Evdev
{
    using static Evdev;

    /// <summary>
    /// Managed public version of the <see cref="Evdev"/> class
    /// </summary>
    public class EvdevDevice : IDisposable
    {
        public EvdevDevice(string deviceName)
        {
            this.device = libevdev_new();
            libevdev_set_name(this.device, deviceName);
        }

        [MemberNotNullWhen(true, nameof(uidev))]
        public bool CanWrite => !uidev?.IsInvalid ?? false;

        private readonly EvdevHandle device;
        private EvdevUinputHandle? uidev;

        public ERRNO Initialize() => libevdev_uinput_create_from_device(this.device, out this.uidev);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _isDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (uidev is { IsInvalid: false })
                uidev.Dispose();
            uidev = null;

            if (device is { IsInvalid: false })
                device.Dispose();

            _isDisposed = true;
        }

        ~EvdevDevice() => Dispose(false);

        public void EnableProperty(InputProperty prop) => libevdev_enable_property(this.device, prop);

        public void EnableType(EventType type) => libevdev_enable_event_type(this.device, type);

        public void EnableCode(EventType type, EventCode code) => libevdev_enable_event_code(this.device, type, code);
        public void EnableCodes(EventType type, params EventCode[] codes)
        {
            foreach (var code in codes)
                EnableCode(type, code);
        }

        public void EnableAbsCode(EventCode code, input_absinfo absinfo) => libevdev_enable_event_code_abs(this.device, code, absinfo);

        public ERRNO Write(EventType type, EventCode code, int value)
        {
            return CanWrite ? libevdev_uinput_write_event(this.uidev, type, code, value) : throw new InvalidOperationException("Unable to write to an unavailable device");
        }

        public bool Sync()
        {
            var err = Write(EventType.EV_SYN, EventCode.SYN_REPORT, 0);
            return err == 0;
        }
    }
}
