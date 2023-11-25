using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenTabletDriver.Daemon.Contracts;
using OpenTabletDriver.Daemon.Contracts.Persistence;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.UI.Models;

public interface ITabletService : INotifyPropertyChanged, INotifyPropertyChanging, IDisposable
{
    int TabletId { get; }
    InputDeviceState TabletState { get; }
    string Name { get; }
    Profile Profile { get; }
    TabletConfiguration Configuration { get; }

    Task Initialize(bool initialize);
    Task ApplyProfile();
    Task DiscardProfile();
    Task ResetProfile();
}

public sealed partial class TabletService : ObservableObject, ITabletService
{
    private readonly IDriverDaemon _daemon;
    private InputDeviceState _tabletState;
    private Profile _profile;
    private bool _disposed;

    public int TabletId { get; }
    public string Name => Configuration.Name;

    public InputDeviceState TabletState
    {
        get => _tabletState;
        private set => SetProperty(ref _tabletState, value);
    }
    public Profile Profile
    {
        get => _profile;
        private set => SetProperty(ref _profile, value);
    }
    public TabletConfiguration Configuration { get; }

    private TabletService(IDriverDaemon daemon, int id, TabletConfiguration configuration, InputDeviceState state, Profile profile)
    {
        _daemon = daemon;
        _daemon.TabletStateChanged += Daemon_TabletStateChanged;
        _daemon.TabletProfileChanged += Daemon_TabletProfileChanged;
        TabletId = id;
        Configuration = configuration;
        _tabletState = state;
        _profile = profile;
    }

    public static async Task<TabletService> CreateAsync(IDriverDaemon daemon, int id)
    {
        var configuration = await daemon.GetTabletConfiguration(id);
        var state = await daemon.GetTabletState(id);
        var profile = await daemon.GetTabletProfile(id);
        return new TabletService(daemon, id, configuration, state, profile);
    }

    public async Task Initialize(bool initialize)
    {
        ThrowIfDisposed();
        await _daemon.SetTabletState(TabletId, initialize ? InputDeviceState.Normal : InputDeviceState.Uninitialized);
    }

    public async Task SetTabletState(InputDeviceState state)
    {
        ThrowIfDisposed();
        await _daemon.SetTabletState(TabletId, state);
    }

    public async Task ApplyProfile()
    {
        ThrowIfDisposed();
        await _daemon.SetTabletProfile(TabletId, Profile);
    }

    public async Task DiscardProfile()
    {
        ThrowIfDisposed();
        Profile = await _daemon.GetTabletProfile(TabletId);
    }

    public async Task ResetProfile()
    {
        ThrowIfDisposed();
        await _daemon.ResetTabletProfile(TabletId);
        Profile = await _daemon.GetTabletProfile(TabletId);
    }

    private void Daemon_TabletStateChanged(object? sender, TabletProperty<InputDeviceState> args)
    {
        if (args.Id == TabletId)
            TabletState = args.Value;
    }

    private void Daemon_TabletProfileChanged(object? sender, TabletProperty<Profile> args)
    {
        if (args.Id == TabletId)
        {
            Debug.Assert(args.Value != null);
            Profile = args.Value;
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(TabletService));
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _daemon.TabletStateChanged -= Daemon_TabletStateChanged;
        _daemon.TabletProfileChanged -= Daemon_TabletProfileChanged;
    }
}
