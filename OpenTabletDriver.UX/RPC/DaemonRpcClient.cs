using System;
using System.Collections.Generic;
using Eto.Forms;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Logging;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.UX.RPC
{
    public class DaemonRpcClient : RpcClient<IDriverDaemon>
    {
        public DaemonRpcClient(string pipeName) : base(pipeName)
        {
        }

        public event EventHandler<LogMessage> Message;
        public event EventHandler<DebugReportData> DeviceReport;
        public event EventHandler<IEnumerable<TabletReference>> TabletsChanged;

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
    }
}
