using System.Collections.Generic;
using Eto.Drawing;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public class ExpanderBase : Expander
    {
        public ExpanderBase(string name, bool isExpanded)
        {
            base.Header = name;
            base.Expanded = isExpanded;
            base.Padding = new Padding(0, 5, 0, 0);
        }
    }
}