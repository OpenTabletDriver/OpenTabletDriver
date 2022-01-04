using System.Collections;
using System.Collections.Generic;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public class StackedContent : StackLayout, IEnumerable<Control>, IEnumerable<StackLayoutItem>
    {
        public StackedContent()
        {
            this.Padding = 5;
            this.Spacing = 5;
            this.HorizontalContentAlignment = HorizontalAlignment.Center;
        }

        public void Add(Control control)
        {
            var item = new StackLayoutItem(control);
            this.Items.Add(item);
        }

        public void Add(params string[] items)
        {
            var control = new TextContent(items);
            this.Add(control);
        }

        public void Add(StackLayoutItem item) => this.Items.Add(item);

        IEnumerator<Control> IEnumerable<Control>.GetEnumerator() => this.Controls.GetEnumerator();

        IEnumerator<StackLayoutItem> IEnumerable<StackLayoutItem>.GetEnumerator() => this.Items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.Controls.GetEnumerator();
    }
}
