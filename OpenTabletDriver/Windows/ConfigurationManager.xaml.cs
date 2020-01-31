using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OpenTabletDriver.Windows
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