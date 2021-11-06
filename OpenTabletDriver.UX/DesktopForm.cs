using Eto.Drawing;
using Eto.Forms;

namespace OpenTabletDriver.UX
{
    using static App;

    public abstract class DesktopForm : Form
    {
        public DesktopForm()
        {
            Icon = Logo.WithSize(Logo.Size);
        }

        public DesktopForm(Window parentWindow)
            : this()
        {
            Owner = parentWindow;
        }

        private bool initialized;

        protected virtual void InitializeForm()
        {
            var x = Owner.Location.X + (Owner.Size.Width / 2);
            var y = Owner.Location.Y + (Owner.Size.Height / 2);
            var center = new PointF(x, y);

            Location = new Point((int)(center.X - (ClientSize.Width / 2)), (int)(center.Y - (ClientSize.Height / 2)));
        }

        public new void Show()
        {
            if (!initialized)
            {
                InitializeForm();
                initialized = true;
            }
            base.Show();
        }
    }
}
