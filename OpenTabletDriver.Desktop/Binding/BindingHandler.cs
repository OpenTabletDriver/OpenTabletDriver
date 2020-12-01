using System.Collections.Generic;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    public static class BindingHandler
    {
        public static float TipActivationPressure { set; get; }
        public static IBinding TipBinding { set; get; } = null;
        public static Dictionary<int, IBinding> PenButtonBindings { set; get; } = new Dictionary<int, IBinding>();
        public static Dictionary<int, IBinding> AuxButtonBindings { set; get; } = new Dictionary<int, IBinding>();

        private static bool TipState = false;
        private static IList<bool> PenButtonStates = new bool[2];
        private static IList<bool> AuxButtonStates = new bool[6];

        public static void HandleBinding(TabletState tablet, IDeviceReport report)
        {
            if (tablet == null)
                return;

            if (report is ITabletReport tabletReport && tablet.Digitizer.ActiveReportID.IsInRange(tabletReport.ReportID))
                HandlePenBinding(tablet.Digitizer, tabletReport);
            if (report is IAuxReport auxReport)
                HandleAuxBinding(auxReport);
        }

        private static void HandlePenBinding(DigitizerIdentifier identifier, ITabletReport report)
        {
            if (TipBinding != null && TipActivationPressure != 0)
            {
                float pressurePercent = (float)report.Pressure / identifier.MaxPressure * 100f;

                if (pressurePercent >= TipActivationPressure && !TipState)
                    TipBinding.Press();
                else if (pressurePercent < TipActivationPressure && TipState)
                    TipBinding.Release();
                TipState = pressurePercent >= TipActivationPressure;
            }

            for (var penButton = 0; penButton < report.PenButtons.Length; penButton++)
            {
                if (PenButtonBindings.TryGetValue(penButton, out var binding) && binding != null)
                {
                    if (report.PenButtons[penButton] && !PenButtonStates[penButton])
                        binding.Press();
                    else if (!report.PenButtons[penButton] && PenButtonStates[penButton])
                        binding.Release();
                }
                PenButtonStates[penButton] = report.PenButtons[penButton];
            }
        }

        private static void HandleAuxBinding(IAuxReport report)
        {
            for (var auxButton = 0; auxButton < report.AuxButtons.Length; auxButton++)
            {
                if (AuxButtonBindings.TryGetValue(auxButton, out var binding) && binding != null)
                {
                    if (report.AuxButtons[auxButton] && !AuxButtonStates[auxButton])
                        binding.Press();
                    else if (!report.AuxButtons[auxButton] && AuxButtonStates[auxButton])
                        binding.Release();
                }
                AuxButtonStates[auxButton] = report.AuxButtons[auxButton];
            }
        }
    }
}
