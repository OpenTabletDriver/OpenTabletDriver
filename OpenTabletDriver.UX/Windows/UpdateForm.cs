using System.Diagnostics;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Desktop.Interop.AppInfo;
using OpenTabletDriver.Desktop.Updater;
using OpenTabletDriver.Interop;
using OpenTabletDriver.UX.Components;

namespace OpenTabletDriver.UX.Windows
{
    public sealed class UpdateForm : DesktopForm
    {
        private readonly IDriverDaemon _daemon;
        private readonly App _app;
        private int _isUpdateRequested;

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
            if (Interlocked.Exchange(ref _isUpdateRequested, 1) != 0)
                return;

            Enabled = false;

            await _daemon.InstallUpdate();

            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var path = Directory.EnumerateFiles(basePath, "OpenTabletDriver.UI*").FirstOrDefault(); // 0.7.x avalonia mirgration
            path ??= SystemInterop.CurrentPlatform switch
            {
                SystemPlatform.Windows => Path.Join(basePath, "OpenTabletDriver.UX.Wpf.exe"),
                SystemPlatform.MacOS => Path.Join(basePath, "OpenTabletDriver.UX.MacOS"),
                _ => throw new NotSupportedException("Unsupported platform")
            };

            _app.StopDaemon();
            Application.Instance.MainForm.Close();

            Process.Start(path);

            if (Application.Instance.QuitIsSupported)
                Application.Instance.Quit();
            else
                Environment.Exit(0);
        }
    }
}
