using System;
using System.Runtime.InteropServices;
using OpenTabletDriver.Native.Linux.Timers.Structs;

namespace OpenTabletDriver.Native.Linux.Timers
{
    public static class Timers
    {
        private const string libc = "libc.so.6";

        [DllImport(libc, EntryPoint = "timerfd_create", SetLastError = true)]
        public static extern int TimerCreate(ClockID clockID, TimerFlag flags);

        [DllImport(libc, EntryPoint = "timerfd_settime", SetLastError = true)]
        public static extern ERRNO TimerSetTime(int fd, TimerFlag flags, ref ITimerSpec newValue, ref ITimerSpec oldValue);

        [DllImport(libc, EntryPoint = "timerfd_settime", SetLastError = true)]
        public static extern ERRNO TimerSetTime(int fd, TimerFlag flags, ref ITimerSpec newValue, IntPtr oldValue);

        [DllImport(libc, EntryPoint = "read", SetLastError = true)]
        public static extern int TimerGetTime(int fd, ref ulong buf, int count);

        [DllImport(libc, EntryPoint = "close", SetLastError = true)]
        public static extern ERRNO CloseTimer(int fd);
    }
}
