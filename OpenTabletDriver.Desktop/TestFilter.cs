using System;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop
{
    [PluginName("Test Filter")]
    public class TestFilter : IPositionedPipelineElement<IDeviceReport>
    {
        public event Action<IDeviceReport> Emit;

        public void Consume(IDeviceReport value)
        {
            Emit?.Invoke(value);
        }

        [Property("Dummy Property")]
        public string Dummy { set; get; }

        public PipelinePosition Position => PipelinePosition.Raw;
    }
}