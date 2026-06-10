using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.UX
{
    public abstract class DesktopForm : Form
    {
        public DesktopForm()
        {
            Icon = App.Logo.WithSize(App.Logo.Size);
        }

        public DesktopForm(Window parentWindow)
            : this()
        {
            Owner = parentWindow;
        }

        private bool initialized;

        protected virtual void InitializeForm()
        {
            ToCenter();
        }

        public new void Show()
        {
            if (!initialized)
            {
                initialized = true;
                if (ClientSize.Width == 0 && ClientSize.Height == 0)
                {
                    base.Show();
                    InitializeForm();
                }
                else
                {
                    InitializeForm();
                    base.Show();
                }
            }
            else
            {
                base.Show();
            }
        }

        private void ToCenter()
        {
            if (SystemInterop.CurrentPlatform == PluginPlatform.Windows)
            {
                var x = Owner.Location.X + (Owner.Size.Width / 2);
                var y = Owner.Location.Y + (Owner.Size.Height / 2);
                var center = new PointF(x, y);

                Location = new Point((int)(center.X - (ClientSize.Width / 2)), (int)(center.Y - (ClientSize.Height / 2)));
            }
        }
    }
}
