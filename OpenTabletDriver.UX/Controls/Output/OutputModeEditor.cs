using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop;
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
                StoreBinding.Convert<TypeInfo>(
                    c => c?.GetPluginReference().GetTypeReference(),
                    t => PluginSettingStore.FromPath(t.FullName)
                )
            );
        }

        private PluginSettingStore store;
        public PluginSettingStore Store
        {
            set
            {
                this.store = value;
                this.OnStoreChanged();
            }
            get => this.store;
        }

        public event EventHandler<EventArgs> StoreChanged;

        protected virtual void OnStoreChanged()
        {
            StoreChanged?.Invoke(this, new EventArgs());
            UpdateOutputMode(this.Store);
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

        private Panel editorContainer = new Panel();
        private AbsoluteModeEditor absoluteModeEditor = new AbsoluteModeEditor();
        private RelativeModeEditor relativeModeEditor = new RelativeModeEditor();
        private TypeDropDown<IOutputMode> outputModeSelector = new TypeDropDown<IOutputMode> { Width = 300 };

        public void SetTabletSize(TabletState tablet)
        {
            var tabletAreaEditor = absoluteModeEditor.tabletAreaEditor;
            if (tablet?.Properties?.Specifications?.Digitizer is DigitizerSpecifications digitizer)
            {
                tabletAreaEditor.AreaBounds = new RectangleF[]
                {
                    new RectangleF(0, 0, digitizer.Width, digitizer.Height)
                };

                var settings = App.Current.Settings;
                if (settings != null && settings.TabletWidth == 0 && settings.TabletHeight == 0)
                {
                    settings.TabletWidth = digitizer.Width;
                    settings.TabletHeight = digitizer.Height;
                    settings.TabletX = digitizer.Width / 2;
                    settings.TabletY = digitizer.Height / 2;
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
