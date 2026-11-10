using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.UX.Controls.Generic.Reflection;

namespace OpenTabletDriver.UX.Controls.Output
{
    public class OutputModeEditor : Panel
    {
        public OutputModeEditor()
        {
            this.Content = new StackLayout
            {
                Padding = 5,
                Spacing = 5,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Items =
                {
                    new StackLayoutItem(editorContainer, true),
                    new StackLayoutItem(new StackLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 5,
                        Items = { outputModeSelector, outputModeOptions }
                    }, HorizontalAlignment.Left)
                }
            };

            absoluteModeEditor.SettingsBinding.Bind(ProfileBinding.Child(p => p!.AbsoluteModeSettings));
            relativeModeEditor.SettingsBinding.Bind(ProfileBinding.Child(p => p!.RelativeModeSettings));

            outputModeSelector.SelectedItemBinding.Convert<PluginSettingStore?>(
                c => PluginSettingStore.FromPath(c?.FullName),
                v => v?.GetTypeInfo()
            ).Bind(ProfileBinding.Child(c => (PluginSettingStore?)c!.OutputMode));

            outputModeSelector.SelectedValueChanged += (sender, e) => UpdateOutputMode(Profile?.OutputMode);
            outputModeOptions.Click += (_, _) => EditOutputModeOptions();

            App.Driver.TabletsChanged += (sender, e) => UpdateTablet(e);
            UpdateTablet();
        }

        // ReSharper disable once AsyncVoidMethod
        private void UpdateTablet(IEnumerable<TabletReference>? tablets = null) => Application.Instance.AsyncInvoke(async void () =>
        {
            tablets ??= await App.Driver.Instance!.GetTablets();
            var selectedTablet = tablets.FirstOrDefault(t => t.Properties.Name == Profile?.Tablet);
            if (selectedTablet != null)
                SetTabletSize(selectedTablet);
        });

        private Profile? profile;
        public Profile? Profile
        {
            set
            {
                this.profile = value;
                this.OnProfileChanged();
            }
            get => this.profile;
        }

        public event EventHandler<EventArgs>? ProfileChanged;

        protected virtual void OnProfileChanged()
        {
            ProfileChanged?.Invoke(this, new EventArgs());
            UpdateTablet();
            UpdateOutputMode(Profile?.OutputMode);
            outputModeSelector.Enabled = Profile != null;
        }

        public BindableBinding<OutputModeEditor, Profile?> ProfileBinding
        {
            get
            {
                return new BindableBinding<OutputModeEditor, Profile?>(
                    this,
                    c => c.Profile,
                    (c, v) => c.Profile = v,
                    (c, h) => c.ProfileChanged += h,
                    (c, h) => c.ProfileChanged -= h
                );
            }
        }

        private Panel editorContainer = new Panel();
        private AbsoluteModeEditor absoluteModeEditor = new AbsoluteModeEditor();
        private RelativeModeEditor relativeModeEditor = new RelativeModeEditor();
        private TypeDropDown<IOutputMode> outputModeSelector = new TypeDropDown<IOutputMode> { Width = 300 };
        private Button outputModeOptions = new Button { Text = "Options...", Visible = false };
        private Label outputModeUnsupported = new Label { Text = "No supported output mode selected.", TextAlignment = TextAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        private Label tabletUnavailable = new Label { Text = "No tablets were detected or selected.", TextAlignment = TextAlignment.Center, VerticalAlignment = VerticalAlignment.Center };

        public void SetTabletSize(TabletReference tablet)
        {
            var tabletAreaEditor = absoluteModeEditor.tabletAreaEditor;
            if (tablet.Properties.Specifications.Digitizer is DigitizerSpecifications digitizer)
            {
                tabletAreaEditor.AreaBounds =
                [
                    new RectangleF(0, 0, digitizer.Width, digitizer.Height),
                ];
            }
            else
            {
                tabletAreaEditor.AreaBounds = null;
            }
        }

        public void SetDisplaySize(IEnumerable<IDisplay>? displays)
        {
            var bgs = from disp in displays
                      where !(disp is IVirtualScreen)
                      select new RectangleF(disp.Position.X, disp.Position.Y, disp.Width, disp.Height);
            absoluteModeEditor.displayAreaEditor.AreaBounds = bgs;
        }

        private void UpdateOutputMode(PluginSettingStore? store)
        {
            bool showAbsolute = false;
            bool showRelative = false;
            outputModeOptions.Visible = store?.GetTypeInfo<IOutputMode>()?.GetProperties()
                .Any(property => property.GetCustomAttributes(true).Any(attribute => attribute is PropertyAttribute)) == true;

            if (store?.GetTypeInfo<IOutputMode>() is TypeInfo outputMode)
            {
                showAbsolute = outputMode.IsSubclassOf(typeof(AbsoluteOutputMode));
                showRelative = outputMode.IsSubclassOf(typeof(RelativeOutputMode));
            }

            if (showAbsolute)
                editorContainer.Content = absoluteModeEditor;
            else if (showRelative)
                editorContainer.Content = relativeModeEditor;
            else if (Profile != null)
                editorContainer.Content = outputModeUnsupported;
            else
                editorContainer.Content = tabletUnavailable;
        }

        private void EditOutputModeOptions()
        {
            if (Profile?.OutputMode is not PluginSettingStore store)
                return;

            // Stage edits just like the binding editor: Cancel must leave the
            // current profile untouched, and OK uses the normal Save/Apply flow.
            var edited = store.Clone();
            if (edited == null)
                return;
            var editor = new PluginSettingStoreEditor<IOutputMode>();
            var scroll = new Scrollable
            {
                Border = BorderType.None,
                Content = new StackLayout
                {
                    Width = 1,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Items = { editor }
                }
            };
            using var dialog = new Dialog<bool>
            {
                Title = $"{store.Name ?? "Output mode"} options",
                Size = new Size(500, 450),
                Padding = 5,
                Resizable = true
            };
            var accept = new Button { Text = "OK" };
            var cancel = new Button { Text = "Cancel" };
            accept.Click += (_, _) => dialog.Close(true);
            cancel.Click += (_, _) => dialog.Close(false);
            dialog.DefaultButton = accept;
            dialog.AbortButton = cancel;
            dialog.Content = new StackLayout
            {
                Spacing = 5,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Items =
                {
                    new StackLayoutItem(scroll, true),
                    new Label { Text = "Use Save or Apply in the main window to activate changes." },
                    new StackLayoutItem(new StackLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 5,
                        Items = { cancel, accept }
                    }, HorizontalAlignment.Right)
                }
            };
            scroll.SizeChanged += (_, _) => editor.ResizeLayoutItems();
            editor.Store = edited;
            if (dialog.ShowModal(this))
                store.Settings = edited.Settings;
        }

    }
}
