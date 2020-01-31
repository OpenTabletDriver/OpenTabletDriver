using System;
using System.Linq;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using System.Collections.Generic;
using OpenTabletDriver.Windows;

namespace OpenTabletDriver
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override async void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Program.UnhandledException);

                MainWindow = new MainWindow();
                await MainWindow.ViewModel.Initialize();
                MainWindow.Show();
            }
            base.OnFrameworkInitializationCompleted();
        }

        public static void SetTheme(StyleInclude style)
        {
            App.Current.Styles[1] = style;
        }

        public static void Restart(MainWindowViewModel vm)
        {
            MainWindow = new MainWindow
            {
                ViewModel = vm
            };
            MainWindow.Show();
            
            foreach (Window window in Windows.Where(w => w != MainWindow))
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