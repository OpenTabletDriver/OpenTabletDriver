using Avalonia;
using Avalonia.Markup.Xaml;

namespace OpenTabletDriverGUI
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
   }
}