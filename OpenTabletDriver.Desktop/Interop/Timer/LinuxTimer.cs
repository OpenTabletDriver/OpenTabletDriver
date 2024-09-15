using System;
using System.Runtime.InteropServices;
using System.Threading;
using OpenTabletDriver.Native.Linux;
using OpenTabletDriver.Native.Linux.Timers;
using OpenTabletDriver.Native.Linux.Timers.Structs;

namespace OpenTabletDriver.Desktop.Interop.Timer
{
    using static Timers;

    internal class LinuxTimer : ITimer
    {
        private Thread? _timerThread;
        private readonly object _stateLock = new object();
        private int _timerFD;
        private ITimerSpec _timerSpec;

        private volatile bool _enabled;
        public bool Enabled => _enabled;

        public float Interval { set; get; } = 1;

        public event Action? Elapsed;

        public void Start()
        {
            lock (_stateLock)
            {
                if (!_enabled)
                {
                    int timerFD = TimerCreate(ClockID.Monotonic, 0);

                    if (timerFD == -1)
                    {
                        Log.Write("LinuxTimer", $"Failed creating timer: {(ERRNO)Marshal.GetLastWin32Error()}", LogLevel.Error);
                        return;
                    }

                    _timerFD = timerFD;

                    long seconds = (long)(Interval / 1000.0f);
                    long nseconds = (long)((Interval - seconds * 1000) * 1000.0 * 1000.0);

                    _timerSpec = new ITimerSpec
                    {
                        it_interval = new TimeSpec
                        {
                            sec = seconds,
                            nsec = nseconds
                        },
                        it_value = new TimeSpec
                        {
                            sec = seconds,
                            nsec = nseconds
                        }
                    };

                    if (TimerSetTime(_timerFD, TimerFlag.Default, ref _timerSpec, IntPtr.Zero) != ERRNO.NONE)
                    {
                        Log.Write("LinuxTimer", $"Failed activating the timer: ${(ERRNO)Marshal.GetLastWin32Error()}", LogLevel.Error);
                        return;
                    }

                    _timerThread = new Thread(() =>
                    {
                        while (_enabled)
                        {
                            ulong timerExpirations = 0;

                            if (TimerGetTime(_timerFD, ref timerExpirations, sizeof(ulong)) == sizeof(ulong) && _enabled)
                            {
                                try
                                {
                                    Elapsed?.Invoke();
                                }
                                catch (Exception ex)
                                {
                                    Log.Write("LinuxTimer", $"Elapsed delegate returned an exception", LogLevel.Error);
                                    Log.Exception(ex);
                                }
                            }
                            else if (_enabled)
                            {
                                Log.Write("LinuxTimer", $"Unexpected timer error: ${(ERRNO)Marshal.GetLastWin32Error()}", LogLevel.Error);
                                break;
                            }
                        }
                    });

                    _enabled = true;

                    _timerThread.Priority = ThreadPriority.Highest;
                    _timerThread.Start();
                }
            }
        }

        public void Stop()
        {
            lock (_stateLock)
            {
                if (_enabled)
                {
                    _enabled = false;

                    var timerSpec = new ITimerSpec
                    {
                        it_interval = new TimeSpec
                        {
                            sec = 0,
                            nsec = 0
                        },
                        it_value = new TimeSpec
                        {
                            sec = 0,
                            nsec = 1 // makes it loop once more to safely close
                        }
                    };

                    if (TimerSetTime(_timerFD, TimerFlag.Default, ref timerSpec, IntPtr.Zero) != ERRNO.NONE)
                    {
                        Log.Write("LinuxTimer", $"Failed deactivating the timer: ${(ERRNO)Marshal.GetLastWin32Error()}", LogLevel.Error);
                        return;
                    }

                    _timerThread?.Join();
                    _timerThread = null;

                    if (CloseTimer(_timerFD) != ERRNO.NONE)
                    {
                        Log.Write("LinuxTimer", $"Failed deleting the timer: ${(ERRNO)Marshal.GetLastWin32Error()}", LogLevel.Error);
                        return;
                    }

                    _timerFD = -1;
                }
            }
        }

        public void Dispose()
        {
            if (Enabled)
                Stop();

            GC.SuppressFinalize(this);
        }
    }
}
