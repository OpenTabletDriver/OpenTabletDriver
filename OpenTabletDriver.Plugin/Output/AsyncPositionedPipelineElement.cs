using System;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Touch;
using OpenTabletDriver.Plugin.Timers;
using OpenTabletDriver.Plugin.Timing;

namespace OpenTabletDriver.Plugin.Output
{
    public abstract class AsyncPositionedPipelineElement<T> : IPositionedPipelineElement<T>, IDisposable
    {
        private readonly object synchronizationObject = new object();
        private HPETDeltaStopwatch consumeWatch = new HPETDeltaStopwatch(false);
        private ITimer? scheduler;
        private float? reportMsAvg;
        private float frequency;

        /// <summary>
        /// The current state of the <see cref="AsyncPositionedPipelineElement{T}"/>.
        /// </summary>
        protected T? State { set; get; }

        public event Action<T>? Emit;

        public abstract PipelinePosition Position { get; }

        [Resolved]
        public ITimer? Scheduler
        {
            set
            {
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
                    this.scheduler.Interval = 1000 / Frequency;
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
                this.frequency = value;
                if (Scheduler != null)
                {
                    if (Scheduler is { Enabled: true })
                        Scheduler.Stop();
                    Scheduler.Interval = 1000f / value;
                    Scheduler.Start();
                }
            }
            get => this.frequency;
        }

        public void Consume(T? value)
        {
            if (!FilterState(value))
            {
                if (value != null)
                    Emit?.Invoke(value);
                return;
            }

            lock (synchronizationObject)
            {
                State = value;
                var consumeDelta = (float)consumeWatch.Restart().TotalMilliseconds;
                if (consumeDelta < 150 && consumeDelta != 0)
                    reportMsAvg = (reportMsAvg + ((consumeDelta - reportMsAvg) * 0.1f)) ?? consumeDelta;
                ConsumeState();
            }
        }

        /// <summary>
        /// Sets the internal state of the <see cref="AsyncPositionedPipelineElement{T}"/> within a synchronized context
        /// to avoid race conditions against <see cref="UpdateState"/>.
        /// </summary>
        /// <remarks>
        /// This is called by <see cref="Consume"/> whenever a report is received from a linked upstream element.
        /// </remarks>
        protected abstract void ConsumeState();

        /// <summary>
        /// Updates the state of the <see cref="AsyncPositionedPipelineElement{T}"/> within a synchronized context.
        /// This is invoked by the <see cref="Scheduler"/> on the interval defined by <see cref="Frequency"/>.
        /// The implementer must invoke <see cref="OnEmit"/> to continue the input pipeline.
        /// </summary>
        /// <remarks>
        /// Call <see cref="PenIsInRange"/> to check if the pen is in range and avoid false emit.
        /// </remarks>
        protected abstract void UpdateState();

        /// <summary>
        /// Allows the implementer to filter out reports they do not want in state updates.
        /// Some reports such as <see cref="DeviceReport"/> or <see cref="ITouchReport"/> may consume a large amount of state changes and clog up <see cref="UpdateState"/>. These can be desirable to filter out.
        /// Filtered reports are not removed from the pipeline, they skip over to the next element. Similar to <see cref="OnEmit"/> but without requiring a state update.
        /// </summary>
        /// <remarks>
        /// By default, <see cref="DeviceReport"/> and <see cref="ITouchReport"/> are filtered out. Override <see cref="FilterState"/> if you need these reports.
        /// </remarks>
        protected virtual bool FilterState(T? value)
        {
            // Block DeviceReport and ITouchReport from being consumed by default
            // Avoids timer polls being consumed by tablets that spam idle reports or touch reports without any way of disabling them
            if (value is DeviceReport or ITouchReport)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if pen is in tablet hover range.
        /// </summary>
        /// <remarks>
        /// We determine that a pen is out of range when <see cref="PenIsInRange"/> is called within <see cref="UpdateState"/>
        /// after a time equivalent to a report and a half has already passed. If however, the report interval is faster than 3ms,
        /// we instead check if 3ms has already passed before declaring that the pen is out of range.
        /// </remarks>
        /// <returns>True if pen is in range</returns>
        protected bool PenIsInRange()
        {
            return (float)consumeWatch.Elapsed.TotalMilliseconds < Math.Max(3, (reportMsAvg * 1.5f) ?? float.MaxValue);
        }

        /// <summary>
        /// Invokes <see cref="Emit"/> event and transfers data to the next element.
        /// </summary>
        protected void OnEmit()
        {
            if (State != null)
                Emit?.Invoke(State);
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
                Scheduler = null;
            }

            _isDisposed = true;
        }
    }
}
