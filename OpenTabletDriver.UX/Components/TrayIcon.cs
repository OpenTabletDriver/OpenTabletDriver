using Eto.Forms;
using OpenTabletDriver.UX.Controls;

namespace OpenTabletDriver.UX.Components
{
    public class TrayIcon : TrayIndicator
    {
        public TrayIcon(MainForm mainForm, App app, IControlBuilder controlBuilder)
        {
            Title = "OpenTabletDriver";
            Image = Metadata.Logo;

            var showItem = new AppCommand("Show...", mainForm.BringToFront).CreateMenuItem();
            showItem.Visible = mainForm.WindowState == WindowState.Minimized;

            Activated += (_, _) => mainForm.BringToFront();

            Menu = new ContextMenu
            {
                Items =
                {
                    showItem,
                    controlBuilder.Build<PresetsMenuItem>(),
                    new AppCommand("Quit", app.Exit)
                }
            };

            // Change window state change behavior
            mainForm.WindowStateChanged += delegate
            {
                showItem.Visible = mainForm.WindowState == WindowState.Minimized;
                mainForm.ShowInTaskbar = !showItem.Visible;
            };
        }
    }
}
