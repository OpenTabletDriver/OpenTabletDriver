using System;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Timers;
using OpenTabletDriver.Plugin.Timing;

namespace OpenTabletDriver.Plugin.Output
{
    public abstract class AsyncPositionedPipelineElement<T> : IPositionedPipelineElement<T>, IDisposable
    {
        private readonly object synchronizationObject = new object();
        private HPETDeltaStopwatch consumeWatch = new HPETDeltaStopwatch();
        private ITimer scheduler;
        private float? reportMsAvg;
        private float frequency;

        /// <summary>
        /// The current state of the <see cref="AsyncPositionedPipelineElement{T}"/>
        /// </summary>
        protected T State { set; get; }

        public event Action<T> Emit;

        public abstract PipelinePosition Position { get; }

        [Resolved]
        public ITimer Scheduler
        {
            set
            {
                this.scheduler = value;
                this.scheduler.Elapsed += OnTimerElapsed;
                this.scheduler.Start();
            }
            get => this.scheduler;
        }

        [Property("Frequency"), Unit("hz"), DefaultPropertyValue(1000.0f)]
        public float Frequency
        {
            set
            {
                this.frequency = value;
                if (Scheduler.Enabled)
                    Scheduler.Stop();
                Scheduler.Interval = 1000f / value;
                Scheduler.Start();
            }
            get => this.frequency;
        }

        public void Consume(T value)
        {
            lock (synchronizationObject)
            {
                State = value;
                var consumeDelta = (float)consumeWatch.Restart().TotalMilliseconds;
                if (consumeDelta < 150)
                    reportMsAvg += ((consumeDelta - reportMsAvg) * 0.1f) ?? consumeDelta;
            }
        }

        /// <summary>
        /// Sets the internal state.
        /// </summary>
        /// <param name="value"></param>
        protected abstract void ConsumeState();

        /// <summary>
        /// Updates the state of the <see cref="AsyncPositionedPipelineElement{T}"/> within a synchronized context.
        /// This is invoked by the <see cref="Scheduler"/> on the interval defined by <see cref="Frequency"/>.
        /// The implementer must invoke <see cref="Emit"/> to continue the input processing.
        /// </summary>
        /// <remarks>
        /// Call <see cref="PenIsInRange"/> to check if the pen is in range and avoid false emit.
        /// </remarks>
        protected abstract void UpdateState();

        /// <summary>
        /// Determines if pen is in tablet hover range
        /// </summary>
        /// <returns><see cref="true"/> if pen is in range</returns>
        protected bool PenIsInRange()
        {
            return (float)consumeWatch.Elapsed.TotalMilliseconds < Math.Max(3, (reportMsAvg * 1.5f) ?? float.MaxValue);
        }

        private void OnTimerElapsed()
        {
            lock (synchronizationObject)
            {
                UpdateState();
            }
        }

        public void Dispose()
        {
            Scheduler?.Dispose();
            Scheduler = null;
        }

        ~AsyncPositionedPipelineElement() => Dispose();
    }
}