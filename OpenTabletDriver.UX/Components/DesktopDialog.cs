using Eto.Forms;

namespace OpenTabletDriver.UX.Components
{
    public class DesktopDialog<T> : Dialog<T>
    {
        public BindableBinding<DesktopDialog<T>, object?> DataContextBinding => new BindableBinding<DesktopDialog<T>, object?>(
            this,
            c => c.DataContext,
            (d, v) => d.DataContext = v,
            (d, h) => d.DataContextChanged += h,
            (d, h) => d.DataContextChanged -= h
        );
    }
}
