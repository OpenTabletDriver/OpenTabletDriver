using System;
using System.Runtime.InteropServices;
using OpenTabletDriver.Native.Linux;
using OpenTabletDriver.Native.Linux.Timers;
using OpenTabletDriver.Native.Linux.Timers.Structs;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Timers;

namespace OpenTabletDriver.Desktop.Interop.Timer
{
    using static Timers;

    internal class LinuxTimer : ITimer, IDisposable
    {
        private IntPtr timerID;
        private readonly TimerCallback callbackDelegate;
        private GCHandle callbackHandle;
        private readonly object stateLock = new object();
        private TimerSpec timeSpec;
        private SigEvent sigEvent;

        public LinuxTimer()
        {
            callbackDelegate = Callback;
            callbackHandle = GCHandle.Alloc(callbackDelegate);
        }

        public unsafe void Start()
        {
            lock (stateLock)
            {
                sigEvent = new SigEvent
                {
                    notify = SigEv.Thread,
                    thread = new SigEvThread
                    {
                        function = Marshal.GetFunctionPointerForDelegate(callbackDelegate),
                        attribute = IntPtr.Zero
                    },
                    value = new SigVal()
                };

                var createErr = TimerCreate(ClockID.Monotonic, ref sigEvent, out timerID);
                if (createErr != ERRNO.NONE)
                {
                    Log.Write("LinuxTimer", $"Failed creating timer: {(ERRNO)Marshal.GetLastWin32Error()}", LogLevel.Error);
                }

                double interval = Interval * 1000 * 1000;

                timeSpec = new TimerSpec
                {
                    interval = new TimeSpec
                    {
                        sec = 0,
                        nsec = (long)interval
                    },
                    value = new TimeSpec
                    {
                        sec = 0,
                        nsec = 100
                    }
                };

                var oldTimeSpec = new TimerSpec();

                var setErr = TimerSetTime(timerID, TimerFlag.Default, ref timeSpec, ref oldTimeSpec);
                if (setErr != ERRNO.NONE)
                {
                    Log.Write("LinuxTimer", $"Failed activating the timer: ${(ERRNO)Marshal.GetLastWin32Error()}", LogLevel.Error);
                }

                Enabled = true;
            }
        }

        public void Stop()
        {
            lock (stateLock)
            {
                Enabled = false;

                var timeSpec = new TimerSpec
                {
                    interval = new TimeSpec
                    {
                        sec = 0,
                        nsec = 0
                    }
                };

                var setErr = TimerSetTime(timerID, TimerFlag.Default, ref timeSpec, IntPtr.Zero);
                if (setErr != ERRNO.NONE)
                {
                    Log.Write("LinuxTimer", $"Failed deactivating the timer: ${(ERRNO)Marshal.GetLastWin32Error()}", LogLevel.Error);
                }

                var delErr = TimerDelete(timerID);
                if (delErr != ERRNO.NONE)
                {
                    Log.Write("LinuxTimer", $"Failed deleting the timer: ${(ERRNO)Marshal.GetLastWin32Error()}", LogLevel.Error);
                }
            }
        }

        private void Callback(SigVal _)
        {
            Elapsed();
        }

        public void Dispose()
        {
            if (Enabled)
                Stop();
            callbackHandle.Free();
            GC.SuppressFinalize(this);
        }

        public bool Enabled { private set; get; }

        public float Interval { set; get; } = 1;

        public event Action Elapsed;
    }
}
