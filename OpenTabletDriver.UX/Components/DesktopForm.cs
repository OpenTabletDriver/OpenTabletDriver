using Eto.Forms;
using JetBrains.Annotations;

namespace OpenTabletDriver.UX.Components
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class DesktopForm : Form
    {
        public BindableBinding<DesktopForm, object?> DataContextBinding => new BindableBinding<DesktopForm, object?>(
            this,
            c => c.DataContext,
            (p, o) => p.DataContext = o,
            (p, h) => p.DataContextChanged += h,
            (p, h) => p.DataContextChanged -= h
        );

        public BindableBinding<DesktopForm, string> TitleBinding => new BindableBinding<DesktopForm, string>(
            this,
            c => c.Title,
            (c, v) => Application.Instance.AsyncInvoke(() => c.Title = v),
            (c, e) => c.TitleChanged += e,
            (c, e) => c.TitleChanged -= e
        );

        #pragma warning disable CS0067
        public event EventHandler<EventArgs>? TitleChanged;
    }
}
