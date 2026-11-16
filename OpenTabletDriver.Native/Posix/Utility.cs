using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using OpenTabletDriver.Native.Linux;

namespace OpenTabletDriver.Native.Posix
{
    public static class Utility
    {
        public static int HandleEintr([InstantHandle] Func<int> func)
        {
            int ret, count = 0;

            do
            {
                ret = func();
            }
            while (ret == -1 && (ERRNO)Marshal.GetLastWin32Error() == ERRNO.EINTR && count++ < 100);

            return ret;
        }
    }
}
