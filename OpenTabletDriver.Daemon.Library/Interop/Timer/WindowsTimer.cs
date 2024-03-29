using System;
using System.Runtime.InteropServices;
using OpenTabletDriver.Native.Windows;
using OpenTabletDriver.Native.Windows.Timers;

namespace OpenTabletDriver.Daemon.Interop.Timer
{
    using static Windows;

    internal class WindowsTimer : ITimer, IDisposable
    {
        public WindowsTimer()
        {
            _callbackDelegate = Callback;
            _callbackHandle = GCHandle.Alloc(_callbackDelegate);
        }

        private uint _timerId;
        private FallbackTimer? _fallbackTimer;
        private readonly TimerCallback _callbackDelegate;
        private GCHandle _callbackHandle;
        private readonly object _stateLock = new object();

        public bool Enabled { private set; get; }
        public float Interval { set; get; } = 1;

        public event Action? Elapsed;

        public unsafe void Start()
        {
            lock (_stateLock)
            {
                if (!Enabled)
                {
                    if (IsSupportedNatively(Interval))
                    {
                        var caps = new TimeCaps();
                        _ = timeGetDevCaps(ref caps, (uint)sizeof(TimeCaps));
                        var clampedInterval = Math.Clamp((uint)Interval, caps.wPeriodMin, caps.wPeriodMax);
                        _ = timeBeginPeriod(clampedInterval);
                        _timerId = timeSetEvent(clampedInterval, 1, _callbackDelegate, IntPtr.Zero, EventType.TIME_PERIODIC | EventType.TIME_KILL_SYNCHRONOUS);
                        Enabled = true;
                    }
                    else
                    {
                        Log.WriteNotify("Timer", "Unsupported interval detected, will use fallback timer. Expect high CPU usage. Please use 1000hz, 500hz, 250hz or 125hz instead.", LogLevel.Warning);
                        _fallbackTimer = new FallbackTimer
                        {
                            Interval = Interval
                        };
                        _fallbackTimer.Elapsed += () => Elapsed?.Invoke();
                        _fallbackTimer.Start();
                        Enabled = true;
                    }
                }
            }
        }

        public void Stop()
        {
            lock (_stateLock)
            {
                if (Enabled)
                {
                    if (_fallbackTimer == null)
                    {
                        _ = timeKillEvent(_timerId);
                        _ = timeEndPeriod((uint)Interval);
                    }
                    else
                    {
                        _fallbackTimer.Stop();
                        _fallbackTimer = null;
                    }
                    Enabled = false;
                }
            }
        }

        private void Callback(uint uTimerID, uint uMsg, UIntPtr dwUser, UIntPtr dw1, UIntPtr dw2)
        {
            Elapsed?.Invoke();
        }

        private static bool IsSupportedNatively(float interval)
        {
            return interval == (int)interval;
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
