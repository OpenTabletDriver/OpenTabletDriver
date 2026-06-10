using System;
using Eto.Forms;
using OpenTabletDriver.UX.Tools;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public abstract class ScheduledDrawable : Drawable
    {
        protected override void OnLoadComplete(EventArgs e)
        {
            base.OnLoadComplete(e);
            base.ParentWindow.Closing += (sender, e) => CompositionScheduler.Unregister(OnCompose);
            base.ParentWindow.WindowStateChanged += (sender, e) =>
            {
                this.Enabled =
                    base.ParentWindow != null &&
                    base.ParentWindow.WindowState != WindowState.Minimized;
            };
        }

        protected abstract void OnNextFrame(PaintEventArgs e);

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            OnNextFrame(e);
        }

        protected void OnCompose(object? _, EventArgs a)
        {
            Invalidate();
        }

        public override bool Enabled
        {
            get => base.Enabled;
            set
            {
                base.Enabled = value;
                if (value)
                    CompositionScheduler.Register(OnCompose);
                else
                    CompositionScheduler.Unregister(OnCompose);
            }
        }

        protected override void Dispose(bool disposing)
        {
            CompositionScheduler.Unregister(OnCompose);
            base.Dispose(disposing);
        }

        ~ScheduledDrawable()
        {
            Dispose(false);
        }
    }
}
