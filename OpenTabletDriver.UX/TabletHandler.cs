using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.UX
{
    public class TabletHandler
    {
        private TabletHandler(IDriverDaemon daemon, int id, int persistentId, InputDeviceState state, TabletConfiguration configuration, Profile profile)
        {
            Id = id;
            PersistentId = persistentId;
            State = state;
            Configuration = configuration;
            Profile = profile;

            daemon.TabletStateChanged += (sender, e) =>
            {
                if (e.Id == Id)
                {
                    State = e.Value;
                    StateChanged?.Invoke(this, e.Value);
                }
            };

            daemon.TabletProfileChanged += (sender, e) =>
            {
                if (e.Id == Id)
                {
                    Profile = e.Value!;
                    ProfileChanged?.Invoke(this, e.Value!);
                }
            };
        }

        public int Id { get; }
        public int PersistentId { get; }
        public string Name => Configuration.Name;
        public InputDeviceState State { get; private set; }
        public TabletConfiguration Configuration { get; }
        public Profile Profile { get; private set; }

        public event EventHandler<InputDeviceState>? StateChanged;
        public event EventHandler<Profile>? ProfileChanged;

        public static async Task<TabletHandler> Create(IDriverDaemon daemon, int tabletId, Settings settings)
        {
            var persistentId = await daemon.GetPersistentId(tabletId);
            var tabletConfiguration = await daemon.GetTabletConfiguration(tabletId);
            var state = await daemon.GetTabletState(tabletId);
            var profile = settings.Profiles.Find(p => p.Tablet == tabletConfiguration.Name && p.PersistentId == persistentId);

            if (profile is null)
            {
                profile = await daemon.GetTabletProfile(tabletId);
                settings.Profiles.Add(profile);
            }

            return new TabletHandler(daemon, tabletId, persistentId, state, tabletConfiguration, profile);
        }

        public void ExtractProfile(Settings settings)
        {
            var profile = settings.Profiles.Find(p => p.Tablet == Configuration.Name && p.PersistentId == PersistentId);
            Profile = profile!;
            ProfileChanged?.Invoke(this, profile!);
        }
    }
}
