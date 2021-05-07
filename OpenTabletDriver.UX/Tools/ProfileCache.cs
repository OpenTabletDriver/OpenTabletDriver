using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.UX.Tools
{
    public class ProfileCache : Bindable
    {
        public ProfileCache()
        {
            App.Driver.Instance.TabletHandlerCreated += async (_, id) =>
            {
                Add(id, await App.Driver.Instance.GetProfile(id));
                HandlerInFocus = id;
            };

            App.Driver.Instance.TabletHandlerDestroyed += (_, id) =>
            {
                Remove(id);
                if (id == HandlerInFocus)
                {
                    if (Profiles.Keys.Count > 0)
                    {
                        HandlerInFocus = Profiles.Keys.TakeWhile(cachedID => cachedID != HandlerInFocus).Last();
                    }
                    else
                    {
                        HandlerInFocus = TabletHandlerID.Invalid;
                    }
                }
            };
        }

        private TabletHandlerID handlerInFocus = TabletHandlerID.Invalid;
        private Profile profileInFocus;
        private Dictionary<TabletHandlerID, Profile> Profiles = new Dictionary<TabletHandlerID, Profile>();

        public event EventHandler<EventArgs> HandlerInFocusChanged;
        public event EventHandler<EventArgs> ProfileInFocusChanged;

        public TabletHandlerID HandlerInFocus
        {
            get => handlerInFocus;
            set
            {
                if (value != handlerInFocus)
                {
                    handlerInFocus = value;
                    OnHandlerInFocusChanged();
                }
            }
        }

        public Profile ProfileInFocus
        {
            get => profileInFocus;
            set
            {
                profileInFocus = value;
                ProfileInFocusChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public BindableBinding<ProfileCache, TabletHandlerID> HandlerInFocusBinding
        {
            get
            {
                return new BindableBinding<ProfileCache, TabletHandlerID>(
                    this,
                    t => t.HandlerInFocus,
                    (t, h) => t.handlerInFocus = h,
                    (t, hh) => t.HandlerInFocusChanged += hh,
                    (t, hh) => t.HandlerInFocusChanged -= hh
                );
            }
        }

        public BindableBinding<ProfileCache, Profile> ProfileInFocusBinding
        {
            get
            {
                return new BindableBinding<ProfileCache, Profile>(
                    this,
                    t => t.ProfileInFocus,
                    (t, p) => t.ProfileInFocus = p,
                    (t, h) => t.ProfileInFocusChanged += h,
                    (t, h) => t.ProfileInFocusChanged -= h
                );
            }
        }

        public async Task<Profile> Get(TabletHandlerID ID)
        {
            if (!Profiles.TryGetValue(ID, out var profile))
            {
                profile = await App.Driver.Instance.GetProfile(ID);
                Add(ID, profile);
                Log.Debug("UX", $"Fetched profile '{profile.ProfileName}' for {ID}");
            }
            return profile;
        }

        public void Add(TabletHandlerID ID, Profile profile)
        {
            if (Profiles.TryAdd(ID, profile))
            {
                Log.Debug("UX", $" Profile '{profile}' cached for {ID}");
            }
        }

        public bool Remove(TabletHandlerID ID)
        {
            return Profiles.Remove(ID);
        }

        public async Task UpdateCache()
        {
            Profiles = new Dictionary<TabletHandlerID, Profile>();

            var IDs = (await App.Driver.Instance.GetActiveTabletHandlerIDs()).ToArray();
            foreach (var ID in IDs)
            {
                var updatedProfile = await App.Driver.Instance.GetProfile(ID);
                Add(ID, updatedProfile);
            }

            // Force FocusChanged event to update subscribers
            handlerInFocus = IDs.Any() ? IDs[0] : TabletHandlerID.Invalid;
            OnHandlerInFocusChanged();
        }

        private void OnHandlerInFocusChanged()
        {
            Application.Instance.AsyncInvoke(async () =>
            {
                ProfileInFocus = HandlerInFocus != TabletHandlerID.Invalid ? await Get(HandlerInFocus) : null;
                HandlerInFocusChanged?.Invoke(this, EventArgs.Empty);
            });
        }
    }
}