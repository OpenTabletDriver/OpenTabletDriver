using System;
using System.Collections.Generic;
using Eto.Forms;
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
            var control = new TabControl();

            control.Pages.Add(new TabPage
            {
                Text = "Output",
                Content = outputModeEditor = new()
            });

            control.Pages.Add(new TabPage
            {
                Text = "Filters",
                Padding = 5,
                Content = filterEditor = new()
            });

            control.Pages.Add(new TabPage
            {
                Text = "Pen Settings",
                Content = penBindingEditor = new PenBindingEditor()
            });

            control.Pages.Add(new TabPage
            {
                Text = "Auxiliary Settings",
                Content = auxBindingEditor = new AuxiliaryBindingEditor()
            });

            control.Pages.Add(new TabPage
            {
                ID = "mouse",
                Text = "Mouse Settings",
                Content = mouseBindingEditor = new MouseBindingEditor()
            });

            control.Pages.Add(new TabPage
            {
                Text = "Tools",
                Padding = 5,
                Content = toolEditor = new()
            });

            control.Pages.Add(new TabPage
            {
                Text = "Info",
                Padding = 5,
                Content = placeholder = new Placeholder
                {
                    Text = "No tablets are detected."
                }
            });

            control.Pages.Add(new TabPage
            {
                Text = "Console",
                Padding = 5,
                Content = logView = new()
            });

            this.Content = tabControl = control;

            outputModeEditor.ProfileBinding.Bind(ProfileBinding);
            penBindingEditor.ProfileBinding.Bind(ProfileBinding);
            auxBindingEditor.ProfileBinding.Bind(ProfileBinding);
            mouseBindingEditor.ProfileBinding.Bind(ProfileBinding);
            filterEditor.StoreCollectionBinding.Bind(ProfileBinding.Child(p => p!.Filters)!);
            toolEditor.StoreCollectionBinding.Bind(App.Current, a => a.Settings.Tools);

            outputModeEditor.SetDisplaySize(DesktopInterop.VirtualScreen?.Displays);

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
        private List<BindingEditor> wheelBindingEditors = [];
        private PluginSettingStoreCollectionEditor<IPositionedPipelineElement<IDeviceReport>> filterEditor;
        private PluginSettingStoreCollectionEditor<ITool> toolEditor;

        private Profile? profile;

        private Profile? Profile
        {
            set
            {
                this.profile = value;
                this.OnProfileChanged();
            }
            get => this.profile;
        }

        public event EventHandler<EventArgs>? ProfileChanged;

        // ReSharper disable once AsyncVoidMethod
        protected virtual void OnProfileChanged() => Application.Instance.AsyncInvoke(async void () =>
        {
            ProfileChanged?.Invoke(this, EventArgs.Empty);

            var tablet = Profile != null ? await Profile.GetTabletReference() : null;

            OnTabletChanged(tablet);

            if (Platform.IsMac)
                tabControl.Pages.Clear();

            if (tablet != null)
            {
                bool switchToOutput = tabControl.SelectedPage == placeholder.Parent;

                SetPageVisibility(placeholder, false);
                SetPageVisibility(outputModeEditor, true);
                SetPageVisibility(filterEditor, true);
                SetPageVisibility(penBindingEditor, true);
                SetPageVisibility(auxBindingEditor, tablet.Properties.Specifications.AuxiliaryButtons != null);

                for (int i = 0; i < wheelBindingEditors.Count; i++)
                    SetPageVisibility(wheelBindingEditors[i], (tablet.Properties.Specifications.Wheels?.Count ?? 0) > i);

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
                foreach (var controlItem in wheelBindingEditors)
                    SetPageVisibility(controlItem, false);
                SetPageVisibility(mouseBindingEditor, false);
                SetPageVisibility(toolEditor, false);

                if (tabControl.SelectedPage != logView.Parent)
                {
                    tabControl.SelectedIndex = Profile == null ?
                        tabControl.Pages.IndexOf(placeholder.Parent as TabPage) :
                        0;
                }
            }

            SetPageVisibility(logView, true);
        });

        private void OnTabletChanged(TabletReference? tablet)
        {
            // ensure we have enough wheel binding editors
            int tabletWheels = tablet?.Properties.Specifications.Wheels?.Count ?? 0;
            if (tabletWheels > wheelBindingEditors.Count)
            {
                for (int i = wheelBindingEditors.Count; i < tabletWheels; i++)
                {
                    var wheelBindingEditor = new WheelBindingEditor(i);
                    wheelBindingEditor.ProfileBinding.Bind(ProfileBinding);
                    var pageIndex = tabControl.Pages.IndexOf(mouseBindingEditor.Parent as TabPage);
                    wheelBindingEditors.Add(wheelBindingEditor);
                    var wheelPage = new TabPage(wheelBindingEditor) { Text = $"Wheel {i + 1} Bindings" };
                    if (pageIndex >= 0)
                        tabControl.Pages.Insert(pageIndex, wheelPage);
                    else
                        tabControl.Pages.Add(wheelPage);
                }
            }
        }

        public BindableBinding<ControlPanel, Profile?> ProfileBinding
        {
            get
            {
                return new BindableBinding<ControlPanel, Profile?>(
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
