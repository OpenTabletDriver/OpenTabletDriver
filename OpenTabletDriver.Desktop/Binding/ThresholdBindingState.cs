using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    public class ThresholdBindingState : BindingState
    {
        public float ActivationThreshold { set; get; }

        public void Invoke(TabletReference tablet, IDeviceReport report, float value)
        {
            bool newState = value > ActivationThreshold;
            base.Invoke(tablet, report, newState);
        }
    }
}