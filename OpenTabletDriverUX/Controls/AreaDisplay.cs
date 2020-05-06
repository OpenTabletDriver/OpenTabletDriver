using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Eto.Drawing;
using Eto.Forms;
using TabletDriverPlugin;

namespace OpenTabletDriverUX.Controls
{
    public class AreaDisplay : Drawable, IViewModelRoot<AreaViewModel>
    {
        public AreaDisplay()
        {
            this.DataContext = new AreaViewModel();
            this.Paint += (sender, e) => Draw(e.Graphics);
            this.MouseDown += (sender, e) => BeginAreaDrag();
            this.MouseUp += (sender, e) => EndAreaDrag();
        }

        public AreaViewModel ViewModel
        {
            set => this.DataContext = value;
            get => (AreaViewModel)this.DataContext;
        }
        
        private void Draw(Graphics graphics)
        {
            var background = new RectangleF(ViewModel.X, ViewModel.Y, ViewModel.Width, ViewModel.Height);
            var foreground = new RectangleF(0, 0, ViewModel.MaxHeight, ViewModel.MaxHeight);
            
            DrawRect(graphics, background, new Color(0.5f, 0.5f, 0.5f), false);
            DrawRect(graphics, foreground, new Color(1, 1, 1), true);
            graphics.RotateTransform(ViewModel.Rotation);
        }

        private void DrawRect(Graphics graphics, RectangleF rect, Color color, bool offsetArea)
        {
            var center = new PointF(Parent.Width / 2, Parent.Height / 2);
            
            var drawRect = new RectangleF
            {
                Width = rect.Width,
                Height = rect.Height,
                Center = new PointF(
                    rect.X + center.X,
                    rect.Y + center.Y),
            };

            graphics.FillRectangle(color, drawRect);
        }

        private PointF? lastMouseLocation;

        private void BeginAreaDrag()
        {
            this.MouseMove += MoveArea;
        }

        private void EndAreaDrag()
        {
            this.MouseMove -= MoveArea;
            lastMouseLocation = null;
        }

        private void MoveArea(object sender, MouseEventArgs e)
        {
            if (lastMouseLocation is PointF lastPos)
            {
                var delta = lastPos - e.Location;
                ViewModel.X -= delta.X;
                ViewModel.Y -= delta.Y;
                Invalidate();
            }
            lastMouseLocation = e.Location;
        }
    }
}