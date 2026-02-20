using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    public class BindingState
    {
        public IBinding Binding { set; get; }

        protected bool PreviousState { set; get; }

        public bool RequiresPenPressure { set; get; } // "drag bindings"

        public virtual void Invoke(TabletReference tablet, IDeviceReport report, bool newState)
        {
            // Check if drag binding is needed or unnecessary
            // NOTE: report _must_ include pressure to work properly
            // TODO: use relevant threshold instead of '0'
            bool pressureThresholdIsMetOrUnneeded =
                !RequiresPenPressure || (RequiresPenPressure && report is ITabletReport { Pressure: > 0 });

            if (Binding is IStateBinding stateBinding)
            {
                if (newState && !PreviousState)
                {
                    if (pressureThresholdIsMetOrUnneeded)
                        stateBinding.Press(tablet, report);
                }
                else if (!newState && PreviousState)
                    stateBinding.Release(tablet, report);
            }
            else if (Binding == null)
                Log.Write(nameof(BindingState), $"Binding state not set for binding associated with {tablet}", LogLevel.Warning);

            if (!newState || pressureThresholdIsMetOrUnneeded) // don't update state to true without threshold
                PreviousState = newState;
        }
    }
}
