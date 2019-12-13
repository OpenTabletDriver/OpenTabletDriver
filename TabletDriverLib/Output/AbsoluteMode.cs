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
        private ICursorHandler CursorHandler { set; get; } = Platform.CursorHandler;

        public Area DisplayArea { set; get; }
        public Area TabletArea { set; get; }

        public override TabletProperties TabletProperties { set; get; }

        public bool Clipping { set; get; }
        public bool TipEnabled { set; get; }
        public float TipActivationPressure { set; get; }
        public MouseButton TipBinding { set; get; } = 0;
        public BindingDictionary PenButtonBindings { set; get; } = new BindingDictionary();
        public BindingDictionary AuxButtonBindings { set; get; } = new BindingDictionary();

        public override void Position(ITabletReport report)
        {
            if (report.Lift <= TabletProperties.MinimumRange)
                return;
            
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

            for (var penButton = 0; penButton < TabletProperties.PenButtons; penButton++)
            {
                MouseButton binding = PenButtonBindings[penButton];
                bool isButtonPressed = CursorHandler.GetMouseButtonState(binding);

                if (report.PenButtons[penButton] && !isButtonPressed)
                    CursorHandler.MouseDown(binding);
                else if (!report.PenButtons[penButton] && isButtonPressed)
                    CursorHandler.MouseUp(binding);
            }
        }

        public override void Aux(IAuxReport report)
        {
            for (var auxButton = 0; auxButton < TabletProperties.AuxButtons; auxButton++)
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