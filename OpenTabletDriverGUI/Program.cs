using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging.Serilog;
using Avalonia.ReactiveUI;
using NativeLib;

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
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;
            Thread.CurrentThread.Name = "OpenTabletDriverGUI";
            var rootCommand = new RootCommand("OpenTabletDriver")
            {
                new Option(new string[] { "--settings", "-s" }, "Settings directory")
                {
                    Argument = new Argument<DirectoryInfo>("settings")
                },
                new Option(new string[] { "--config", "-c" }, "Configuration directory")
                {
                    Argument = new Argument<DirectoryInfo> ("config")
                }
            };
            
            rootCommand.Handler = CommandHandler.Create<DirectoryInfo, DirectoryInfo>((settings, config) => 
            {
                SettingsDirectory = settings ?? GetDefaultSettingsDirectory();
                ConfigurationDirectory = config ?? GetDefaultConfigurationDirectory();
            });
            rootCommand.Invoke(args);
            
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, ShutdownMode.OnLastWindowClose);
        }

        internal static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = (Exception)e.ExceptionObject ?? default;
            var crashPath = Path.Join(SettingsDirectory.FullName, "crash.log");
            File.AppendAllText(crashPath, string.Format("{0}: {1}", DateTime.Now, exception));
        }
        
        internal static DirectoryInfo SettingsDirectory { private set; get; } = GetDefaultSettingsDirectory();
        private static DirectoryInfo GetDefaultSettingsDirectory()
        {
            if (PlatformInfo.IsWindows)
            {
                var appdata = Environment.GetEnvironmentVariable("LOCALAPPDATA");
                return new DirectoryInfo(Path.Join(appdata, "OpenTabletDriver"));
            }
            else if (PlatformInfo.IsLinux)
            {
                var home = Environment.GetEnvironmentVariable("HOME");
                return new DirectoryInfo(Path.Join(home, ".config", "OpenTabletDriver"));
            }
            else if (PlatformInfo.IsOSX)
            {
                var macHome = Environment.GetEnvironmentVariable("HOME");
                return new DirectoryInfo(Path.Join(macHome, "Library", "Application Support", "OpenTabletDriver"));
            }
            else
            {
                return null;
            }
        }

        internal static DirectoryInfo ConfigurationDirectory { private set; get; }
        private static DirectoryInfo GetDefaultConfigurationDirectory()
        {
            var path = Path.Join(Environment.CurrentDirectory, "Configurations");
            return new DirectoryInfo(path);
        }
    }
}
