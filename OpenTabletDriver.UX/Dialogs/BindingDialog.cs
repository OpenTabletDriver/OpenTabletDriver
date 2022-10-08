using System.Text;
using Eto.Forms;
using OpenTabletDriver.Desktop.Binding;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Platform.Pointer;
using OpenTabletDriver.UX.ViewModels;

namespace OpenTabletDriver.UX.Dialogs
{
    public sealed class BindingDialog : Dialog<DialogModel<PluginSettings>>
    {
        private readonly IPluginFactory _pluginFactory;
        private const string NULL_PROMPT = "Press a key, combination of keys, or a mouse button.";

        public BindingDialog(IPluginFactory pluginFactory, DirectBinding<PluginSettings?> binding)
        {
            _pluginFactory = pluginFactory;
            DataContext = binding.DataValue;

            Title = "Select a binding...";

            Width = 300;
            Height = 200;

            var tb = new TextArea
            {
                TextAlignment = TextAlignment.Center,
                Text = NULL_PROMPT,
                ReadOnly = true
            };
            tb.TextBinding.BindDataContext((PluginSettings? s) => Format(s));

            tb.MouseDown += MouseHandler;
            tb.KeyDown += KeyHandler;

            Content = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Padding = 5,
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem(tb, true),
                    new StackLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 5,
                        Items =
                        {
                            new StackLayoutItem(null, true),
                            new Button(Clear)
                            {
                                Text = "Clear"
                            },
                            new Button(Apply)
                            {
                                Text = "Apply"
                            }
                        }
                    }
                }
            };
        }

        private void Clear(object? sender, EventArgs e)
        {
            Close(new DialogModel<PluginSettings>(DialogResult.Ok));
        }

        private void Apply(object? sender, EventArgs e)
        {
            Close(new DialogModel<PluginSettings>(DialogResult.Ok, DataContext as PluginSettings));
        }

        private string Format(PluginSettings? settings)
        {
            return settings?.Format(_pluginFactory) ?? NULL_PROMPT;
        }

        private void MouseHandler(object? sender, MouseEventArgs args)
        {
            var button = args.Buttons switch
            {
                MouseButtons.Primary => MouseButton.Left,
                MouseButtons.Alternate => MouseButton.Right,
                MouseButtons.Middle => MouseButton.Middle,
                _ => MouseButton.None
            };

            if (button != MouseButton.None)
            {
                var settings = new PluginSettings(typeof(MouseBinding));
                settings.Set((MouseBinding b) => b.Button, Enum.GetName(button));

                DataContext = settings;
            }
        }

        private void KeyHandler(object? sender, KeyEventArgs args)
        {
            if (args.Key == Keys.None)
                return;

            var keys = args.KeyData;

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
                var settings = new PluginSettings(typeof(KeyBinding));
                settings.Set((KeyBinding k) => k.Key, keys.ToString());
                DataContext = settings;
            }
            else
            {
                var settings = new PluginSettings(typeof(MultiKeyBinding));
                settings.Set((MultiKeyBinding k) => k.Keys, CreateShortcutString(keys));
                DataContext = settings;
            }
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

        private static void AppendSeparator(StringBuilder sb, string separator, string text)
        {
            if (sb.Length > 0)
                sb.Append(separator);
            sb.Append(text);
        }
    }
}
