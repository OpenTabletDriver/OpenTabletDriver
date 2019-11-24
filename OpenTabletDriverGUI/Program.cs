using System;
using System.IO;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Logging.Serilog;
using Avalonia.ReactiveUI;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using OpenTabletDriverGUI.ViewModels;
using OpenTabletDriverGUI.Views;

namespace OpenTabletDriverGUI
{
    class Program
    {
        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToDebug()
                .UseReactiveUI();
        
        public static void Main(string[] args) 
        {
            if (args.Length > 0)
                Environment.CurrentDirectory = args[0];
            
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, ShutdownMode.OnLastWindowClose);
        }

        internal static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
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
