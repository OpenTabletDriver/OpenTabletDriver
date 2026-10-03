using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autofac;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Profiles
{
    public class ProfileCollection : ObservableCollection<Profile>
    {
        public ProfileCollection()
        {
        }

        public ProfileCollection(IEnumerable<Profile> profiles)
            : base(profiles)
        {
        }

        public ProfileCollection(ILifetimeScope lifetimeScope, IEnumerable<TabletReference> tablets)
            : this(tablets.Select(tablet => Profile.GetDefaults(lifetimeScope, tablet)))
        {
        }

        public void SetProfile(TabletReference tablet, Profile profile)
        {
            if (this.FirstOrDefault(t => t.Tablet == tablet.Properties.Name) is Profile oldProfile)
            {
                this.Remove(oldProfile);
            }
            this.Add(profile);
        }

        public Profile GetProfile(ILifetimeScope lifetimeScope, TabletReference tablet)
        {
            return this.FirstOrDefault(t => t.Tablet == tablet.Properties.Name) ?? Generate(lifetimeScope, tablet);
        }

        public Profile? GetProfile(string tablet)
        {
            return this.FirstOrDefault(t => t.Tablet == tablet);
        }

        public Profile Generate(ILifetimeScope lifetimeScope, TabletReference tablet)
        {
            var profile = Profile.GetDefaults(lifetimeScope, tablet);
            SetProfile(tablet, profile);
            return profile;
        }
    }
}
