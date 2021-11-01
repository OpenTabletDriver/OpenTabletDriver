using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.UX.Controls.Bindings;
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
                        Text = "Filters",
                        Padding = 5,
                        Content = filterEditor = new PluginSettingStoreCollectionEditor<IPositionedPipelineElement<IDeviceReport>>()
                    },
                    new TabPage
                    {
                        Text = "Pen Settings",
                        Content = penBindingEditor = new PenBindingEditor()
                    },
                    new TabPage
                    {
                        Text = "Auxiliary Settings",
                        Content = auxBindingEditor = new AuxiliaryBindingEditor()
                    },
                    new TabPage
                    {
                        Text = "Mouse Settings",
                        Content = mouseBindingEditor = new MouseBindingEditor()
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
            penBindingEditor.ProfileBinding.Bind(ProfileBinding);
            auxBindingEditor.ProfileBinding.Bind(ProfileBinding);
            mouseBindingEditor.ProfileBinding.Bind(ProfileBinding);
            filterEditor.StoreCollectionBinding.Bind(ProfileBinding.Child(p => p.Filters));
            toolEditor.StoreCollectionBinding.Bind(App.Current, a => a.Settings.Tools);

            outputModeEditor.SetDisplaySize(DesktopInterop.VirtualScreen.Displays);

            this.Content = tabControl;
        }

        private TabControl tabControl;
        private readonly Placeholder placeholder = new Placeholder
        {
            Text = "No tablets detected. Ensure a supported tablet is connected properly, and check the Console tab for any errors."
        };
        private OutputModeEditor outputModeEditor;
        private BindingEditor penBindingEditor, auxBindingEditor, mouseBindingEditor;
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

        protected virtual async void OnProfileChanged()
        {
            ProfileChanged?.Invoke(this, new EventArgs());
            if (Profile != null && await Profile.GetTabletReference() is TabletReference tablet)
            {
                Application.Instance.AsyncInvoke(() => 
                {
                    tabControl.Pages.Single(p => p.Text == "Output").Content = outputModeEditor;
                    penBindingEditor.Parent.Visible = tablet.Properties.Specifications.Pen != null;
                    auxBindingEditor.Parent.Visible = tablet.Properties.Specifications.AuxiliaryButtons != null;
                    mouseBindingEditor.Parent.Visible = tablet.Properties.Specifications.MouseButtons != null;
                });
            }
            else //no tablets connected, or Profile otherwise null
            {
                Application.Instance.AsyncInvoke(() =>
                {
                    tabControl.Pages.Single(p => p.Text == "Output").Content = placeholder;
                    //hide the 3 tabs related to having a tablet connected
                    penBindingEditor.Parent.Visible   = false;
                    auxBindingEditor.Parent.Visible   = false;
                    mouseBindingEditor.Parent.Visible = false;
                });
            }
        }

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