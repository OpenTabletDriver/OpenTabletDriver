using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OpenTabletDriverGUI.Views
{
    public class ConfigurationManager : Window
    {
        public ConfigurationManager()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}