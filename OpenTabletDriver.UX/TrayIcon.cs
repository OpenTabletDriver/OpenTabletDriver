using System;
using System.Collections.Generic;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.UX
{
    public class TrayIcon : IDisposable
    {
        public TrayIcon(MainForm window)
        {
            this.window = window;

            Indicator = new TrayIndicator
            {
                Title = "OpenTabletDriver",
                Image = App.Logo
            };

            RefreshMenuItems();

            Indicator.Activated += (object sender, System.EventArgs e) =>
            {
                window.Show();
                window.BringToFront();
            };
        }

        public TrayIndicator Indicator { get; }
        private MainForm window;

        public void Dispose()
        {
            Indicator.Hide();
            Indicator.Dispose();
        }

        public void RefreshMenuItems()
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

            var items = new List<MenuItem>();
            var presets = AppInfo.PresetManager.GetPresets();

            if (presets.Count != 0)
            {
                foreach (var preset in presets)
                {
                    var presetItem = new ButtonMenuItem
                    {
                        Text = preset.Name
                    };
                    presetItem.Click += MainForm.PresetButtonHandler;

                    items.Add(presetItem);
                }

                items.Add(new SeparatorMenuItem());
            }

            items.Add(showWindow);
            items.Add(close);

            Indicator.Menu = new ContextMenu(items);
        }
    }
}
