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
        public LinuxTimer()
        {
            callbackDelegate = Callback;
            callbackHandle = GCHandle.Alloc(callbackDelegate);
        }

        private IntPtr timerID;
        private readonly TimerCallback callbackDelegate;
        private GCHandle callbackHandle;
        private readonly object stateLock = new object();
        private TimerSpec timeSpec;
        private SigEvent sigEvent;

        public bool Enabled { private set; get; }
        public float Interval { set; get; } = 1;

        public event Action Elapsed;

        public void Start()
        {
            lock (stateLock)
            {
                if (!Enabled)
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

                    if (TimerCreate(ClockID.Monotonic, ref sigEvent, out timerID) != ERRNO.NONE)
                    {
                        Log.Write("LinuxTimer", $"Failed creating timer: {(ERRNO)Marshal.GetLastWin32Error()}", LogLevel.Error);
                        return;
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
                    if (TimerSetTime(timerID, TimerFlag.Default, ref timeSpec, ref oldTimeSpec) != ERRNO.NONE)
                    {
                        Log.Write("LinuxTimer", $"Failed activating the timer: ${(ERRNO)Marshal.GetLastWin32Error()}", LogLevel.Error);
                        return;
                    }

                    Enabled = true;
                }
            }
        }

        public void Stop()
        {
            lock (stateLock)
            {
                if (Enabled)
                {
                    var timeSpec = new TimerSpec
                    {
                        interval = new TimeSpec
                        {
                            sec = 0,
                            nsec = 0
                        }
                    };

                    if (TimerSetTime(timerID, TimerFlag.Default, ref timeSpec, IntPtr.Zero) != ERRNO.NONE)
                    {
                        Log.Write("LinuxTimer", $"Failed deactivating the timer: ${(ERRNO)Marshal.GetLastWin32Error()}", LogLevel.Error);
                        return;
                    }

                    if (TimerDelete(timerID) != ERRNO.NONE)
                    {
                        Log.Write("LinuxTimer", $"Failed deleting the timer: ${(ERRNO)Marshal.GetLastWin32Error()}", LogLevel.Error);
                        return;
                    }

                    Enabled = false;
                }
            }
        }

        private void Callback(SigVal _)
        {
            Elapsed?.Invoke();
        }

        public void Dispose()
        {
            if (Enabled)
                Stop();
            callbackHandle.Free();
            GC.SuppressFinalize(this);
        }
    }
}
