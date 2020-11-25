using System;
using System.Diagnostics;
using System.Threading;
using OpenTabletDriver.Plugin.Timers;

namespace OpenTabletDriver.Interop.Timer
{
    internal class FallbackTimer : ITimer, IDisposable
    {
        public event Action Elapsed;

        private Thread threadTimer;
        private float ignoreEventIfLateBy = float.MaxValue;
        private float timerInterval;
        private bool runTimer = true;

        public FallbackTimer()
        {
        }

        public float Interval
        {
            get => Interlocked.Exchange(
                    ref this.timerInterval, this.timerInterval);
            set => Interlocked.Exchange(
                    ref this.timerInterval, value);
        }

        public float IgnoreEventIfLateBy
        {
            get => Interlocked.Exchange(
                    ref this.ignoreEventIfLateBy, this.ignoreEventIfLateBy);
            set => Interlocked.Exchange(
                    ref this.ignoreEventIfLateBy, value <= 0 ? float.MaxValue : value);
        }

        public bool Enabled => this.threadTimer != null && this.threadTimer.IsAlive;

        public void Start()
        {
            if (Enabled || Interval <= 0)
            {
                return;
            }

            this.runTimer = true;

            this.threadTimer = new Thread(ThreadMain)
            {
                Priority = ThreadPriority.Highest
            };
            this.threadTimer.Start();
        }

        public void Stop()
        {
            this.runTimer = false;
        }

        void ThreadMain()
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
                    Thread.Sleep(0);
                }

                float timerLateBy = elapsedMilliseconds - nextNotification;

                if (timerLateBy >= IgnoreEventIfLateBy)
                {
                    continue;
                }

                Elapsed();
            }

            stopWatch.Stop();
        }

        public void Dispose()
        {
        }
    }
}