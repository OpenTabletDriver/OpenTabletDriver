using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    public class RangeBindingState : BindingState
    {
        public float StartThreshold { set; get; }
        public float EndThreshold { set; get; }

        public void Invoke(TabletReference tablet, IDeviceReport report, float value)
        {
            bool newState = value >= StartThreshold && value <= EndThreshold;

            if (report is ITabletReport tabletReport)
            {
                if (!newState)
                {
                    tabletReport.Pressure = 0;
                }
                else // remap pressure when beyond threshold
                {
                    var maxPressure = tablet.Properties.Specifications.Pen.MaxPressure;
                    var remappedPressure = (value - StartThreshold) / (100f - StartThreshold);
                    tabletReport.Pressure = (uint)(maxPressure * remappedPressure);
                }
            }

            base.Invoke(tablet, report, newState);
        }

        public override string ToString()
            => $"{Binding}: {StartThreshold}-{EndThreshold}";
    }
}