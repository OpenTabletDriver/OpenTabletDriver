using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Profiles
{
    public class ProfileCollection : ObservableCollection<Profile>
    {
        public ProfileCollection()
            : base()
        {
        }

        public ProfileCollection(IEnumerable<Profile> profiles)
            : base(profiles)
        {
        }

        public ProfileCollection(IEnumerable<TabletReference> tablets)
            : this(tablets.Select(s => Profile.GetDefaults(s)))
        {
        }

        public Profile this[TabletReference tablet]
        {
            set => SetProfile(tablet, value);
            get => GetProfile(tablet);
        }

        public void SetProfile(TabletReference tablet, Profile profile)
        {
            if (this.FirstOrDefault(t => t.Tablet == tablet.Properties.Name) is Profile oldProfile)
            {
                this.Remove(oldProfile);
            }
            this.Add(profile);
        }

        public Profile GetProfile(TabletReference tablet)
        {
            return this.FirstOrDefault(t => t.Tablet == tablet.Properties.Name) is Profile profile ? profile : Generate(tablet);
        }

        public Profile Generate(TabletReference tablet)
        {
            var profile = Profile.GetDefaults(tablet);
            SetProfile(tablet, profile);
            return profile;
        }
    }
}
