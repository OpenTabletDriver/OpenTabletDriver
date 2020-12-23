using System;
using System.Diagnostics;

namespace OpenTabletDriver.Plugin.Utils
{
    public class DeltaStopwatch
    {
        public DeltaStopwatch(bool startRunning = true)
        {
            isRunning = startRunning;
            start = isRunning ? internalWatch.Elapsed : default;
        }

        public static TimeSpan RunitmeElapsed => internalWatch.Elapsed;
        public static double RuntimeElapsedMs => RunitmeElapsed.TotalMilliseconds;

        public TimeSpan Elapsed => isRunning ? internalWatch.Elapsed - start : end - start;
        public double ElapsedMs => Elapsed.TotalMilliseconds;

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
        public double RestartMs() => Restart().TotalMilliseconds;

        public TimeSpan Stop()
        {
            if (isRunning)
            {
                isRunning = false;
                end = internalWatch.Elapsed;
            }
            return end - start;
        }
        public double StopMs() => Stop().TotalMilliseconds;

        public TimeSpan Reset()
        {
            var delta = Stop();
            start = end = default;
            return delta;
        }
        public double ResetMs() => Reset().TotalMilliseconds;

        private static Stopwatch internalWatch = Stopwatch.StartNew();
        protected TimeSpan start;
        protected TimeSpan end;
        protected bool isRunning;
    }
}