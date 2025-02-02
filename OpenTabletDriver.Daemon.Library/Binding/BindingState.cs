using OpenTabletDriver.Daemon.Contracts.Persistence;
using OpenTabletDriver.Daemon.Library.Reflection;
using OpenTabletDriver.Platform.Pointer;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Daemon.Library.Binding
{
    public class BindingState
    {
        private readonly IBinding _binding;

        public BindingState(IPluginFactory pluginFactory, InputDevice device, IMouseButtonHandler mouseButtonHandler, PluginSettings settings)
        {
            _binding = pluginFactory.Construct<IBinding>(settings, device, mouseButtonHandler)!;
        }

        private bool _previousState;

        public void Invoke(IDeviceReport report, bool newState)
        {
            if (_binding is IStateBinding stateBinding)
            {
                if (newState && !_previousState)
                    stateBinding.Press(report);
                else if (!newState && _previousState)
                    stateBinding.Release(report);
            }

            _previousState = newState;
        }
    }
}
