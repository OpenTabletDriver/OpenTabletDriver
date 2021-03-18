using System;
using System.Numerics;
using OpenTabletDriver.Plugin.Output.Interpolator;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Timers;
using OpenTabletDriver.Plugin.Timing;

namespace OpenTabletDriver.Plugin.Output.Async
{
    public sealed class AsyncFilterHandler : IDisposable
    {
        public AsyncFilterHandler(ITimer scheduler)
        {
            this.scheduler = scheduler;
            this.scheduler.Elapsed += SchedulerFunc;
        }

        private object asyncLock = new object();
        private ITimer scheduler;
        private HPETDeltaStopwatch asyncWatch = new HPETDeltaStopwatch(true);
        private Action<ITabletReport> output;
        private double reportMsAvg = 10.0;
        private float frequency = 1000;
        private bool penInRange, isDisposed;

        public bool Enabled { get; set; }
        public bool PenInRange
        {
            set
            {
                if (penInRange != value)
                {
                    penInRange = value;

                    if (penInRange)
                        scheduler.Start();
                    else
                        scheduler.Stop();
                }
            }
            get => penInRange;
        }

        public IAsyncFilter AsyncFilter { get; set; }
        public IFilter[] PreAsyncFilters { get; set; }
        public float Frequency
        {
            set
            {
                frequency = value;
                this.scheduler.Interval = 1000.0f / frequency;
            }
            get => frequency;
        }

        public void Bind(Action<ITabletReport> function)
        {
            output = function;
        }

        public void Process(ITabletReport report)
        {
            lock (asyncLock)
            {
                var reportSpan = asyncWatch.Restart();
                reportMsAvg += (reportSpan.TotalMilliseconds - reportMsAvg) / 10.0;
                PenInRange = true;

                if (Enabled)
                {
                    var synthesizedReport = new SyntheticTabletReport(report);
                    synthesizedReport.Position = ApplyPreAsyncFilters(synthesizedReport.Position);
                    AsyncFilter.UpdateState(synthesizedReport, reportSpan);
                }
            }
        }

        public Vector2 ApplyPreAsyncFilters(Vector2 pos)
        {
            foreach (var filter in this.PreAsyncFilters)
                pos = filter.Filter(pos);
            return pos;
        }

        private void SchedulerFunc()
        {
            lock (asyncLock)
            {
                var reportSpan = asyncWatch.Elapsed;
                if (WithinDelayTolerance(reportSpan.TotalMilliseconds, reportMsAvg) && PenInRange)
                {
                    var report = AsyncFilter.Filter(reportSpan);
                    output(report);
                }
                else
                {
                    PenInRange = false;
                }
            }
        }

        public static bool WithinDelayTolerance(double reportMs, double reportMsAvg)
        {
            return reportMs < Math.Max(3, reportMsAvg * 1.5);
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                lock (asyncLock)
                {
                    if (Enabled)
                        Enabled = false;
                    scheduler.Elapsed -= SchedulerFunc;
                    scheduler.Dispose();

                    try
                    {
                        if (AsyncFilter is IDisposable asyncFilter)
                            asyncFilter.Dispose();
                    }
                    catch
                    {
                        Log.Write("Plugin", $"Unable to dispose object '{AsyncFilter.GetType().Name}'", LogLevel.Error);
                    }
                    GC.SuppressFinalize(this);
                    isDisposed = true;
                }
            }
        }

        ~AsyncFilterHandler()
        {
            Dispose();
        }
    }
}