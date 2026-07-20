using System;
using System.Collections.Generic;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    /// <summary>
    /// Bindings associated with a single wheel
    /// </summary>
    // TODO: probably needs support for absolute wheels that don't start at 0
    public class WheelBindings
    {
        private readonly uint _wheelSteps;
        private readonly double _halfWheelSteps;
        private readonly double _threeHalfWheelSteps;
        private readonly double _stepsPerTick;

        public WheelBindings(WheelSpecifications wheelSpec)
        {
            uint stepCount = wheelSpec.StepCount ??
                             throw new InvalidOperationException("Could not determine step count from wheel");

            _wheelSteps = stepCount;
            _halfWheelSteps = stepCount / 2d;
            _threeHalfWheelSteps = _halfWheelSteps * 3d;
            _stepsPerTick = 360d / stepCount;
        }

        public Dictionary<int, BindingState?> WheelButtons { get; } = new();

        public DeltaThresholdBindingState? ClockwiseRotation { set; get; }
        public DeltaThresholdBindingState? CounterClockwiseRotation { set; get; }

        private uint? _lastAbsolutePosition;

        /// <summary>
        /// Invoke the appropriate <see cref="ClockwiseRotation"/> or <see cref="CounterClockwiseRotation"/>
        /// <see cref="DeltaThresholdBindingState"/> depending on consecutive position deltas.
        /// <para/>
        /// The position will be normalized to 360 degrees, so make sure that
        /// your <see cref="WheelSpecifications.StepCount"/> is correct.
        /// <para/>
        /// You must send at least 2 consecutive different positions to properly invoke
        /// the appropriate binding state.
        /// <para/>
        /// It is suggested to send <c>null</c> in <paramref name="position"/> at the end of a wheel handling "session",
        /// or manually invoke <see cref="Reset"/>
        /// </summary>
        /// <param name="tabletReference">The <see cref="TabletReference"/> to pass to the binding</param>
        /// <param name="report">The <see cref="IDeviceReport"/> to pass to the binding</param>
        /// <param name="position">The current position of the wheel, or <c>null</c> to reset deltas</param>
        public void HandleAbsoluteWheel(TabletReference tabletReference, IDeviceReport report, uint? position)
        {
            if (position == null)
            {
                Reset(); // next delta will now be invalid, reset states
                return;
            }

            if (_lastAbsolutePosition == null)
            {
                _lastAbsolutePosition = position.Value;
                // need 2 consecutive positions to compute a delta, do nothing until next wheel report
                return;
            }

            HandleWheelDelta(tabletReference, report, ComputeAbsoluteWheelDelta(_lastAbsolutePosition.Value, position.Value));

            _lastAbsolutePosition = position.Value;
        }

        /// <summary>
        /// Invoke the appropriate <see cref="ClockwiseRotation"/> or <see cref="CounterClockwiseRotation"/>
        /// <see cref="DeltaThresholdBindingState"/> depending on consecutive position deltas.
        /// <para/>
        /// It is suggested to send <c>0</c> in <paramref name="wheelSteps"/> at the end of a wheel handling "session",
        /// or manually invoke <see cref="Reset"/>
        /// </summary>
        /// <param name="tabletReference">The <see cref="TabletReference"/> to pass to the binding</param>
        /// <param name="report">The <see cref="IDeviceReport"/> to pass to the binding</param>
        /// <param name="wheelSteps">The amount of wheel steps to send to the binding, or <c>0</c> to reset deltas</param>
        public void HandleRelativeWheel(TabletReference tabletReference, IDeviceReport report, int wheelSteps) =>
            HandleWheelDelta(tabletReference, report, wheelSteps);

        /// <summary>
        /// Clear the last absolute position and any accumulated deltas in bindings
        /// </summary>
        internal void Reset()
        {
            _lastAbsolutePosition = null;
            ClockwiseRotation?.Reset();
            CounterClockwiseRotation?.Reset();
        }

        private void HandleWheelDelta(TabletReference tablet, IDeviceReport report, int counts)
        {
            switch (counts)
            {
                case > 0:
                    ClockwiseRotation?.Invoke(tablet, report, counts * _stepsPerTick);
                    CounterClockwiseRotation?.Reset();
                    break;
                case < 0:
                    CounterClockwiseRotation?.Invoke(tablet, report, counts * _stepsPerTick);
                    ClockwiseRotation?.Reset();
                    break;
                case 0:
                    Reset();
                    break;
            }
        }

        /// <summary>
        /// Compute the delta, accounting for values wrapping around
        ///
        /// Suppose a wheel with <see cref="WheelSpecifications.AbsoluteWheelMax"/> of <c>71</c>,
        /// going from <c>71</c> to <c>1</c> would equal <c>2</c> steps.
        /// </summary>
        /// <param name="from">The previous position</param>
        /// <param name="to">The current position</param>
        /// <returns>The delta between the 2 positions, accounting for wheel step counts</returns>
        private int ComputeAbsoluteWheelDelta(uint from, uint to) =>
            (int)((((int)to - from + _threeHalfWheelSteps) % _wheelSteps) - _halfWheelSteps);
    }
}
