using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OpenTabletDriver.ViewModels;

namespace OpenTabletDriver.Views
{
    public class BindingConfig : Window
    {
        public BindingConfig(string binding)
        {
            DataContext = new BindingConfigViewModel(binding);
            InitializeComponent();
        }

        public BindingConfig()
        {
            DataContext = new BindingConfigViewModel();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public string Binding
        {
            set => ViewModel.Binding = value;
            get => ViewModel.Binding;
        }

        public BindingConfigViewModel ViewModel
        {
            set => DataContext = value;
            get => (BindingConfigViewModel)DataContext;
        }
    }
}