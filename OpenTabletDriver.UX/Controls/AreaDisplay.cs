using System;
using System.Collections.Generic;
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

            ViewModel.Unit = unit;
            ViewModel.PropertyChanged += (sender, e) => Application.Instance.AsyncInvoke(() => this.Invalidate());
        }

        public AreaViewModel ViewModel
        {
            set => this.DataContext = value;
            get => (AreaViewModel)this.DataContext;
        }
        
        private void Draw(Graphics graphics)
        {
            if (ViewModel.Background == null | ViewModel.FullBackground.Width <= 0 | ViewModel.FullBackground.Height <= 0)
            {
                DrawError(graphics, InvalidSizeError);
                return;
            }

            var foreground = new RectangleF
            {
                X = ViewModel.X - (ViewModel.Width / 2),
                Y = ViewModel.Y - (ViewModel.Height / 2),
                Width = ViewModel.Width,
                Height = ViewModel.Height
            };
            
            if (foreground.Width > 0 & foreground.Height > 0 & ViewModel.FullBackground.Width > 0 & ViewModel.FullBackground.Height > 0)
            {
                PixelScale = GetRelativeScale(ViewModel.FullBackground.Width, ViewModel.FullBackground.Height);
                DrawBackground(graphics, ViewModel.Background, (ViewModel.FullBackground * PixelScale), SystemColors.ControlBackground, SystemColors.Highlight);
                DrawForeground(graphics, foreground, (ViewModel.FullBackground * PixelScale), SystemColors.Highlight);
            }
            else
            {
                DrawError(graphics, "Invalid area size.");
            }
        }

        private void DrawBackground(Graphics graphics, IEnumerable<RectangleF> rects, RectangleF fullBg, Color fill, Color border)
        {
            using (graphics.SaveTransformState())
            {
                graphics.TranslateTransform((this.Width - fullBg.Width) / 2, (this.Height - fullBg.Height) / 2);
                graphics.TranslateTransform(-fullBg.TopLeft);
                foreach (var rect in rects)
                {
                    var scaledRect = rect * PixelScale;
                    graphics.FillRectangle(fill, scaledRect);
                    graphics.DrawRectangle(border, scaledRect);
                }
            }
        }

        private void DrawForeground(Graphics graphics, RectangleF rect, RectangleF fullBg, Color color)
        {
            using (graphics.SaveTransformState())
            {
                rect *= PixelScale;
                graphics.TranslateTransform((this.Width - fullBg.Width) / 2, (this.Height - fullBg.Height) / 2);
                graphics.TranslateTransform(rect.Center);
                graphics.RotateTransform(ViewModel.Rotation);
                graphics.TranslateTransform(-rect.Center);
                graphics.FillRectangle(color, rect);
                
                var originEllipse = new RectangleF(0, 0, 3, 3);
                originEllipse.Offset(rect.Center - (originEllipse.Size / 2));
                graphics.DrawEllipse(SystemColors.ControlText, originEllipse);

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

        public float PixelScale { protected set; get; }

        public string InvalidSizeError { set; get; } = "Invalid area size";
    }
}