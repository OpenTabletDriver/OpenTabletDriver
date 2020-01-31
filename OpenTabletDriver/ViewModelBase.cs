using Avalonia.Controls;
using System.Linq;
using ReactiveUI;

namespace OpenTabletDriver
{
    public class ViewModelBase : ReactiveObject
    {
        public Window GetRootWindow() => GetRootWindow<Window>();

        public T GetRootWindow<T>() where T : Window
        {
            var matching = from window in App.Windows
                where window.DataContext == this
                where window is T
                select (T)window;
            return matching.FirstOrDefault();
        }
    }
}
