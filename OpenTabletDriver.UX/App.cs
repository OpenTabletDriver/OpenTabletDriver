using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Reflection;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Desktop.Migration;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.UX
{
    public static class App
    {
        public static void Run(string platform, string[] args)
        {
            UserInterfaceProxy.Invoke(async userInterface =>
            {
                await userInterface.ShowClient();
                UserInterfaceProxy.Dispose();
                Environment.Exit(0);
            });

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
            UserInterfaceProxy.Attach(mainForm);

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

        public const string FaqUrl = "https://github.com/InfinityGhost/OpenTabletDriver/wiki#frequently-asked-questions";

        public static RpcProxy<IUserInterface> UserInterfaceProxy = new RpcProxy<IUserInterface>("OpenTabletDriver.UX");
        public static RpcClient<IDriverDaemon> Driver { get; } = new RpcClient<IDriverDaemon>("OpenTabletDriver.Daemon");
        public static Bitmap Logo => _logo.Value;

        public static event Action<Settings> SettingsChanged;
        private static Settings settings;
        public static Settings Settings
        {
            set
            {
                settings = SettingsMigrator.Migrate(value);
                SettingsChanged?.Invoke(Settings);
            }
            get => settings;
        }

        public static AboutDialog AboutDialog => new AboutDialog
        {
            Title = "OpenTabletDriver",
            ProgramName = "OpenTabletDriver",
            ProgramDescription = "Open source, cross-platform tablet configurator",
            WebsiteLabel = "OpenTabletDriver GitHub Repository",
            Website = new Uri(@"https://github.com/InfinityGhost/OpenTabletDriver"),
            Version = $"v{Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion}",
            Developers = new string[] { "InfinityGhost" },
            Designers = new string[] { "InfinityGhost" },
            Documenters = new string[] { "InfinityGhost" },
            License = string.Empty,
            Copyright = string.Empty,
            Logo = Logo.WithSize(256, 256)
        };

        public readonly static bool EnableTrayIcon = SystemInterop.CurrentPlatform switch
        {
            PluginPlatform.Windows => true,
            PluginPlatform.MacOS   => true,
            _                       => false
        };

        private static readonly Lazy<Bitmap> _logo = new Lazy<Bitmap>(() =>
        {
            var dataStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("OpenTabletDriver.UX.Assets.otd.png");
            return new Bitmap(dataStream);
        });
    }
}
