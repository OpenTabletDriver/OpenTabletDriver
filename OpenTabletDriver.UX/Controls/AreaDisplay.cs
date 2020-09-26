using System;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls
{
    public class AreaDisplay : Drawable, IViewModelRoot<AreaViewModel>
    {
        public AreaDisplay(string unit)
        {
            this.DataContext = new AreaViewModel();
            this.Paint += (sender, e) => Draw(e.Graphics);
            this.MouseDown += (sender, e) => BeginAreaDrag(e.Buttons);
            this.MouseUp += (sender, e) => EndAreaDrag(e.Buttons);
            
            ViewModel.Unit = unit;
            ViewModel.PropertyChanged += (sender, e) => this.Invalidate();
        }

        public AreaViewModel ViewModel
        {
            set => this.DataContext = value;
            get => (AreaViewModel)this.DataContext;
        }
        
        private void Draw(Graphics graphics)
        {
            if (ViewModel.Background.Min(d => d.Height) <= 0 | ViewModel.Background.Min(d => d.Width) <= 0)
            {
                DrawError(graphics, InvalidSizeError);
                return;
            }

            var foreground = new RectangleF(
                ViewModel.X - (ViewModel.Width / 2),
                ViewModel.Y - (ViewModel.Height / 2),
                ViewModel.Width,
                ViewModel.Height);

            var fullBg = new RectangleF
            {
                Left = ViewModel.Background.Min(r => r.Left),
                Top = ViewModel.Background.Min(r => r.Top),
                Right = ViewModel.Background.Max(r => r.Right),
                Bottom = ViewModel.Background.Max(r => r.Bottom),
            };
            fullBg.Offset(-2 * fullBg.TopLeft);

            if (foreground.Width > 0 & foreground.Height > 0 & fullBg.Width > 0 & fullBg.Height > 0)
            {
                pixelScale = GetRelativeScale(ViewModel.Background.Max(d => d.Width), ViewModel.Background.Max(d => d.Height));
                foreach (var background in ViewModel.Background)
                    DrawBackground(graphics, background, SystemColors.WindowBackground, SystemColors.Highlight);
                DrawForeground(graphics, foreground, fullBg, SystemColors.Highlight);
            }
            else
            {
                DrawError(graphics, "Invalid area size.");
            }
        }

        private void DrawBackground(Graphics graphics, RectangleF rect, Color fill, Color border)
        {
            using (graphics.SaveTransformState())
            {
                rect *= pixelScale;
                
                graphics.TranslateTransform(this.Width / 2, this.Height / 2);
                graphics.TranslateTransform(-rect.Center);
                graphics.FillRectangle(fill, rect);
                graphics.DrawRectangle(border, rect);
            }
        }

        private void DrawForeground(Graphics graphics, RectangleF rect, RectangleF bg, Color color)
        {
            using (graphics.SaveTransformState())
            {
                rect *= pixelScale;
                bg *= pixelScale;

                graphics.TranslateTransform(bg.TopLeft);
                graphics.TranslateTransform((this.Width - bg.Width) / 2, (this.Height - bg.Height) / 2);
                graphics.TranslateTransform(rect.Center);
                graphics.RotateTransform(ViewModel.Rotation);
                graphics.TranslateTransform(-rect.Center);
                graphics.FillRectangle(color, rect);
                
                var originEllipse = new RectangleF(0, 0, 3, 3);
                originEllipse.Offset(rect.Center - (originEllipse.Size / 2));
                graphics.FillEllipse(SystemColors.ControlText, originEllipse);

                // Ratio text
                string ratio = Math.Round(ViewModel.Width / ViewModel.Height, 4).ToString();
                SizeF ratioMeasure = graphics.MeasureString(Font, ratio);
                var ratioPos = new PointF(
                    rect.Center.X - (ratioMeasure.Width / 2),
                    rect.Center.Y + (ratioMeasure.Height / 2)
                );
                graphics.DrawText(Font, TextBrush, ratioPos, ratio);
                
                // Width Text
                string widthText = $"{ViewModel.Width}{ViewModel.Unit}";
                var widthPos = rect.MiddleTop - (graphics.MeasureString(Font, widthText) / 2);
                graphics.DrawText(Font, TextBrush, widthPos, widthText);

                // Height Text
                string heightText = $"{ViewModel.Height}{ViewModel.Unit}";
                var heightSize = graphics.MeasureString(Font, heightText) / 2;
                var heightPos = new PointF(-rect.MiddleLeft.Y - heightSize.Width, rect.MiddleLeft.X - heightSize.Height);
                graphics.RotateTransform(-90);
                graphics.DrawText(Font, TextBrush, heightPos, heightText);
            }
        }

        private void DrawError(Graphics graphics, string errorText)
        {
            var errSize = graphics.MeasureString(Font, errorText);
            var x = (this.Width / 2) - (errSize.Width / 2);
            var y = (this.Height / 2) - (errSize.Height / 2);
            graphics.DrawText(Font, TextBrush, x, y, errorText);
        }

        private float GetRelativeScale(float width, float height)
        {
            return GetScale(this.Width - this.Padding.Size.Width, this.Height - this.Padding.Size.Height, width, height);
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

        private static readonly Font Font = SystemFonts.User(8);
        private static readonly Brush TextBrush = new SolidBrush(SystemColors.ControlText);

        private float pixelScale;
        private PointF? lastMouseLocation;

        public string InvalidSizeError { set; get; } = "Invalid area size";

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