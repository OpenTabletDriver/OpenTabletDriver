using System;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.UX.Controls;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Windows.Updater
{
    public class UpdaterWindow : Form
    {
        public UpdaterWindow()
        {
            this.Title = "OpenTabletDriver Updater";
            this.ClientSize = new Size(400, 380);

            this.Content = new Placeholder
            {
                Text = "Checking for updates...",
                ExtraContent = new Panel
                {
                    Padding = 10,
                    Content = new ProgressBar
                    {
                        Indeterminate = true
                    }
                }
            };

            _ = InitializeAsync();
        }

        public const string LATEST_RELEASE_URL = "https://github.com/OpenTabletDriver/OpenTabletDriver/releases/latest";
        private TaskCompletionSource<bool> _updateAvailable = new();
        public Task<bool> HasUpdates() => _updateAvailable.Task;

        private async Task InitializeAsync()
        {
            var updateAvailable = await App.Driver.Instance.CheckForUpdates();
            if (updateAvailable is not null)
            {
                this.Content = new StackLayout()
                {
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    Items =
                    {
                        new PaddingSpacerItem(),
                        new Bitmap(App.Logo.WithSize(256, 256)),
                        "An update is available to install",
                        $"OpenTabletDriver v{updateAvailable.Version}",
                        new PaddingSpacerItem(),
                        new StackLayout()
                        {
                            Orientation = Orientation.Horizontal,
                            Items =
                            {
                                new Button(OpenRelease)
                                {
                                    Text = "Go to Release"
                                },
                                new Button(OpenDirectory)
                                {
                                    Text = "Open Directory"
                                }
                            },
                            Spacing = 5
                        },
                        new PaddingSpacerItem(),
                    }
                };
                _updateAvailable.SetResult(true);
            }
            else
            {
                this.Content = new Placeholder
                {
                    Text = "No updates are available."
                };
                _updateAvailable.SetResult(false);
            }
        }

        private void OpenRelease(object sender, EventArgs e)
            => DesktopInterop.Open(LATEST_RELEASE_URL);

        private void OpenDirectory(object sender, EventArgs e)
            => DesktopInterop.Open(AppContext.BaseDirectory);
    }
}
