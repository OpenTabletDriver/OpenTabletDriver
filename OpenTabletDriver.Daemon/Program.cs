using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using JKang.IpcServiceFramework.Hosting.NamedPipe;
using JKang.IpcServiceFramework.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TabletDriverLib;
using TabletDriverLib.Contracts;
using TabletDriverPlugin;
using System.CommandLine;
using System.IO;
using System.CommandLine.Invocation;

namespace OpenTabletDriver.Daemon
{
    using static Log;

    partial class Program
    {
        static async Task Main(string[] args)
        {
            var rootCommand = new RootCommand("OpenTabletDriver")
            {
                new Option(new string[] { "--appdata", "-a" }, "Application data directory")
                {
                    Argument = new Argument<DirectoryInfo>("appdata")
                },
                new Option(new string[] { "--config", "-c" }, "Configuration directory")
                {
                    Argument = new Argument<DirectoryInfo> ("config")
                }
            };
            rootCommand.Handler = CommandHandler.Create<DirectoryInfo, DirectoryInfo>((appdata, config) => 
            {
                AppInfo.AppDataDirectory = appdata;
                AppInfo.ConfigurationDirectory = config;
            });
            rootCommand.Invoke(args);

            Daemon = new DriverDaemon();
            await CreateHostBuilder().Build().RunAsync();
        }

        static IHostBuilder CreateHostBuilder() => 
            Host.CreateDefaultBuilder()
                .ConfigureServices(services => 
                {
                    services.AddSingleton<IDriverDaemon, DriverDaemon>((s) => Daemon);
                })
                .ConfigureIpcHost(builder => 
                {
                    builder.AddNamedPipeEndpoint<IDriverDaemon>("OpenTabletDriver");
                })
                .ConfigureLogging(builder => 
                {                    
                });

        static DriverDaemon Daemon { set; get; }
        static bool Running { set; get; }
    }
}
