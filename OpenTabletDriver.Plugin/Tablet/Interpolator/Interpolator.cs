using System;
using System.Collections.Generic;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Timers;
using OpenTabletDriver.Plugin.Timing;

namespace OpenTabletDriver.Plugin.Tablet.Interpolator
{
    public abstract class Interpolator : IDisposable
    {
        public Interpolator(ITimer scheduler)
        {
            this.scheduler = scheduler;
            this.scheduler.Elapsed += InterpolateHook;
            Info.Driver.ReportRecieved += HandleReport;
            reportStopwatch.Start();
        }

        public abstract SyntheticTabletReport Interpolate();
        public abstract void UpdateState(SyntheticTabletReport report);

        protected double reportMsAvg = 5.0f;
        protected bool enabled;
        protected ITimer scheduler;
        protected HPETDeltaStopwatch reportStopwatch = new HPETDeltaStopwatch(true);
        protected readonly object stateLock = new object();

        protected bool inRange;
        protected bool InRange
        {
            set
            {
                if (this.inRange != value)
                {
                    this.Enabled = value;
                    this.inRange = value;
                }
            }
            get => inRange;
        }

        [Property("Hertz"), Unit("Hz"), DefaultPropertyValue(1000.0f)]
        public float Hertz { get; set; } = 1000.0f;

        public virtual bool Enabled
        {
            set
            {
                this.enabled = value;
                if (value)
                {
                    this.scheduler.Interval = 1000.0f / Hertz;
                    this.scheduler.Start();
                }
                else
                {
                    this.scheduler.Stop();
                }
            }
            get => this.enabled;
        }

        public virtual IList<IFilter> Filters { get; set; }

        protected virtual void HandleReport(object _, IDeviceReport report)
        {
            if (report is ITabletReport tabletReport && !(report is ISyntheticReport))
            {
                if (Info.Driver.Tablet.Digitizer.ActiveReportID.IsInRange(tabletReport.ReportID))
                {
                    lock (this.stateLock)
                    {
                        this.reportMsAvg += (reportStopwatch.Restart().TotalMilliseconds - reportMsAvg) / 10.0f;
                        this.InRange = true;

                        if (Enabled)
                        {
                            var synthesizedReport = new SyntheticTabletReport(tabletReport);
                            foreach (var filter in this.Filters)
                                synthesizedReport.Position = filter.Filter(synthesizedReport.Position);
                            UpdateState(synthesizedReport);
                        }
                    }
                }
                else
                {
                    this.InRange = false;
                }
            }
        }

        protected virtual void InterpolateHook()
        {
            lock (this.stateLock)
            {
                var limit = Limiter.Transform(this.reportMsAvg);
                if ((reportStopwatch.Elapsed.TotalMilliseconds < limit) && this.InRange)
                {
                    var report = Interpolate();
                    Info.Driver.HandleReport(report);
                }
                else
                {
                    this.InRange = false;
                }
            }
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                if (Enabled)
                    Enabled = false;
                this.scheduler.Elapsed -= InterpolateHook;
                Info.Driver.ReportRecieved -= HandleReport;
                this.scheduler.Dispose();
                GC.SuppressFinalize(this);
                isDisposed = true;
            }
        }

        ~Interpolator()
        {
            Dispose();
        }

        private bool isDisposed;
    }
}
