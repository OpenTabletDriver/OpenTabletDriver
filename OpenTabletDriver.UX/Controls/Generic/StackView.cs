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
            base.HorizontalContentAlignment = defaultHorizontalAlignment;
            base.VerticalContentAlignment = defaultVerticalAlignment;
            base.Spacing = 5;
        }

        private const HorizontalAlignment defaultHorizontalAlignment = HorizontalAlignment.Stretch;
        private const VerticalAlignment defaultVerticalAlignment = VerticalAlignment.Stretch;

        public void AddControl(Control ctrl, bool expand = false)
        {
            var newItem = new StackLayoutItem(ctrl, expand);
            base.Items.Add(newItem);
        }

        public void AddControls(IEnumerable<Control> controls)
        {
            foreach (var ctrl in controls)
                AddControl(ctrl);
        }
    }
}
