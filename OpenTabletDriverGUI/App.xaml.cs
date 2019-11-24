using System;
using System.Linq;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Controls.ApplicationLifetimes;
using OpenTabletDriverGUI.ViewModels;
using OpenTabletDriverGUI.Views;
using Avalonia.Controls;
using System.Collections.Generic;

namespace OpenTabletDriverGUI
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Program.UnhandledException);

                var viewModel = new MainWindowViewModel();
                viewModel.Initialize();
                MainWindow = new MainWindow
                {
                    DataContext = viewModel
                };
                MainWindow.Show();
            }
            base.OnFrameworkInitializationCompleted();
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
            if (Windows.FirstOrDefault(w => w != MainWindow) is Window window)
                window.Close();
            
            MainWindow.Focus();
        }

        public static MainWindow MainWindow 
        {
            set => (App.Current.ApplicationLifetime as ClassicDesktopStyleApplicationLifetime).MainWindow = value;
            get => (App.Current.ApplicationLifetime as ClassicDesktopStyleApplicationLifetime).MainWindow as MainWindow;
        }

        public static IReadOnlyList<Window> Windows
        {
            get => (App.Current.ApplicationLifetime as ClassicDesktopStyleApplicationLifetime).Windows;
        }
   }
}