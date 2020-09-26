using System;
using OpenTabletDriver.Contracts;
using OpenTabletDriver.RPC;

namespace OpenTabletDriver.Console
{
    partial class Program
    {
        public static RpcClient<IDriverDaemon> Driver => _driverDaemon.Value;
        private static Lazy<RpcClient<IDriverDaemon>> _driverDaemon = new Lazy<RpcClient<IDriverDaemon>>(() => 
        {
            return new RpcClient<IDriverDaemon>("OpenTabletDriver.Daemon");
        });
    }
}