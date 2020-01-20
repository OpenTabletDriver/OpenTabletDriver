using System;
using System.Collections.Generic;
using TabletDriverLib.Interop;
using TabletDriverLib.Interop.Cursor;
using TabletDriverPlugin;
using TabletDriverPlugin.Tablet;

namespace TabletDriverLib.Output
{
    public class RelativeMode : IRelativeMode, IBindingHandler<MouseButton>
    {
        public float XSensitivity { set; get; }
        public float YSensitivity { set; get; }
        public IFilter Filter { set; get; }
        public TabletProperties TabletProperties { set; get; }
        public TimeSpan ResetTime { set; get; } = TimeSpan.FromMilliseconds(100);

        private ICursorHandler CursorHandler { set; get; } = Platform.CursorHandler;
        private ITabletReport _lastReport;
        private DateTime _lastReceived;
        private Point _lastPosition;

        public void Read(IDeviceReport report)
        {
            if (report is ITabletReport tabletReport)
                Position(tabletReport);
        }

        public void Position(ITabletReport report)
        {
            if (report.Lift <= TabletProperties.MinimumRange)
                return;
            
            var difference = DateTime.Now - _lastReceived;
            if (difference > ResetTime && _lastReceived != DateTime.MinValue)
            {
                _lastReport = null;
                _lastPosition = null;
            }

            if (_lastReport != null)
            {
                var pos = new Point(report.Position.X - _lastReport.Position.X, report.Position.Y - _lastReport.Position.Y);
                
                // Normalize (ratio of 1)
                pos.X /= TabletProperties.MaxX;
                pos.Y /= TabletProperties.MaxY;

                // Scale to tablet dimensions (mm)
                pos.X *= TabletProperties.Width;
                pos.Y *= TabletProperties.Height;

                // Sensitivity setting
                pos.X *= XSensitivity;
                pos.Y *= YSensitivity;
                
                // Translate by cursor position
                pos += GetCursorPosition();

                // Filter
                if (Filter != null)
                    pos = Filter.Filter(pos);

                CursorHandler.SetCursorPosition(pos);
                _lastPosition = pos;
            }
            
            _lastReport = report;
            _lastReceived = DateTime.Now;
        }

        private Point GetCursorPosition()
        {
            if (_lastPosition != null)
                return _lastPosition;
            else
                return CursorHandler.GetCursorPosition();
        }

        public float TipActivationPressure { set; get; }
        public MouseButton TipBinding { set; get; } = 0;
        public Dictionary<int, MouseButton> PenButtonBindings { set; get; } = new Dictionary<int, MouseButton>();
        public Dictionary<int, MouseButton> AuxButtonBindings { set; get; } = new Dictionary<int, MouseButton>();

        private IList<bool> PenButtonStates = new bool[2];

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
    }
}