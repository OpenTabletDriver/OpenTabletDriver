using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenTabletDriver.Daemon.Contracts;
using OpenTabletDriver.Daemon.Contracts.Persistence;
using OpenTabletDriver.Daemon.Contracts.RPC;
using OpenTabletDriver.Logging;
using OpenTabletDriver.UI.Models;

namespace OpenTabletDriver.UI.Services;

public enum DaemonState
{
    Disconnected,
    Connecting,
    Connected
}

public interface IDaemonService : INotifyPropertyChanged, INotifyPropertyChanging
{
    DaemonState State { get; }
    IDriverDaemon? Instance { get; }
    ObservableCollection<ITabletService> Tablets { get; }
    ObservableCollection<PluginSettings> Tools { get; }
    ObservableCollection<PluginContextDto> PluginContexts { get; }
    Task ConnectAsync();
    Task ConnectAsync(TimeSpan timeout, CancellationToken cancellationToken = default);
    Task ReconnectAsync();
    Task ReconnectAsync(TimeSpan timeout, CancellationToken cancellationToken = default);
}

public static class DaemonServiceExtensions
{
    public static IDisposable HandleStateChange(
        this IDaemonService daemonService,
        Action onConnect,
        Action? onDisconnect = null,
        Action? onConnecting = null)
    {
        return daemonService.HandleProperty(
            nameof(IDaemonService.State),
            d => d.State,
            (d, s) =>
            {
                switch (s)
                {
                    case DaemonState.Connected:
                        onConnect();
                        break;
                    case DaemonState.Connecting:
                        onConnecting?.Invoke();
                        break;
                    case DaemonState.Disconnected:
                        onDisconnect?.Invoke();
                        break;
                }
            }
        );
    }

    public static PluginDto? FindPlugin(this IDaemonService daemonService, string path)
    {
        return daemonService.PluginContexts.SelectMany(p => p.Plugins).FirstOrDefault(p => p.Path == path);
    }
}

public partial class DaemonService : ObservableObject, IDaemonService
{
    private readonly IRpcClient<IDriverDaemon> _rpcClient;
    private readonly IDispatcher _dispatcher;
    private DaemonState _state;
    private IDriverDaemon? _instance;
    private bool _suppressStateChangedEvents;

    public DaemonState State
    {
        get => _state;
        private set => SetProperty(ref _state, value);
    }

    public IDriverDaemon? Instance
    {
        get => _instance;
        private set => SetProperty(ref _instance, value);
    }

    public ObservableCollection<ITabletService> Tablets { get; } = new();
    public ObservableCollection<PluginSettings> Tools { get; } = new();
    public ObservableCollection<PluginContextDto> PluginContexts { get; } = new();

    public DaemonService(IRpcClient<IDriverDaemon> rpcClient, IDispatcher dispatcher)
    {
        _rpcClient = rpcClient;
        _dispatcher = dispatcher;
        rpcClient.Connected += OnConnected;
        rpcClient.Disconnected += OnDisconnected;
    }

    public Task ConnectAsync()
    {
        return ConnectAsync(TimeSpan.FromSeconds(5));
    }

    public async Task ConnectAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        if (!_rpcClient.IsConnected)
        {
            State = DaemonState.Connecting;
            try
            {
                await _rpcClient.ConnectAsync(timeout, cancellationToken);
            }
            catch
            {
                State = DaemonState.Disconnected;
                throw;
            }
        }
    }

    public Task ReconnectAsync()
    {
        return ReconnectAsync(TimeSpan.FromSeconds(5));
    }

    public async Task ReconnectAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        _suppressStateChangedEvents = true;
        State = DaemonState.Connecting;
        _rpcClient.Disconnect();
        await _rpcClient.ConnectAsync(timeout, cancellationToken);
        _suppressStateChangedEvents = false;
    }

    private void OnConnected(object? _, EventArgs args)
    {
        var daemon = _rpcClient.Instance!;
        daemon.TabletAdded += async (sender, tabletId) => await createTabletService(tabletId);
        daemon.TabletRemoved += (sender, tabletId) => removeTabletService(tabletId);
        daemon.ToolsChanged += (sender, tools) => updateTools(tools);
        daemon.PluginAdded += (sender, plugin) => addPlugin(plugin);
        daemon.PluginRemoved += (sender, plugin) => removePlugin(plugin);
        Log.Output += Log_Output;

        _dispatcher.Post(async () =>
        {
            Instance = daemon;

            await daemon.GetPlugins().ForEachAsync(plugin => PluginContexts.Add(plugin));
            await daemon.GetToolSettings().ForEachAsync(tool => Tools.Add(tool));

            if (!_suppressStateChangedEvents)
                State = DaemonState.Connected;

            await daemon.GetTablets().ForEachAsync(async tabletId =>
            {
                var tabletService = await TabletService.CreateAsync(daemon, tabletId);
                Tablets.Add(tabletService);
            });
        });

        async Task createTabletService(int tabletId)
        {
            try
            {
                var tabletService = await TabletService.CreateAsync(daemon!, tabletId);
                _dispatcher.ProbablySynchronousPost(() => Tablets.Add(tabletService));
            }
            catch
            {
                // TODO: Log
            }
        }

        void removeTabletService(int tabletId)
        {
            var tabletService = Tablets.FirstOrDefault(tablet => tablet.TabletId == tabletId);
            if (tabletService is not null)
            {
                _dispatcher.ProbablySynchronousPost(() =>
                {
                    Tablets.Remove(tabletService);
                    tabletService.Dispose();
                });
            }
        }

        void updateTools(IEnumerable<PluginSettings> tools)
        {
            var cachedTools = Tools.ToArray();
            _dispatcher.ProbablySynchronousPost(() =>
            {
                Tools.Clear();
                Tools.AddRange(cachedTools);
            });
        }

        void addPlugin(PluginContextDto plugin)
        {
            _dispatcher.ProbablySynchronousPost(() => PluginContexts.Add(plugin));
        }

        void removePlugin(PluginContextDto plugin)
        {
            var existingPlugin = PluginContexts.First(p => PluginMetadata.Match(p.Metadata, plugin.Metadata));
            _dispatcher.ProbablySynchronousPost(() => PluginContexts.Remove(existingPlugin));
        }
    }

    private void OnDisconnected(object? _, EventArgs args)
    {
        Log.Output -= Log_Output;

        _dispatcher.Post(() =>
        {
            Instance = null;
            Tablets.Clear();
            Tools.Clear();
            PluginContexts.Clear();

            if (!_suppressStateChangedEvents)
                State = DaemonState.Disconnected;
        });
    }

    private void Log_Output(object? _, LogMessage message)
    {
        Instance!.WriteMessage(message).ConfigureAwait(false);
    }
}
