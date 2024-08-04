using System;
using System.Runtime.InteropServices;
using OpenTabletDriver.Native.Linux.Timers.Structs;

namespace OpenTabletDriver.Native.Linux.Timers
{
    public delegate void TimerCallback();

    public static class Timers
    {
        private const string libc = "libc.so.6";

        [DllImport(libc, EntryPoint = "timerfd_create", SetLastError = true)]
        public static extern int TimerCreate(ClockID clockID, TimerFlag flags);

        [DllImport(libc, EntryPoint = "timerfd_settime", SetLastError = true)]
        public static extern ERRNO TimerSetTime(int fd, TimerFlag flags, ref TimerSpec newValue, ref TimerSpec oldValue);

        [DllImport(libc, EntryPoint = "timerfd_settime", SetLastError = true)]
        public static extern ERRNO TimerSetTime(int fd, TimerFlag flags, ref TimerSpec newValue, IntPtr oldValue);

        [DllImport(libc, EntryPoint = "read", SetLastError = true)]
        public static extern int TimerGetTime(int fd, ref ulong buf, int count);

        [DllImport(libc, EntryPoint = "close", SetLastError = true)]
        public static extern ERRNO CloseFileDescriptor(int fd);


        private const int EPOLL_CTL_ADD = 0x1;
        private const int EPOLL_CTL_DEL = 0x2;

        [DllImport(libc, EntryPoint = "epoll_create", SetLastError = true)]
        public static extern int EpollCreate(int size = 1); // unnecessary to set a real size

        public static ERRNO EpollAdd(int epfd, int fd, ref EpollEvent events)
        {
            return __epoll_ctl(epfd, EPOLL_CTL_ADD, fd, ref events);
        }

        public static ERRNO EpollRemove(int epfd, int fd)
        {
            return __epoll_ctl(epfd, EPOLL_CTL_DEL, fd);
        }

        [DllImport(libc, EntryPoint = "epoll_wait", SetLastError = true)]
        public static extern int EpollWait(int epfd, out EpollEvent events, int maxEvents, int timeout);

        [DllImport(libc, EntryPoint = "epoll_ctl", SetLastError = true)]
        private static extern ERRNO __epoll_ctl(int epfd, int op, int fd, ref EpollEvent events);

        [DllImport(libc, EntryPoint = "epoll_ctl", SetLastError = true)]
        private static extern ERRNO __epoll_ctl(int epfd, int op, int fd);
    }
}
