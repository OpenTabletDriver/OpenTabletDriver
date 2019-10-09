using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OpenTabletDriverGUI.ViewModels;

namespace OpenTabletDriverGUI.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}