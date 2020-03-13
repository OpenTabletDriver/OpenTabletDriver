using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OpenTabletDriver.Windows
{
    public class MainWindow : Window, IViewModelRoot<MainWindowViewModel>
    {
        public MainWindow()
        {
            ViewModel = new MainWindowViewModel();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.Closing += (s, e) => ViewModel.DisposeResidents();
        }

        public MainWindowViewModel ViewModel
        {
            set => DataContext = value;
            get => (MainWindowViewModel)DataContext;
        }
    }
}