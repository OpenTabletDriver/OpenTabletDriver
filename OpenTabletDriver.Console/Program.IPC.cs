using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Desktop.RPC;

namespace OpenTabletDriver.Console
{
    internal partial class Program
    {
        public static readonly RpcClient<IDriverDaemon> Driver = new RpcClient<IDriverDaemon>("OpenTabletDriver.Daemon");
    }
}
