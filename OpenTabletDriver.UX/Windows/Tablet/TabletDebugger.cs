using System;
using System.ComponentModel;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Timing;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Tools;

namespace OpenTabletDriver.UX.Windows.Tablet
{
    public class TabletDebugger : DesktopForm
    {
        public TabletDebugger()
        {
            Title = "Tablet Debugger";

            var debugger = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Height = 270,
                Padding = 5,
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = new TableLayout
                        {
                            Spacing = new Size(5, 5),
                            Rows =
                            {
                                new TableRow
                                {
                                    ScaleHeight = true,
                                    Cells =
                                    {
                                        new TableCell(rawTabletBox = new TextGroup("Raw Tablet Data"), true),
                                        new TableCell(tabletBox = new TextGroup("Tablet Report"), true)
                                    }
                                }
                            }
                        }
                    },
                    new StackLayoutItem(reportRateBox = new TextGroup("Report Rate"))
                }
            };

            this.Content = new Splitter
            {
                Orientation = Orientation.Vertical,
                Width = 640,
                Height = 640,
                FixedPanel = SplitterFixedPanel.Panel2,
                Panel1 = new DebuggerGroup
                {
                    Text = "Visualizer",
                    Content = tabletVisualizer = new TabletVisualizer()
                },
                Panel2 = debugger
            };
        }

        protected override async void OnLoadComplete(EventArgs e)
        {
            base.OnLoadComplete(e);

            App.Driver.Instance.DeviceReport += HandleReport;
            await App.Driver.Instance.SetTabletDebug(true);

            var tablet = await App.Driver.Instance.GetTablet();
            HandleTabletChanged(this, tablet);
            App.Driver.Instance.TabletChanged += HandleTabletChanged;
        }

        protected override async void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            App.Driver.Instance.DeviceReport -= HandleReport;
            await App.Driver.Instance.SetTabletDebug(false);
            App.Driver.Instance.TabletChanged -= HandleTabletChanged;
        }

        private TextGroup rawTabletBox, tabletBox, reportRateBox;
        private TabletVisualizer tabletVisualizer;
        private double reportPeriod;
        private HPETDeltaStopwatch stopwatch = new HPETDeltaStopwatch(true);

        private void HandleReport(object sender, RpcData rpcData)
        {
            var report = rpcData.GetData(AppInfo.PluginManager);

            tabletVisualizer.SetData(rpcData);

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

        private void HandleTabletChanged(object sender, TabletState tablet)
        {
            tabletVisualizer.SetTablet(tablet);
            this.Title = $"Tablet Debugger" + (tablet != null ? $" - {tablet.Properties.Name}" : string.Empty);
        }

        private class DebuggerGroup : Group
        {
            protected override Color VerticalBackgroundColor => base.HorizontalBackgroundColor;
        }

        private class TextGroup : DebuggerGroup
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

        private class TabletVisualizer : TimedDrawable
        {
            private static readonly Color AccentColor = SystemColors.Highlight;

            private RpcData data;
            private TabletState tablet;

            public void SetData(RpcData data) => this.data = data;
            public void SetTablet(TabletState tablet) => this.tablet = tablet;

            protected override void OnNextFrame(PaintEventArgs e)
            {
                if (tablet != null)
                {
                    var graphics = e.Graphics;
                    using (graphics.SaveTransformState())
                    {
                        var pxToMM = (float)graphics.DPI / 25.4f;

                        var digitizer = tablet.Properties.Specifications.Digitizer;
                        var clientCenter = new PointF(this.ClientSize.Width, this.ClientSize.Height) / 2;
                        var tabletCenter = new PointF(digitizer.Width, digitizer.Height) / 2 * pxToMM;

                        graphics.TranslateTransform(clientCenter - tabletCenter);

                        DrawBackground(graphics, pxToMM);
                        DrawPosition(graphics, pxToMM);
                    }
                }
            }

            protected void DrawBackground(Graphics graphics, float scale)
            {
                var digitizer = tablet.Properties.Specifications.Digitizer;
                var bg = new RectangleF(0, 0, digitizer.Width, digitizer.Height) * scale;
                graphics.FillRectangle(SystemColors.WindowBackground, bg);
                graphics.DrawRectangle(AccentColor, bg);
            }

            protected void DrawPosition(Graphics graphics, float scale)
            {
                var report = data?.GetData(AppInfo.PluginManager);

                var digitizer = tablet.Properties.Specifications.Digitizer;
                var pen = tablet.Properties.Specifications.Pen;
                if (report is ITabletReport tabletReport && pen.ActiveReportID.IsInRange(tabletReport.ReportID))
                {
                    var tabletMm = new SizeF(digitizer.Width, digitizer.Height);
                    var tabletPx = new SizeF(digitizer.MaxX, digitizer.MaxY);
                    var tabletScale = tabletMm / tabletPx * scale;
                    var position = new PointF(tabletReport.Position.X, tabletReport.Position.Y) * tabletScale;

                    var drawRect = RectangleF.FromCenter(position, new SizeF(5, 5));
                    graphics.FillEllipse(AccentColor, drawRect);
                }
            }
        }
    }
}
