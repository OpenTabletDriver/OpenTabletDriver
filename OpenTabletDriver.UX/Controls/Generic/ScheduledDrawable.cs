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
                if (base.ParentWindow == null || base.ParentWindow.WindowState == WindowState.Minimized)
                    CompositionScheduler.Unregister(OnCompose);
                else
                    CompositionScheduler.Register(OnCompose);
            };

            CompositionScheduler.Register(OnCompose);
        }

        protected abstract void OnNextFrame(PaintEventArgs e);

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            OnNextFrame(e);
        }

        protected void OnCompose(object _, EventArgs a)
        {
            Invalidate();
        }

        ~ScheduledDrawable()
        {
            CompositionScheduler.Unregister(OnCompose);
        }
    }
}
