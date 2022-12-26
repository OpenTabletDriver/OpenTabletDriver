using System;
using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.MacOS.Timers
{
    public static class Timers
    {
        private const string libc = "libc";

        [DllImport(libc, EntryPoint = "close", SetLastError = true)]
        public static extern int Close(int fd);

        [DllImport(libc, EntryPoint = "kqueue", SetLastError = true)]
        public static extern int KQueue();

        [DllImport(libc, EntryPoint = "kevent", SetLastError = true)]
        public static extern int KEvent(
            int kq,
            [In] KEvent[]? changelist, int nchanges,
            [Out] KEvent[]? eventlist, int nevents,
            in TimeSpan tiemout);
    }
}
