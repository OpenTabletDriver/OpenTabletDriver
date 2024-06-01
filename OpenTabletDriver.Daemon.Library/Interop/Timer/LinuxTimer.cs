using System;
using System.Runtime.InteropServices;
using OpenTabletDriver.Native.Linux;
using OpenTabletDriver.Native.Linux.Timers;
using OpenTabletDriver.Native.Linux.Timers.Structs;

namespace OpenTabletDriver.Daemon.Interop.Timer
{
    using static Timers;

    internal class LinuxTimer : ITimer
    {
        public LinuxTimer()
        {
            _callbackDelegate = Callback;
            _callbackHandle = GCHandle.Alloc(_callbackDelegate);
        }

        private readonly TimerCallback _callbackDelegate;
        private readonly object _stateLock = new object();
        private IntPtr _timerID;
        private GCHandle _callbackHandle;
        private TimerSpec _timeSpec;
        private SigEvent _sigEvent;

        public bool Enabled { private set; get; }
        public float Interval { set; get; } = 1;

        public event Action? Elapsed;

        public void Start()
        {
            lock (_stateLock)
            {
                if (!Enabled)
                {
                    _sigEvent = new SigEvent
                    {
                        notify = SigEv.Thread,
                        thread = new SigEvThread
                        {
                            function = Marshal.GetFunctionPointerForDelegate(_callbackDelegate),
                            attribute = IntPtr.Zero
                        },
                        value = new SigVal()
                    };

                    if (TimerCreate(ClockID.Monotonic, ref _sigEvent, out _timerID) != ERRNO.NONE)
                    {
                        Log.Write("LinuxTimer", $"Failed creating timer: {(ERRNO)Marshal.GetLastWin32Error()}", LogLevel.Error);
                        return;
                    }

                    double interval = Interval * 1000 * 1000;

                    _timeSpec = new TimerSpec
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
                    if (TimerSetTime(_timerID, TimerFlag.Default, ref _timeSpec, ref oldTimeSpec) != ERRNO.NONE)
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
            lock (_stateLock)
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

                    if (TimerSetTime(_timerID, TimerFlag.Default, ref timeSpec, IntPtr.Zero) != ERRNO.NONE)
                    {
                        Log.Write("LinuxTimer", $"Failed deactivating the timer: ${(ERRNO)Marshal.GetLastWin32Error()}", LogLevel.Error);
                        return;
                    }

                    if (TimerDelete(_timerID) != ERRNO.NONE)
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
            _callbackHandle.Free();
            GC.SuppressFinalize(this);
        }
    }
}
