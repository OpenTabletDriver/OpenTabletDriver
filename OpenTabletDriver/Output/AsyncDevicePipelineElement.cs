using System;
using System.ComponentModel;
using JetBrains.Annotations;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Output
{
    /// <summary>
    /// An asynchronous pipeline element, controlled by a hardware scheduler.
    /// </summary>
    /// <typeparam name="T">
    /// The pipeline element type.
    /// </typeparam>
    [PublicAPI]
    [PluginInterface]
    public abstract class AsyncDevicePipelineElement : IDevicePipelineElement, IDisposable
    {
        private readonly ITimer _scheduler;
        private readonly HPETDeltaStopwatch _consumeWatch = new HPETDeltaStopwatch();

        protected AsyncDevicePipelineElement(ITimer scheduler)
        {
            _scheduler = scheduler;
            _scheduler.Elapsed += () =>
            {
                lock (_synchronizationObject)
                {
                    UpdateState();
                }
            };
        }

        private readonly object _synchronizationObject = new object();
        private float? _reportMsAvg;
        private float _frequency;

        /// <summary>
        /// The current state of the <see cref="AsyncDevicePipelineElement{T}"/>.
        /// </summary>
        protected IDeviceReport? State { set; get; }

        public event Action<IDeviceReport>? Emit;

        public abstract PipelinePosition Position { get; }

        [Setting("Frequency"), Unit("hz"), DefaultValue(1000.0f)]
        public float Frequency
        {
            set
            {
                _frequency = value;
                if (_scheduler.Enabled)
                    _scheduler.Stop();
                _scheduler.Interval = 1000f / value;
                _scheduler.Start();
            }
            get => _frequency;
        }

        public void Consume(IDeviceReport value)
        {
            lock (_synchronizationObject)
            {
                State = value;
                ConsumeState();
                var consumeDelta = (float)_consumeWatch.Restart().TotalMilliseconds;
                if (consumeDelta < 150)
                    _reportMsAvg = (_reportMsAvg + ((consumeDelta - _reportMsAvg) * 0.1f)) ?? consumeDelta;
            }
        }

        /// <summary>
        /// Sets the internal state of the <see cref="AsyncDevicePipelineElement{T}"/> within a synchronized context
        /// to avoid race conditions against <see cref="UpdateState"/>.
        /// </summary>
        /// <remarks>
        /// This is called by <see cref="Consume"/> whenever a report is received from a linked upstream element.
        /// </remarks>
        protected abstract void ConsumeState();

        /// <summary>
        /// Updates the state of the <see cref="AsyncDevicePipelineElement{T}"/> within a synchronized context.
        /// This is invoked by the <see cref="Scheduler"/> on the interval defined by <see cref="Frequency"/>.
        /// The implementer must invoke <see cref="OnEmit"/> to continue the input pipeline.
        /// </summary>
        /// <remarks>
        /// Call <see cref="PenIsInRange"/> to check if the pen is in range and avoid false emit.
        /// </remarks>
        protected abstract void UpdateState();

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
            return (float)_consumeWatch.Elapsed.TotalMilliseconds < Math.Max(3, (_reportMsAvg * 1.5f) ?? float.MaxValue);
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
            _scheduler.Dispose();
        }

        ~AsyncDevicePipelineElement() => Dispose();
    }
}
