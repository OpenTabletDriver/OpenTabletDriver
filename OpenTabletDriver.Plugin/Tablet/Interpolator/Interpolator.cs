using System;
using System.Collections.Generic;
using System.Linq;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Timers;

namespace OpenTabletDriver.Plugin.Tablet.Interpolator
{
    public abstract class Interpolator : IDisposable
    {
        public Interpolator(ITimer scheduler)
        {
            this.scheduler = scheduler;
            this.scheduler.Elapsed += InterpolateHook;
            Info.Driver.ReportRecieved += HandleReport;
        }

        public abstract SyntheticTabletReport Interpolate();
        public abstract void UpdateState(SyntheticTabletReport report);

        protected double reportMsAvg = 5.0f;
        protected bool enabled;
        protected IList<IFilter> filters;
        protected ITimer scheduler;
        protected DateTime lastTime = DateTime.UtcNow;
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

        [Property("Hertz"), Unit("hz")]
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

        public virtual IEnumerable<IFilter> Filters
        {
            set => this.filters = value.ToArray();
            get => this.filters;
        }

        protected virtual void HandleReport(object _, IDeviceReport report)
        {
            if (report is ITabletReport tabletReport && !(report is ISyntheticReport))
            {
                if (Info.Driver.Tablet.Digitizer.ActiveReportID.IsInRange(tabletReport.ReportID))
                {
                    lock (this.stateLock)
                    {
                        var timeNow = DateTime.UtcNow;
                        this.reportMsAvg += ((timeNow - this.lastTime).TotalMilliseconds - this.reportMsAvg) / 50.0f;
                        this.lastTime = timeNow;
                        this.InRange = true;

                        if (Enabled)
                        {
                            var synthesizedReport = new SyntheticTabletReport(tabletReport);
                            foreach (var filter in this.filters)
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
                if (((DateTime.UtcNow - this.lastTime).TotalMilliseconds < limit) && this.InRange)
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
