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

namespace OpenTabletDriverDaemon
{
    using static Log;

    partial class Program
    {
        static async Task Main(string[] args)
        {
            Write("Daemon", "Welcome to OpenTabletDriver");
            Daemon = new DriverDaemon();
            Write("Daemon", "Driver interface created.");

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
