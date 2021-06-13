using System;
using Eto.Drawing;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls
{
    public class Placeholder : Panel
    {
        public Placeholder()
        {
            this.Content = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Items =
                {
                    new StackLayoutItem(null, true),
                    new Bitmap(App.Logo.WithSize(256, 256)),
                    new StackLayoutItem
                    {
                        Control = label = new Label()
                    },
                    new StackLayoutItem(null, true)
                }
            };

            label.TextBinding.Bind(TextBinding);
        }

        private Label label;
        
        private string text;
        public string Text
        {
            set
            {
                this.text = value;
                this.OnTextChanged();
            }
            get => this.text;
        }
        
        public event EventHandler<EventArgs> TextChanged;
        
        protected virtual void OnTextChanged() => TextChanged?.Invoke(this, new EventArgs());
        
        public BindableBinding<Placeholder, string> TextBinding
        {
            get
            {
                return new BindableBinding<Placeholder, string>(
                    this,
                    c => c.Text,
                    (c, v) => c.Text = v,
                    (c, h) => c.TextChanged += h,
                    (c, h) => c.TextChanged -= h
                );
            }
        }
    }
}