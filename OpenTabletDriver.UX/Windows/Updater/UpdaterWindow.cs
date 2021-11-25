using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.UX.Controls;

namespace OpenTabletDriver.UX.Windows.Updater
{
    public class UpdaterWindow : Form
    {
        public UpdaterWindow()
        {
            this.Title = "OpenTabletDriver Updater";
            this.ClientSize = new Size(930, 730);

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

        private async Task InitializeAsync()
        {
            var updateAvailable = await App.Driver.Instance.HasUpdate();
            if (updateAvailable)
            {
                var release = await App.Driver.Instance.GetUpdateInfo();
                this.Content = new StackLayout()
                {
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    Items =
                    {
                        "An update is available to install",
                        release.TagName,
                        new Button(Update)
                        {
                            Text = "Install"
                        }
                    }
                };
            }
            else
            {
                this.Content = new Placeholder
                {
                    Text = "No updates are available."
                };
            }
        }

        private void Update(object sender, EventArgs e) => Application.Instance.AsyncInvoke(async () =>
        {
            await App.Driver.Instance.InstallUpdate();
            Environment.Exit(0);
        });
    }
}