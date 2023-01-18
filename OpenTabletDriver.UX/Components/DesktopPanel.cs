using Eto.Forms;

namespace OpenTabletDriver.UX.Components
{
    public class DesktopPanel : Panel
    {
        public BindableBinding<DesktopPanel, object?> DataContextBinding => new BindableBinding<DesktopPanel, object?>(
            this,
            c => c.DataContext,
            (p, o) => p.DataContext = o,
            (p, h) => p.DataContextChanged += h,
            (p, h) => p.DataContextChanged -= h
        );
    }
}
