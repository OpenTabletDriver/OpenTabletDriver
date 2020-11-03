using System.Collections.Generic;
using Eto.Drawing;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public class ExpanderBase : Expander
    {
        public ExpanderBase(string name, bool isExpanded, params Control[] controls) : this(name, isExpanded, (IEnumerable<Control>)controls)
        {
        }

        public ExpanderBase(string name, bool isExpanded, IEnumerable<Control> controls)
        {
            base.Header = name;
            base.Content = new StackView(controls);
            base.Expanded = isExpanded;
            base.Padding = new Padding(0, 5, 0, 0);
        }

        public StackView StackView
        {
            set => base.Content = value;
            get => base.Content as StackView;
        }
    }
}