using System.Collections.Generic;
using TabletDriverLib.Interop;
using TabletDriverLib.Interop.Cursor;
using TabletDriverPlugin;
using TabletDriverPlugin.Tablet;

namespace TabletDriverLib.Output
{
    public class AbsoluteMode : IAbsoluteMode, IBindingHandler<MouseButton>
    {
        public void Read(IDeviceReport report)
        {
            if (report is ITabletReport tabletReport)
                Position(tabletReport);
        }
        
        private Area _displayArea, _tabletArea;
        private TabletProperties _tabletProperties;
        
        public Area Output
        {
            set
            {
                _displayArea = value;
                UpdateCache();
            }
            get => _displayArea;
        }

        public Area Input
        {
            set
            {
                _tabletArea = value;
                UpdateCache();
            }
            get => _tabletArea;
        }

        public TabletProperties TabletProperties
        {
            set
            {
                _tabletProperties = value;
                UpdateCache();
            }
            get => _tabletProperties;
        }

        private ICursorHandler CursorHandler { set; get; } = Platform.CursorHandler;
        public bool AreaClipping { set; get; }
        public float TipActivationPressure { set; get; }
        public IFilter Filter { set; get; }
        public MouseButton TipBinding { set; get; } = 0;
        public Dictionary<int, MouseButton> PenButtonBindings { set; get; } = new Dictionary<int, MouseButton>();
        public Dictionary<int, MouseButton> AuxButtonBindings { set; get; } = new Dictionary<int, MouseButton>();

        private IList<bool> PenButtonStates = new bool[2];

        private void UpdateCache()
        {
            _rotationMatrix = Input?.GetRotationMatrix();
            
            _halfDisplayWidth = Output?.Width / 2 ?? 0;
            _halfDisplayHeight = Output?.Height / 2 ?? 0;
            _halfTabletWidth = Input?.Width / 2 ?? 0;
            _halfTabletHeight = Input?.Height / 2 ?? 0;

            _minX = Output?.Position.X - _halfDisplayWidth ?? 0;
            _maxX = Output?.Position.X + Output?.Width - _halfDisplayWidth ?? 0;
            _minY = Output?.Position.Y - _halfDisplayHeight ?? 0;
            _maxY = Output?.Position.Y + Output?.Height - _halfDisplayHeight ?? 0;
        }

        private float[] _rotationMatrix;
        private float _halfDisplayWidth, _halfDisplayHeight, _halfTabletWidth, _halfTabletHeight;
        private float _minX, _maxX, _minY, _maxY;

        public void Position(ITabletReport report)
        {
            if (report.Lift <= TabletProperties.MinimumRange)
                return;
            
            var pos = new Point(report.Position.X, report.Position.Y);

            // Normalize (ratio of 1)
            pos.X /= TabletProperties.MaxX;
            pos.Y /= TabletProperties.MaxY;

            // Scale to tablet dimensions (mm)
            pos.X *= TabletProperties.Width;
            pos.Y *= TabletProperties.Height;

            // Adjust area to set origin to 0,0
            pos -= Input.Position;

            // Rotation
            if (Input.Rotation != 0f)
            {
                var tempCopy = new Point(pos.X, pos.Y);
                pos.X = (tempCopy.X * _rotationMatrix[0]) + (tempCopy.Y * _rotationMatrix[1]);
                pos.Y = (tempCopy.X * _rotationMatrix[2]) + (tempCopy.Y * _rotationMatrix[3]);
            }

            // Move area back
            pos.X += _halfTabletWidth;
            pos.Y += _halfTabletHeight;

            // Scale to tablet area (ratio of 1)
            pos.X /= Input.Width;
            pos.Y /= Input.Height;

            // Scale to display area
            pos.X *= Output.Width;
            pos.Y *= Output.Height;

            // Adjust display offset by center
            pos.X += Output.Position.X - _halfDisplayWidth;
            pos.Y += Output.Position.Y - _halfDisplayHeight;

            // Clipping to display bounds
            if (AreaClipping)
            {
                if (pos.X < _minX)
                    pos.X = _minX;
                if (pos.X > _maxX)
                    pos.X = _maxX;
                if (pos.Y < _minY)
                    pos.Y = _minY;
                if (pos.Y > _maxY)
                    pos.Y = _maxY;
            }
            
            if (Filter is IFilter filter)
                pos = filter.Filter(pos);

            // Setting cursor position
            CursorHandler.SetCursorPosition(pos);
        }

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