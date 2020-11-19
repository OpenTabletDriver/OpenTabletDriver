using System;
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
        protected bool enabled, inRange;
        protected ITimer scheduler;
        protected DateTime lastTime = DateTime.UtcNow;
        protected readonly object stateLock = new object();

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

        protected virtual void HandleReport(IDeviceReport report)
        {
            if (report is ITabletReport tabletReport && !(report is ISyntheticReport))
            {
                if (Info.Driver.TabletIdentifier.ActiveReportID.IsInRange(tabletReport.ReportID))
                {
                    lock (this.stateLock)
                    {
                        var timeNow = DateTime.UtcNow;
                        this.reportMsAvg += ((timeNow - this.lastTime).TotalMilliseconds - this.reportMsAvg) / 50.0f;
                        this.lastTime = timeNow;
                        this.inRange = true;

                        if (Enabled)
                        {
                            UpdateState(new SyntheticTabletReport(tabletReport));
                        }
                    }
                }
                else
                {
                    this.inRange = false;
                }
            }
        }

        protected virtual void InterpolateHook()
        {
            lock (this.stateLock)
            {
                var limit = Limiter.Transform(this.reportMsAvg);
                if (((DateTime.UtcNow - this.lastTime).TotalMilliseconds < limit) && this.inRange)
                {
                    var report = Interpolate();
                    Info.Driver.InjectReport(report);
                }
                else
                {
                    this.inRange = false;
                }
            }
        }

        public virtual void Dispose()
        {
            Enabled = false;
            this.scheduler.Dispose();
        }
    }
}