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

            base.Invoke(tablet, report, newState);
        }

        public override string ToString()
            => $"{Binding}: {StartThreshold}-{EndThreshold}";
    }
}
