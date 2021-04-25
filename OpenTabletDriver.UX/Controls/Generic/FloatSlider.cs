using System;
using System.ComponentModel;
using Eto.Forms;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Controls.Generic.Text;

namespace OpenTabletDriver.UX.Controls
{
    /// <summary>
    /// A slider with a textbox for fine tuning a floating point value.
    /// </summary>
    public class FloatSlider : Panel
    {
        public FloatSlider()
        {
            var slider = new Slider
            {
                MinValue = Minimum,
                MaxValue = Maximum
            };

            var nb = new FloatNumberBox();

            slider.Bind(
                s => s.Value,
                nb.ValueBinding
            );

            nb.ValueBinding.Bind(this.ValueBinding);

            this.Content = new StackView
            {
                Orientation = Orientation.Horizontal,
                VerticalContentAlignment = VerticalAlignment.Center,
                Items =
                {
                    new StackLayoutItem(slider, true),
                    new StackLayoutItem(nb, false)
                }
            };
        }

        public event EventHandler<EventArgs> ValueChanged;

        private float value;
        public float Value
        {
            set
            {
                this.value = value;
                ValueChanged?.Invoke(this, new EventArgs());
            }
            get => this.value;
        }

        [DefaultValue(0)]
        public int Minimum { set; get; } = 0;

        [DefaultValue(100)]
        public int Maximum { set; get; } = 100;

        public BindableBinding<FloatSlider, float> ValueBinding
        {
            get
            {
                return new BindableBinding<FloatSlider, float>(
                    this,
                    c => c.Value,
                    (c, v) => c.Value = v,
                    (c, h) => c.ValueChanged += h,
                    (c, h) => c.ValueChanged -= h
                );
            }
        }
    }
}
