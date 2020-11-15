using System;
using System.Runtime.InteropServices;
using OpenTabletDriver.Plugin.Timers;
using static OpenTabletDriver.Native.OSX.OSX;

namespace OpenTabletDriver.Interop.Timer
{
    public class MacOSTimer : ITimer
    {
        private IntPtr timerHandle;
        private IntPtr dispatchQueue;
        private readonly TimerCallback callbackFunction;
        private GCHandle callbackHandle;
        private readonly object stateLock = new object();

        public MacOSTimer()
        {
            callbackFunction = Callback;
            callbackHandle = GCHandle.Alloc(callbackFunction);
        }

        public void Start()
        {
            lock (stateLock)
            {
                dispatchQueue = DispatchQueueCreate("timerQueue", IntPtr.Zero);
                timerHandle = DispatchSourceCreate(IntPtr.Zero, UIntPtr.Zero, UIntPtr.Zero, dispatchQueue);

                DispatchSourceSetEventHandler(timerHandle, callbackFunction);

                var start = DispatchTime(0, 100);

                DispatchSourceSetTimer(timerHandle, start, (ulong)(Interval * 1000 * 1000), 0);
                DispatchResume(timerHandle);

                Enabled = true;
            }
        }

        public void Stop()
        {
            lock (stateLock)
            {
                Enabled = false;
                DispatchSourceCancel(timerHandle);
                DispatchRelease(timerHandle);
                DispatchRelease(dispatchQueue);
            }
        }

        public bool Stop(int milliseconds)
        {
            Stop();
            return true;
        }

        public void Dispose()
        {
            callbackHandle.Free();
        }

        private void Callback()
        {
            if (Enabled)
                Elapsed();
        }

        public bool Enabled { private set; get; }

        public float Interval { set; get; }

        public event Action Elapsed;
    }
}