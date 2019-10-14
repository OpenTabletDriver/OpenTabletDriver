using System;
using System.Linq;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using OpenTabletDriverGUI.ViewModels;
using OpenTabletDriverGUI.Views;

namespace OpenTabletDriverGUI
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void SetTheme(StyleInclude style)
        {
            Styles[1] = style;
        }

        public void Restart(MainWindowViewModel vm)
        {
            MainWindow = new MainWindow()
            {
                DataContext = vm
            };
            MainWindow.Show();
            Windows[0].Close();
        }
   }
}