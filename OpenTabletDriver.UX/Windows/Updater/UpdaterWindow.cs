using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Plugin;
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
                        new PaddingSpacerItem(),
                        new Bitmap(App.Logo.WithSize(256, 256)),
                        "An update is available to install",
                        release.TagName,
                        new Button(Update)
                        {
                            Text = "Install"
                        },
                        new PaddingSpacerItem(),
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
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string path = SystemInterop.CurrentPlatform switch
            {
                PluginPlatform.Windows => Path.Join(basePath, "OpenTabletDriver.UX.Wpf.exe"),
                PluginPlatform.MacOS => Path.Join(basePath, "OpenTabletDriver.UX.MacOS"),
                _ => throw new NotSupportedException("Current platform does not support updating.")
            };

            // Disallow multiple invocations
            (sender as Control)!.Enabled = false;

            await App.Driver.Instance.InstallUpdate();

            Process.Start(path);

            if (Application.Instance.QuitIsSupported)
                Application.Instance.Quit();
            else
                Environment.Exit(0);
        });
    }
}
