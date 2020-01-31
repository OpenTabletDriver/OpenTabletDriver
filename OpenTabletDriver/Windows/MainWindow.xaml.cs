using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OpenTabletDriver.Windows
{
    public class MainWindow : Window, IViewModelRoot<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public MainWindowViewModel ViewModel
        {
            set => DataContext = value;
            get => (MainWindowViewModel)DataContext;
        }
    }
}