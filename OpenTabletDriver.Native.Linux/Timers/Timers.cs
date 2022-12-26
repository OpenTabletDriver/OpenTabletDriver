using System;
using System.Runtime.InteropServices;
using OpenTabletDriver.Native.Linux.Timers.Structs;

namespace OpenTabletDriver.Native.Linux.Timers
{
    public delegate void TimerCallback(SigVal a);

    public static class Timers
    {
        private const string librt = "librt.so.1";

        [DllImport(librt, EntryPoint = "timer_create", SetLastError = true)]
        public static extern ERRNO TimerCreate(ClockID clockID, ref SigEvent eventPtr, out IntPtr timerID);

        [DllImport(librt, EntryPoint = "timer_settime", SetLastError = true)]
        public static extern ERRNO TimerSetTime(IntPtr timerID, TimerFlag flags, ref TimerSpec newValue, ref TimerSpec oldValue);

        [DllImport(librt, EntryPoint = "timer_settime", SetLastError = true)]
        public static extern ERRNO TimerSetTime(IntPtr timerID, TimerFlag flags, ref TimerSpec newValue, IntPtr oldValue);

        [DllImport(librt, EntryPoint = "timer_delete", SetLastError = true)]
        public static extern ERRNO TimerDelete(IntPtr timerID);
    }
}
