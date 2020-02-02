using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OpenTabletDriver.Windows
{
    public class ConfigurationManager : Window, IViewModelRoot<ConfigurationManagerViewModel>
    {
        public ConfigurationManager()
        {
            InitializeComponent();
        }

        public ConfigurationManager(ConfigurationManagerViewModel viewModel) : this()
        {
            ViewModel = viewModel;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public ConfigurationManagerViewModel ViewModel
        {
            set => DataContext = value;
            get => (ConfigurationManagerViewModel)DataContext;
        }
    }
}