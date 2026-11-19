using System;
using System.Collections.Generic;
using System.Diagnostics;
using Eto.Forms;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Plugin.Logging;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.UX.RPC
{
    /// <summary>
    /// The RPC client for <see cref="IDriverDaemon"/>.
    /// </summary>
    public class DaemonRpcClient : RpcClient<IDriverDaemon>
    {
        public DaemonRpcClient(string pipeName) : base(pipeName)
        {
        }

        /// <summary>
        /// Client-side <see cref="IDriverDaemon.Message"/> event.
        /// </summary>
        public event EventHandler<LogMessage>? Message;

        /// <summary>
        /// Client-side <see cref="IDriverDaemon.DeviceReport"/> event.
        /// </summary>
        public event EventHandler<DebugReportData>? DeviceReport;

        /// <summary>
        /// Client-side <see cref="IDriverDaemon.TabletsChanged"/> event.
        /// </summary>
        public event EventHandler<IEnumerable<TabletReference>>? TabletsChanged;
        public event EventHandler? Resynchronize;

        protected override void OnConnected()
        {
            Debug.Assert(IsConnected, $"{nameof(OnConnected)} called without being connected");
            base.OnConnected();

            Instance.Message += (sender, e) =>
                Application.Instance.AsyncInvoke(() => Message?.Invoke(sender, e));
            Instance.DeviceReport += (sender, e) =>
                Application.Instance.AsyncInvoke(() => DeviceReport?.Invoke(sender, e));
            Instance.TabletsChanged += (sender, e) =>
                Application.Instance.AsyncInvoke(() => TabletsChanged?.Invoke(sender, e));
            Instance.Resynchronize += (sender, e) =>
                Application.Instance.AsyncInvoke(() => Resynchronize?.Invoke(sender, e));
        }

        // No OnDisconnected() needed, StreamJsonRpc automatically unsubscribes events on disconnect, see https://microsoft.github.io/vs-streamjsonrpc/docs/disconnecting.html
    }
}
