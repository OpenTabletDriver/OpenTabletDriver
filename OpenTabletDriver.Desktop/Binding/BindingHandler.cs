using System.Collections.Generic;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    public static class BindingHandler
    {
        public static float TipActivationPressure { set; get; }
        public static IBinding TipBinding { set; get; } = null;

        public static float EraserActivationPressure { set; get; }
        public static IBinding EraserBinding { set; get; } = null;

        public static Dictionary<int, IBinding> PenButtonBindings { set; get; } = new Dictionary<int, IBinding>();
        public static Dictionary<int, IBinding> AuxButtonBindings { set; get; } = new Dictionary<int, IBinding>();

        private static bool TipState { set; get; } = false;
        private static bool EraserState { set; get; } = false;
        private static IList<bool> PenButtonStates { get; } = new bool[2];
        private static IList<bool> AuxButtonStates { get; } = new bool[8];

        public static void HandleBinding(TabletState tablet, IDeviceReport report)
        {
            if (tablet == null)
                return;

            if (report is ITabletReport tabletReport && tablet.Digitizer.ActiveReportID.IsInRange(tabletReport.ReportID))
                HandleTabletReport(tablet.Digitizer, tabletReport);
            if (report is IAuxReport auxReport)
                HandleAuxiliaryReport(auxReport);
        }

        private static void HandleTabletReport(DigitizerIdentifier identifier, ITabletReport report)
        {
            float pressurePercent = (float)report.Pressure / (float)identifier.MaxPressure * 100f;
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

        private static void HandleAuxiliaryReport(IAuxReport report)
        {
            HandleBindingCollection(report, AuxButtonStates, report.AuxButtons, AuxButtonBindings);
        }

        private static void HandleBindingCollection(IDeviceReport report, IList<bool> prevStates, IList<bool> newStates, IDictionary<int, IBinding> bindings)
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
