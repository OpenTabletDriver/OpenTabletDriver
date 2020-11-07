using System.Collections.Generic;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public class StackView : StackLayout
    {
        public StackView(IEnumerable<Control> controls) : this()
        {
            foreach (var ctrl in controls)
                AddControl(ctrl);
        }

        public StackView(params Control[] controls) : this((IEnumerable<Control>)controls)
        {
        }

        protected StackView()
        {
            base.HorizontalContentAlignment = defaultAlignment;
            base.Spacing = 5;
        }

        private const HorizontalAlignment defaultAlignment = HorizontalAlignment.Stretch;

        public void AddControl(Control ctrl, HorizontalAlignment alignment = defaultAlignment)
        {
            var newItem = new StackLayoutItem
            {
                Control = ctrl,
                HorizontalAlignment = alignment
            };
            base.Items.Add(newItem);
        }

        public void AddControls(IEnumerable<Control> controls, HorizontalAlignment alignment = defaultAlignment)
        {
            foreach (var ctrl in controls)
                AddControl(ctrl, alignment);
        }
    }
}