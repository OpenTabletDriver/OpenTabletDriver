using System.Collections.Generic;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin.Output
{
    [PluginIgnore]
    public class BindingHandler : IBindingHandler<IBinding>
    {
        public virtual DigitizerIdentifier Digitizer { set; get; }

        public float TipActivationPressure { set; get; }
        public IBinding TipBinding { set; get; } = null;
        public Dictionary<int, IBinding> PenButtonBindings { set; get; } = new Dictionary<int, IBinding>();
        public Dictionary<int, IBinding> AuxButtonBindings { set; get; } = new Dictionary<int, IBinding>();

        private bool TipState = false;
        private IList<bool> PenButtonStates = new bool[2];
        private IList<bool> AuxButtonStates = new bool[6];

        public void HandleBinding(IDeviceReport report)
        {
            if (report is ITabletReport tabletReport && Digitizer.ActiveReportID.IsInRange(tabletReport.ReportID))
                HandlePenBinding(tabletReport);
            if (report is IAuxReport auxReport)
                HandleAuxBinding(auxReport);
        }

        private void HandlePenBinding(ITabletReport report)
        {
            if (TipBinding != null && TipActivationPressure != 0)
            {
                float pressurePercent = (float)report.Pressure / Digitizer.MaxPressure * 100f;

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

        private void HandleAuxBinding(IAuxReport report)
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