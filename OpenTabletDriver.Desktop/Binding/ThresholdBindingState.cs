using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    public class ThresholdBindingState : BindingState
    {
        public float ActivationThreshold { set; get; }

        // TODO: Maximum is magic number 100f because this type of binding is currently only exposed as a FloatSlider
        //    Ideally this is fixed by telling the client the maximum value allowed for fields, somehow
        private const float MaximumThreshold = 100f;

        public void Invoke(TabletReference tablet, IDeviceReport report, float value)
        {
            bool newState = value > ActivationThreshold;

            // account for special case where threshold == max value
            // ReSharper disable CompareOfFloatsByEqualityOperator
            bool valueMaxed = ActivationThreshold == MaximumThreshold && value == MaximumThreshold;
            // ReSharper restore CompareOfFloatsByEqualityOperator

            bool meetsThreshold = newState || valueMaxed;

            if (report is ITabletReport tabletReport)
            {
                if (meetsThreshold)
                {
                    uint maxPressure = tablet.Properties.Specifications.Pen.MaxPressure;

                    if (valueMaxed)
                        tabletReport.Pressure = maxPressure;
                    else // remap pressure based on the amount we went above the activation threshold
                        tabletReport.Pressure =
                            (uint)(maxPressure * ((value - ActivationThreshold) / (MaximumThreshold - ActivationThreshold)));
                }
                else
                    tabletReport.Pressure = 0;
            }

            base.Invoke(tablet, report, meetsThreshold);
        }
    }
}
