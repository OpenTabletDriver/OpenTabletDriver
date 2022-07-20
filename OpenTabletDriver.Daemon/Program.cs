using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Interop.AppInfo;
using OpenTabletDriver.Desktop.RPC;

namespace OpenTabletDriver.Daemon
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            using (var instance = new Instance("OpenTabletDriver.Daemon"))
            {
                if (instance.AlreadyExists)
                {
                    Console.WriteLine("OpenTabletDriver Daemon is already running.");
                    Thread.Sleep(1000);
                    return;
                }

                var rootCommand = new RootCommand("OpenTabletDriver")
                {
                    new Option(new[] { "--appdata", "-a" }, "Application data directory")
                    {
                        Argument = new Argument<string>("appdata")
                    },
                    new Option(new[] { "--config", "-c" }, "Configuration directory")
                    {
                        Argument = new Argument<string> ("config")
                    }
                };

                rootCommand.Handler = CommandHandler.Create<string, string>(Run);
                await rootCommand.InvokeAsync(args);
            }
        }

        private static async Task Run(string appdata, string config)
        {
            var serviceCollection = DesktopServiceCollection.GetPlatformServiceCollection();
            var appInfo = AppInfo.GetPlatformAppInfo();

            if (!string.IsNullOrWhiteSpace(appdata))
                appInfo.AppDataDirectory = FileUtilities.InjectEnvironmentVariables(appdata);
            if (!string.IsNullOrWhiteSpace(config))
                appInfo.ConfigurationDirectory = FileUtilities.InjectEnvironmentVariables(config);

            serviceCollection.AddSingleton(appInfo);

            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            {
                var exception = (Exception)e.ExceptionObject;
                File.AppendAllLines(Path.Join(appInfo.AppDataDirectory, "daemon.log"),
                    new[]
                    {
                        DateTime.Now.ToString(CultureInfo.InvariantCulture),
                        exception.GetType().FullName!,
                        exception.Message,
                        exception.Source ?? string.Empty,
                        exception.StackTrace ?? string.Empty,
                        exception.TargetSite?.Name ?? string.Empty
                    }
                );
            };

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var daemon = serviceProvider.CreateInstance<DriverDaemon>();

            var rpcHost = new RpcHost<DriverDaemon>("OpenTabletDriver.Daemon");
            rpcHost.ConnectionStateChanged += (_, state) =>
                Log.Write("IPC", $"{(state ? "Connected to" : "Disconnected from")} a client.", LogLevel.Debug);

            await daemon.Initialize();
            await rpcHost.Run(daemon);
        }
    }
}
