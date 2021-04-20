using System;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Binding;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.UX.Windows.Bindings
{
    public class BindingEditorDialog : Dialog<PluginSettingStore>
    {
        public BindingEditorDialog(PluginSettingStore currentBinding = null)
        {
            Title = "Binding Editor";

            this.Content = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Padding = new Padding(5),
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = bindingController = new BindingController
                        {
                            Store = currentBinding,
                            Width = 300,
                            Height = 150
                        }
                    },
                    new StackLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 5,
                        Items =
                        {
                            new StackLayoutItem
                            {
                                Expand = true,
                                Control = new Button(ClearBinding)
                                {
                                    Text = "Clear"
                                }
                            },
                            new StackLayoutItem
                            {
                                Expand = true,
                                Control = new Button(ApplyBinding)
                                {
                                    Text = "Apply"
                                }
                            }
                        }
                    }
                },
            };
        }

        private BindingController bindingController;

        private void ClearBinding(object sender, EventArgs e)
        {
            Close(null);
        }

        private void ApplyBinding(object sender, EventArgs e)
        {
            Close(bindingController.Store);
        }

        private static string ParseMouseButton(MouseEventArgs e)
        {
            switch (e.Buttons)
            {
                case MouseButtons.Primary:
                    return nameof(MouseButton.Left);
                case MouseButtons.Middle:
                    return nameof(MouseButton.Middle);
                case MouseButtons.Alternate:
                    return nameof(MouseButton.Right);
                default:
                    return nameof(MouseButton.None);
            }
        }

        private class BindingController : TextArea
        {
            public BindingController()
            {
                TextAlignment = TextAlignment.Center;
                ReadOnly = true;
                ToolTip = TOOLTIP;
                Text = TOOLTIP;
            }

            private const string TOOLTIP = "Press a key, combination of keys, or a mouse button.";

            private PluginSettingStore store;
            public PluginSettingStore Store
            {
                set
                {
                    this.store = value;
                    Refresh();
                }
                get => this.store;
            }

            public void Refresh()
            {
                this.Text = Store?.GetHumanReadableString() ?? TOOLTIP;
            }

            protected override void OnKeyDown(KeyEventArgs e)
            {
                PluginSettingStore store;
                if (e.Modifiers == 0)
                {
                    store = new PluginSettingStore(typeof(KeyBinding));
                    store[nameof(KeyBinding.Key)].SetValue(e.Key.ToString());   
                }
                else
                {
                    store = new PluginSettingStore(typeof(MultiKeyBinding));
                    store[nameof(MultiKeyBinding.Keys)].SetValue(e.KeyData.ToShortcutString());
                }
                this.Store = store;
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                var store = new PluginSettingStore(typeof(MouseBinding));
                store[nameof(MouseBinding.Button)].SetValue(ParseMouseButton(e));
                this.Store = store;
            }

        }
    }
}
