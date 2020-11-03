using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public class GroupBoxBase : GroupBox
    {
        public GroupBoxBase(string name, Control control) : this(name)
        {
            base.Content = control;
        }

        protected GroupBoxBase(string name) : this()
        {
            base.Text = name;
        }

        protected GroupBoxBase()
        {
            base.Padding = App.GroupBoxPadding;
        }
    }
}