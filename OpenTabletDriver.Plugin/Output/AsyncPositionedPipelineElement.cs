using System;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Timers;
using OpenTabletDriver.Plugin.Timing;

namespace OpenTabletDriver.Plugin.Output
{
    public abstract class AsyncPositionedPipelineElement : IPositionedPipelineElement, IDisposable
    {
        private readonly object synchronizationObject = new object();
        private HPETDeltaStopwatch consumeWatch = new HPETDeltaStopwatch(false);
        private ITimer? scheduler;
        private float? reportMsAvg;
        private float frequency;

        public event RefAction<DeviceReport>? Emit;

        public abstract PipelinePosition Position { get; }

        [Resolved]
        public ITimer? Scheduler
        {
            set
            {
                if (this.scheduler != null) throw new NotSupportedException("Changing a timer after it has already been set is not supported yet");

                this.scheduler = value;

                if (this.scheduler != null)
                {
                    this.scheduler.Elapsed += () =>
                    {
                        lock (synchronizationObject)
                        {
                            UpdateState();
                        }
                    };
                    this.scheduler.Start();
                }
            }
            get => this.scheduler;
        }

        [Property("Frequency"), Unit("hz"), DefaultPropertyValue(1000.0f)]
        public float Frequency
        {
            set
            {
                if (value < 101)
                    Log.Write("Timer", "Low frequency detected, Output may appear choppy. please use 1000hz, 500hz, 250hz or 125hz instead.", LogLevel.Warning);

                this.frequency = MathF.Max(1, value);
                if (Scheduler != null)
                {
                    if (Scheduler is { Enabled: true })
                        Scheduler.Stop();
                    Scheduler.Interval = 1000f / this.frequency;
                    Scheduler.Start();
                }
            }
            get => this.frequency;
        }

        public void Consume(scoped ref DeviceReport report)
        {
            if (!FilterState(in report))
            {
                Emit?.Invoke(ref report);
                return;
            }

            lock (synchronizationObject)
            {
                var consumeDelta = (float)consumeWatch.Restart().TotalMilliseconds;
                if (consumeDelta < 150 && consumeDelta != 0)
                    reportMsAvg = (reportMsAvg + ((consumeDelta - reportMsAvg) * 0.1f)) ?? consumeDelta;
                ConsumeState(ref report);
            }
        }

        /// <summary>
        /// Sets the internal state of the <see cref="AsyncPositionedPipelineElement"/> within a synchronized context
        /// to avoid race conditions against <see cref="UpdateState"/>.
        /// </summary>
        /// <remarks>
        /// This is called by <see cref="Consume"/> whenever a report is received from a linked upstream element.
        /// </remarks>
        protected abstract void ConsumeState(scoped ref DeviceReport report);

        /// <summary>
        /// Updates the state of the <see cref="AsyncPositionedPipelineElement"/> within a synchronized context.
        /// This is invoked by the <see cref="Scheduler"/> on the interval defined by <see cref="Frequency"/>.
        /// The implementer must invoke <see cref="OnEmit"/> to continue the input pipeline.
        /// </summary>
        /// <remarks>
        /// Call <see cref="PenIsInRange"/> to check if the pen is in range and avoid false emit.
        /// </remarks>
        protected abstract void UpdateState();

        /// <summary>
        /// Allows the implementer to filter out reports they do not want in state updates.
        /// Some reports such as blocked report or touch reports may consume a large amount of state changes and clog up <see cref="UpdateState"/>. These can be desirable to filter out.
        /// Filtered reports are not removed from the pipeline, they skip over to the next element. Similar to <see cref="OnEmit"/> but without requiring a state update.
        /// </summary>
        /// <remarks>
        /// By default, <see cref="DeviceReport.Continue"/> false and reports with touches are filtered out. Override <see cref="FilterState"/> if you need these reports.
        /// </remarks>
        protected virtual bool FilterState(scoped in DeviceReport report)
        {
            // Block request-stopped reports and touch report from being consumed by default
            // Avoids timer polls being consumed by tablets that spam idle reports or touch reports without any way of disabling them
            return report.Continue && report.Touches == null;
        }

        /// <summary>
        /// Determines if pen is in tablet hover range.
        /// </summary>
        /// <remarks>
        /// We determine that a pen is out of range when <see cref="PenIsInRange"/> is called within <see cref="UpdateState"/>
        /// after a time equivalent to a report and a half has already passed. If however, the report interval is faster than 3ms,
        /// we instead check if 3ms has already passed before declaring that the pen is out of range.
        /// <para/>
        /// TODO: This doesn't account for out of range reports
        /// </remarks>
        /// <returns>True if pen is in range</returns>
        protected bool PenIsInRange()
        {
            return (float)consumeWatch.Elapsed.TotalMilliseconds < Math.Max(3, (reportMsAvg * 1.5f) ?? float.MaxValue);
        }

        /// <summary>
        /// Invokes <see cref="Emit"/> event and transfers data to the next element.
        /// </summary>
        protected void OnEmit(scoped ref DeviceReport report)
        {
            Emit?.Invoke(ref report);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _isDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                Scheduler?.Dispose();
                scheduler = null; // set backing field directly to dodge the fake-readonly setter
            }

            _isDisposed = true;
        }
    }
}
