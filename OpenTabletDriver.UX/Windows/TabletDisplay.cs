using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.UX.Windows
{
    public class TabletDisplay : Drawable
    {
        private readonly App _app;
        private DebugReportData? _data;
        private TabletConfiguration? _configuration;

        public TabletDisplay(App app)
        {
            _app = app;
        }

        private static Color PointColor { get; } = Colors.White;
        private static Color PointLinesColor { get; } = new Color(Colors.White, 0.25f);
        private static Color BackgroundFillColor { get; } = new Color(SystemColors.Highlight, 0.75f);
        private static Color BackgroundBorderColor { get; } = SystemColors.ControlText;

        private static Font TextFont { get; } = Fonts.Monospace(8);
        private static SolidBrush TextBrush { get; } = new SolidBrush(PointLinesColor);

        public void Draw(DebugReportData reportData)
        {
            _data = reportData;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_data == null)
                return;

            if (_configuration?.ToString() != _data.DeviceName)
                _configuration = _app.GetTablet(_data.DeviceName);

            if (_configuration != null)
                DrawDigitizer(e.Graphics);
        }

        private void DrawDigitizer(Graphics graphics)
        {
            var digitizer = _configuration!.Specifications.Digitizer!;
            var background = new RectangleF(0, 0, digitizer.MaxX, digitizer.MaxY);

            var scaleX = (Width - 5) / background.Width;
            var scaleY = (Height - 5) / background.Height;
            var scale = scaleX > scaleY ? scaleY : scaleX;

            var offsetX = (Width - background.Width * scale) / 2;
            var offsetY = (Height - background.Height * scale) / 2;

            using (graphics.SaveTransformState())
            {
                graphics.TranslateTransform(offsetX, offsetY);

                var scaledBackground = background * scale;
                graphics.FillRectangle(BackgroundFillColor, scaledBackground);
                graphics.DrawRectangle(BackgroundBorderColor, scaledBackground);

                if (_data!.RawPosition == null)
                    return;

                var x = _data.RawPosition.Value.X * scale;
                var y = _data.RawPosition.Value.Y * scale;
                var point = RectangleF.FromCenter(new PointF(x, y), new SizeF(5, 5));
                graphics.DrawEllipse(PointColor, point);

                graphics.DrawLine(PointLinesColor, x, 0, x, scaledBackground.Height);
                graphics.DrawLine(PointLinesColor, 0, y, scaledBackground.Width, y);

                var text = new FormattedText
                {
                    Font = TextFont,
                    ForegroundBrush = TextBrush,
                    Text = _data.RawPosition.ToString(),
                };

                var textPos = new PointF(x + 5, y + 5);
                var textSize = text.Measure();

                if (textSize.Height + textPos.Y > Height - 10)
                    textPos.Y -= textSize.Height + 10;
                if (textSize.Width + textPos.X > Width - 10)
                    textPos.X -= textSize.Width + 10;

                graphics.DrawText(text, textPos);
            }
        }
    }
}
