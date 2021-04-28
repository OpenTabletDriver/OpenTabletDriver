using System;
using Eto.Forms;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Controls.Generic.Text;

namespace OpenTabletDriver.UX.Windows.Configurations.Controls.Specifications
{
    public class DigitizerSpecificationsEditor : Panel
    {
        public DigitizerSpecificationsEditor()
        {
            this.Content = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Spacing = 5,
                Padding = 5,
                Items = 
                {
                    new Group
                    {
                        Text = "Width (mm)",
                        Orientation = Orientation.Horizontal,
                        Content = width = new FloatNumberBox()
                    },
                    new Group
                    {
                        Text = "Height (mm)",
                        Orientation = Orientation.Horizontal,
                        Content = height = new FloatNumberBox()
                    },
                    new Group
                    {
                        Text = "Horizontal Resolution",
                        Orientation = Orientation.Horizontal,
                        Content = maxX = new FloatNumberBox()
                    },
                    new Group
                    {
                        Text = "Vertical Resolution",
                        Orientation = Orientation.Horizontal,
                        Content = maxY = new FloatNumberBox()
                    }
                }
            };

            width.ValueBinding.Bind(DigitizerSpecificationsBinding.Child<float>(c => c.Width));
            height.ValueBinding.Bind(DigitizerSpecificationsBinding.Child<float>(c => c.Height));
            maxX.ValueBinding.Bind(DigitizerSpecificationsBinding.Child<float>(c => c.MaxX));
            maxY.ValueBinding.Bind(DigitizerSpecificationsBinding.Child<float>(c => c.MaxY));
        }

        private MaskedTextBox<float> width, height, maxX, maxY;

        private DigitizerSpecifications digitizerSpecs;
        public DigitizerSpecifications DigitizerSpecifications
        {
            set
            {
                this.digitizerSpecs = value;
                this.OnDigitizerSpecificationsChanged();
            }
            get => this.digitizerSpecs;
        }
        
        public event EventHandler<EventArgs> DigitizerSpecificationsChanged;
        
        protected virtual void OnDigitizerSpecificationsChanged() => DigitizerSpecificationsChanged?.Invoke(this, new EventArgs());
        
        public BindableBinding<DigitizerSpecificationsEditor, DigitizerSpecifications> DigitizerSpecificationsBinding
        {
            get
            {
                return new BindableBinding<DigitizerSpecificationsEditor, DigitizerSpecifications>(
                    this,
                    c => c.DigitizerSpecifications,
                    (c, v) => c.DigitizerSpecifications = v,
                    (c, h) => c.DigitizerSpecificationsChanged += h,
                    (c, h) => c.DigitizerSpecificationsChanged -= h
                );
            }
        }
    }
}