using System;
using System.Runtime.InteropServices;
using System.Threading;
using OpenTabletDriver.Native.Linux;
using OpenTabletDriver.Native.Linux.Timers;
using OpenTabletDriver.Native.Linux.Timers.Structs;
using OpenTabletDriver.Plugin;

#nullable enable

namespace OpenTabletDriver.Desktop.Interop.Timer
{
    using static Timers;
    using ITimer = Plugin.Timers.ITimer;

    internal class LinuxTimer : ITimer, IDisposable
    {
        private Thread? _timerThread;
        private readonly object _stateLock = new object();
        private TimerHandle? _timerFD;
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
                    var timerFD = TimerCreate();

                    if (timerFD.IsInvalid)
                    {
                        Log.Write("LinuxTimer", $"Failed creating timer: {(ERRNO)Marshal.GetLastWin32Error()}", LogLevel.Error);
                        return;
                    }

                    _timerFD = timerFD;

                    long totalNS = (long)(Interval * 1000.0 * 1000.0);
                    const long ns_per_second = 1000 * 1000 * 1000;

                    long seconds = totalNS / ns_per_second;
                    long nseconds = totalNS % ns_per_second;

                    var timeSpec = new TimeSpec(seconds, nseconds);

                    _timerSpec = new ITimerSpec(timeSpec, timeSpec);

                    if (!TimerSetTime(_timerFD, TimerFlag.Default, in _timerSpec, out _, out var startError))
                    {
                        Log.Write("LinuxTimer", $"Failed activating the timer: {startError}", LogLevel.Error);
                        return;
                    }

                    _timerThread = new Thread(() =>
                    {
                        while (_enabled)
                        {
                            if (TimerGetTime(_timerFD, out _, out var readError) && _enabled)
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
                                if (readError == ERRNO.EAGAIN)
                                    throw new NotImplementedException("Non-blocking timers are unimplemented");

                                Log.Write("LinuxTimer", $"Unexpected timer error: ${readError}", LogLevel.Error);
                                break;
                            }
                        }
                    });

                    _enabled = true;

                    _timerThread.Priority = ThreadPriority.Highest;
                    _timerThread.IsBackground = true;
                    _timerThread.Start();
                }
            }
        }

        private static readonly ITimerSpec s_StoppingTimerSpec = new()
        {
            it_interval = new TimeSpec(0, 0),
            it_value = new TimeSpec(0, 1), // makes it loop once more to safely close
        };

        public void Stop()
        {
            lock (_stateLock)
            {
                if (_enabled)
                {
                    _enabled = false;

                    if (_timerFD == null)
                    {
                        Log.Write(nameof(LinuxTimer), "Failed deactivating the timer: timerFD was null (did you forget to start it first?)", LogLevel.Error);
                    }
                    else if (!TimerSetTime(_timerFD, TimerFlag.Default, in s_StoppingTimerSpec, out _, out var deactivateError))
                    {
                        Log.Write("LinuxTimer", $"Failed deactivating the timer: ${deactivateError}", LogLevel.Error);
                        return;
                    }

                    _timerThread?.Join();
                    _timerThread = null;

                    _timerFD?.Dispose();
                    _timerFD = null;
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
