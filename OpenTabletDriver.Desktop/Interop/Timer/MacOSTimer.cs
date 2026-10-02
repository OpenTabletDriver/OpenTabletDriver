using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenTabletDriver.Native.Linux;
using OpenTabletDriver.Native.OSX;
using OpenTabletDriver.Native.OSX.Timers;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Timers;
using static OpenTabletDriver.Native.OSX.Timers.Timers;
using static OpenTabletDriver.Native.Posix.Utility;
using Thread = System.Threading.Thread;

namespace OpenTabletDriver.Desktop.Interop.Timer
{
    internal class MacOSTimer : ITimer, IDisposable
    {
        const int TIMER_CANCELLED = 1;
        const float MAX_REALTIME_INTERVAL_MS = 20;

        private Thread? thread;
        private int kqueue;
        private readonly object stateLock = new();

        public MacOSTimer()
        {

        }

        public bool Enabled { private set; get; }

        public float Interval { get; set; } = 1;

        public event Action? Elapsed;

        public void Dispose()
        {
            Stop();
        }

        public void Start()
        {
            lock (stateLock)
            {
                if (!Enabled)
                {
                    kqueue = KQueue();

                    if (kqueue == -1)
                    {
                        Log.Write("MacOSTimer", $"Failed creating kqueue: {(ERRNO)Marshal.GetLastWin32Error()}", LogLevel.Error);
                        return;
                    }

                    var events = new[]
                    {
                        new KEvent
                        {
                            ident   = UIntPtr.Zero,
                            filter  = FilterType.EVFILT_TIMER,
                            flags   = Flags.EV_ADD,
                            fflags  = FilterFlags.NOTE_USECONDS | FilterFlags.NOTE_CRITICAL,
                            data    = (IntPtr)(Interval * 1000),
                            udata   = IntPtr.Zero
                        }
                    };

                    if (HandleEintr(() => KEvent(kqueue, events, events.Length, null, 0, in Unsafe.NullRef<TimeSpan>())) == -1)
                    {
                        Log.Write("MacOSTimer", $"Failed creating timer: {(ERRNO)Marshal.GetLastWin32Error()}", LogLevel.Error);
                        Close(kqueue);
                        return;
                    }

                    thread = new Thread(ThreadMain);
                    thread.IsBackground = true;
                    thread!.Start();
                    Enabled = true;
                }
            }
        }

        public void Stop()
        {
            lock (stateLock)
            {
                if (Enabled)
                {
                    SendCancelEvent();
                    thread!.Join();
                    Close(kqueue);
                    Enabled = false;
                }
            }
        }
        private void ThreadMain(object? data)
        {
            SetRealtimePolicy();
            var events = new[] { new KEvent() };

            while (true)
            {
                if (HandleEintr(() => KEvent(kqueue, null, 0, events, events.Length, in Unsafe.NullRef<TimeSpan>())) == -1)
                {
                    Log.Write("MacOSTimer", $"Failed timer: {(ERRNO)Marshal.GetLastWin32Error()}", LogLevel.Error);
                    return;
                }

                if (events[0].udata != TIMER_CANCELLED)
                {
                    Elapsed?.Invoke();
                }
                else
                {
                    break;
                }
            }
        }

        // Under the default policy, idle cores coalesce the wakeups and a busy CPU delays them, so ticks arrive late or merge.
        private void SetRealtimePolicy()
        {
            // Real-time scheduling only pays off for short intervals; above 20 ms (below 50 Hz) the default policy is
            // enough. The cap also keeps the computation (half the interval) well under the kernel's 50 ms limit.
            if (Interval > MAX_REALTIME_INTERVAL_MS)
                return;

            var period = TimeSpan.FromMilliseconds(Interval);
            var result = Mach.SetCurrentThreadTimeConstraint(period, period / 2, period);
            if (result != 0)
                Log.Write("MacOSTimer", $"Failed to set real-time thread policy: kern_return {result}", LogLevel.Warning);
        }

        private void SendCancelEvent()
        {
            var events = new[] {
                new KEvent
                {
                    ident   = UIntPtr.Zero,
                    filter  = FilterType.EVFILT_TIMER,
                    flags   = Flags.EV_ADD,
                    fflags  = FilterFlags.NOTE_MACHTIME | FilterFlags.NOTE_CRITICAL,
                    data    = IntPtr.Zero,
                    udata   = TIMER_CANCELLED
                }
            };

            if (HandleEintr(() => KEvent(kqueue, events, events.Length, null, 0, in Unsafe.NullRef<TimeSpan>())) == -1)
            {
                Log.Write("MacOSTimer", $"Failed sending cancel event: {(ERRNO)Marshal.GetLastWin32Error()}", LogLevel.Error);
            }
        }
    }
}
