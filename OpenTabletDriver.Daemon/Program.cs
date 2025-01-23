using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Daemon.Contracts;
using OpenTabletDriver.Daemon.Contracts.RPC;
using OpenTabletDriver.Daemon.Library;
using OpenTabletDriver.Daemon.Library.Interop;
using OpenTabletDriver.Interop;

namespace OpenTabletDriver.Daemon.Executable
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

                rootCommand.Handler = CommandHandler.Create<string, string>(InvokeAsync);
                await rootCommand.InvokeAsync(args);
            }
        }

        private static async Task InvokeAsync(string appdata, string config)
        {
            var serviceCollection = DesktopServiceCollection.GetPlatformServiceCollection();
            var appInfo = GetPlatformAppInfo();

            if (!string.IsNullOrWhiteSpace(appdata))
                appInfo.AppDataDirectory = FileUtilities.InjectEnvironmentVariables(appdata);
            if (!string.IsNullOrWhiteSpace(config))
                appInfo.ConfigurationDirectory = FileUtilities.InjectEnvironmentVariables(config);

            serviceCollection.AddSingleton(appInfo)
                .AddSingleton(s => s.CreateInstance<RpcHost<IDriverDaemon>>("OpenTabletDriver.Daemon"))
                .AddSingleton<IDriverDaemon, DriverDaemon>();

            TaskScheduler.UnobservedTaskException += (_, e) =>
            {
                Log.Exception(e.Exception);
                e.SetObserved();
            };

            try
            {
                await RunAsync(serviceCollection.BuildServiceProvider());
            }
            catch (Exception e)
            {
                Log.Exception(e, LogLevel.Fatal);
                Environment.Exit(1);
            }
        }

        private static async Task RunAsync(IServiceProvider serviceProvider)
        {
            var daemon = serviceProvider.GetRequiredService<IDriverDaemon>();
            var rpcHost = serviceProvider.GetRequiredService<RpcHost<IDriverDaemon>>();
            rpcHost.ConnectionStateChanged += (_, state) =>
                Log.Write("IPC", $"{(state ? "Connected to" : "Disconnected from")} a client.", LogLevel.Debug);

            await daemon.Initialize();
            await rpcHost.Run(daemon);
        }

        private static AppInfo GetPlatformAppInfo()
        {
            return SystemInterop.CurrentPlatform switch
            {
                SystemPlatform.Windows => new WindowsAppInfo(),
                SystemPlatform.Linux => new LinuxAppInfo(),
                SystemPlatform.MacOS => new MacOSAppInfo(),
                _ => throw new PlatformNotSupportedException("This platform is not supported by OpenTabletDriver.")
            };
        }
    }
}
