using System;
using System.Reflection;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Contracts;
using OpenTabletDriver.Native;
using OpenTabletDriver.RPC;

namespace OpenTabletDriver.UX
{
    public static class App
    {
        public const string PluginRepositoryUrl = "https://github.com/InfinityGhost/OpenTabletDriver/wiki/Plugin-Repository";

        public static RpcClient<IDriverDaemon> Driver => _daemon.Value;
        public static Bitmap Logo => _logo.Value;
        public static Padding GroupBoxPadding => _groupBoxPadding.Value;
        public static Settings Settings { set; get; }

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

        private static readonly Lazy<RpcClient<IDriverDaemon>> _daemon = new Lazy<RpcClient<IDriverDaemon>>(() => 
        {
            return new RpcClient<IDriverDaemon>("OpenTabletDriver.Daemon");
        });

        private static readonly Lazy<Bitmap> _logo = new Lazy<Bitmap>(() => 
        {
            var dataStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("OpenTabletDriver.UX.Assets.otd.png");
            return new Bitmap(dataStream);
        });

        private static readonly Lazy<Padding> _groupBoxPadding = new Lazy<Padding>(() => 
        {
            return SystemInfo.CurrentPlatform switch
            {
                RuntimePlatform.Windows => new Padding(0),
                _                       => new Padding(5)
            };
        });
    }
}