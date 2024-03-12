using System;

namespace OpenTabletDriver.Native.MacOS.Timers
{
    public enum FilterType : Int16
    {
        EVFILT_READ = -1,
        EVFILT_AIO = -2,
        EVFILT_WRITE = -3,
        EVFILT_VNODE = -4,
        EVFILT_PROC = -5,
        EVFILT_SIGNAL = -6,
        EVFILT_TIMER = -7,
        EVFILT_MACHPORT = -8,
        EVFILT_FS = -9,
        EVFILT_USER = -10,
        EVFILT_VM = -12,
        EVFILT_EXCEPT = -15,
    };
}
