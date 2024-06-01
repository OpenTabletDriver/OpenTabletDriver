using OpenTabletDriver.Daemon.Contracts.Persistence;
using OpenTabletDriver.Daemon.Reflection;
using OpenTabletDriver.Platform.Pointer;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Daemon.Binding
{
    public class ThresholdBindingState : BindingState
    {
        private readonly InputDevice _device;

        public ThresholdBindingState(
            IPluginFactory pluginFactory,
            InputDevice device,
            IMouseButtonHandler mouseButtonHandler,
            PluginSettings settings
        ) : base(pluginFactory, device, mouseButtonHandler, settings)
        {
            _device = device;
        }

        public float ActivationThreshold { set; get; }

        public void Invoke(IDeviceReport report, float value)
        {
            var newState = value > ActivationThreshold;

            if (report is ITabletReport tabletReport)
            {
                if (!newState)
                {
                    tabletReport.Pressure = 0;
                }
                else // remap pressure when beyond threshold
                {
                    var maxPressure = _device.Configuration.Specifications.Pen!.MaxPressure;
                    var remappedPressure = (value - ActivationThreshold) / (100f - ActivationThreshold);
                    tabletReport.Pressure = (uint)(maxPressure * remappedPressure);
                }
            }

            base.Invoke(report, newState);
        }
    }
}
