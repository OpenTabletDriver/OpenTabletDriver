using System;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Timers;

namespace OpenTabletDriver.Plugin.Output
{
    public abstract class AsyncPositionedPipelineElement<T> : IPositionedPipelineElement<T>, IDisposable
    {
        private ITimer scheduler;
        private float frequency;

        [Resolved]
        public ITimer Scheduler
        {
            set
            {
                this.scheduler = value;
                this.scheduler.Elapsed += OnEmit;
                this.scheduler.Start();
            }
            get => this.scheduler;
        }

        public abstract PipelinePosition Position { get; }

        [PropertyAttribute("Frequency"), UnitAttribute("hz"), DefaultPropertyValue(1000.0f)]
        public float Frequency
        {
            set
            {
                this.frequency = value;
                this.scheduler.Interval = 1000.0f / value;
            }
            get => this.frequency;
        }

        /// <summary>
        /// The current state of the <see cref="AsyncPositionedPipelineElement{T}"/>
        /// </summary>
        protected T State { set; get; }

        public event Action<T> Emit;

        /// <summary>
        /// Sets the internal state.
        /// </summary>
        /// <param name="value"></param>
        public virtual void Consume(T value)
        {
            State = value;
        }

        /// <summary>
        /// Invoked to emit a report.
        /// This is invoked by the <see cref="Scheduler"/> on the interval defined by <see cref="Frequency"/>.
        /// </summary>
        protected virtual void OnEmit()
        {
            Emit?.Invoke(State);
        }

        public void Dispose()
        {
            Scheduler?.Dispose();
            Scheduler = null;
        }

        ~AsyncPositionedPipelineElement() => Dispose();
    }
}