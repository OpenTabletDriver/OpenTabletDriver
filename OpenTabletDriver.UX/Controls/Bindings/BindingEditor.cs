using System;
using Eto.Forms;
using OpenTabletDriver.Desktop.Profiles;

namespace OpenTabletDriver.UX.Controls.Bindings
{
    public abstract class BindingEditor : Panel
    {        
        private BindingSettings settings;
        public BindingSettings Settings
        {
            set
            {
                this.settings = value;
                this.OnSettingsChanged();
            }
            get => this.settings;
        }
        
        public event EventHandler<EventArgs> SettingsChanged;
        
        protected virtual void OnSettingsChanged() => SettingsChanged?.Invoke(this, new EventArgs());
        
        public BindableBinding<BindingEditor, BindingSettings> SettingsBinding
        {
            get
            {
                return new BindableBinding<BindingEditor, BindingSettings>(
                    this,
                    c => c.Settings,
                    (c, v) => c.Settings = v,
                    (c, h) => c.SettingsChanged += h,
                    (c, h) => c.SettingsChanged -= h
                );
            }
        }
    }
}
