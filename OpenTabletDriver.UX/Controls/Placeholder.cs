using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.UX.Components;

namespace OpenTabletDriver.UX.Controls
{
    public sealed class Placeholder : DesktopPanel
    {
        private readonly Panel _extraPanel;
        private string? _text;
        private Control? _extraContent;

        // TODO: Clean up Placeholder before using extensively, lots of legacy code smell
        public Placeholder()
        {
            Label label;
            Content = new StackLayout
            {
                Spacing = 5,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Items =
                {
                    new StackLayoutItem(null, true),
                    new Bitmap(Metadata.Logo.WithSize(256, 256)),
                    new StackLayoutItem
                    {
                        Control = label = new Label()
                    },
                    new StackLayoutItem
                    {
                        Control = _extraPanel = new Panel()
                    },
                    new StackLayoutItem(null, true)
                }
            };

            label.TextBinding.Bind(TextBinding);
        }

        public string? Text
        {
            set
            {
                _text = value;
                OnTextChanged();
            }
            get => _text;
        }

        public event EventHandler<EventArgs>? TextChanged;

        private void OnTextChanged() => TextChanged?.Invoke(this, EventArgs.Empty);

        public BindableBinding<Placeholder, string?> TextBinding
        {
            get
            {
                return new BindableBinding<Placeholder, string?>(
                    this,
                    c => c.Text,
                    (c, v) => c.Text = v,
                    (c, h) => c.TextChanged += h,
                    (c, h) => c.TextChanged -= h
                );
            }
        }

        public Control? ExtraContent
        {
            set
            {
                _extraContent = value;
                _extraPanel.Content = value;
            }
            get => _extraContent;
        }
    }
}
