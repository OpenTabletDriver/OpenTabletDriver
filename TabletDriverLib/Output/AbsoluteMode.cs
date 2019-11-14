using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TabletDriverLib.Component;
using TabletDriverLib.Interop;
using TabletDriverLib.Interop.Cursor;

namespace TabletDriverLib.Output
{
    public class AbsoluteMode : OutputMode
    {
        public AbsoluteMode(TabletProperties tabletProperties) : base(tabletProperties)
        {
            CursorHandler = Platform.CursorHandler;
        }

        private ICursorHandler CursorHandler { set; get; }
        public Area DisplayArea { set; get; }
        public Area TabletArea { set; get; }
        public bool Clipping { set; get; }
        public bool BindingsEnabled { set; get; }
        public float TipActivationPressure { set; get; }
        public Dictionary<int, MouseButton> MouseButtonBindings { set; get; } = new Dictionary<int, MouseButton>()
        {
            { 0, MouseButton.None }
        };

        public override void Position(TabletReport report)
        {
            var scaleX = (DisplayArea.Width * TabletProperties.Width) / (TabletArea.Width * TabletProperties.MaxX);
            var scaleY = (DisplayArea.Height * TabletProperties.Height) / (TabletArea.Height * TabletProperties.MaxY);
            var reportXOffset = (TabletProperties.MaxX / TabletProperties.Width) * TabletArea.Position.X;
            var reportYOffset = (TabletProperties.MaxY / TabletProperties.Height) * TabletArea.Position.Y;
            var pos = new Point(
                (scaleX * (report.Position.X - reportXOffset)) + DisplayArea.Position.X,
                (scaleY * (report.Position.Y - reportYOffset)) + DisplayArea.Position.Y
            );

            if (Clipping)
            {
                // X position clipping
                if (pos.X > DisplayArea.Width + DisplayArea.Position.X)
                    pos.X = DisplayArea.Width + DisplayArea.Position.X;
                else if (pos.X < DisplayArea.Position.X)
                    pos.X = DisplayArea.Position.X;
                // Y position clipping
                if (pos.Y > DisplayArea.Height + DisplayArea.Position.Y)
                    pos.Y = DisplayArea.Height + DisplayArea.Position.Y;
                else if (pos.Y < DisplayArea.Position.Y)
                    pos.Y = DisplayArea.Position.Y;
            }

            CursorHandler.SetCursorPosition(pos);
            HandleButton(report);
        }

        public void HandleButton(TabletReport report)
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