using System.Linq.Expressions;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Output;

namespace OpenTabletDriver.UX.Controls.Editors
{
    public abstract class AreaDisplay : Drawable
    {
        private Func<Area>? _getArea;

        public float Scale { private set; get; } = 0;
        public PointF ControlOffset { private set; get; } = PointF.Empty;

        protected abstract string Unit { get; }
        protected abstract IEnumerable<RectangleF> Backgrounds { get; }
        protected abstract Expression<Func<AbsoluteOutputMode, Area>> Foreground { get; }

        public RectangleF FullBackground
        {
            get
            {
                var left = Backgrounds.Min(r => r.Left);
                var top = Backgrounds.Min(r => r.Top);
                var right = Backgrounds.Max(r => r.Right);
                var bottom = Backgrounds.Max(r => r.Bottom);
                return RectangleF.FromSides(left, top, right, bottom);
            }
        }

        private static Font Font { get; } = SystemFonts.User(9);
        private static Brush TextBrush { get; } = new SolidBrush(SystemColors.ControlText);
        private static Color ForegroundFillColor { get; } = new Color(SystemColors.Highlight, 0.75f);
        private static Color ForegroundBorderColor { get; } = SystemColors.ControlText;
        private static Color BackgroundFillColor { get; } = new Color(Colors.Black, 0.05f);
        private static Color BackgroundBorderColor { get; } = new Color(Colors.Black, 0.25f);
        private static Color IntersectColor = new Color(SystemColors.ControlText, 0.6f);

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            if (DataContext is Profile profile)
            {
                _getArea = profile.OutputMode.GetterFor(Foreground);
            }
            else
            {
                _getArea = null;
            }

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var graphics = e.Graphics;

            if (_getArea == null)
            {
                DrawText(graphics, "Invalid area!");
            }
            else
            {
                DrawArea(graphics, _getArea());
            }
        }

        private void DrawText(Graphics graphics, string text)
        {
            var formattedText = new FormattedText
            {
                Text = text,
                Font = Font,
                ForegroundBrush = TextBrush
            };

            using (graphics.SaveTransformState())
            {
                graphics.TranslateTransform(Width / 2f, Height / 2f);
                graphics.DrawText(formattedText, PointF.Empty);
            }
        }

        private void DrawArea(Graphics graphics, Area area)
        {
            var scaleX = (Width - 2) / FullBackground.Width;
            var scaleY = (Height - 2) / FullBackground.Height;
            Scale = scaleX > scaleY ? scaleY : scaleX;

            var clientCenter = new PointF(Width, Height);
            var backgroundCenter = new PointF(FullBackground.Width, FullBackground.Height) * Scale;
            ControlOffset = (clientCenter - backgroundCenter) / 2;

            graphics.TranslateTransform(ControlOffset);

            // Draw background area
            var backgrounds = Backgrounds.Select(r => r * Scale);
            foreach (var rect in backgrounds)
            {
                graphics.FillRectangle(BackgroundFillColor, rect);
                graphics.DrawRectangle(BackgroundBorderColor, rect);

                // Draw borders on intersecting areas
                foreach (var otherRect in backgrounds.Where(x => x != rect))
                {
                    if (rect == otherRect) continue;

                    var match = false;
                    var startPoint = new PointF(0,0);
                    var endPoint = new PointF(0,0);

                    if (rect.Right == otherRect.Left) // right<->left intersection
                    {
                        match = true;
                        startPoint = new PointF(rect.Right, Math.Max(rect.TopRight.Y, otherRect.TopLeft.Y));
                        endPoint = new PointF(rect.Right, Math.Min(rect.BottomRight.Y, otherRect.BottomLeft.Y));
                    }
                    else if (rect.Bottom == otherRect.Top) // top<->bottom intersection
                    {
                        match = true;
                        startPoint = new PointF(Math.Max(rect.BottomLeft.X, otherRect.TopLeft.X), rect.Bottom);
                        endPoint = new PointF(Math.Min(rect.BottomRight.X, otherRect.TopRight.X), rect.Bottom);
                    }

                    if (match)
                        graphics.DrawLine(IntersectColor, startPoint, endPoint);
                }
            }

            // Draw foreground area
            using (graphics.SaveTransformState())
            {
                var offset = new PointF(area.XPosition, area.YPosition) * Scale;
                graphics.TranslateTransform(offset);

                if (area is AngledArea angledArea)
                    graphics.RotateTransform(angledArea.Rotation);

                var size = new SizeF(area.Width, area.Height) * Scale;
                var foreground = RectangleF.FromCenter(PointF.Empty, size);
                graphics.FillRectangle(ForegroundFillColor, foreground);
                graphics.DrawRectangle(ForegroundBorderColor, foreground);

                var centerPoint = RectangleF.FromCenter(PointF.Empty, new SizeF(3, 3));
                graphics.DrawEllipse(SystemColors.ControlText, centerPoint);

                var ratioText = CreateText($"{Math.Round(area.Width / area.Height, 4)}");
                var ratioSize = ratioText.Measure();
                graphics.DrawText(ratioText, new PointF(-ratioSize.Width / 2, ratioSize.Height / 2));

                var widthText = CreateText(area.Width + Unit);
                var widthSize = widthText.Measure();
                graphics.DrawText(widthText, new PointF(-widthSize.Width / 2, size.Height / 2 - widthSize.Height - 5));

                graphics.RotateTransform(270);

                var heightText = CreateText(area.Height + Unit);
                var heightSize = heightText.Measure();
                graphics.DrawText(heightText, new PointF(-heightSize.Width / 2, size.Width / 2 - heightSize.Height - 5));
            }
        }

        private static FormattedText CreateText(string text)
        {
            return new FormattedText
            {
                Text = text,
                Font = Font,
                ForegroundBrush = TextBrush
            };
        }
    }
}
