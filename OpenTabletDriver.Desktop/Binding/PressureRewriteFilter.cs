using System;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    [PluginIgnore]
    public class PressureRewriteFilter : IPositionedPipelineElement
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

        public void Consume(scoped ref DeviceReport report)
        {
            if (report.PenPressure.HasValue && report.PenPressure.Value != MaxPenPressure)
            {
                float activationThreshold = report.Eraser.HasValue && report.Eraser.Value ? EraserPressureThreshold : TipPressureThreshold;

                float pressurePercent = report.PenPressure.Value / (float)MaxPenPressure;

                if (pressurePercent > activationThreshold)
                {
                    report.PenPressure =
                        (uint)(MaxPenPressure * ((pressurePercent - activationThreshold) / (1f - activationThreshold)));
                }
                else
                    report.PenPressure = 0;
            }

            Emit?.Invoke(ref report);
        }

        public event RefAction<DeviceReport>? Emit;
        public PipelinePosition Position => PipelinePosition.PreTransform;
    }
}
