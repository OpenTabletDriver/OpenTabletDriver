using System;
using Eto.Forms;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Controls.Output.Area
{
    public class UnitGroup : Group
    {
        public UnitGroup()
        {
            unitLabel.TextBinding.Bind(UnitBinding);
        }

        private string unit;
        public string Unit
        {
            set
            {
                this.unit = value;
                this.OnUnitChanged();
            }
            get => this.unit;
        }

        public event EventHandler<EventArgs> UnitChanged;

        protected virtual void OnUnitChanged() => UnitChanged?.Invoke(this, new EventArgs());

        public BindableBinding<UnitGroup, string> UnitBinding
        {
            get
            {
                return new BindableBinding<UnitGroup, string>(
                    this,
                    c => c.Unit,
                    (c, v) => c.Unit = v,
                    (c, h) => c.UnitChanged += h,
                    (c, h) => c.UnitChanged -= h
                );
            }
        }

        private Label unitLabel = new Label();

        private Control content;
        public new Control Content
        {
            set
            {
                this.content = value;
                base.Content = new StackLayout
                {
                    Spacing = 5,
                    Orientation = Orientation.Horizontal,
                    Items =
                    {
                        new StackLayoutItem(this.Content, true),
                        new StackLayoutItem
                        {
                            VerticalAlignment = VerticalAlignment.Center,
                            Control = this.unitLabel
                        }
                    }
                };
            }
            get => this.content;
        }
    }
}
