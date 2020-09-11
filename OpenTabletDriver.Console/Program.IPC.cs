using System;
using JKang.IpcServiceFramework.Client;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Contracts;

namespace OpenTabletDriver.Console
{
    partial class Program
    {
        public static IIpcClient<IDriverDaemon> DriverDaemon => _driverDaemon.Value;
        private static Lazy<IIpcClient<IDriverDaemon>> _driverDaemon = new Lazy<IIpcClient<IDriverDaemon>>(() => 
        {
            // Register IPC Clients
            ServiceProvider serviceProvider = new ServiceCollection()
                .AddNamedPipeIpcClient<IDriverDaemon>("OpenTabletDriver.Console", "OpenTabletDriver")
                .BuildServiceProvider();

            // Resolve IPC client factory
            IIpcClientFactory<IDriverDaemon> clientFactory = serviceProvider
                .GetRequiredService<IIpcClientFactory<IDriverDaemon>>();

            // Create client
            return clientFactory.CreateClient("OpenTabletDriver.Console");
        });
    }
}