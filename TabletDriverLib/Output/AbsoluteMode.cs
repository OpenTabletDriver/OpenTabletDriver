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

        public Area DisplayArea { set; get; }
        public Area TabletArea { set; get; }

        public override TabletProperties TabletProperties { set; get; }

        public bool Clipping { set; get; }
        public bool BindingsEnabled { set; get; }
        public float TipActivationPressure { set; get; }
        public Dictionary<int, MouseButton> MouseButtonBindings { set; get; } = new Dictionary<int, MouseButton>()
        {
            { 0, MouseButton.None }
        };

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

            // Adjust area to set origin to 0,0
            pos.X -= TabletArea.Position.X;
            pos.Y -= TabletArea.Position.Y;

            // Rotation
            if (TabletArea.Rotation != 0f)
            {
                var tempCopy = new Point(pos.X, pos.Y);
                var rotateMatrix = TabletArea.GetRotationMatrix();
                pos.X = (tempCopy.X * rotateMatrix[0]) + (tempCopy.Y * rotateMatrix[1]);
                pos.Y = (tempCopy.X * rotateMatrix[2]) + (tempCopy.Y * rotateMatrix[3]);
            }

            // Move area back
            pos.X += TabletArea.Width / 2;
            pos.Y += TabletArea.Height / 2;

            // Scale to tablet area (ratio of 1)
            pos.X /= TabletArea.Width;
            pos.Y /= TabletArea.Height;

            // Scale to display area
            pos.X *= DisplayArea.Width;
            pos.Y *= DisplayArea.Height;

            // Adjust display offset by center
            pos.X -= DisplayArea.Position.X - (DisplayArea.Width / 2);
            pos.Y -= DisplayArea.Position.Y - (DisplayArea.Height / 2);

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