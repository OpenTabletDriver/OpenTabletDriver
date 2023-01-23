using System;
using System.Diagnostics;
using System.Threading;

namespace OpenTabletDriver.Daemon.Interop.Timer
{
    public class FallbackTimer : ITimer
    {
        private Thread? _threadTimer;
        private bool _runTimer = true;
        private float _ignoreEventIfLateBy = float.MaxValue;

        public float Interval { get; set; }
        public bool Enabled => _threadTimer is { IsAlive: true };

        public event Action? Elapsed;

        public void Start()
        {
            if (Enabled || Interval <= 0)
                return;

            _runTimer = true;

            _ignoreEventIfLateBy = Interval * 2;

            _threadTimer = new Thread(ThreadMain)
            {
                Priority = ThreadPriority.AboveNormal
            };
            _threadTimer.Start();
        }

        public void Stop()
        {
            _runTimer = false;
            _threadTimer!.Join();
        }

        private void ThreadMain()
        {
            float nextNotification = 0;
            float elapsedMilliseconds;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            while (_runTimer)
            {
                nextNotification += Interval;

                while ((elapsedMilliseconds = (float)stopWatch.Elapsed.TotalMilliseconds)
                        < nextNotification)
                {
                    Thread.Yield();
                }

                if (elapsedMilliseconds - nextNotification >= _ignoreEventIfLateBy)
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
