using System.Collections.Generic;
using System.Linq;
using TabletDriverLib.Component;
using TabletDriverLib.Interop;
using TabletDriverLib.Interop.Cursor;
using TabletDriverLib.Tablet;

namespace TabletDriverLib.Output
{
    public class AbsoluteMode : OutputMode
    {
        private Area _displayArea, _tabletArea;
        private TabletProperties _tabletProperties;
        
        public Area DisplayArea
        {
            set
            {
                _displayArea = value;
                UpdateCache();
            }
            get => _displayArea;
        }

        public Area TabletArea
        {
            set
            {
                _tabletArea = value;
                UpdateCache();
            }
            get => _tabletArea;
        }

        public override TabletProperties TabletProperties
        {
            set
            {
                _tabletProperties = value;
                UpdateCache();
            }
            get => _tabletProperties;
        }

        private ICursorHandler CursorHandler { set; get; } = Platform.CursorHandler;
        public bool Clipping { set; get; }
        public bool TipEnabled { set; get; }
        public float TipActivationPressure { set; get; }
        public MouseButton TipBinding { set; get; } = 0;
        public BindingDictionary PenButtonBindings { set; get; } = new BindingDictionary();
        public BindingDictionary AuxButtonBindings { set; get; } = new BindingDictionary();

        private IList<bool> PenButtonStates = new bool[4];

        private void UpdateCache()
        {
            _rotationMatrix = TabletArea?.GetRotationMatrix();
            
            _halfDisplayWidth = DisplayArea?.Width / 2 ?? 0;
            _halfDisplayHeight = DisplayArea?.Height / 2 ?? 0;
            _halfTabletWidth = TabletArea?.Width / 2 ?? 0;
            _halfTabletHeight = TabletArea?.Height / 2 ?? 0;

            _minX = DisplayArea?.Position.X - _halfDisplayWidth ?? 0;
            _maxX = DisplayArea?.Position.X + DisplayArea?.Width - _halfDisplayWidth ?? 0;
            _minY = DisplayArea?.Position.Y - _halfDisplayHeight ?? 0;
            _maxY = DisplayArea?.Position.Y + DisplayArea?.Height - _halfDisplayHeight ?? 0;
        }

        private float[] _rotationMatrix;
        private float _halfDisplayWidth, _halfDisplayHeight, _halfTabletWidth, _halfTabletHeight;
        private float _minX, _maxX, _minY, _maxY;

        public override void Position(ITabletReport report)
        {
            if (report.Lift <= TabletProperties.MinimumRange)
                return;
            
            var pos = report.Position;

            // Normalize (ratio of 1)
            pos.X /= TabletProperties.MaxX;
            pos.Y /= TabletProperties.MaxY;

            // Scale to tablet dimensions (mm)
            pos.X *= TabletProperties.Width;
            pos.Y *= TabletProperties.Height;

            // Adjust area to set origin to 0,0
            pos.X -= TabletArea.Position.X;
            pos.Y -= TabletArea.Position.Y;

            // Rotation
            if (TabletArea.Rotation != 0f)
            {
                var tempCopy = new Point(pos.X, pos.Y);
                pos.X = (tempCopy.X * _rotationMatrix[0]) + (tempCopy.Y * _rotationMatrix[1]);
                pos.Y = (tempCopy.X * _rotationMatrix[2]) + (tempCopy.Y * _rotationMatrix[3]);
            }

            // Move area back
            pos.X += _halfTabletWidth;
            pos.Y += _halfTabletHeight;

            // Scale to tablet area (ratio of 1)
            pos.X /= TabletArea.Width;
            pos.Y /= TabletArea.Height;

            // Scale to display area
            pos.X *= DisplayArea.Width;
            pos.Y *= DisplayArea.Height;

            // Adjust display offset by center
            pos.X -= DisplayArea.Position.X - _halfDisplayWidth;
            pos.Y -= DisplayArea.Position.Y - _halfDisplayHeight;

            // Clipping to display bounds
            if (Clipping)
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

            // Setting cursor position
            CursorHandler.SetCursorPosition(pos);
            HandleButton(report);
        }

        public void HandleButton(ITabletReport report)
        {
            if (TipEnabled)
            {
                float pressurePercent = (float)report.Pressure / TabletProperties.MaxPressure * 100f;
                var binding = TipBinding;
                bool isButtonPressed = CursorHandler.GetMouseButtonState(binding);

                if (pressurePercent >= TipActivationPressure && !isButtonPressed)
                    CursorHandler.MouseDown(binding);
                else if (pressurePercent < TipActivationPressure && isButtonPressed)
                    CursorHandler.MouseUp(binding);
            }

            for (var penButton = 0; penButton < 4; penButton++)
            {
                MouseButton binding = PenButtonBindings[penButton];
                bool isButtonPressed = CursorHandler.GetMouseButtonState(binding);
                
                if (report.PenButtons[penButton] && !PenButtonStates[penButton] && !isButtonPressed)
                    CursorHandler.MouseDown(binding);
                else if (!report.PenButtons[penButton] && PenButtonStates[penButton] && isButtonPressed)
                    CursorHandler.MouseUp(binding);
                
                PenButtonStates[penButton] = report.PenButtons[penButton];
            }
        }

        public override void Aux(IAuxReport report)
        {
            for (var auxButton = 0; auxButton < 4; auxButton++)
            {
                MouseButton binding = AuxButtonBindings[auxButton];
                bool isButtonPressed = CursorHandler.GetMouseButtonState(binding);

                if (report.AuxButtons[auxButton] && !isButtonPressed)
                    CursorHandler.MouseDown(binding);
                else if (!report.AuxButtons[auxButton] && isButtonPressed)
                    CursorHandler.MouseUp(binding);
            }
        }
    }
}