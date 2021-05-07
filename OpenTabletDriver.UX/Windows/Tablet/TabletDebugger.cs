using System;
using System.ComponentModel;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Timing;
using OpenTabletDriver.UX.Controls;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Tools;

namespace OpenTabletDriver.UX.Windows.Tablet
{
    public class TabletDebugger : DesktopForm
    {
        public TabletDebugger()
        {
            Title = "Tablet Debugger";

            var reportDebugger = new StackLayout
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

            var debuggingView = new Splitter
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
                Panel2 = reportDebugger
            };

            tabletDropDown.Initialized += async (_, _) =>
            {
                tabletFilter = tabletDropDown.SelectedID;
                App.Driver.Instance.DebugReport += HandleReport;
                await App.Driver.Instance.SetTabletDebug(tabletFilter, true);
                HandleTabletChanged(await App.Driver.Instance.GetTablet(tabletFilter));
            };

            tabletDropDown.SelectedIDChanged += async (_, _) =>
            {
                await App.Driver.Instance.SetTabletDebug(tabletFilter, false);

                tabletFilter = tabletDropDown.SelectedID;
                await App.Driver.Instance.SetTabletDebug(tabletFilter, true);
                HandleTabletChanged(await App.Driver.Instance.GetTablet(tabletFilter));
            };

            this.Content = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Items =
                {
                    tabletDropDown,
                    debuggingView
                }
            };
        }

        private TextGroup rawTabletBox, tabletBox, reportRateBox;
        private TabletDropDown tabletDropDown = new TabletDropDown { Width = 300 };
        private TabletHandlerID tabletFilter;
        private TabletVisualizer tabletVisualizer;
        private double reportPeriod;
        private HPETDeltaStopwatch stopwatch = new HPETDeltaStopwatch(true);

        protected override async void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            App.Driver.Instance.DebugReport -= HandleReport;
            await App.Driver.Instance.SetTabletDebug(tabletFilter, false);
        }

        private void HandleReport(object sender, (TabletHandlerID, RpcData) taggedRpcData)
        {
            (var id, var rpcData) = taggedRpcData;
            var report = rpcData.GetData(AppInfo.PluginManager);

            if (id == tabletFilter)
            {
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
        }

        private void HandleTabletChanged(TabletState tablet)
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
            private DigitizerSpecifications digitizer;
            private PenSpecifications pen;

            public void SetData(RpcData data) => this.data = data;
            public void SetTablet(TabletState tablet)
            {
                this.tablet = tablet;
                this.digitizer = tablet?.Properties.Specifications.Digitizer;
                this.pen = tablet?.Properties.Specifications.Pen;
            }

            protected override void OnNextFrame(PaintEventArgs e)
            {
                if (tablet != null)
                {
                    var graphics = e.Graphics;
                    using (graphics.SaveTransformState())
                    {
                        var pxToMM = (float)graphics.DPI / 25.4f;

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
                var bg = new RectangleF(0, 0, digitizer.Width, digitizer.Height) * scale;
                graphics.FillRectangle(SystemColors.WindowBackground, bg);
                graphics.DrawRectangle(AccentColor, bg);
            }

            protected void DrawPosition(Graphics graphics, float scale)
            {
                var report = data?.GetData(AppInfo.PluginManager);

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