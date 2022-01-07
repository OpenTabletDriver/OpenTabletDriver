using System;
using System.Diagnostics;

namespace OpenTabletDriver.Plugin.Timing
{
    public class HPETDeltaStopwatch
    {
        public HPETDeltaStopwatch(bool startRunning = true)
        {
            isRunning = startRunning;
            start = isRunning ? internalWatch.Elapsed : default;
        }

        public static TimeSpan RuntimeElapsed => internalWatch.Elapsed;

        public TimeSpan Elapsed => isRunning ? internalWatch.Elapsed - start : end - start;

        public void Start()
        {
            if (!isRunning)
            {
                isRunning = true;
                start = internalWatch.Elapsed;
            }
        }

        public TimeSpan Restart()
        {
            if (isRunning)
            {
                var current = internalWatch.Elapsed;
                var delta = current - start;
                start = current;
                return delta;
            }
            else
            {
                var delta = end - start;
                Start();
                return delta;
            }
        }

        public TimeSpan Stop()
        {
            if (isRunning)
            {
                isRunning = false;
                end = internalWatch.Elapsed;
            }
            return end - start;
        }

        public TimeSpan Reset()
        {
            var delta = Stop();
            start = end = default;
            return delta;
        }

        private static Stopwatch internalWatch = Stopwatch.StartNew();
        protected TimeSpan start;
        protected TimeSpan end;
        protected bool isRunning;
    }
}
