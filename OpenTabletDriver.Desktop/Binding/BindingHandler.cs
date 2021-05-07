using System.Collections.Generic;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    public class BindingHandler
    {
        public float TipActivationPressure { set; get; }
        public IBinding TipBinding { set; get; } = null;

        public float EraserActivationPressure { set; get; }
        public IBinding EraserBinding { set; get; } = null;

        public Dictionary<int, IBinding> PenButtonBindings { set; get; } = new Dictionary<int, IBinding>();
        public Dictionary<int, IBinding> AuxButtonBindings { set; get; } = new Dictionary<int, IBinding>();

        private bool TipState;
        private bool EraserState;
        private IList<bool> PenButtonStates = new bool[2];
        private IList<bool> AuxButtonStates = new bool[8];

        public void HandleBinding(TabletState tablet, IDeviceReport report)
        {
            if (tablet == null)
                return;

            var pen = tablet.Properties.Specifications.Pen;
            if (report is ITabletReport tabletReport && pen.ActiveReportID.IsInRange(tabletReport.ReportID))
                HandleTabletReport(pen, tabletReport);
            if (report is IAuxReport auxReport)
                HandleAuxiliaryReport(auxReport);
        }

        private void HandleTabletReport(PenSpecifications pen, ITabletReport report)
        {
            float pressurePercent = (float)report.Pressure / (float)pen.MaxPressure * 100f;
            if (report is IEraserReport eraserReport && eraserReport.Eraser)
            {
                bool threshold = pressurePercent > EraserActivationPressure;
                InvokeBinding(report, EraserBinding, EraserState, threshold);
                EraserState = threshold;
            }
            else
            {
                bool threshold = pressurePercent > TipActivationPressure;
                InvokeBinding(report, TipBinding, TipState, threshold);
                TipState = threshold;
            }

            HandleBindingCollection(report, PenButtonStates, report.PenButtons, PenButtonBindings);
        }

        private void HandleAuxiliaryReport(IAuxReport report)
        {
            HandleBindingCollection(report, AuxButtonStates, report.AuxButtons, AuxButtonBindings);
        }

        private void HandleBindingCollection(IDeviceReport report, IList<bool> prevStates, IList<bool> newStates, IDictionary<int, IBinding> bindings)
        {
            for (int i = 0; i < newStates.Count; i++)
            {
                if (bindings.TryGetValue(i, out IBinding binding))
                    InvokeBinding(report, binding, prevStates[i], newStates[i]);
                prevStates[i] = newStates[i];
            }
        }

        private static void InvokeBinding(IDeviceReport report, IBinding binding, bool prevState, bool newState)
        {
            if (binding != null)
            {
                if (newState && !prevState)
                    binding.Press(report);
                else if (!newState && prevState)
                    binding.Release(report);
            }
        }
    }
}
