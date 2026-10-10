using System;
using System.Linq;
using Eto.Forms;
using MonoMac.AppKit;

namespace OpenTabletDriver.UX.MacOS;

internal class FormHandler : Eto.Mac.Forms.FormHandler
{
    protected override void Initialize()
    {
        base.Initialize();
        Widget.WindowStateChanged += UpdateActivationPolicy;
        Widget.LostFocus += UpdateActivationPolicy;
        Widget.GotFocus += UpdateActivationPolicy;
    }

    public override void Show()
    {
        NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
        base.Show();
    }

    // Form.Show() only calls the handler's Show() the first time the form is shown.
    // Any later call (e.g. the tray icon's "Show OpenTabletDriver" menu item) just sets
    // Visible = true, so the app has to be activated from here to be brought to the front.
    public override bool Visible
    {
        get => base.Visible;
        set
        {
            if (value)
                NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);

            base.Visible = value;
        }
    }

    private void UpdateActivationPolicy(object sender, EventArgs e)
    {
        var hasNonMinimizedVisibleWindow =
            Application.Instance.Windows.Any(window => window.Visible && window.WindowState != WindowState.Minimized);
        NSApplication.SharedApplication.ActivationPolicy = hasNonMinimizedVisibleWindow
            ? NSApplicationActivationPolicy.Regular : NSApplicationActivationPolicy.Accessory;
    }
}
