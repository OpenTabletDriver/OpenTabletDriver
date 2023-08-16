using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
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

        private int _isUpdateRequested;
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
                        new Button(Update)
                        {
                            Text = "Install"
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

        private void Update(object sender, EventArgs e) => Application.Instance.AsyncInvoke(async () =>
        {
            if (Interlocked.Exchange(ref _isUpdateRequested, 1) != 0)
                return;

            // Disallow multiple invocations
            (sender as Control)!.Enabled = false;

            try
            {
                await App.Driver.Instance.InstallUpdate();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, MessageBoxType.Error);
                Close();
                return;
            }

            var mainForm = (MainForm)Application.Instance.MainForm;
            mainForm.SilenceDaemonShutdown = true;

            Close();
            mainForm.Close();

            if (App.DaemonWatchdog is not null)
                App.DaemonWatchdog.Dispose();

            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string path = Directory.EnumerateFiles(basePath, "OpenTabletDriver.UI*").FirstOrDefault(); // 0.7.x mirgration
            path ??= SystemInterop.CurrentPlatform switch
            {
                PluginPlatform.Windows => Path.Join(basePath, "OpenTabletDriver.UX.Wpf.exe"),
                PluginPlatform.MacOS => Path.Join(basePath, "OpenTabletDriver.UX.MacOS"),
                _ => throw new NotSupportedException("Unsupported platform")
            };

            Process.Start(path);

            if (Application.Instance.QuitIsSupported)
                Application.Instance.Quit();
            else
                Environment.Exit(0);
        });
    }
}
