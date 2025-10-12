using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    public class DeltaThresholdBindingState : BindingState
    {
        public float ActivationThreshold { set; get; }
        public bool IsNegativeThreshold { set; get; }

        public void Invoke(TabletReference tablet, IDeviceReport report, ref float delta)
        {
            bool newState = IsNegativeThreshold ? delta < ActivationThreshold : delta > ActivationThreshold;

            if (newState)
                delta -= ActivationThreshold;

            base.Invoke(tablet, report, newState);
        }
    }
}
