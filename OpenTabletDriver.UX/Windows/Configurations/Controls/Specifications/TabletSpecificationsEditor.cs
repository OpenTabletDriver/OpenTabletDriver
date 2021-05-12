using System;
using Eto.Forms;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Windows.Configurations.Controls.Specifications
{
    public class TabletSpecificationsEditor : Panel
    {
        public TabletSpecificationsEditor()
        {
            this.Content = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Spacing = 5,
                Padding = 5,
                Items =
                {
                    new Expander
                    {
                        Header = "Digitizer",
                        Content = digitizer = new DigitizerSpecificationsEditor()
                    },
                    new Expander
                    {
                        Header = "Pen",
                        Content = pen = new PenSpecificationsEditor()
                    },
                    new Expander
                    {
                        Header = "Auxiliary Buttons",
                        Padding = 5,
                        Content = auxButtons = new ButtonSpecificationsEditor()
                    },
                    new Expander
                    {
                        Header = "Touch",
                        Content = touch = new DigitizerSpecificationsEditor()
                    }
                }
            };

            digitizer.DigitizerSpecificationsBinding.Bind(TabletSpecificationsBinding.Child(c => c.Digitizer));
            pen.PenSpecificationsBinding.Bind(TabletSpecificationsBinding.Child(c => c.Pen));
            auxButtons.ButtonSpecificationsBinding.Bind(TabletSpecificationsBinding.Child(c => c.AuxiliaryButtons));
            touch.DigitizerSpecificationsBinding.Bind(TabletSpecificationsBinding.Child(c => c.Touch));
        }

        private DigitizerSpecificationsEditor digitizer, touch;
        private PenSpecificationsEditor pen;
        private ButtonSpecificationsEditor auxButtons;

        private TabletSpecifications tabletSpecs;
        public TabletSpecifications TabletSpecifications
        {
            set
            {
                this.tabletSpecs = value;
                this.OnSpecificationsChanged();
            }
            get => this.tabletSpecs;
        }
        
        public event EventHandler<EventArgs> TabletSpecificationsChanged;
        
        protected virtual void OnSpecificationsChanged() => TabletSpecificationsChanged?.Invoke(this, new EventArgs());
        
        public BindableBinding<TabletSpecificationsEditor, TabletSpecifications> TabletSpecificationsBinding
        {
            get
            {
                return new BindableBinding<TabletSpecificationsEditor, TabletSpecifications>(
                    this,
                    c => c.TabletSpecifications,
                    (c, v) => c.TabletSpecifications = v,
                    (c, h) => c.TabletSpecificationsChanged += h,
                    (c, h) => c.TabletSpecificationsChanged -= h
                );
            }
        }
    }
}