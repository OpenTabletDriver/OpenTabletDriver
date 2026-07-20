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
        /// <returns>Driver instance if connected successfully, <c>null</c> if daemon couldn't be found</returns>
        public static async Task<IDriverDaemon?> GetDaemon()
        {
            if (!Instance.Exists("OpenTabletDriver.Daemon"))
            {
                System.Console.WriteLine("OpenTabletDriver Daemon not running");
                return null;
            }

            if (!Driver.IsConnected)
                await Driver.Connect();

            if (!pluginsLoaded)
            {
                pluginsLoaded = true;
                AppInfo.PluginManager.Load();
            }

            return Driver.Instance;
        }

    }
}
