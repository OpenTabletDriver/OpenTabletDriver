using System;
using System.CommandLine;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Daemon;
using OpenTabletDriver.Daemon.Contracts;
using OpenTabletDriver.Daemon.Contracts.RPC;

namespace OpenTabletDriver.Console
{
    public class ApplicationLifetime : IApplicationLifetime
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly RpcClient<IDriverDaemon> _rpcClient;

        public ApplicationLifetime(IServiceProvider serviceProvider, RpcClient<IDriverDaemon> rpcClient)
        {
            _serviceProvider = serviceProvider;
            _rpcClient = rpcClient;
        }

        public async Task Run(string[] args)
        {
            if (!Instance.Exists("OpenTabletDriver.Daemon"))
            {
                System.Console.WriteLine("OpenTabletDriver Daemon not running");
                Environment.Exit(1);
            }

            await _rpcClient.ConnectAsync();

            var commands = ActivatorUtilities.CreateInstance<ProgramCommands>(_serviceProvider);
            var root = commands.Build("otd");
            await root.InvokeAsync(args);
        }
    }
}
