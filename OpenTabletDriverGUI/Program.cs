using System;
using Avalonia;
using Avalonia.Logging.Serilog;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using OpenTabletDriverGUI.ViewModels;
using OpenTabletDriverGUI.Views;

namespace OpenTabletDriverGUI
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args) => BuildAvaloniaApp().Start(AppMain, args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToDebug()
                .UseReactiveUI();

        // Your application's entry point. Here you can initialize your MVVM framework, DI
        // container, etc.
        private static void AppMain(Application app, string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledException);

            var viewModel = new MainWindowViewModel();
            viewModel.Initialize();
            var window = new MainWindow
            {
                DataContext = viewModel
            };

            app.Run(window);
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = (Exception)e.ExceptionObject ?? default;
            var msgbox = MessageBoxWindow.CreateCustomWindow(new MessageBoxCustomParams
            {
                ContentTitle = "Fatal Exception",
                ContentHeader = exception.GetType().FullName,
                ContentMessage = exception.ToString(),
                ShowInCenter = true,
                CanResize = false,
            });
            msgbox.Show();
        }
    }
}
