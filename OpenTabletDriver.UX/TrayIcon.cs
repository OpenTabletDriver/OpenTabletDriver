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
                window.BringToFront();
            };

            var close = new ButtonMenuItem
            {
                Text = "Close"
            };
            close.Click += (sender, e) => window.Close();

            Indicator = new TrayIndicator
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
			Indicator.Activated += (object sender, System.EventArgs e) =>
            {
                window.Show();
                window.BringToFront();
            };
            Indicator.Show();
        }

        public TrayIndicator Indicator { get; }

        public void Dispose()
        {
            Indicator.Hide();
            Indicator.Dispose();
        }
    }
}