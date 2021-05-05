using System;
using Eto.Forms;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Controls.Generic.Text;

namespace OpenTabletDriver.UX.Windows.Configurations.Controls.Specifications
{
    public class ButtonSpecificationsEditor : Panel
    {
        public ButtonSpecificationsEditor()
        {
            this.Content = new Group
            {
                Text = "Button Count",
                Orientation = Orientation.Horizontal,
                Content = buttonCount = new UnsignedIntegerNumberBox()
            };

            buttonCount.ValueBinding.Bind(ButtonSpecificationsBinding.Child(b => b.ButtonCount));
        }

        private MaskedTextBox<uint> buttonCount;

        private ButtonSpecifications buttonSpecs;
        public ButtonSpecifications ButtonSpecifications
        {
            set
            {
                this.buttonSpecs = value;
                this.OnButtonSpecificationsChanged();
            }
            get => this.buttonSpecs;
        }
        
        public event EventHandler<EventArgs> ButtonSpecificationsChanged;
        
        protected virtual void OnButtonSpecificationsChanged() => ButtonSpecificationsChanged?.Invoke(this, new EventArgs());
        
        public BindableBinding<ButtonSpecificationsEditor, ButtonSpecifications> ButtonSpecificationsBinding
        {
            get
            {
                return new BindableBinding<ButtonSpecificationsEditor, ButtonSpecifications>(
                    this,
                    c => c.ButtonSpecifications,
                    (c, v) => c.ButtonSpecifications = v,
                    (c, h) => c.ButtonSpecificationsChanged += h,
                    (c, h) => c.ButtonSpecificationsChanged -= h
                );
            }
        }
    }
}