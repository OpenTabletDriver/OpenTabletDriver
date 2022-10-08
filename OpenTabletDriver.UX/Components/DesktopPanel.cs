using System.Diagnostics;
using Eto.Forms;
using OpenTabletDriver.Desktop.Reflection;

namespace OpenTabletDriver.UX.Components
{
    public class DesktopPanel : Panel
    {
        public DesktopPanel()
        {
            if (Debugger.IsAttached)
            {
                var type = GetType();
                var group = type.GetFullyQualifiedName();
                MouseEnter += (_, _) => Console.WriteLine($"[{group}] {DataContext?.ToString() ?? "Null DataContext"}");
            }
        }

        public BindableBinding<DesktopPanel, object?> DataContextBinding => new BindableBinding<DesktopPanel, object?>(
            this,
            c => c.DataContext,
            (p, o) => p.DataContext = o,
            (p, h) => p.DataContextChanged += h,
            (p, h) => p.DataContextChanged -= h
        );
    }
}
