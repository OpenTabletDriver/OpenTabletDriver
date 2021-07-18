using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    public class BindingState
    {
        public IBinding Binding { set; get; }
        public bool State { protected set; get; }

        protected bool PreviousState { set; get; }

        public virtual void Invoke(TabletReference tablet, IDeviceReport report, bool newState)
        {
            if (Binding is IStateBinding stateBinding)
            {
                if (newState && !PreviousState)
                    stateBinding.Press(tablet, report);
                else if (!newState && PreviousState)
                    stateBinding.Release(tablet, report);
            }

            if (Binding is IInterruptBinding interruptBinding)
            {
                interruptBinding.Invoke(tablet, report);
            }

            PreviousState = newState;
        }
    }
}