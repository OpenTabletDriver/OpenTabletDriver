using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Interop;
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
            this.Content = tabControl = new TabControl
            {
                Pages =
                {
                    new TabPage
                    {
                        Text = "Output",
                        Content = outputModeEditor = new()
                    },
                    new TabPage
                    {
                        Text = "Filters",
                        Padding = 5,
                        Content = filterEditor = new()
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
                        Content = toolEditor = new()
                    },
                    new TabPage
                    {
                        Text = "Info",
                        Padding = 5,
                        Content = placeholder = new Placeholder
                        {
                            Text = "No tablets are detected."
                        }
                    },
                    new TabPage
                    {
                        Text = "Console",
                        Padding = 5,
                        Content = logView = new()
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

            Log.Output += (_, message) => Application.Instance.AsyncInvoke(() =>
            {
                if (message.Level > LogLevel.Info)
                {
                    tabControl.SelectedPage = logView.Parent as TabPage;
                }
            });
        }

        private TabControl tabControl;
        private Placeholder placeholder;
        private LogView logView;
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

        protected virtual void OnProfileChanged() => Application.Instance.AsyncInvoke(async () =>
        {
            ProfileChanged?.Invoke(this, EventArgs.Empty);

            var tablet = Profile != null ? await Profile.GetTabletReference() : null;

            if (Platform.IsMac)
                tabControl.Pages.Clear();

            if (tablet != null)
            {
                bool switchToOutput = tabControl.SelectedPage == placeholder.Parent;

                SetPageVisibility(placeholder, false);
                SetPageVisibility(outputModeEditor, true);
                SetPageVisibility(filterEditor, true);
                SetPageVisibility(penBindingEditor, tablet.Properties.Specifications.Pen != null);
                SetPageVisibility(auxBindingEditor, tablet.Properties.Specifications.AuxiliaryButtons != null);
                SetPageVisibility(mouseBindingEditor, tablet.Properties.Specifications.MouseButtons != null);
                SetPageVisibility(toolEditor, true);

                if (switchToOutput)
                    tabControl.SelectedIndex = 0;
            }
            else
            {
                SetPageVisibility(placeholder, true);
                SetPageVisibility(outputModeEditor, false);
                SetPageVisibility(filterEditor, false);
                SetPageVisibility(penBindingEditor, false);
                SetPageVisibility(auxBindingEditor, false);
                SetPageVisibility(mouseBindingEditor, false);
                SetPageVisibility(toolEditor, false);

                if (tabControl.SelectedPage != logView.Parent)
                    tabControl.SelectedIndex = 0;
            }

            SetPageVisibility(logView, true);
        });

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

        private void SetPageVisibility(Control control, bool visible)
        {
            // This works around a bug in Eto.Forms with TabPage visibility
            // https://github.com/picoe/Eto/issues/1224
            if (Platform.IsMac)
            {
                if (visible)
                {
                    var page = control.Parent as TabPage;
                    tabControl.Pages.Add(page);
                }
            }
            else
            {
                control.Parent.Visible = visible;
            }
        }
    }
}
