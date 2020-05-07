using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Eto.Drawing;
using Eto.Forms;
using JKang.IpcServiceFramework.Client;
using Microsoft.Extensions.DependencyInjection;
using TabletDriverLib;
using TabletDriverLib.Contracts;
using TabletDriverPlugin;

namespace OpenTabletDriverUX
{
    public static class App
    {
        public static IIpcClient<IDriverDaemon> DriverDaemon => _driverDaemon.Value;
        public static AboutDialog AboutDialog => _aboutDialog.Value;
        public static Bitmap Logo => _logo.Value;

        public static IReadOnlyDictionary<string, Color> ColorDictionary => _colorDict.Value;

        private static Lazy<Dictionary<string, Color>> _colorDict = new Lazy<Dictionary<string, Color>>(() => 
        {
            var colors = new Dictionary<string, Color>();
            var ctor = themeSource.GetConstructor(new Type[0]);
            var obj = ctor.Invoke(null);
            
            foreach (var property in themeSource.GetProperties())
			{
				if (property.PropertyType == typeof(Color))
				{
                    var color = (Color)property.GetValue(obj);
                    colors.Add(property.Name, color);
				}
			}
            return colors;
        });

        private static Type themeSource;

        public static void ThemeSetup(Type type)
        {
            themeSource = type;
        }

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

        private static readonly Lazy<AboutDialog> _aboutDialog = new Lazy<AboutDialog>(() => new AboutDialog
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
        });

        private static readonly Lazy<Bitmap> _logo = new Lazy<Bitmap>(() => 
        {
            var dataStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("OpenTabletDriverUX.Assets.otd.png");
            return new Bitmap(dataStream);
        });
    }
}