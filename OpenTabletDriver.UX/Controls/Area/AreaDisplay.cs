using System;
using System.Linq;
using System.Numerics;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Controls.Area
{
    public class AreaDisplay : TimedDrawable, IViewModelRoot<AreaViewModel>
    {
        public AreaViewModel ViewModel
        {
            set => this.DataContext = value;
            get => (AreaViewModel)this.DataContext;
        }

        public float PixelScale => CalculateScale(BackgroundRect);

        private static readonly Font Font = SystemFonts.User(8);
        private static readonly Brush TextBrush = new SolidBrush(SystemColors.ControlText);

        private readonly Color AccentColor = SystemColors.Highlight;
        private readonly Color AreaBoundsBorderColor = SystemColors.Control;
        private readonly Color AreaBoundsFillColor = SystemColors.ControlBackground;

        private bool mouseDragging;
        private PointF? lastMouseLocation;

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

        public Vector2[] GetAreaCorners()
        {
            var origin = new Vector2(ViewModel.X, ViewModel.Y);
            var matrix = Matrix3x2.CreateTranslation(-origin);
            matrix *= Matrix3x2.CreateRotation((float)(ViewModel.Rotation * Math.PI / 180));
            matrix *= Matrix3x2.CreateTranslation(origin);

            float halfWidth = ViewModel.Width / 2;
            float halfHeight = ViewModel.Height / 2;

            return new Vector2[]
            {
                Vector2.Transform(new Vector2(ViewModel.X - halfWidth, ViewModel.Y - halfHeight), matrix),
                Vector2.Transform(new Vector2(ViewModel.X - halfWidth, ViewModel.Y + halfHeight), matrix),
                Vector2.Transform(new Vector2(ViewModel.X + halfWidth, ViewModel.Y + halfHeight), matrix),
                Vector2.Transform(new Vector2(ViewModel.X + halfWidth, ViewModel.Y - halfHeight), matrix),
            };
        }

        public Vector2 GetAreaCenterOffset()
        {
            var corners = GetAreaCorners();
            var min = new Vector2(
                corners.Min(v => v.X),
                corners.Min(v => v.Y)
            );
            var max = new Vector2(
                corners.Max(v => v.X),
                corners.Max(v => v.Y)
            );
            return (max - min) / 2;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            switch (e.Buttons)
            {
                case MouseButtons.Primary:
                {
                    mouseDragging = true;
                    break;
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            switch (e.Buttons)
            {
                case MouseButtons.Primary:
                {
                    mouseDragging = false;
                    break;
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (mouseDragging)
            {
                if (lastMouseLocation != null)
                {
                    var delta = e.Location - lastMouseLocation.Value;
                    var newX = ViewModel.X + delta.X / PixelScale;
                    var newY = ViewModel.Y + delta.Y / PixelScale;

                    if (ViewModel.LockToUsableArea)
                    {
                        var bounds = ViewModel.FullBackground;
                        var rect = RectangleF.FromCenter(PointF.Empty, new SizeF(ViewModel.Width, ViewModel.Height));
                        var corners = new PointF[]
                        {
                            PointF.Rotate(rect.TopLeft, ViewModel.Rotation),
                            PointF.Rotate(rect.BottomRight, ViewModel.Rotation)
                        };
                        var pseudoArea = new RectangleF(
                            PointF.Min(corners[0], corners[1]),
                            PointF.Max(corners[0], corners[1])
                        );
                        pseudoArea.Center += new PointF(newX, newY);

                        if (!bounds.Contains(pseudoArea))
                        {
                            if (pseudoArea.Left < bounds.Left)
                                newX = bounds.Left + (pseudoArea.Width - 1) / 2;
                            else if (pseudoArea.Right > bounds.Right)
                                newX = bounds.Right - (pseudoArea.Width - 1) / 2;
                            if (pseudoArea.Top < bounds.Top)
                                newY = bounds.Top + (pseudoArea.Height - 1) / 2;
                            else if (pseudoArea.Bottom > bounds.Bottom)
                                newY = bounds.Bottom - (pseudoArea.Height - 1) / 2;
                        }
                    }

                    ViewModel.X = newX;
                    ViewModel.Y = newY;
                }
                lastMouseLocation = e.Location;
            }
            else if (!mouseDragging && lastMouseLocation != null)
            {
                lastMouseLocation = null;
            }
        }

        protected override void OnNextFrame(PaintEventArgs e)
        {
            var graphics = e.Graphics;

            switch (IsValid(ForegroundRect), IsValid(BackgroundRect))
            {
                case (true, true):
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
                    break;
                }
                case (true, false):
                {
                    DrawError(graphics, "Invalid area size.");
                    break;
                }
                case (_, true):
                {
                    DrawError(graphics, ViewModel.InvalidSizeError);
                    break;
                }
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
