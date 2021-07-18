using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    public class ThresholdBindingState : BindingState
    {
        public float ActivationThreshold { set; get; }

        public void Invoke(TabletReference tablet, IDeviceReport report, float value)
        {
            bool newState = value > ActivationThreshold;

            if (report is ITabletReport tabletReport)
            {
                if (!newState)
                {
                    tabletReport.Pressure = 0;
                }
                else // remap pressure when beyond threshold
                {
                    var maxPressure = tablet.Properties.Specifications.Pen.MaxPressure;
                    var remappedPressure = (value - ActivationThreshold) / (100f - ActivationThreshold);
                    tabletReport.Pressure = (uint)(maxPressure * remappedPressure);
                }
            }

            base.Invoke(tablet, report, newState);
        }
    }
}
