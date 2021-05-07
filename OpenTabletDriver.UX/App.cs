using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Reflection;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.UX.Tools;

namespace OpenTabletDriver.UX
{
    public class App : Bindable
    {
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

        public const string FaqUrl = "https://github.com/OpenTabletDriver/OpenTabletDriver/wiki#frequently-asked-questions";
        public static readonly string Version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        private static readonly Lazy<Bitmap> _logo = new Lazy<Bitmap>(() =>
        {
            var dataStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("OpenTabletDriver.UX.Assets.otd.png");
            return new Bitmap(dataStream);
        });

        public readonly static bool EnableTrayIcon = DesktopInterop.CurrentPlatform switch
        {
            PluginPlatform.Windows => true,
            PluginPlatform.MacOS => true,
            _ => false
        };

        public static RpcClient<IDriverDaemon> Driver { get; } = new RpcClient<IDriverDaemon>("OpenTabletDriver.Daemon");
        public static Bitmap Logo => _logo.Value;

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

        public static readonly App Current = new App();

        private Settings settings;
        private ProfileCache profileCache;

        public event EventHandler<EventArgs> SettingsChanged;
        public event EventHandler<EventArgs> ProfileCacheChanged;

        public Settings Settings
        {
            get => this.settings;
            set
            {
                settings = value;
                SettingsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public ProfileCache ProfileCache
        {
            get => this.profileCache;
            set
            {
                profileCache = value;
                ProfileCacheChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public BindableBinding<App, Settings> SettingsBinding
        {
            get
            {
                return new BindableBinding<App, Settings>(
                    this,
                    a => a.Settings,
                    (a, s) => a.Settings = s,
                    (a, h) => a.SettingsChanged += h,
                    (a, h) => a.SettingsChanged -= h
                );
            }
        }

        public BindableBinding<App, Profile> ProfileBinding
        {
            get
            {
                return new BindableBinding<App, Profile>(
                    this,
                    a => a.ProfileCache.ProfileInFocus,
                    (a, p) => a.ProfileCache.ProfileInFocus = p,
                    (a, h) => a.ProfileCache.ProfileInFocusChanged += h,
                    (a, h) => a.ProfileCache.ProfileInFocusChanged -= h
                );
            }
        }
    }
}
