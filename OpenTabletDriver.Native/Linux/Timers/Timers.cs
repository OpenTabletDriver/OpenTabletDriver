using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using OpenTabletDriver.Native.Linux.Timers.Structs;

#nullable enable

namespace OpenTabletDriver.Native.Linux.Timers
{
    public static partial class Timers
    {
        public class TimerHandle() : SafeHandleZeroOrMinusOneIsInvalid(true)
        {
            protected override bool ReleaseHandle() => CloseTimer(handle) != -1;
        }

        private const string _LIBC = "libc.so.6";

        /// <summary>
        /// Extern of libc's <c>timerfd_create</c>. Created timer must be manually started with <see cref="TimerSetTime"/>
        /// </summary>
        /// <returns>A <see cref="TimerHandle"/> pointing to the timer file descriptor</returns>
        /// <remarks>Not all clock IDs are supported - please check <see cref="TimerHandle.IsInvalid"/> carefully after calling</remarks>
        [LibraryImport(_LIBC, EntryPoint = "timerfd_create", SetLastError = true)]
        public static partial TimerHandle TimerCreate(ClockID clockID = ClockID.Monotonic, TimerFlag flags = TimerFlag.Default);

        /// <summary>
        /// Arms or disarms a timer referred to by <paramref name="handle"/>
        /// <para/>
        /// Extern of libc's <c>timerfd_settime</c>
        /// </summary>
        /// <param name="handle">A valid <see cref="TimerHandle"/></param>
        /// <param name="flags">The <see cref="TimerFlag"/>s for the timer</param>
        /// <param name="newValue">The new <see cref="ITimerSpec"/> to use</param>
        /// <param name="oldValue">The old <see cref="ITimerSpec"/> that was in use</param>
        /// <param name="error">The upstream errno, if any. Will be <see cref="ERRNO.NONE"/> if function returns <c>true</c></param>
        /// <returns>
        /// <c>true</c> on success, or <c>false</c> on error<br/>
        /// If <c>false</c>, <see cref="ERRNO"/> can be read from <paramref name="error"/>
        /// </returns>
        /// <exception cref="ArgumentException">If <paramref name="handle"/> is invalid</exception>
        public static bool TimerSetTime(TimerHandle handle, TimerFlag flags, in ITimerSpec newValue, out ITimerSpec oldValue, out ERRNO error)
        {
            error = ERRNO.NONE;

            if (handle.IsInvalid) throw new ArgumentException("Invalid handle", nameof(handle));

            var rv = Sys_TimerSetTime(handle, flags, in newValue, out oldValue);

            if (rv == -1)
                error = (ERRNO)Marshal.GetLastWin32Error();

            return rv != -1;
        }

        /// <summary>
        /// Check if timer has lapsed/expired, blocking if the timer has not yet lapsed.
        /// </summary>
        /// <param name="handle">A valid <see cref="TimerHandle"/></param>
        /// <param name="expirations">An output buffer for the amount of times that the timer has cycled</param>
        /// <param name="error">The upstream errno, if any. Will be <see cref="ERRNO.NONE"/> if function returns <c>true</c></param>
        /// <returns>
        /// <c>true</c> on success, or <c>false</c> on error or unexpected return value from <c>read</c>.<br/>
        /// If <c>false</c>, any potential API errors can be read as <see cref="ERRNO"/> from <paramref name="error"/>
        /// </returns>
        /// <exception cref="ArgumentException">If <paramref name="handle"/> is invalid</exception>
        /// <remarks>
        /// Does not block if <see cref="TimerFlag.NonBlocking"/> was set in <see cref="TimerCreate"/>,
        /// and instead returns <c>false</c> with error <see cref="ERRNO.EAGAIN"/> if the timer has not yet lapsed.
        /// </remarks>
        public static bool TimerGetTime(TimerHandle handle, out ulong expirations, out ERRNO error)
        {
            error = ERRNO.NONE;

            if (handle.IsInvalid)
                throw new ArgumentException("Handle is invalid", nameof(handle));

            var rv = Sys_TimerGetTime(handle, out expirations, sizeof(ulong));

            if (rv == -1)
                error = (ERRNO)Marshal.GetLastWin32Error();

            return rv == sizeof(ulong);
        }

        /// <returns><c>0</c> on success, or <c>-1</c> on error and <c>errno</c> will be set (use <see cref="Marshal.GetLastWin32Error"/>)</returns>
        [LibraryImport(_LIBC, EntryPoint = "timerfd_settime", SetLastError = true)]
        private static partial int Sys_TimerSetTime(TimerHandle handle, TimerFlag flags, in ITimerSpec newValue, out ITimerSpec oldValue);

        /// <returns><c>0</c> on success, or <c>-1</c> on error and <c>errno</c> will be set (use <see cref="Marshal.GetLastWin32Error"/>)</returns>
        [LibraryImport(_LIBC, EntryPoint = "read", SetLastError = true)]
        private static partial int Sys_TimerGetTime(TimerHandle handle, out ulong expirations, int count);

        /// <returns><c>0</c> on success, or <c>-1</c> on error and <c>errno</c> will be set (use <see cref="Marshal.GetLastWin32Error"/>)</returns>
        [LibraryImport(_LIBC, EntryPoint = "close", SetLastError = true)]
        private static partial int CloseTimer(IntPtr fdHandle);
    }
}
