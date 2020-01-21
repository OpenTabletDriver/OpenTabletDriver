using System.Collections.Generic;
using TabletDriverLib.Interop;
using TabletDriverLib.Interop.Cursor;
using TabletDriverPlugin;
using TabletDriverPlugin.Attributes;
using TabletDriverPlugin.Tablet;

namespace TabletDriverLib.Output
{
    [PluginIgnore]
    public class BindingHandler : IOutputMode, IBindingHandler<MouseButton>
    {
        public virtual IFilter Filter { set; get; }
        public virtual TabletProperties TabletProperties { set; get; }

        public float TipActivationPressure { set; get; }
        public MouseButton TipBinding { set; get; } = 0;
        public Dictionary<int, MouseButton> PenButtonBindings { set; get; } = new Dictionary<int, MouseButton>();
        public Dictionary<int, MouseButton> AuxButtonBindings { set; get; } = new Dictionary<int, MouseButton>();

        private IList<bool> PenButtonStates = new bool[2];
        protected ICursorHandler CursorHandler { set; get; } = Platform.CursorHandler;

        public void HandleBinding(IDeviceReport report)
        {
            if (report is ITabletReport tabletReport && tabletReport.Lift >= TabletProperties.MinimumRange)
                HandlePenBinding(tabletReport);
            if (report is IAuxReport auxReport)
                HandleAuxBinding(auxReport);
        }

        private void HandlePenBinding(ITabletReport report)
        {
            if (TipBinding != MouseButton.None)
            {
                float pressurePercent = (float)report.Pressure / TabletProperties.MaxPressure * 100f;
                var binding = TipBinding;
                bool isButtonPressed = CursorHandler.GetMouseButtonState(binding);

                if (pressurePercent >= TipActivationPressure && !isButtonPressed)
                    CursorHandler.MouseDown(binding);
                else if (pressurePercent < TipActivationPressure && isButtonPressed)
                    CursorHandler.MouseUp(binding);
            }

            for (var penButton = 0; penButton < 2; penButton++)
            {
                if (PenButtonBindings.TryGetValue(penButton, out var binding) && binding != MouseButton.None)
                {
                    bool isButtonPressed = CursorHandler.GetMouseButtonState(binding);
                
                    if (report.PenButtons[penButton] && !PenButtonStates[penButton] && !isButtonPressed)
                        CursorHandler.MouseDown(binding);
                    else if (!report.PenButtons[penButton] && PenButtonStates[penButton] && isButtonPressed)
                        CursorHandler.MouseUp(binding);
                }
                PenButtonStates[penButton] = report.PenButtons[penButton];
            }
        }

        private void HandleAuxBinding(IAuxReport report)
        {
            for (var auxButton = 0; auxButton < 4; auxButton++)
            {
                if (AuxButtonBindings.TryGetValue(auxButton, out var binding) && binding != MouseButton.None)
                {
                    bool isButtonPressed = CursorHandler.GetMouseButtonState(binding);

                    if (report.AuxButtons[auxButton] && !isButtonPressed)
                        CursorHandler.MouseDown(binding);
                    else if (!report.AuxButtons[auxButton] && isButtonPressed)
                        CursorHandler.MouseUp(binding);
                }
            }
        }

        public virtual void Read(IDeviceReport report)
        {
        }
    }
}