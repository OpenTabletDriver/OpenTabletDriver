using System;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Utilities
{
    public class ActionCommand : Command
    {
        public Action Action { set; get; }

        protected override void OnExecuted(EventArgs e)
        {
            base.OnExecuted(e);
            Action?.Invoke();
        }
    }
}
