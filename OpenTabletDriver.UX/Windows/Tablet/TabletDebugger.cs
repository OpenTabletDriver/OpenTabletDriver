using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Contracts;
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
                Height = 400,
                Padding = 5,
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem
                    {
                        Control = deviceNameBox = new TextGroup("Device")
                    },
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
                    new StackLayoutItem(reportRateBox = new TextGroup("Report Rate")),
                    new StackLayoutItem
                    {
                        Control = new StackLayout
                        {
                            Orientation = Orientation.Horizontal,
                            VerticalContentAlignment = VerticalAlignment.Bottom,
                            Items =
                            {
                                new StackLayoutItem(numReportsRecordedBox = new TextGroup("Number of Reports Recorded"), true),
                                new Group
                                {
                                    Text = "Toggles",
                                    Content = enableDataRecording = new CheckBox
                                    {
                                        Text = "Enable Data Recording"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            this.Content = new Splitter
            {
                Orientation = Orientation.Vertical,
                Width = 640,
                Height = 800,
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

            App.Driver.AddConnectionHook(ConnectionHook);
            await App.Driver.Instance.SetTabletDebug(true);

            var tablets = await App.Driver.Instance.GetTablets();
            HandleTabletsChanged(this, tablets);

            var outputStream = File.OpenWrite(Path.Join(AppInfo.Current.AppDataDirectory, "tablet-data.txt"));
            dataRecordingOutput = new StreamWriter(outputStream);
        }

        protected override async void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            await App.Driver.Instance.SetTabletDebug(false);
            App.Driver.RemoveConnectionHook(ConnectionHook);
            dataRecordingOutput?.Close();
            dataRecordingOutput = null;
        }

        private TextGroup deviceNameBox, rawTabletBox, tabletBox, reportRateBox, numReportsRecordedBox;
        private TabletVisualizer tabletVisualizer;
        private CheckBox enableDataRecording;

        private double reportPeriod;
        private int numReportsRecorded;
        private HPETDeltaStopwatch stopwatch = new HPETDeltaStopwatch(true);
        private TextWriter dataRecordingOutput;

        private void ConnectionHook(IDriverDaemon daemon)
        {
            daemon.DeviceReport += HandleReport;
            daemon.TabletsChanged += HandleTabletsChanged;
            Plugin.Log.Debug(nameof(ConnectionHook), "Activated connection hook.");
        }

        private void HandleReport(object sender, DebugReportData data)
        {
            tabletVisualizer.SetData(data);
            var report = data.ToObject();

            if (report is IDeviceReport deviceReport)
            {
                deviceNameBox.Update(data.Tablet.Properties.Name);

                reportPeriod += (stopwatch.Restart().TotalMilliseconds - reportPeriod) / 10.0f;
                reportRateBox.Update($"{(uint)(1000 / reportPeriod)}hz");

                string formatted = ReportFormatter.GetStringFormat(deviceReport);
                tabletBox.Update(formatted);

                string raw = ReportFormatter.GetStringRaw(deviceReport);
                rawTabletBox.Update(raw);
                
                if (enableDataRecording.Checked ?? false)
                {
                    var output = string.Join(' ', deviceReport.Raw.Select(d => d.ToString("X2")));
                    dataRecordingOutput.WriteLine(output);
                    numReportsRecorded++;
                    numReportsRecordedBox.Update(numReportsRecorded.ToString());
                }
            }
        }

        private void HandleTabletsChanged(object sender, IEnumerable<TabletReference> tablets) => Application.Instance.AsyncInvoke(() =>
        {
            StringBuilder sb = new StringBuilder("Tablet Debugger");
            if (tablets != null && tablets.Any())
            {
                var numTablets = Math.Min(tablets.Count(), 3);
                sb.Append(" - ");
                sb.Append(string.Join(", ", tablets.Take(numTablets).Select(t => t.Properties.Name)));
            }
            this.Title = sb.ToString();
        });

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

            public void Update(string text) => Application.Instance.AsyncInvoke(() => label.Text = text);

            protected override Color VerticalBackgroundColor => base.HorizontalBackgroundColor;
        }

        private class TabletVisualizer : TimedDrawable
        {
            private static readonly Color AccentColor = SystemColors.Highlight;

            private DebugReportData data;
            private TabletReference tablet;

            public void SetData(DebugReportData data)
            {
                this.data = data;
                this.tablet = data.Tablet;
            }

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
                var report = data?.ToObject();

                var digitizer = tablet.Properties.Specifications.Digitizer;
                var pen = tablet.Properties.Specifications.Pen;
                if (report is IAbsolutePositionReport absReport)
                {
                    var tabletMm = new SizeF(digitizer.Width, digitizer.Height);
                    var tabletPx = new SizeF(digitizer.MaxX, digitizer.MaxY);
                    var tabletScale = tabletMm / tabletPx * scale;
                    var position = new PointF(absReport.Position.X, absReport.Position.Y) * tabletScale;

                    var drawRect = RectangleF.FromCenter(position, new SizeF(5, 5));
                    graphics.FillEllipse(AccentColor, drawRect);
                }
            }
        }
    }
}
