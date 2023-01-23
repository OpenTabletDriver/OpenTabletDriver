using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenTabletDriver.Daemon.Contracts;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.UX.Models
{
    public sealed partial class TabletHandler : ObservableObject, IDisposable
    {
        private readonly IDriverDaemon _daemon;

        [ObservableProperty]
        private InputDeviceState _tabletState;

        [ObservableProperty]
        private Profile _profile;

        public int TabletId { get; }
        public string Name => Configuration.Name;
        public TabletConfiguration Configuration { get; }

        private TabletHandler(IDriverDaemon daemon, int id, TabletConfiguration configuration, InputDeviceState state, Profile profile)
        {
            _daemon = daemon;
            _daemon.TabletStateChanged += Daemon_TabletStateChanged;
            _daemon.TabletProfileChanged += Daemon_TabletProfileChanged;
            TabletId = id;
            Configuration = configuration;
            _tabletState = state;
            _profile = profile;
        }

        public static async Task<TabletHandler> CreateAsync(IDriverDaemon daemon, int id)
        {
            var configuration = await daemon.GetTabletConfiguration(id);
            var state = await daemon.GetTabletState(id);
            var profile = await daemon.GetTabletProfile(id);
            return new TabletHandler(daemon, id, configuration, state, profile);
        }

        public async Task SetTabletState(InputDeviceState state)
        {
            await _daemon.SetTabletState(TabletId, state);
        }

        public async Task ApplyProfile()
        {
            await _daemon.SetTabletProfile(TabletId, Profile);
        }

        public async Task DiscardProfile()
        {
            Profile = await _daemon.GetTabletProfile(TabletId);
        }

        public async Task ResetProfile()
        {
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

        public void Dispose()
        {
            try
            {
                _daemon.TabletStateChanged -= Daemon_TabletStateChanged;
                _daemon.TabletProfileChanged -= Daemon_TabletProfileChanged;
            }
            catch
            {
                // noop
            }
        }
    }
}
