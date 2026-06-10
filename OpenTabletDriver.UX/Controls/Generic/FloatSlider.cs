using System;
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
                    new StackLayoutItem(nb)
                }
            };

            this.slider.MaxValue = 100; // as default only
        }

        public event EventHandler<EventArgs>? ValueChanged;

        private readonly Slider slider = new();

        private float _value;

        public float Value
        {
            set
            {
                this._value = value;
                ValueChanged?.Invoke(this, new EventArgs());
            }
            get => this._value;
        }

        public int Minimum
        {
            get => this.slider.MinValue;
            set => this.slider.MinValue = value;
        }

        public int Maximum
        {
            get => this.slider.MaxValue;
            set => this.slider.MaxValue = value;
        }

        public int StepSize
        {
            get => this.slider.TickFrequency;
            set => this.slider.TickFrequency = value;
        }

        public bool SnapToTick
        {
            get => slider.SnapToTick;
            set => slider.SnapToTick = value;
        }

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
