using System.Linq.Expressions;
using Eto.Drawing;
using OpenTabletDriver.Output;

namespace OpenTabletDriver.UX.Controls.Editors
{
    public class OutputAreaDisplay : AreaDisplay
    {
        private readonly App _app;
        private IEnumerable<RectangleF>? _backgrounds;

        public OutputAreaDisplay(App app)
        {
            _app = app;
        }

        protected override string Unit => "px";
        protected override Expression<Func<AbsoluteOutputMode, Area>> Foreground { get; } = m => m.Output!;

        protected override IEnumerable<RectangleF> Backgrounds => _backgrounds ??=
            _app.Displays.Select(d => new RectangleF(d.Position.X, d.Position.Y, d.Width, d.Height)).ToArray();
    }
}
