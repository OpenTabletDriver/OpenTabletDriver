using MonoMac.AppKit;

namespace OpenTabletDriver.UX.MacOS;

class FormHandler : Eto.Mac.Forms.FormHandler
{
    public override void Show()
    {
        NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
        base.Show();
    }
}
