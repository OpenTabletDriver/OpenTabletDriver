using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.UX.Controls.Bindings;
using OpenTabletDriver.UX.Controls.Generic;
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
            filterEditor.StoreCollectionBinding.Bind(ProfileBinding.Child(p => p.Filters));
            toolEditor.StoreCollectionBinding.Bind(App.Current, a => a.Settings.Tools);

            outputModeEditor.SetDisplaySize(DesktopInterop.VirtualScreen.Displays);

            control.SelectedIndexChanged += (_, __) =>
                auxBindingEditor.SetVisible(control.SelectedPage?.Content == auxBindingEditor);

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
        private PenBindingEditor penBindingEditor;
        private AuxiliaryBindingEditor auxBindingEditor;
        private MouseBindingEditor mouseBindingEditor;
        private List<BindingEditor> wheelBindingEditors = [];
        private List<TabPage> wheelTabPages = [];
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
                SetPageVisibility(penBindingEditor, tablet.Properties.Specifications.Pen != null);
                SetPageVisibility(auxBindingEditor, tablet.Properties.Specifications.AuxiliaryButtons != null);

                foreach (var page in wheelTabPages)
                    SetPageVisibility(page.Content, true);

                SetPageVisibility(mouseBindingEditor, tablet.Properties.Specifications.MouseButtons != null);
                SetPageVisibility(toolEditor, true);

                penBindingEditor.SetButtonNames(tablet.Properties.Specifications.Pen);
                auxBindingEditor.SetButtonNames(tablet.Properties.Specifications.AuxiliaryButtons);
                mouseBindingEditor.SetButtonNames(tablet.Properties.Specifications.MouseButtons);

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
                foreach (var page in wheelTabPages)
                    SetPageVisibility(page.Content, false);
                SetPageVisibility(mouseBindingEditor, false);
                SetPageVisibility(toolEditor, false);

                penBindingEditor.SetButtonNames(null);
                auxBindingEditor.SetButtonNames(null);
                mouseBindingEditor.SetButtonNames(null);

                if (tabControl.SelectedPage != logView.Parent)
                {
                    tabControl.SelectedIndex = Profile == null ?
                        tabControl.Pages.IndexOf(placeholder.Parent as TabPage) :
                        0;
                }
            }

            SetPageVisibility(logView, true);
        });

        public void OnTabletChanged(TabletReference tablet)
        {
            foreach (var page in wheelTabPages)
                tabControl.Pages.Remove(page);
            wheelTabPages.Clear();
            wheelBindingEditors.Clear();

            var wheels = tablet?.Properties.Specifications.Wheels;
            if (wheels == null || wheels.Count == 0) return;

            bool hasGroups = wheels.Any(w => w.Group != null);

            for (int i = 0; i < wheels.Count; i++)
            {
                var editor = new WheelBindingEditor(i, scrollable: !hasGroups);
                editor.ProfileBinding.Bind(ProfileBinding);
                wheelBindingEditors.Add(editor);
            }

            int insertAt = tabControl.Pages.IndexOf(mouseBindingEditor.Parent as TabPage);

            if (hasGroups)
            {
                var groupOrder = new List<string>();
                foreach (var w in wheels)
                {
                    if (w.Group != null && !groupOrder.Contains(w.Group))
                        groupOrder.Add(w.Group);
                }

                foreach (var groupName in groupOrder)
                {
                    var stackLayout = new StackLayout
                    {
                        HorizontalContentAlignment = HorizontalAlignment.Stretch,
                        Spacing = 5
                    };

                    for (int i = 0; i < wheels.Count; i++)
                    {
                        if (wheels[i].Group != groupName) continue;
                        stackLayout.Items.Add(new StackLayout
                        {
                            HorizontalContentAlignment = HorizontalAlignment.Stretch,
                            Spacing = 5,
                            Padding = new Padding(0, 8, 0, 0),
                            Items =
                            {
                                new Label
                                {
                                    Text = wheels[i].Name ?? $"Wheel {i + 1}",
                                    Font = SystemFonts.Bold(14),
                                    TextColor = SystemColors.HighlightText
                                },
                                wheelBindingEditors[i]
                            }
                        });
                    }

                    var page = new TabPage
                    {
                        Text = groupName,
                        Content = new Scrollable
                        {
                            Border = BorderType.None,
                            Content = stackLayout
                        }
                    };

                    wheelTabPages.Add(page);

                    if (insertAt >= 0)
                        tabControl.Pages.Insert(insertAt++, page);
                    else
                        tabControl.Pages.Add(page);
                }
            }
            else
            {
                for (int i = 0; i < wheels.Count; i++)
                {
                    var name = wheels[i].Name != null
                        ? $"{wheels[i].Name} Bindings"
                        : $"Wheel {i + 1} Bindings";
                    var page = new TabPage(wheelBindingEditors[i]) { Text = name };

                    wheelTabPages.Add(page);

                    if (insertAt >= 0)
                        tabControl.Pages.Insert(insertAt++, page);
                    else
                        tabControl.Pages.Add(page);
                }
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
