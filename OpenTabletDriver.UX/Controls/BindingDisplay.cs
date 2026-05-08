using System;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.UX.Windows.Bindings;

namespace OpenTabletDriver.UX.Controls
{
    public class BindingDisplay : Panel
    {
        public BindingDisplay()
        {
            Button mainButton, advancedButton;

            this.Content = new StackLayout
            {
                Spacing = 5,
                MinimumSize = new Size(300, 0),
                Orientation = Orientation.Horizontal,
                Items =
                {
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = mainButton = new Button()
                    },
                    new StackLayoutItem
                    {
                        Control = advancedButton = new Button
                        {
                            Text = "...",
                            Width = 25
                        }
                    }
                }
            };

            mainButton.TextBinding.Bind(this.StoreBinding.Convert<string>(s => s?.GetHumanReadableString()));

            mainButton.Click += async (_, _) =>
            {
                var dialog = new BindingEditorDialog(Store);
                this.Store = await dialog.ShowModalAsync(this);
            };

            advancedButton.Click += async (_, _) =>
            {
                var dialog = new AdvancedBindingEditorDialog(Store);
                this.Store = await dialog.ShowModalAsync(this);
            };
        }

        public event EventHandler<EventArgs> StoreChanged;

        private PluginSettingStore store;
        public PluginSettingStore Store
        {
            set
            {
                this.store = value;
                StoreChanged?.Invoke(this, EventArgs.Empty);
            }
            get => this.store;
        }

        public BindableBinding<BindingDisplay, PluginSettingStore> StoreBinding
        {
            get
            {
                return new BindableBinding<BindingDisplay, PluginSettingStore>(
                    this,
                    c => c.Store,
                    (c, v) => c.Store = v,
                    (c, h) => c.StoreChanged += h,
                    (c, h) => c.StoreChanged -= h
                );
            }
        }
    }
}
