using Avalonia.Controls;
using System.Linq;
using ReactiveUI;
using Avalonia.Controls.ApplicationLifetimes;

namespace OpenTabletDriver
{
    public class ViewModelBase : ReactiveObject
    {
        public Window GetParentWindow() => GetParentWindow<Window>();

        public T GetParentWindow<T>() where T : Window
        {
            var matching = from window in (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).Windows
                where window.DataContext == this
                where window is T
                select (T)window;
            return matching.FirstOrDefault();
        }
    }
}
