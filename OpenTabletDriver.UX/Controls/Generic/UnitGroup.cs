using System;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public class UnitGroup : Group
    {
        public UnitGroup()
        {
            unitLabel.TextBinding.Bind(UnitBinding);
        }

        public string Unit
        {
            set
            {
                field = value;
                this.OnUnitChanged();
            }
            get;
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

        private readonly Label unitLabel = new Label();

        public new Control Content
        {
            set
            {
                field = value;
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
            get;
        }
    }
}
