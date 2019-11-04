using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
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
            var mouseButtonsBox = this.Find<ComboBox>("MouseButtonsBox");
            mouseButtonsBox.Items = Enum.GetValues(typeof(TabletDriverLib.Interop.Input.MouseButton));
        }
    }
}