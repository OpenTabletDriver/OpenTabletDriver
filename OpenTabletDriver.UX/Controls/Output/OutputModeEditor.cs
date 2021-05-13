using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.UX.Controls.Generic;

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

            outputModeSelector.SelectedValueChanged += (sender, args) =>
            {
                if (outputModeSelector.SelectedType is TypeInfo type)
                    this.Store = new PluginSettingStore(type);
            };

            outputModeSelector.SelectedTypeBinding.Bind(
                StoreBinding.Convert(
                    c => c?.GetPluginReference().GetTypeReference(),
                    t => PluginSettingStore.FromPath(t.FullName)
                )
            );

            StoreBinding.Bind(App.Current.ProfileBinding.Child(p => p.OutputMode));
            SetDisplaySize(DesktopInterop.VirtualScreen.Displays);
        }

        private Panel editorContainer = new Panel();
        private AbsoluteModeEditor absoluteModeEditor = new AbsoluteModeEditor();
        private RelativeModeEditor relativeModeEditor = new RelativeModeEditor();
        private TypeDropDown<IOutputMode> outputModeSelector = new TypeDropDown<IOutputMode> { Width = 300 };
        private PluginSettingStore store;

        public event EventHandler<EventArgs> StoreChanged;

        public PluginSettingStore Store
        {
            set
            {
                this.store = value;
                this.OnStoreChanged();
            }
            get => this.store;
        }

        public BindableBinding<OutputModeEditor, PluginSettingStore> StoreBinding
        {
            get
            {
                return new BindableBinding<OutputModeEditor, PluginSettingStore>(
                    this,
                    c => c.Store,
                    (c, v) => c.Store = v,
                    (c, h) => c.StoreChanged += h,
                    (c, h) => c.StoreChanged -= h
                );
            }
        }

        public void SetTabletSize(TabletState tablet)
        {
            var tabletAreaEditor = absoluteModeEditor.tabletAreaEditor;
            if (tablet?.Properties?.Specifications?.Digitizer is DigitizerSpecifications digitizer)
            {
                tabletAreaEditor.AreaBounds = new RectangleF[]
                {
                    new RectangleF(0, 0, digitizer.Width, digitizer.Height)
                };

                var profile = App.Current.ProfileCache.ProfileInFocus;
                if (profile != null && profile.TabletWidth == 0 && profile.TabletHeight == 0)
                {
                    profile.TabletWidth = digitizer.Width;
                    profile.TabletHeight = digitizer.Height;
                    profile.TabletX = digitizer.Width / 2;
                    profile.TabletY = digitizer.Height / 2;
                }
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

        private void OnStoreChanged()
        {
            StoreChanged?.Invoke(this, EventArgs.Empty);
            UpdateOutputMode(this.Store);
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
