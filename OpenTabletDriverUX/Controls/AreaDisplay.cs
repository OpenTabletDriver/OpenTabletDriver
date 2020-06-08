using Eto.Drawing;
using Eto.Forms;

namespace OpenTabletDriverUX.Controls
{
    public class AreaDisplay : Drawable, IViewModelRoot<AreaViewModel>
    {
        public AreaDisplay()
        {
            this.DataContext = new AreaViewModel();
            this.Paint += (sender, e) => Draw(e.Graphics);
            this.MouseDown += (sender, e) => BeginAreaDrag(e.Buttons);
            this.MouseUp += (sender, e) => EndAreaDrag(e.Buttons);

            ViewModel.PropertyChanged += (sender, e) => this.Invalidate();
        }

        public AreaViewModel ViewModel
        {
            set => this.DataContext = value;
            get => (AreaViewModel)this.DataContext;
        }
        
        private void Draw(Graphics graphics)
        {
            var background = new RectangleF(0, 0, ViewModel.MaxWidth, ViewModel.MaxHeight);
            var foreground = new RectangleF(
                ViewModel.X - (ViewModel.Width / 2),
                ViewModel.Y - (ViewModel.Height / 2),
                ViewModel.Width,
                ViewModel.Height);
            
            pixelScale = GetRelativeScale(background.Width, background.Height);

            DrawBackgroundRect(graphics, background, SystemColors.WindowBackground, SystemColors.Highlight);
            DrawForegroundRect(graphics, foreground, SystemColors.Highlight);
        }

        private float GetRelativeScale(float width, float height)
        {
            return GetScale(Width, Height, width, height);
        }

        private float GetScale(float baseWidth, float baseHeight, float width, float height)
        {
            if (width != 0 && height != 0)
            {
                var scaleX = baseWidth / width;
                var scaleY = baseHeight / height;
                return scaleX > scaleY ? scaleY : scaleX;
            }
            else
            {
                return 1;
            }
        }

        private void DrawForegroundRect(Graphics graphics, RectangleF foreground, Color color)
        {
            var width = (foreground.Width * pixelScale) - 5;
            var height = (foreground.Height * pixelScale) - 5;
            
            var x = (foreground.X * pixelScale);
            var y = (foreground.Y * pixelScale);

            var offsetX = Width - (ViewModel.MaxWidth * pixelScale) + 5;
            if (offsetX / 2 > 0)
                x += offsetX / 2;

            var offsetY = Height - (ViewModel.MaxHeight * pixelScale) + 5;
            if (offsetY > 0)
                y += offsetY / 2;

            // Set rotation origin to center of rectangle
            var drawRect = new RectangleF(-width / 2, -height / 2, width, height);
            graphics.TranslateTransform(x + (width / 2), y + (height / 2));
            graphics.RotateTransform(ViewModel.Rotation);

            graphics.FillRectangle(color, drawRect);
        }

        private void DrawBackgroundRect(Graphics graphics, RectangleF rect, Color fill, Color border)
        {
            var centerPoint = new PointF(Width / 2, Height / 2);

            var width = (rect.Width * pixelScale) - 5;
            var height = (rect.Height * pixelScale) - 5;
            var x = ((float)Width - width) / 2;
            var y = ((float)Height - height) / 2;
            
            var drawRect = new RectangleF(x, y, width, height);
            graphics.FillRectangle(fill, drawRect);
            graphics.DrawRectangle(border, drawRect);
        }

        private float pixelScale;
        private PointF? lastMouseLocation;

        private void BeginAreaDrag(MouseButtons buttons)
        {
            if (buttons.HasFlag(MouseButtons.Primary))
                this.MouseMove += MoveArea;
        }

        private void EndAreaDrag(MouseButtons buttons)
        {
            if (buttons.HasFlag(MouseButtons.Primary))
            {
                this.MouseMove -= MoveArea;
                lastMouseLocation = null;
            }
        }
        
        private void MoveArea(object sender, MouseEventArgs e)
        {
            if (lastMouseLocation is PointF lastPos)
            {
                var delta = lastPos - e.Location;
                ViewModel.X -= delta.X / pixelScale;
                ViewModel.Y -= delta.Y / pixelScale;
                Invalidate();
            }
            lastMouseLocation = e.Location;
        }
    }
}