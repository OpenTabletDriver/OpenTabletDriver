using System;
using System.ComponentModel;
using System.Threading;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public abstract class TimedDrawable : Drawable
    {
        private const int FRAMES_PER_MS = 1000 / 60;

        private Timer timer;

        protected override void OnLoadComplete(EventArgs e)
        {
            base.OnLoadComplete(e);

            timer = new Timer(
                (o) => Application.Instance.AsyncInvoke(this.Invalidate),
                null,
                0,
                FRAMES_PER_MS
            );

            base.ParentWindow.Closing += (sender, e) => OnWindowClosing(e);
        }

        protected abstract void OnNextFrame(PaintEventArgs e);

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            OnNextFrame(e);
        }

        protected virtual void OnWindowClosing(CancelEventArgs e)
        {
            if (!e.Cancel)
            {
                timer?.Dispose();
                timer = null;
            }
        }

        ~TimedDrawable()
        {
            timer?.Dispose();
            timer = null;
        }
    }
}
