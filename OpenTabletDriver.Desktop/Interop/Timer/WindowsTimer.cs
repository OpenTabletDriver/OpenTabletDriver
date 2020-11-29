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
                var caps = new TimeCaps();
                _ = timeGetDevCaps(ref caps, (uint)sizeof(TimeCaps));
                _ = timeBeginPeriod(Math.Clamp((uint)Interval, caps.wPeriodMin, caps.wPeriodMax));
                Enabled = true;
                timerId = timeSetEvent(0, 1, callbackDelegate, IntPtr.Zero, EventType.TIME_PERIODIC);
            }
        }

        public void Stop()
        {
            lock (stateLock)
            {
                _ = timeKillEvent(timerId);
                _ = timeEndPeriod((uint)Interval);
                Enabled = false;
            }
        }

        private void Callback(uint uTimerID, uint uMsg, UIntPtr dwUser, UIntPtr dw1, UIntPtr dw2)
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
