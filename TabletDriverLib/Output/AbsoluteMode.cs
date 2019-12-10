using System.Collections.Generic;
using TabletDriverLib.Component;
using TabletDriverLib.Interop;
using TabletDriverLib.Interop.Cursor;
using TabletDriverLib.Tablet;

namespace TabletDriverLib.Output
{
    public class AbsoluteMode : OutputMode
    {
        private ICursorHandler CursorHandler { set; get; } = Platform.CursorHandler;
        private Area _display, _tablet;
        private TabletProperties _properties;

        public Area DisplayArea
        {
            set
            {
                _display = value;
                UpdateCache();
            }
            get => _display;
        }

        public Area TabletArea
        {
            set
            {
                _tablet = value;
                UpdateCache();
            }
            get => _tablet;
        }

        public override TabletProperties TabletProperties
        {
            set
            {
                _properties = value;
                UpdateCache();
            }
            get => _properties;
        }

        public bool Clipping { set; get; }
        public bool BindingsEnabled { set; get; }
        public float TipActivationPressure { set; get; }
        public Dictionary<int, MouseButton> MouseButtonBindings { set; get; } = new Dictionary<int, MouseButton>()
        {
            { 0, MouseButton.None }
        };

        private float scaleX, scaleY, reportXOffset, reportYOffset;

        public void UpdateCache()
        {
            if (DisplayArea != null && TabletArea != null && TabletProperties != null)
            {
                scaleX = (DisplayArea.Width * TabletProperties.Width) / (TabletArea.Width * TabletProperties.MaxX);
                scaleY = (DisplayArea.Height * TabletProperties.Height) / (TabletArea.Height * TabletProperties.MaxY);
                reportXOffset = (TabletProperties.MaxX / TabletProperties.Width) * TabletArea.Position.X;
                reportYOffset = (TabletProperties.MaxY / TabletProperties.Height) * TabletArea.Position.Y;
            }
        }

        public override void Position(ITabletReport report)
        {
            var pos = new Point(
                report.Position.X,
                report.Position.Y
            );

            // Normalize (ratio of 1)
            pos.X /= TabletProperties.MaxX;
            pos.Y /= TabletProperties.MaxY;

            // Scale to tablet dimensions (mm)
            pos.X *= TabletProperties.Width;
            pos.Y *= TabletProperties.Height;

            // Adjust tablet offset by center
            pos.X -= TabletArea.Position.X - (TabletArea.Width / 2);
            pos.Y -= TabletArea.Position.Y - (TabletArea.Height / 2);

            // Scale to tablet area (ratio of 1)
            pos.X /= TabletArea.Width;
            pos.Y /= TabletArea.Height;

            // Scale to display area
            pos.X *= DisplayArea.Width;
            pos.Y *= DisplayArea.Height;

            // Adjust display offset by center
            pos.X -= DisplayArea.Position.X - (DisplayArea.Width / 2);
            pos.Y -= DisplayArea.Position.Y - (DisplayArea.Height / 2);

            // Rotation
            if (TabletArea.Rotation != 0f)
            {
                var tempCopy = new Point(pos.X, pos.Y);
                var rotateMatrix = TabletArea.GetRotationMatrix();
                pos.X = (tempCopy.X * rotateMatrix[0]) + (tempCopy.Y * rotateMatrix[1]);
                pos.Y = (tempCopy.X * rotateMatrix[2]) + (tempCopy.Y * rotateMatrix[3]);
            }

            // Clipping to display bounds
            if (Clipping)
            {
                if (pos.X < DisplayArea.Position.X - (DisplayArea.Width / 2))
                    pos.X = DisplayArea.Position.X - (DisplayArea.Width / 2);
                if (pos.X > DisplayArea.Position.X + DisplayArea.Width - (DisplayArea.Width / 2))
                    pos.X = DisplayArea.Position.X + DisplayArea.Width - (DisplayArea.Width / 2);
                if (pos.Y < DisplayArea.Position.Y - (DisplayArea.Height / 2))
                    pos.Y = DisplayArea.Position.Y - (DisplayArea.Height / 2);
                if (pos.Y > DisplayArea.Position.Y + DisplayArea.Height - (DisplayArea.Height / 2))
                    pos.Y = DisplayArea.Position.Y + DisplayArea.Height - (DisplayArea.Height / 2);
            }

            // Setting cursor position
            CursorHandler.SetCursorPosition(pos);
            HandleButton(report);
        }

        public void HandleButton(ITabletReport report)
        {
            if (BindingsEnabled)
            {
                float pressurePercent = (float)report.Pressure / TabletProperties.MaxPressure * 100f;
                if (pressurePercent >= TipActivationPressure && !CursorHandler.GetMouseButtonState(MouseButtonBindings[0]))
                    CursorHandler.MouseDown(MouseButtonBindings[0]);
                else if (pressurePercent < TipActivationPressure && CursorHandler.GetMouseButtonState(MouseButtonBindings[0]))
                    CursorHandler.MouseUp(MouseButtonBindings[0]);
            }
        }
    }
}