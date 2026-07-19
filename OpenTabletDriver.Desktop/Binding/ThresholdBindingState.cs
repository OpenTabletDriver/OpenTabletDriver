using System;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    public class ThresholdBindingState : BindingState
    {
        [Obsolete("Activation threshold responsibility has been moved to PressureRewriteFilter")]
        public float ActivationThreshold { set; get; }

        public void Invoke(TabletReference tablet, IDeviceReport report, float value)
        {
            bool newState = value > 0;

            base.Invoke(tablet, report, newState);
        }
    }
}
