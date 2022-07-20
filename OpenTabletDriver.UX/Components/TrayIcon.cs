using Eto.Forms;

namespace OpenTabletDriver.UX.Components
{
    public class TrayIcon : TrayIndicator
    {
        public TrayIcon()
        {
            // TODO: Implement Tray Icon
            Title = "OpenTabletDriver";
            Image = Metadata.Logo;
        }
    }
}
