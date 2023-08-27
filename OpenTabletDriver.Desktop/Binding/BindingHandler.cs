using System;
using System.Collections.Generic;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;

#nullable enable

namespace OpenTabletDriver.Desktop.Binding
{
    [PluginIgnore]
    public class BindingHandler : IPositionedPipelineElement<IDeviceReport>
    {
        public BindingHandler(TabletReference tablet)
        {
            this.tablet = tablet;
        }

        public ThresholdBindingState? Tip { set; get; }
        public ThresholdBindingState? Eraser { set; get; }

        public Dictionary<int, BindingState?> PenButtons { set; get; } = new Dictionary<int, BindingState?>();
        public Dictionary<int, BindingState?> AuxButtons { set; get; } = new Dictionary<int, BindingState?>();
        public Dictionary<int, BindingState?> MouseButtons { set; get; } = new Dictionary<int, BindingState?>();

        public BindingState? MouseScrollDown { set; get; }
        public BindingState? MouseScrollUp { set; get; }

        public PipelinePosition Position => PipelinePosition.PostTransform;

        private readonly TabletReference tablet;

        public event Action<IDeviceReport>? Emit;

        public void Consume(IDeviceReport report)
        {
            HandleBinding(report);
            Emit?.Invoke(report);
        }

        public void HandleBinding(IDeviceReport report)
        {
            if (report is ITabletReport tabletReport)
                HandleTabletReport(tablet, tablet.Properties.Specifications.Pen, tabletReport);
            if (report is IAuxReport auxReport)
                HandleAuxiliaryReport(tablet, auxReport);
            if (report is IMouseReport mouseReport)
                HandleMouseReport(tablet, mouseReport);
        }

        private void HandleTabletReport(TabletReference tablet, PenSpecifications pen, ITabletReport report)
        {
            float pressurePercent = (float)report.Pressure / (float)pen.MaxPressure * 100f;
            if (report is IEraserReport eraserReport && eraserReport.Eraser)
                Eraser?.Invoke(tablet, report, pressurePercent);
            else
                Tip?.Invoke(tablet, report, pressurePercent);

            HandleBindingCollection(tablet, report, PenButtons, report.PenButtons);
        }

        private void HandleAuxiliaryReport(TabletReference tablet, IAuxReport report)
        {
            HandleBindingCollection(tablet, report, AuxButtons, report.AuxButtons);
        }

        private void HandleMouseReport(TabletReference tablet, IMouseReport report)
        {
            HandleBindingCollection(tablet, report, MouseButtons, report.MouseButtons);

            MouseScrollDown?.Invoke(tablet, report, report.Scroll.Y < 0);
            MouseScrollUp?.Invoke(tablet, report, report.Scroll.Y > 0);
        }

        private static void HandleBindingCollection(TabletReference tablet, IDeviceReport report, IDictionary<int, BindingState?> bindings, IList<bool> newStates)
        {
            for (int i = 0; i < newStates.Count; i++)
            {
                if (bindings.TryGetValue(i, out var binding))
                    binding?.Invoke(tablet, report, newStates[i]);
            }
        }
    }
}
