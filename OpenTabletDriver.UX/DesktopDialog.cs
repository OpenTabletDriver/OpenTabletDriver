using System;
using Eto.Drawing;
using Eto.Forms;

namespace OpenTabletDriver.UX
{
    using static App;

    public abstract class DesktopDialog : Dialog
    {
        protected DesktopDialog(Window parentWindow)
        {
            Owner = parentWindow;

            Title = "OpenTabletDriver";
            Icon = Logo.WithSize(Logo.Size);
            ClientSize = new Size(DefaultWidth, DefaultHeight);
        }

        private bool platformInit;

        protected int DefaultWidth => (int)(Owner.Width * 0.9);
        protected int DefaultHeight => (int)(Owner.Height * 0.9);

        public event EventHandler<EventArgs> InitializePlatform;

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (!this.platformInit)
            {
                // Adjust to any platform quirks
                OnInitializePlatform(e);
            }
        }

        protected virtual void OnInitializePlatform(EventArgs e)
        {
            this.platformInit = true;
            InitializePlatform?.Invoke(this, e);

            if (this.ClientSize.Width > Screen.WorkingArea.Width || this.ClientSize.Height > Screen.WorkingArea.Height)
            {
                int width = (int)Math.Min(Screen.WorkingArea.Width * 0.9, DefaultWidth);
                int height = (int)Math.Min(Screen.WorkingArea.Height * 0.9, DefaultHeight);
                this.ClientSize = new Size(width, height);
            }
        }
    }
}
