using CommunityToolkit.Mvvm.ComponentModel;
using OpenTabletDriver.Daemon.Contracts;
using OpenTabletDriver.Daemon.Contracts.RPC;
using OpenTabletDriver.Logging;

namespace OpenTabletDriver.UX.Models;

public partial class DaemonService : ObservableObject
{
    private readonly RpcClient<IDriverDaemon> _rpc;

    [ObservableProperty]
    private bool _isConnected;

    public IDriverDaemon Instance => _rpc.Instance ?? throw new InvalidOperationException("Daemon is not connected");

    public DaemonService()
    {
        _rpc = new RpcClient<IDriverDaemon>("OpenTabletDriver.Daemon");

        _rpc.Connected += (sender, e) =>
        {
            Log.Output += Log_Output;
            IsConnected = true;
        };

        _rpc.Disconnected += (sender, e) =>
        {
            Log.Output -= Log_Output;
            IsConnected = false;
        };
    }

    public async Task ConnectAsync()
    {
        await _rpc.Connect();
    }

    public void Disconnect()
    {
        _rpc.Disconnect();
    }

    private void Log_Output(object? _, LogMessage message)
    {
        _rpc.Instance!.WriteMessage(message).ConfigureAwait(false);
    }
}
