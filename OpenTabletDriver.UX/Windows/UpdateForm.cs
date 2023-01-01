using System.Diagnostics;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Desktop.Interop.AppInfo;
using OpenTabletDriver.Desktop.Updater;
using OpenTabletDriver.UX.Components;

namespace OpenTabletDriver.UX.Windows
{
    public sealed class UpdateForm : DesktopForm
    {
        private readonly IDriverDaemon _daemon;
        private readonly App _app;

        public UpdateForm(IDriverDaemon daemon, App app, SerializedUpdateInfo update)
        {
            _daemon = daemon;
            _app = app;

            Title = "OpenTabletDriver Updater";

            Width = 400;
            Height = 380;
            Resizable = false;

            Content = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Spacing = 5,
                Padding = 5,
                Items =
                {
                    new StackLayoutItem(null, true),
                    new Bitmap(Metadata.Logo.WithSize(256, 256)),
                    "An update is available to install",
                    $"v{update.Version}",
                    new Panel
                    {
                        Padding = 5,
                        Content = new Button((_, _) => InstallUpdate().Run())
                        {
                            Text = "Install Update"
                        }
                    },
                    new StackLayoutItem(null, true)
                }
            };
        }

        private async Task InstallUpdate()
        {
            Enabled = false;
            var path = Process.GetCurrentProcess().MainModule!.FileName;
            await _daemon.InstallUpdate();
            Process.Start(path, _app.Arguments);
            _app.Exit();
        }
    }
}
