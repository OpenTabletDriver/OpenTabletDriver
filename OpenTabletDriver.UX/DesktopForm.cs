using System;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.UX
{
    using static App;

    public abstract class DesktopForm : Form
    {
        protected DesktopForm()
        {
            Icon = Logo.WithSize(Logo.Size);
        }
        
        protected DesktopForm(Window parent)
            : this()
        {
            Owner = parent;
        }

        private bool platformInit;
        public const int DEFAULT_CLIENT_WIDTH = 960;
        public const int DEFAULT_CLIENT_HEIGHT = 760;

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
                int width = (int)Math.Min(Screen.WorkingArea.Width * 0.9, DEFAULT_CLIENT_WIDTH);
                int height = (int)Math.Min(Screen.WorkingArea.Height * 0.9, DEFAULT_CLIENT_HEIGHT);
                this.ClientSize = new Size(width, height);
            }

            switch (SystemInterop.CurrentPlatform)
            {
                case PluginPlatform.Windows:
                case PluginPlatform.MacOS:
                {
                    var x = Screen.WorkingArea.Center.X - (this.Width / 2);
                    var y = Screen.WorkingArea.Center.Y - (this.Height / 2);
                    this.Location = new Point((int)x, (int)y);
                    break;
                }
            }
        }
    }
}
