using System;
using System.Runtime.InteropServices;
using OpenTabletDriver.Native.Windows;
using OpenTabletDriver.Native.Windows.Timers;
using OpenTabletDriver.Plugin.Timers;

namespace OpenTabletDriver.Desktop.Interop.Timer
{
    using static Windows;

    internal class WindowsTimer : ITimer, IDisposable
    {
        private uint timerId;
        private readonly TimerCallback callbackDelegate;
        private readonly GCHandle callbackHandle;
        private readonly object stateLock = new object();

        public WindowsTimer()
        {
            callbackDelegate = Callback;
            callbackHandle = GCHandle.Alloc(callbackDelegate);
        }

        public unsafe void Start()
        {
            lock (stateLock)
            {
                if (!Enabled)
                {
                    var caps = new TimeCaps();
                    _ = timeGetDevCaps(ref caps, (uint)sizeof(TimeCaps));
                    var clampedInterval = Math.Clamp((uint)Interval, caps.wPeriodMin, caps.wPeriodMax);
                    _ = timeBeginPeriod(clampedInterval);
                    Enabled = true;
                    timerId = timeSetEvent(clampedInterval, 1, callbackDelegate, IntPtr.Zero, EventType.TIME_PERIODIC | EventType.TIME_KILL_SYNCHRONOUS);
                }
            }
        }

        public void Stop()
        {
            lock (stateLock)
            {
                if (Enabled)
                {
                    Enabled = false;
                    _ = timeKillEvent(timerId);
                    _ = timeEndPeriod((uint)Interval);
                }
            }
        }

        private void Callback(uint uTimerID, uint uMsg, UIntPtr dwUser, UIntPtr dw1, UIntPtr dw2)
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

        public bool Enabled { private set; get; }

        public float Interval { set; get; } = 1;

        public event Action Elapsed;
    }
}
