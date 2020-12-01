using System;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Desktop.RPC;

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
