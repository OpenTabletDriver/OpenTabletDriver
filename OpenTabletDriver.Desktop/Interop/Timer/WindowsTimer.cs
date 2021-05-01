using System;
using System.Runtime.InteropServices;
using OpenTabletDriver.Native.Windows;
using OpenTabletDriver.Native.Windows.Timers;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Timers;

namespace OpenTabletDriver.Desktop.Interop.Timer
{
    using static Windows;

    internal class WindowsTimer : ITimer, IDisposable
    {
        public WindowsTimer()
        {
            callbackDelegate = Callback;
            callbackHandle = GCHandle.Alloc(callbackDelegate);
        }

        private uint timerId;
        private FallbackTimer fallbackTimer;
        private readonly TimerCallback callbackDelegate;
        private readonly GCHandle callbackHandle;
        private readonly object stateLock = new object();

        public bool Enabled { private set; get; }
        public float Interval { set; get; } = 1;

        public event Action Elapsed;

        public unsafe void Start()
        {
            lock (stateLock)
            {
                if (!Enabled)
                {
                    if (IsSupportedNatively(Interval))
                    {
                        var caps = new TimeCaps();
                        _ = timeGetDevCaps(ref caps, (uint)sizeof(TimeCaps));
                        var clampedInterval = Math.Clamp((uint)Interval, caps.wPeriodMin, caps.wPeriodMax);
                        _ = timeBeginPeriod(clampedInterval);
                        timerId = timeSetEvent(clampedInterval, 1, callbackDelegate, IntPtr.Zero, EventType.TIME_PERIODIC | EventType.TIME_KILL_SYNCHRONOUS);
                        Enabled = true;
                    }
                    else
                    {
                        Log.Write("Timer", "Unsupported interval detected, will use fallback timer. Expect high CPU usage", LogLevel.Warning);
                        fallbackTimer = new FallbackTimer
                        {
                            Interval = Interval
                        };
                        fallbackTimer.Elapsed += () => Elapsed?.Invoke();
                        fallbackTimer.Start();
                        Enabled = true;
                    }
                }
            }
        }

        public void Stop()
        {
            lock (stateLock)
            {
                if (Enabled)
                {
                    if (fallbackTimer == null)
                    {
                        _ = timeKillEvent(timerId);
                        _ = timeEndPeriod((uint)Interval);
                    }
                    else
                    {
                        fallbackTimer.Stop();
                        fallbackTimer = null;
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
            callbackHandle.Free();
            GC.SuppressFinalize(this);
        }
    }
}
