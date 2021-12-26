using Eto.Forms;

namespace OpenTabletDriver.UX
{
    using static App;

    public abstract class ChildDialog : Dialog
    {
        protected ChildDialog(Window parentWindow)
        {
            Owner = parentWindow;
            Title = "OpenTabletDriver";
            Icon = Logo.WithSize(Logo.Size);
        }
    }
}
