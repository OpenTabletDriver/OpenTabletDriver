using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.UX.Controls.Generic;
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
                    new StackLayoutItem(outputModeSelector, HorizontalAlignment.Left)
                }
            };

            absoluteModeEditor.SettingsBinding.Bind(ProfileBinding.Child(p => p.AbsoluteModeSettings));
            relativeModeEditor.SettingsBinding.Bind(ProfileBinding.Child(p => p.RelativeModeSettings));

            outputModeSelector.SelectedItemBinding.Convert<PluginSettingStore>(
                c => PluginSettingStore.FromPath(c?.FullName),
                v => v?.GetPluginReference().GetTypeReference()
            ).Bind(ProfileBinding.Child(c => c.OutputMode));

            outputModeSelector.SelectedValueChanged += (sender, e) => UpdateOutputMode(Profile?.OutputMode);

            App.Driver.AddConnectionHook(i => i.TabletsChanged += (sender, e) => UpdateTablet(e));
            UpdateTablet();
        }

        private void UpdateTablet(IEnumerable<TabletReference> tablets = null) => Application.Instance.AsyncInvoke(async () =>
        {
            tablets ??= await App.Driver.Instance.GetTablets();
            var selectedTablet = tablets.FirstOrDefault(t => t.Properties.Name == Profile?.Tablet);
            if (selectedTablet != null)
                SetTabletSize(selectedTablet);
        });

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

        protected virtual void OnProfileChanged()
        {
            ProfileChanged?.Invoke(this, new EventArgs());
            UpdateTablet();
        }

        public BindableBinding<OutputModeEditor, Profile> ProfileBinding
        {
            get
            {
                return new BindableBinding<OutputModeEditor, Profile>(
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

        public void SetTabletSize(TabletReference tablet)
        {
            var tabletAreaEditor = absoluteModeEditor.tabletAreaEditor;
            if (tablet?.Properties?.Specifications?.Digitizer is DigitizerSpecifications digitizer)
            {
                tabletAreaEditor.AreaBounds = new RectangleF[]
                {
                    new RectangleF(0, 0, digitizer.Width, digitizer.Height)
                };
            }
            else
            {
                tabletAreaEditor.AreaBounds = null;
            }
        }

        public void SetDisplaySize(IEnumerable<IDisplay> displays)
        {
            var bgs = from disp in displays
                where !(disp is IVirtualScreen)
                select new RectangleF(disp.Position.X, disp.Position.Y, disp.Width, disp.Height);
            absoluteModeEditor.displayAreaEditor.AreaBounds = bgs;
        }

        private void UpdateOutputMode(PluginSettingStore store)
        {
            bool showAbsolute = false;
            bool showRelative = false;
            if (store != null)
            {
                var outputMode = store.GetPluginReference().GetTypeReference<IOutputMode>();
                showAbsolute = outputMode.IsSubclassOf(typeof(AbsoluteOutputMode));
                showRelative = outputMode.IsSubclassOf(typeof(RelativeOutputMode));
            }

            if (showAbsolute)
                editorContainer.Content = absoluteModeEditor;
            else if (showRelative)
                editorContainer.Content = relativeModeEditor;
        }
    }
}
