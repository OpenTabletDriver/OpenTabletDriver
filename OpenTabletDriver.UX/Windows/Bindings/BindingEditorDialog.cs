using System;
using System.Text;
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
            Result = currentBinding;

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
                Keys keys = e.KeyData;

                if (keys == Keys.None)
                    return;

                if (keys.HasFlag(Keys.Alt | Keys.LeftAlt) || keys.HasFlag(Keys.Alt | Keys.RightAlt))
                    keys &= ~Keys.Alt;
                else if (keys.HasFlag(Keys.Control | Keys.LeftControl) || keys.HasFlag(Keys.Control | Keys.RightControl))
                    keys &= ~Keys.Control;
                else if (keys.HasFlag(Keys.Shift | Keys.LeftShift) || keys.HasFlag(Keys.Shift | Keys.RightShift))
                    keys &= ~Keys.Shift;
                else if (keys.HasFlag(Keys.Application | Keys.LeftApplication) || keys.HasFlag(Keys.Application | Keys.RightApplication))
                    keys &= ~Keys.Application;

                if ((keys & Keys.ModifierMask) == 0)
                {
                    store = new PluginSettingStore(typeof(KeyBinding));
                    store[nameof(KeyBinding.Key)].SetValue(keys.ToString());
                }
                else
                {
                    store = new PluginSettingStore(typeof(MultiKeyBinding));
                    store[nameof(MultiKeyBinding.Keys)].SetValue(CreateShortcutString(keys));
                }
                this.Store = store;
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                var store = new PluginSettingStore(typeof(MouseBinding));
                store[nameof(MouseBinding.Button)].SetValue(ParseMouseButton(e));
                this.Store = store;
            }

            private static void AppendSeparator(StringBuilder sb, string separator, string text)
            {
                if (sb.Length > 0)
                    sb.Append(separator);
                sb.Append(text);
            }

            private static string CreateShortcutString(Keys keys)
            {
                var sb = new StringBuilder();

                if (keys.HasFlag(Keys.Application))
                    AppendSeparator(sb, "+", nameof(Keys.Application));
                if (keys.HasFlag(Keys.Control))
                    AppendSeparator(sb, "+", nameof(Keys.Control));
                if (keys.HasFlag(Keys.Shift))
                    AppendSeparator(sb, "+", nameof(Keys.Shift));
                if (keys.HasFlag(Keys.Alt))
                    AppendSeparator(sb, "+", nameof(Keys.Alt));

                var mainKey = keys & Keys.KeyMask;
                AppendSeparator(sb, "+", mainKey.ToString());

                return sb.ToString();
            }
        }
    }
}
