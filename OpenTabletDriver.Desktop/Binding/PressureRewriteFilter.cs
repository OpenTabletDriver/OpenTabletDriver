using System;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    [PluginIgnore]
    public class PressureRewriteFilter : IPositionedPipelineElement<IDeviceReport>
    {
        public required float TipPressureThreshold
        {
            get;
            init => field = value / 100;
        }

        public required float EraserPressureThreshold
        {
            get;
            init => field = value / 100;
        }

        public required uint MaxPenPressure { get; init; }

        public void Consume(IDeviceReport? report)
        {
            if (report is ITabletReport tabletReport && tabletReport.Pressure != MaxPenPressure)
            {
                float activationThreshold = report is IEraserReport ? EraserPressureThreshold : TipPressureThreshold;

                float pressurePercent = tabletReport.Pressure / (float)MaxPenPressure;

                if (pressurePercent > activationThreshold)
                {
                    tabletReport.Pressure =
                        (uint)(MaxPenPressure * ((pressurePercent - activationThreshold) / (1f - activationThreshold)));
                }
                else
                    tabletReport.Pressure = 0;
            }

            Emit?.Invoke(report);
        }

        public event Action<IDeviceReport?>? Emit;
        public PipelinePosition Position => PipelinePosition.PreTransform;
    }
}
