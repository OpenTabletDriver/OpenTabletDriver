using System;
using Eto.Forms;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.UX.Controls.Bindings
{
    public abstract class BindingEditor : Panel
    {
        public DirectBinding<BindingSettings> SettingsBinding => ProfileBinding.Child(b => b.BindingSettings);

        private Profile profile;
        public Profile Profile
        {
            set
            {
                this.profile = value;
                this.OnProfileChanged();
            }
            get => this.profile;
        }

        private TabletReference tablet;
        public TabletReference Tablet
        {
            set
            {
                this.tablet = value;
                this.OnTabletChanged();
            }
            get => this.tablet;
        }

        public event EventHandler<EventArgs> ProfileChanged;
        public event EventHandler<EventArgs> TabletChanged;

        protected virtual void OnProfileChanged() => ProfileChanged?.Invoke(this, new EventArgs());
        protected virtual void OnTabletChanged() => TabletChanged?.Invoke(this, new EventArgs());

        public BindableBinding<BindingEditor, Profile> ProfileBinding
        {
            get
            {
                return new BindableBinding<BindingEditor, Profile>(
                    this,
                    c => c.Profile,
                    (c, v) => c.Profile = v,
                    (c, h) => c.ProfileChanged += h,
                    (c, h) => c.ProfileChanged -= h
                );
            }
        }

        public BindableBinding<BindingEditor, TabletReference> TabletBinding
        {
            get
            {
                return new BindableBinding<BindingEditor, TabletReference>(
                    this,
                    c => c.Tablet,
                    (c, v) => c.Tablet = v,
                    (c, h) => c.TabletChanged += h,
                    (c, h) => c.TabletChanged -= h
                );
            }
        }
    }
}
