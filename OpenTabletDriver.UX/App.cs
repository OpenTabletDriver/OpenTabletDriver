using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Reflection;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.UX.Windows;
using OpenTabletDriver.UX.Windows.Configurations;
using OpenTabletDriver.UX.Windows.Greeter;
using OpenTabletDriver.UX.Windows.Plugins;
using OpenTabletDriver.UX.Windows.Tablet;

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

            app.Run(mainForm);
        }

        public static App Current { get; } = new App();

        public const string FaqUrl = "https://github.com/OpenTabletDriver/OpenTabletDriver/wiki#frequently-asked-questions";
        public static readonly string Version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        public static RpcClient<IDriverDaemon> Driver { get; } = new RpcClient<IDriverDaemon>("OpenTabletDriver.Daemon");
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

        public readonly static bool EnableTrayIcon = (PluginPlatform.Windows | PluginPlatform.MacOS).HasFlag(SystemInterop.CurrentPlatform);
        public readonly static bool EnableDaemonWatchdog = (PluginPlatform.Windows | PluginPlatform.MacOS).HasFlag(SystemInterop.CurrentPlatform);

        public WindowSingleton<StartupGreeterWindow> StartupGreeterWindow { get; } = new WindowSingleton<StartupGreeterWindow>();
        public WindowSingleton<ConfigurationEditor> ConfigEditorWindow { get; } = new WindowSingleton<ConfigurationEditor>();
        public WindowSingleton<PluginManagerWindow> PluginManagerWindow { get; } = new WindowSingleton<PluginManagerWindow>();
        public WindowSingleton<TabletDebugger> DebuggerWindow { get; } = new WindowSingleton<TabletDebugger>();
        public WindowSingleton<DeviceStringReader> StringReaderWindow { get; } = new WindowSingleton<DeviceStringReader>();
    }
}
