using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    public class ThresholdBindingState : BindingState
    {
        public void Invoke(TabletReference tablet, IDeviceReport report, float value)
        {
            bool newState = value > 0;

            base.Invoke(tablet, report, newState);
        }
    }
}
