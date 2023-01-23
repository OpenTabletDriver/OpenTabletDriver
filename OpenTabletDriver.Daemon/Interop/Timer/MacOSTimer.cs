using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using OpenTabletDriver.Native.Linux;
using OpenTabletDriver.Native.MacOS.Timers;
using static OpenTabletDriver.Daemon.Interop.Posix.Utility;
using static OpenTabletDriver.Native.MacOS.Timers.Timers;

namespace OpenTabletDriver.Daemon.Interop.Timer
{
    internal class MacOSTimer : ITimer, IDisposable
    {
        const int TIMER_CANCELLED = 1;

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
