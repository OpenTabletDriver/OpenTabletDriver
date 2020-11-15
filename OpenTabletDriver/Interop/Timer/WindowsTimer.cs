using System;
using System.Runtime.InteropServices;
using OpenTabletDriver.Native.Windows;
using OpenTabletDriver.Native.Windows.Timers;
using OpenTabletDriver.Plugin.Timers;

namespace OpenTabletDriver.Interop.Timer
{
    using static Windows;

    internal class WindowsTimer : ITimer
    {
        private uint timerId;
        private TimerCallback callbackDelegate;
        private GCHandle callbackHandle;
        private readonly object stateLock = new object();

        public WindowsTimer()
        {
            callbackDelegate = Callback;
            callbackHandle = GCHandle.Alloc(callbackDelegate);
        }

        public void Start()
        {
            lock (stateLock)
            {
                var caps = new TimeCaps();
                timeGetDevCaps(ref caps, (uint)Marshal.SizeOf(caps));
                timeBeginPeriod(Math.Clamp((uint)Interval, caps.wPeriodMin, caps.wPeriodMax));
                Enabled = true;
                timerId = timeSetEvent(1, 1, callbackDelegate, IntPtr.Zero, EventType.TIME_PERIODIC);
            }
        }

        public void Stop()
        {
            lock (stateLock)
            {
                timeKillEvent(timerId);
                timeEndPeriod((uint)Interval);
                Enabled = false;
            }
        }

        public bool Stop(int milliseconds)
        {
            Stop();
            return true; // waiting not implemented
        }

        private void Callback(uint uTimerID, uint uMsg, UIntPtr dwUser, UIntPtr dw1, UIntPtr dw2)
        {
            if (Enabled)
                Elapsed();
        }

        public void Dispose()
        {
            callbackHandle.Free();
        }

        public bool Enabled { private set; get; }

        public float Interval { set; get; } = 1;

        public event Action Elapsed;
    }
}