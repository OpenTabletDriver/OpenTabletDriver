using System;
using System.IO;
using System.Reflection;
using Eto.Drawing;
using Eto.Forms;
using JKang.IpcServiceFramework.Client;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Contracts;
using OpenTabletDriver.Native;

namespace OpenTabletDriver.UX
{
    public static class App
    {
        public async static void UnhandledException(object sender, Eto.UnhandledExceptionEventArgs e)
        {
            var appInfo = await DriverDaemon.InvokeAsync(d => d.GetApplicationInfo());
            var exception = (Exception)e.ExceptionObject;
            await File.WriteAllLinesAsync(Path.Join(appInfo.AppDataDirectory, "ux.log"),
                new string[]
                {
                    DateTime.Now.ToString(),
                    exception.GetType().FullName,
                    exception.Message,
                    exception.Source,
                    exception.StackTrace,
                    exception.TargetSite.Name
                }
            );
        }

        public static IIpcClient<IDriverDaemon> DriverDaemon => _driverDaemon.Value;
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

        private static readonly Lazy<IIpcClient<IDriverDaemon>> _driverDaemon = new Lazy<IIpcClient<IDriverDaemon>>(() => 
        {
            // Register IPC Clients
            ServiceProvider serviceProvider = new ServiceCollection()
                .AddNamedPipeIpcClient<IDriverDaemon>("OpenTabletDriverUX", "OpenTabletDriver")
                .BuildServiceProvider();

            // Resolve IPC client factory
            IIpcClientFactory<IDriverDaemon> clientFactory = serviceProvider
                .GetRequiredService<IIpcClientFactory<IDriverDaemon>>();

            // Create client
            return clientFactory.CreateClient("OpenTabletDriverUX");
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