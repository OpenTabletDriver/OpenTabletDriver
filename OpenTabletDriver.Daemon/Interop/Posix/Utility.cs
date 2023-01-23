using System;
using System.Runtime.InteropServices;
using OpenTabletDriver.Native.Linux;

namespace OpenTabletDriver.Daemon.Interop.Posix
{
    public static class Utility
    {
        public static int HandleEintr(Func<int> func)
        {
            int ret, count = 0;

            do
            {
                ret = func();
            }
            while (ret == -1
                && (ERRNO)Marshal.GetLastWin32Error() == ERRNO.EINTR
                && count++ < 100);

            return ret;
        }
    }
}
