using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    public class BindingState
    {
        public required IStateBinding Binding { get; init; }

        protected bool PreviousState { set; get; }

        public bool RequiresPenPressure { set; get; } // "drag bindings"

        public void Invoke(TabletReference tablet, IDeviceReport report, bool newState)
        {
            // Check if drag binding is needed or unnecessary
            // NOTE: report _must_ include pressure to work properly
            // TODO: use relevant threshold instead of '0'
            bool pressureThresholdIsMetOrUnneeded =
                !RequiresPenPressure || (RequiresPenPressure && report is ITabletReport { Pressure: > 0 });

            if (newState && !PreviousState)
            {
                if (pressureThresholdIsMetOrUnneeded)
                    Binding.Press(tablet, report);
            }
            else if (!newState && PreviousState)
                Binding.Release(tablet, report);

            if (!newState || pressureThresholdIsMetOrUnneeded) // don't update state to true without threshold
                PreviousState = newState;
        }
    }
}
