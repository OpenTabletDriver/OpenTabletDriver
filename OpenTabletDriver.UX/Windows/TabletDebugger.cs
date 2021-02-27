using System;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Timing;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Tools;

namespace OpenTabletDriver.UX.Windows
{
    public class TabletDebugger : DesktopForm
    {
        public TabletDebugger()
        {
            Title = "Tablet Debugger";

            var mainLayout = new TableLayout
            {
                Width = 640,
                Height = 480,
                Spacing = new Size(5, 5),
                Rows =
                {
                    new TableRow
                    {
                        Cells =
                        {
                            new TableCell(rawTabletBox = new TextGroup("Raw Tablet Data"), true),
                            new TableCell(tabletBox = new TextGroup("Tablet Report"), true)
                        },
                        ScaleHeight = true
                    },
                    new TableRow
                    {
                        Cells =
                        {
                            new TableCell(rawAuxBox = new TextGroup("Raw Aux Data"), true),
                            new TableCell(auxBox = new TextGroup("Aux Report"), true)
                        },
                        ScaleHeight = true
                    }
                }
            };

            this.Content = new StackLayout
            {
                Padding = 5,
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem(mainLayout, HorizontalAlignment.Stretch, true),
                    new StackLayoutItem(reportRateBox = new TextGroup("Report Rate"), HorizontalAlignment.Stretch)
                }
            };

            InitializeAsync();
        }

        private void InitializeAsync()
        {
            App.Driver.Instance.DeviceReport += HandleReport;
            App.Driver.Instance.SetTabletDebug(true);
            this.Closing += (sender, e) =>
            {
                App.Driver.Instance.DeviceReport -= HandleReport;
                App.Driver.Instance.SetTabletDebug(false);
            };
        }

        private TextGroup rawTabletBox, tabletBox, rawAuxBox, auxBox, reportRateBox;
        private double reportPeriod;
        private HPETDeltaStopwatch stopwatch = new HPETDeltaStopwatch(true);

        private void HandleReport(object sender, RpcData rpcData)
        {
            var report = rpcData.GetData(AppInfo.PluginManager);

            if (report is IDeviceReport deviceReport)
            {
                reportPeriod += (stopwatch.Restart().TotalMilliseconds - reportPeriod) / 10.0f;
                reportRateBox.Update($"{(uint)(1000 / reportPeriod)}hz");

                string formatted = ReportFormatter.GetStringFormat(deviceReport);
                tabletBox.Update(formatted);

                string raw = ReportFormatter.GetStringRaw(deviceReport);
                rawTabletBox.Update(raw);
            }
        }

        private class TextGroup : Group
        {
            public TextGroup(string title)
            {
                base.Text = title;
                base.Content = label;
            }

            private Label label = new Label
            {
                Font = Fonts.Monospace(10)
            };

            public void Update(string text)
            {
                Application.Instance.AsyncInvoke(() => label.Text = text);
            }

            protected override Color VerticalBackgroundColor => base.HorizontalBackgroundColor;
        }
    }
}
