using System;
using System.Collections.Generic;
using System.Linq;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    public class BindingHandler : IPipelineElement<IDeviceReport>
    {
        public BindingHandler(IOutputMode outputMode)
        {
            this.outputMode = outputMode;

            // Force consume all reports from the last element
            var lastElement = this.outputMode.Elements?.LastOrDefault() ?? (IPipelineElement<IDeviceReport>)outputMode;
            lastElement.Emit += Consume;
        }

        public float TipActivationPressure { set; get; }
        public IBinding TipBinding { set; get; }

        public float EraserActivationPressure { set; get; }
        public IBinding EraserBinding { set; get; }

        public Dictionary<int, IBinding> PenButtonBindings { set; get; } = new Dictionary<int, IBinding>();
        public Dictionary<int, IBinding> AuxButtonBindings { set; get; } = new Dictionary<int, IBinding>();

        private bool TipState { set; get; } = false;
        private bool EraserState { set; get; } = false;
        private IList<bool> PenButtonStates { get; } = new bool[2];
        private IList<bool> AuxButtonStates { get; } = new bool[8];

        private IOutputMode outputMode;

        public event Action<IDeviceReport> Emit;

        public void Consume(IDeviceReport report)
        {
            Emit?.Invoke(report);
            HandleBinding(outputMode.Tablet, report);
        }

        public void HandleBinding(TabletReference tablet, IDeviceReport report)
        {
            if (tablet == null)
                return;

            var pen = tablet.Properties.Specifications.Pen;
            if (report is ITabletReport tabletReport && pen.ActiveReportID.IsInRange(tabletReport.ReportID))
                HandleTabletReport(tablet, pen, tabletReport);
            if (report is IAuxReport auxReport)
                HandleAuxiliaryReport(tablet, auxReport);
        }

        private void HandleTabletReport(TabletReference tablet, PenSpecifications pen, ITabletReport report)
        {
            float pressurePercent = (float)report.Pressure / (float)pen.MaxPressure * 100f;
            if (report is IEraserReport eraserReport && eraserReport.Eraser)
            {
                bool threshold = pressurePercent > EraserActivationPressure;
                InvokeBinding(tablet, report, EraserBinding, EraserState, threshold);
                EraserState = threshold;
            }
            else
            {
                bool threshold = pressurePercent > TipActivationPressure;
                InvokeBinding(tablet, report, TipBinding, TipState, threshold);
                TipState = threshold;
            }

            HandleBindingCollection(tablet, report, PenButtonStates, report.PenButtons, PenButtonBindings);
        }

        private void HandleAuxiliaryReport(TabletReference tablet, IAuxReport report)
        {
            HandleBindingCollection(tablet, report, AuxButtonStates, report.AuxButtons, AuxButtonBindings);
        }

        private void HandleBindingCollection(TabletReference tablet, IDeviceReport report, IList<bool> prevStates, IList<bool> newStates, IDictionary<int, IBinding> bindings)
        {
            for (int i = 0; i < newStates.Count; i++)
            {
                if (bindings.TryGetValue(i, out IBinding binding))
                    InvokeBinding(tablet, report, binding, prevStates[i], newStates[i]);
                prevStates[i] = newStates[i];
            }
        }

        private void InvokeBinding(TabletReference tablet, IDeviceReport report, IBinding binding, bool prevState, bool newState)
        {
            if (binding is IStateBinding stateBinding)
            {
                if (newState && !prevState)
                    stateBinding.Press(tablet, report);
                else if (!newState && prevState)
                    stateBinding.Release(tablet, report);
            }

            if (binding is IInterruptBinding interruptBinding)
            {
                interruptBinding.Invoke(tablet, report);
            }
        }
    }
}
