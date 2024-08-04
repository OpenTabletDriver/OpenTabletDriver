using System;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using OpenTabletDriver.Native.Linux;
using OpenTabletDriver.Native.Linux.Timers;
using OpenTabletDriver.Native.Linux.Timers.Structs;
using ITimer = OpenTabletDriver.ITimer;
using TimerCallback = OpenTabletDriver.Native.Linux.Timers.TimerCallback;

namespace OpenTabletDriver.Desktop.Interop.Timer
{
    using static Timers;

    internal class LinuxTimer : ITimer
    {
        public const int EPOLL_ID = 0x001; // hardcode an ID
        public LinuxTimer()
        {
            _callbackDelegate = Callback;
            _callbackHandle = GCHandle.Alloc(_callbackDelegate);
        }

        private readonly TimerCallback _callbackDelegate;
        private readonly object _stateLock = new object();
        private System.Threading.Thread timerThread;
        private int timerFD;
        private int epollFD;
        private GCHandle _callbackHandle;
        private TimerSpec _timeSpec;

        public bool Enabled { private set; get; }
        public float Interval { set; get; } = 1;

        public event Action? Elapsed;

        public void Start()
        {
            lock (_stateLock)
            {
                if (!Enabled)
                {
                    // Create a timerFG instance
                    timerFD = TimerCreate(ClockID.Monotonic, TimerFlag.TFD_NONBLOCK);
                    if (timerFD == -1)
                    {
                        Log.Write("LinuxTimer", $"Failed creating timer: ${(ERRNO)Marshal.GetLastWin32Error()}", LogLevel.Error);
                        Stop();
                        return;
                    }

                    Log.Write("LinuxTimer", $"timerFD: {timerFD}", LogLevel.Debug);

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

                    // Arm the timer
                    var oldTimeSpec = new TimerSpec();
                    if (TimerSetTime(timerFD, TimerFlag.Default, ref _timeSpec, ref oldTimeSpec) != ERRNO.NONE)
                    {
                        Log.Write("LinuxTimer", $"Failed activating the timer: ${(ERRNO)Marshal.GetLastWin32Error()}", LogLevel.Error);
                        Stop();
                        return;
                    }

                    // Create an epoll instance
                    epollFD = EpollCreate();
                    if (epollFD == -1)
                    {
                        Log.Write("LinuxTimer", $"Error creating epoll instance: ${(ERRNO)Marshal.GetLastWin32Error()}", LogLevel.Error);
                        Stop();
                        return;
                    }

                    // Add the timer file descriptor to the epoll instance
                    EpollEvent epollInstance = new EpollEvent
                    {
                        events = EPOLL_ID,
                        fd = timerFD
                    };

                    if (EpollAdd(epollFD, timerFD, ref epollInstance) != ERRNO.NONE)
                    {
                        Log.Write("LinuxTimer", $"Error adding file descriptor to the epoll instance: ${(ERRNO)Marshal.GetLastWin32Error()}", LogLevel.Error);
                        Stop();
                        return;
                    }

                    EpollEvent epollReadEvs = new EpollEvent();

                    timerThread = new System.Threading.Thread(() =>
                    {
                        try
                        {
                            int epollResult;
                            ulong timerExpirations = 0;

                            // Start the epoll event loop
                            while (Enabled)
                            {
                                if (EpollWait(epollFD, out epollReadEvs, 1, -1) == -1)
                                {
                                    Log.Write("LinuxTimer", $"Error waiting for epoll events", LogLevel.Error);
                                    Stop();
                                    return;
                                }

                                if ((epollReadEvs.events & EPOLL_ID) != 0)
                                {
                                    epollResult = TimerGetTime(timerFD, ref timerExpirations, sizeof(ulong));
                                    if (epollResult == -1)
                                    {
                                        Log.Write("LinuxTimer", $"Error reading from timer FD", LogLevel.Error);
                                        Stop();
                                        return;
                                    }

                                    // Update state for each timer expiration
                                    for (ulong i = 0; i < timerExpirations; i++)
                                    {
                                        _callbackDelegate.Invoke();
                                        //Callback();
                                    }
                                }
                            }
                            Log.Write("LinuxTimer", "Timer thread was stopped", LogLevel.Debug);
                            return;
                        }
                        catch (Exception timerException)
                        {
                            Log.Write("LinuxTimer", $"Exception occurred in timer thread: ${timerException}", LogLevel.Error);
                            Stop();
                            return;
                        }
                    });

                    Enabled = true;
                    timerThread.Priority = System.Threading.ThreadPriority.Highest;
                    timerThread.Start();
                }
            }
        }

        public void Stop()
        {
            lock (_stateLock)
            {
                Enabled = false;
                try
                {
                    timerThread.Join();
                    if (EpollRemove(epollFD, timerFD) != ERRNO.NONE)
                    {
                        Log.Write("LinuxTimer",
                                $"Could not remove the timer from the epoll instance: ${(ERRNO)Marshal.GetLastWin32Error()}",
                                LogLevel.Debug);
                    }

                    if (CloseFileDescriptor(epollFD) != ERRNO.NONE)
                    {
                        Log.Write("LinuxTimer",
                                $"Failed closing the epoll file descriptor: ${(ERRNO)Marshal.GetLastWin32Error()}",
                                LogLevel.Debug);
                    }

                    if (CloseFileDescriptor(timerFD) != ERRNO.NONE)
                    {
                        Log.Write("LinuxTimer",
                                 $"Failed closing the timer file descriptor: ${(ERRNO)Marshal.GetLastWin32Error()}",
                                 LogLevel.Debug);
                    }
                    Log.Write("LinuxTimer",
                              "Finished cleaning up",
                              LogLevel.Debug);

                    return;
                }
                catch { }
            }
        }

        private void Callback()
        {
            Elapsed?.Invoke();
        }

        public void Dispose()
        {
            Stop();
            _callbackHandle.Free();
            GC.SuppressFinalize(this);
        }
    }
}
