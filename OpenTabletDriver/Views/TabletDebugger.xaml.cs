using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OpenTabletDriver.Views
{
    public class TabletDebugger : Window
    {
        public TabletDebugger()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}