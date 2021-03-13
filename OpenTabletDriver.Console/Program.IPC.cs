using System;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Desktop.RPC;

namespace OpenTabletDriver.Console
{
    partial class Program
    {
        public static RpcClient<IDriverDaemon> Driver => _driverDaemon;
        private static RpcClient<IDriverDaemon> _driverDaemon = new RpcClient<IDriverDaemon>("OpenTabletDriver.Daemon");
    }
}
