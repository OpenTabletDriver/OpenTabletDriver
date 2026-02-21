using System;
using System.CommandLine;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.UX.RPC;
using OpenTabletDriver.UX.Windows;
using OpenTabletDriver.UX.Windows.Greeter;
using OpenTabletDriver.UX.Windows.Plugins;
using OpenTabletDriver.UX.Windows.Tablet;
using OpenTabletDriver.UX.Windows.Updater;

namespace OpenTabletDriver.UX
{
    class CommandLineOptions
    {
        public bool StartMinimized { get; set; }
        public bool SkipUpdate { get; set; }
    }

    public sealed class App : ViewModel
    {
        private App()
        {
        }

        public CancellationTokenSource Canceler { get; } = new();

        public static void Run(string platform, string[] args)
        {
            var commandLineOptions = ParseCmdLineOptions(args);

            using (var mutex = new Mutex(true, @$"Global\{APPNAME}.Mutex", out var firstInstance))
            {
                if (firstInstance)
                {
                    RunInternal(platform, commandLineOptions);
                }
                else
                {
                    using var client = new NamedPipeClientStream(".", APPNAME + ".Singleton", PipeDirection.InOut);
                    client.Connect();
                }
            }
        }

        private static void RunInternal(string platform, CommandLineOptions options)
        {
            var app = new Application(platform);
            var mainForm = new MainForm();
            if (options.StartMinimized)
            {
                mainForm.WindowState = WindowState.Minimized;
                if (EnableTrayIcon)
                {
                    mainForm.Show();
                    mainForm.Visible = true;
                    mainForm.WindowState = WindowState.Minimized;
                    mainForm.ShowInTaskbar = false;
                    mainForm.Visible = false;
                }
            }

            mainForm.SkipUpdate = options.SkipUpdate;
            mainForm.Closing += Current.HandleClosing;

            app.UnhandledException += ShowUnhandledException;
            app.Terminating += async (sender, args) => await Current.Canceler.CancelAsync();

            Task.Run(async () =>
            {
                await using var ipcServer = new NamedPipeServerStream(
                    APPNAME + ".Singleton",
                    PipeDirection.InOut,
                    1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                while (!Current.Canceler.IsCancellationRequested)
                {
                    // if something connects to the pipe, it means another instance is trying to start
                    // no need for any actual communication
                    await ipcServer.WaitForConnectionAsync(Current.Canceler.Token);

                    app.AsyncInvoke(() =>
                    {
                        mainForm.Show();
                        mainForm.BringToFront();
                    });
                    ipcServer.Disconnect();
                }
                ipcServer.Close();
            });

            app.Run(mainForm);
        }

        private static CommandLineOptions ParseCmdLineOptions(string[] args)
        {
            var commandLineOptions = new CommandLineOptions();

            var minimizedOption = new Option<bool>("--minimized", "-m")
            {
                Description = "Start the application minimized"
            };

            var skipUpdate = new Option<bool>("--skipupdate")
            {
                Description = "Skip checking for updates"
            };

            var root = new RootCommand("OpenTabletDriver UX")
            {
                minimizedOption,
                skipUpdate
            };

            var results = root.Parse(args);
            if (results.Action is not null)
            {
                // if we hit a built-in like --help or --version
                results.Invoke();
                Environment.Exit(0);
            }

            commandLineOptions.StartMinimized = results.GetValue(minimizedOption);
            commandLineOptions.SkipUpdate = results.GetValue(skipUpdate);

            return commandLineOptions;
        }

        public static App Current { get; } = new App();

        public const string WikiUrl = "https://opentabletdriver.net/Wiki";
        public static readonly string Version = Assembly.GetEntryAssembly()!.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

        public static DaemonRpcClient Driver { get; } = new DaemonRpcClient("OpenTabletDriver.Daemon");
        public static Bitmap Logo { get; } = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("OpenTabletDriver.UX.Assets.otd.png")!);

        public static Uri Website { get; } = new Uri(@"https://github.com/OpenTabletDriver/OpenTabletDriver");
        public static string License { get; } = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("OpenTabletDriver.UX.LICENSE")!).ReadToEnd();

        private Settings? settings;
        public Settings Settings
        {
            set => this.RaiseAndSetIfChanged(ref this.settings!, value);
            get => this.settings ?? throw new InvalidOperationException("Settings cannot be null");
        }

        private const string APPNAME = "OpenTabletDriver.UX";
        public readonly static bool EnableTrayIcon = (PluginPlatform.Windows | PluginPlatform.MacOS).HasFlag(SystemInterop.CurrentPlatform);
        public readonly static bool EnableDaemonWatchdog = (PluginPlatform.Windows | PluginPlatform.MacOS).HasFlag(SystemInterop.CurrentPlatform);
        public static DaemonWatchdog? DaemonWatchdog;

        public WindowSingleton<StartupGreeterWindow> StartupGreeterWindow { get; } = new WindowSingleton<StartupGreeterWindow>();
        public WindowSingleton<PluginManagerWindow> PluginManagerWindow { get; } = new WindowSingleton<PluginManagerWindow>();
        public WindowSingleton<TabletDebugger> DebuggerWindow { get; } = new WindowSingleton<TabletDebugger>();
        public WindowSingleton<DeviceStringReader> StringReaderWindow { get; } = new WindowSingleton<DeviceStringReader>();
        public WindowSingleton<UpdaterWindow> UpdaterWindow { get; } = new WindowSingleton<UpdaterWindow>();

        public WindowSingleton<AboutWindow> AboutWindow { get; } = new WindowSingleton<AboutWindow>();

        private void HandleClosing(object? sender, CancelEventArgs args)
        {
            StartupGreeterWindow.Close();
            PluginManagerWindow.Close();
            DebuggerWindow.Close();
            StringReaderWindow.Close();
            UpdaterWindow.Close();
            AboutWindow.Close();
        }

        private static void ShowUnhandledException(object? sender, Eto.UnhandledExceptionEventArgs e)
        {
            try
            {
                var exception = e.ExceptionObject as Exception;
                Log.Exception(exception);
                exception!.ShowMessageBox();
            }
            catch (Exception ex)
            {
                // Stops recursion of exceptions if the messagebox itself throws an exception
                Log.Exception(ex);
            }
        }
    }
}
