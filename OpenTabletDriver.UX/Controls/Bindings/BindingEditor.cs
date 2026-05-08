using System;
using Eto.Forms;
using OpenTabletDriver.Desktop.Profiles;

namespace OpenTabletDriver.UX.Controls.Bindings
{
    public abstract class BindingEditor : Panel
    {
        public DirectBinding<BindingSettings> SettingsBinding => ProfileBinding.Child(b => b.BindingSettings);

        public Profile Profile
        {
            set
            {
                field = value;
                this.OnProfileChanged();
            }
            get;
        }

        public event EventHandler<EventArgs> ProfileChanged;

        protected virtual void OnProfileChanged() => ProfileChanged?.Invoke(this, new EventArgs());

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
    }
}
