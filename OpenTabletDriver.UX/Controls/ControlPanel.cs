using System;
using Eto.Forms;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.UX.Controls.Output;

namespace OpenTabletDriver.UX.Controls
{
    public class ControlPanel : Panel
    {
        public ControlPanel()
        {
            tabControl = new TabControl
            {
                Pages =
                {
                    new TabPage
                    {
                        Text = "Output",
                        Content = outputModeEditor = new OutputModeEditor()
                    },
                    new TabPage
                    {
                        Text = "Bindings",
                        Content = bindingEditor = new BindingEditor()
                    },
                    new TabPage
                    {
                        Text = "Filters",
                        Padding = 5,
                        Content = filterEditor = new PluginSettingStoreCollectionEditor<IPositionedPipelineElement<IDeviceReport>>()
                    },
                    new TabPage
                    {
                        Text = "Tools",
                        Padding = 5,
                        Content = toolEditor = new PluginSettingStoreCollectionEditor<ITool>()
                    },
                    new TabPage
                    {
                        Text = "Console",
                        Padding = 5,
                        Content = new LogView()
                    }
                }
            };

            outputModeEditor.ProfileBinding.Bind(ProfileBinding);
            bindingEditor.SettingsBinding.Bind(ProfileBinding.Child(p => p.BindingSettings));
            filterEditor.StoreCollectionBinding.Bind(ProfileBinding.Child(p => p.Filters));
            toolEditor.StoreCollectionBinding.Bind(App.Current, a => a.Settings.Tools);

            outputModeEditor.SetDisplaySize(DesktopInterop.VirtualScreen.Displays);

            this.Content = tabControl;
        }

        private TabControl tabControl;
        private OutputModeEditor outputModeEditor;
        private BindingEditor bindingEditor;
        private PluginSettingStoreCollectionEditor<IPositionedPipelineElement<IDeviceReport>> filterEditor;
        private PluginSettingStoreCollectionEditor<ITool> toolEditor;
        
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
        
        public event EventHandler<EventArgs> ProfileChanged;
        
        protected virtual void OnProfileChanged() => ProfileChanged?.Invoke(this, new EventArgs());
        
        public BindableBinding<ControlPanel, Profile> ProfileBinding
        {
            get
            {
                return new BindableBinding<ControlPanel, Profile>(
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