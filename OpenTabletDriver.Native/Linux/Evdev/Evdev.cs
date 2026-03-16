using System;
using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Linux.Evdev
{
    public static partial class Evdev
    {
        private const string libevdev = "libevdev.so.2";

        [DllImport(libevdev)]
        public static extern IntPtr libevdev_new();

        [DllImport(libevdev)]
        public static extern void libevdev_set_name(IntPtr dev, string name);

        [DllImport(libevdev)]
        public static extern int libevdev_enable_property(IntPtr dev, uint prop);

        [DllImport(libevdev)]
        public static extern int libevdev_enable_event_type(IntPtr dev, uint type);

        [DllImport(libevdev)]
        public static extern int libevdev_enable_event_code(IntPtr dev, uint type, uint code, IntPtr data);

        [DllImport(libevdev)]
        public static extern int libevdev_uinput_create_from_device(IntPtr dev, int uinput_fd, out IntPtr uinput_dev);

        [DllImport(libevdev)]
        public static extern void libevdev_uinput_destroy(IntPtr uinput_dev);

        [DllImport(libevdev)]
        public static extern int libevdev_uinput_write_event(IntPtr uinput_dev, uint type, uint code, int value);

        public const int LIBEVDEV_UINPUT_OPEN_MANAGED = -2;
    }
}
