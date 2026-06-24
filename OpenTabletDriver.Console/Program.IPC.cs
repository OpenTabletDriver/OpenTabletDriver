using System.Threading.Tasks;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Desktop.RPC;

namespace OpenTabletDriver.Console
{
    partial class Program
    {
        public static readonly RpcClient<IDriverDaemon> Driver = new RpcClient<IDriverDaemon>("OpenTabletDriver.Daemon");

        /// <summary>
        /// Connect to daemon if not already connected and ensure plugins are loaded
        /// </summary>
        /// <returns><c>true</c> if connected successfully, <c>false</c> if daemon couldn't be found</returns>
        /// <remarks>
        /// <c>Driver.Instance</c> is guaranteed to be available after running this function.
        /// </remarks>
        public static async Task<bool> EnsureDaemonReady()
        {
            if (!Instance.Exists("OpenTabletDriver.Daemon"))
            {
                System.Console.WriteLine("OpenTabletDriver Daemon not running");
                return false;
            }

            if (!Driver.IsConnected)
                await Driver.Connect();

            if (!pluginsLoaded)
            {
                pluginsLoaded = true;
                AppInfo.PluginManager.Load();
            }

            return true;
        }

    }
}
