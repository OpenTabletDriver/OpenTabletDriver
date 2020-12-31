using System;
using Eto.Forms;

namespace OpenTabletDriver.UX
{
    public class TrayIcon : IDisposable
    {
        public TrayIcon(MainForm window)
        {
            var showWindow = new ButtonMenuItem
            {
                Text = "Show Window"
            };
            showWindow.Click += (sender, e) =>
            {
                window.Show();
                window.WindowState = WindowState.Normal;
                window.BringToFront();
                window.WindowStyle = WindowStyle.Default;
            };

            var close = new ButtonMenuItem
            {
                Text = "Close"
            };
            close.Click += (sender, e) => window.Close();

            indicator = new TrayIndicator
            {
                Title = "OpenTabletDriver",
                Image = App.Logo,
                Menu = new ContextMenu
                {
                    Items =
                    {
                        showWindow,
                        close
                    }
                }
            };
			indicator.Activated += (object sender, System.EventArgs e) =>
            {
                window.Show();
                window.WindowState = WindowState.Normal;
                window.BringToFront();
                window.WindowStyle = WindowStyle.Default;
            };
            indicator.Show();
        }

        private TrayIndicator indicator;

        public void Dispose()
        {
            indicator.Hide();
            indicator.Dispose();
        }
    }
}