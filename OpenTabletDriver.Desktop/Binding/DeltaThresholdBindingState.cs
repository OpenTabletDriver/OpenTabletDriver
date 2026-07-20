using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    /// <summary>
    /// A <see cref="BindingState"/> type for handling directional gadgets (wheels/strips).
    /// <para/>
    /// Call <see cref="Invoke"/> with your deltas, and call <see cref="Reset"/> to reset saved deltas as necessary.
    /// </summary>
    public class DeltaThresholdBindingState : BindingState
    {
        public float ActivationThreshold { set; get; }
        public bool IsNegativeThreshold { set; get; }

        private double _accumulatedDelta;

        public void Invoke(TabletReference tablet, IDeviceReport report, double delta)
        {
            _accumulatedDelta += delta;

            while (IsNegativeThreshold ? _accumulatedDelta <= -ActivationThreshold : _accumulatedDelta >= ActivationThreshold)
            {
                _accumulatedDelta -= IsNegativeThreshold ? -ActivationThreshold : ActivationThreshold;

                base.Invoke(tablet, report, true);
                base.Invoke(tablet, report, false);
            }
        }

        public void Reset()
        {
            _accumulatedDelta = 0;
        }
    }
}
