using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Eto.Forms;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Plugin.Logging;
using OpenTabletDriver.Plugin.Tablet;

#nullable enable

namespace OpenTabletDriver.UX.RPC
{
    public sealed class DaemonRpcClient : RpcClient<IDriverDaemon>, IDisposable
    {
        private DaemonRpcClient(string serverName, Instance instance) : base(serverName)
        {
            this.instance = instance;
        }

        private readonly Instance instance;

        // This receives the notifications for the singleton instance.
        private RpcHost<IUserInterface>? singletonNotifierChannel;

        public event EventHandler<LogMessage>? Message;
        public event EventHandler<DebugReportData>? DeviceReport;
        public event EventHandler<IEnumerable<TabletReference>>? TabletsChanged;

        /// <summary>
        ///   Connects to the RPC server, and returns a singleton client on success when this is the first instance.
        /// </summary>
        /// <param name="serverName">Name of the server.</param>
        /// <param name="singletonName">
        ///   An identifier to the singleton instance. If an instance with the same identifier exists then this method
        ///   will return null.
        /// </param>
        /// <param name="singletonNotifier">
        ///   If the singleton instance already exists, this function will be invoked where the argument supplied is an
        ///   active RPC connection to the singleton instance.
        /// </param>
        /// <returns>If successful, returns the singleton client, otherwise null.</returns>
        public static async Task<DaemonRpcClient?> Connect(string serverName, string singletonName, Action<RpcClient<IUserInterface>> singletonNotifier)
        {
            // Check if singleton client already exists
            var instance = new Instance(singletonName);
            if (instance.AlreadyExists)
            {
                // If it does, connect to it
                var singletonInstance = new RpcClient<IUserInterface>(singletonName);
                await singletonInstance.Connect();

                // Then send a notification to the singleton client, or whatever the user of the method wants to do
                singletonNotifier(singletonInstance);
                return null;
            }

            // If it doesn't, create a new singleton client
            return new DaemonRpcClient(serverName, instance);
        }

        public void Attach(MainForm host)
        {
            singletonNotifierChannel = new RpcHost<IUserInterface>(instance.Name);
            var thread = new Thread(async () => await singletonNotifierChannel.Run(host))
            {
                IsBackground = true
            };
            thread.Start();
        }

        protected override void OnConnected()
        {
            base.OnConnected();

            Instance.Message += (sender, e) =>
                Application.Instance.AsyncInvoke(() => Message?.Invoke(sender, e));
            Instance.DeviceReport += (sender, e) =>
                Application.Instance.AsyncInvoke(() => DeviceReport?.Invoke(sender, e));
            Instance.TabletsChanged += (sender, e) =>
                Application.Instance.AsyncInvoke(() => TabletsChanged?.Invoke(sender, e));
        }

        public void Dispose()
        {
            instance?.Dispose();
        }
    }
}
