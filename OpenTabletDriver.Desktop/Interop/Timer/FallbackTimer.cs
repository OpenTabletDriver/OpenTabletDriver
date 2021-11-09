using System;
using System.Diagnostics;
using System.Threading;
using OpenTabletDriver.Plugin.Timers;

namespace OpenTabletDriver.Desktop.Interop.Timer
{
    internal class FallbackTimer : ITimer, IDisposable
    {
        private Thread threadTimer;
        private bool runTimer = true;

        public float Interval { get; set; }
        public float IgnoreEventIfLateBy { get; set; } = float.MaxValue;
        public bool Enabled => this.threadTimer != null && this.threadTimer.IsAlive;

        public event Action Elapsed;

        public void Start()
        {
            if (Enabled || Interval <= 0)
                return;

            this.runTimer = true;

            this.IgnoreEventIfLateBy = Interval * 2;

            this.threadTimer = new Thread(ThreadMain)
            {
                Priority = ThreadPriority.AboveNormal
            };
            this.threadTimer.Start();
        }

        public void Stop()
        {
            this.runTimer = false;
            this.threadTimer.Join();
        }

        private void ThreadMain()
        {
            float nextNotification = 0;
            float elapsedMilliseconds;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            while (this.runTimer)
            {
                nextNotification += Interval;

                while ((elapsedMilliseconds = (float)stopWatch.Elapsed.TotalMilliseconds)
                        < nextNotification)
                {
                    Thread.Yield();
                }

                if (elapsedMilliseconds - nextNotification >= IgnoreEventIfLateBy)
                    continue;
                Elapsed?.Invoke();
            }

            stopWatch.Stop();
        }

        public void Dispose()
        {
            if (Enabled)
                Stop();
        }
    }
}
