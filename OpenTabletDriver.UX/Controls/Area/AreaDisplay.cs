using System;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Controls.Area
{
    public class AreaDisplay : TimedDrawable, IViewModelRoot<AreaViewModel>
    {
        public AreaDisplay()
        {
            this.DataContext = new AreaViewModel();
        }

        public AreaViewModel ViewModel
        {
            set => this.DataContext = value;
            get => (AreaViewModel)this.DataContext;
        }

        public string Unit
        {
            set => ViewModel.Unit = value;
            get => ViewModel.Unit;
        }

        public float PixelScale => CalculateScale(BackgroundRect);

        private static readonly Font Font = SystemFonts.User(8);
        private static readonly Brush TextBrush = new SolidBrush(SystemColors.ControlText);

        private readonly Color AccentColor = SystemColors.Highlight;
        private readonly Color AreaBoundsBorderColor = SystemColors.Control;
        private readonly Color AreaBoundsFillColor = SystemColors.ControlBackground;

        private RectangleF BackgroundRect => new RectangleF(
            new PointF(0, 0),
            new SizeF(
                ViewModel.FullBackground.Width,
                ViewModel.FullBackground.Height
            )
        );

        private RectangleF ForegroundRect => RectangleF.FromCenter(
            new PointF(ViewModel.X, ViewModel.Y),
            new SizeF(ViewModel.Width, ViewModel.Height)
        );

        public string InvalidSizeError { set; get; } = "Invalid area size";

        protected override void OnNextFrame(PaintEventArgs e)
        {
            var graphics = e.Graphics;

            if (ViewModel.Background == null | ViewModel.FullBackground.Width <= 0 | ViewModel.FullBackground.Height <= 0)
            {
                DrawError(graphics, InvalidSizeError);
                return;
            }

            if (IsValid(ForegroundRect) & IsValid(BackgroundRect))
            {
                using (graphics.SaveTransformState())
                {
                    var scale = CalculateScale(BackgroundRect);

                    var clientCenter = new PointF(this.ClientSize.Width, this.ClientSize.Height) / 2;
                    var backgroundCenter = new PointF(BackgroundRect.Width, BackgroundRect.Height) / 2 * scale;
                    var offset = clientCenter - backgroundCenter;

                    graphics.TranslateTransform(offset);

                    DrawBackground(graphics, scale);
                    DrawForeground(graphics, scale);
                }
            }
            else
            {
                DrawError(graphics, "Invalid area size.");
            }
        }

        private void DrawBackground(Graphics graphics, float scale)
        {
            foreach (var rect in ViewModel.Background)
            {
                var scaledRect = rect * scale;
                graphics.FillRectangle(AreaBoundsFillColor, scaledRect);
                graphics.DrawRectangle(AreaBoundsBorderColor, scaledRect);
            }
        }

        private void DrawForeground(Graphics graphics, float scale)
        {
            using (graphics.SaveTransformState())
            {
                var area = ForegroundRect * scale;

                graphics.TranslateTransform(area.Center);
                graphics.RotateTransform(ViewModel.Rotation);
                graphics.TranslateTransform(-area.Center);
                
                graphics.FillRectangle(AccentColor, area);

                var originEllipse = new RectangleF(0, 0, 1, 1);
                originEllipse.Offset(area.Center - (originEllipse.Size / 2));
                graphics.DrawEllipse(SystemColors.ControlText, originEllipse);

                DrawRatioText(graphics, area);
                DrawWidthText(graphics, area);
                DrawHeightText(graphics, area);
            }
        }

        private void DrawRatioText(Graphics graphics, RectangleF area)
        {
            string ratio = Math.Round(ViewModel.Width / ViewModel.Height, 4).ToString();
            SizeF ratioMeasure = graphics.MeasureString(Font, ratio);
            var ratioPos = new PointF(
                area.Center.X - (ratioMeasure.Width / 2),
                area.Center.Y + (ratioMeasure.Height / 2)
            );
            graphics.DrawText(Font, TextBrush, ratioPos, ratio);
        }

        private void DrawWidthText(Graphics graphics, RectangleF area)
        {
            string widthText = $"{ViewModel.Width}{ViewModel.Unit}";
            var widthTextSize = graphics.MeasureString(Font, widthText);
            var widthTextPos = new PointF(
                area.MiddleTop.X - (widthTextSize.Width / 2),
                area.MiddleTop.Y
            );
            graphics.DrawText(Font, TextBrush, widthTextPos, widthText);
        }

        private void DrawHeightText(Graphics graphics, RectangleF area)
        {
            using (graphics.SaveTransformState())
            {
                string heightText = $"{ViewModel.Height}{ViewModel.Unit}";
                var heightSize = graphics.MeasureString(Font, heightText) / 2;
                var heightPos = new PointF(
                    -area.MiddleLeft.Y - heightSize.Width,
                    area.MiddleLeft.X
                );
                graphics.RotateTransform(-90);
                graphics.DrawText(Font, TextBrush, heightPos, heightText);
            }
        }

        private void DrawError(Graphics graphics, string errorText)
        {
            var errSize = graphics.MeasureString(Font, errorText);
            var x = (this.ClientSize.Width / 2f) - (errSize.Width / 2);
            var y = (this.ClientSize.Width / 2f) - (errSize.Height / 2);
            graphics.DrawText(Font, TextBrush, x, y, errorText);
        }

        private float CalculateScale(RectangleF rect)
        {
            float scaleX = this.ClientSize.Width / rect.Width;
            float scaleY = this.ClientSize.Height / rect.Height;
            return scaleX > scaleY ? scaleY : scaleX;
        }

        private static bool IsValid(RectangleF rect)
        {
            return rect.Width > 0 && rect.Height > 0;
        }
    }
}
