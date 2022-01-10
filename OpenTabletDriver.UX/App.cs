using System;
using System.Collections;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Reflection;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.UX.RPC;
using OpenTabletDriver.UX.Windows;
using OpenTabletDriver.UX.Windows.Configurations;
using OpenTabletDriver.UX.Windows.Greeter;
using OpenTabletDriver.UX.Windows.Plugins;
using OpenTabletDriver.UX.Windows.Tablet;
using OpenTabletDriver.UX.Windows.Updater;

namespace OpenTabletDriver.UX
{
    public sealed class App : ViewModel
    {
        private App()
        {
        }

        public static void Run(string platform, string[] args)
        {
            var root = new RootCommand("OpenTabletDriver UX")
            {
                new Option<bool>(new string[] { "-m", "--minimized" }, "Start the application minimized")
                {
                    Argument = new Argument<bool>("minimized")
                }
            };

            bool startMinimized = false;
            root.Handler = CommandHandler.Create<bool>((minimized) =>
            {
                startMinimized = minimized;
            });

            int code = root.Invoke(args);
            if (code != 0)
                Environment.Exit(code);

            var app = new Application(platform);
            var mainForm = new MainForm();
            if (startMinimized)
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

            app.NotificationActivated += Current.HandleNotification;
            app.UnhandledException += ShowUnhandledException;

            app.Run(mainForm);
        }

        public static App Current { get; } = new App();

        public const string WikiUrl = "https://opentabletdriver.net/Wiki";
        public static readonly string Version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        public IDictionary<string, Action> NotificationHandlers { get; } = new Dictionary<string, Action>();

        public static DaemonRpcClient Driver { get; } = new DaemonRpcClient("OpenTabletDriver.Daemon");
        public static Bitmap Logo { get; } = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("OpenTabletDriver.UX.Assets.otd.png"));

        private Settings settings;
        public Settings Settings
        {
            set => this.RaiseAndSetIfChanged(ref this.settings, value);
            get => this.settings;
        }

        public static AboutDialog AboutDialog => new AboutDialog
        {
            Title = "OpenTabletDriver",
            ProgramName = "OpenTabletDriver",
            ProgramDescription = "Open source, cross-platform tablet configurator",
            WebsiteLabel = "OpenTabletDriver GitHub Repository",
            Website = new Uri(@"https://github.com/OpenTabletDriver/OpenTabletDriver"),
            Version = $"v{Version}",
            Developers = new string[] { "InfinityGhost" },
            Designers = new string[] { "InfinityGhost" },
            Documenters = new string[] { "InfinityGhost" },
            License = string.Empty,
            Copyright = string.Empty,
            Logo = Logo.WithSize(256, 256)
        };

        public readonly static bool EnableTrayIcon = (PluginPlatform.Windows | PluginPlatform.MacOS).HasFlag(DesktopInterop.CurrentPlatform);
        public readonly static bool EnableDaemonWatchdog = (PluginPlatform.Windows | PluginPlatform.MacOS).HasFlag(DesktopInterop.CurrentPlatform);

        public WindowSingleton<StartupGreeterWindow> StartupGreeterWindow { get; } = new WindowSingleton<StartupGreeterWindow>();
        public WindowSingleton<ConfigurationEditor> ConfigEditorWindow { get; } = new WindowSingleton<ConfigurationEditor>();
        public WindowSingleton<PluginManagerWindow> PluginManagerWindow { get; } = new WindowSingleton<PluginManagerWindow>();
        public WindowSingleton<TabletDebugger> DebuggerWindow { get; } = new WindowSingleton<TabletDebugger>();
        public WindowSingleton<DeviceStringReader> StringReaderWindow { get; } = new WindowSingleton<DeviceStringReader>();
        public WindowSingleton<UpdaterWindow> UpdaterWindow { get; } = new WindowSingleton<UpdaterWindow>();

        public void AddNotificationHandler(string identifier, Action handler)
        {
            NotificationHandlers.Add(identifier, handler);
        }

        private void HandleNotification(object sender, NotificationEventArgs e)
        {
            if (NotificationHandlers.ContainsKey(e.ID))
                NotificationHandlers[e.ID].Invoke();
        }

        private static void ShowUnhandledException(object sender, Eto.UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            Log.Exception(exception);
            exception.ShowMessageBox();
        }
    }
}
