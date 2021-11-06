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
                    new DebuggerGroup
                    {
                        Text = "Device",
                        Content = deviceName = new Label
                        {
                            Font = Fonts.Monospace(10)
                        }
                    },
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = new StackLayout
                        {
                            Orientation = Orientation.Horizontal,
                            VerticalContentAlignment = VerticalAlignment.Stretch,
                            Items =
                            {
                                new StackLayoutItem
                                {
                                    Expand = true,
                                    Control = new DebuggerGroup
                                    {
                                        Text = "Raw Tablet Data",
                                        Content = rawTablet = new Label
                                        {
                                            Font = Fonts.Monospace(10)
                                        }
                                    }
                                },
                                new StackLayoutItem
                                {
                                    Expand = true,
                                    Control = new DebuggerGroup
                                    {
                                        Text = "Tablet Report",
                                        Content = tablet = new Label
                                        {
                                            Font = Fonts.Monospace(10)
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new DebuggerGroup
                    {
                        Text = "Report Rate",
                        Content = reportRate = new Label
                        {
                            Font = Fonts.Monospace(10)
                        }
                    },
                    new StackLayoutItem
                    {
                        Control = new StackLayout
                        {
                            Orientation = Orientation.Horizontal,
                            VerticalContentAlignment = VerticalAlignment.Bottom,
                            Items =
                            {
                                new StackLayoutItem
                                {
                                    Expand = true,
                                    Control = new DebuggerGroup
                                    {
                                        Text = "Reports Recorded",
                                        Content = reportsRecorded = new Label
                                        {
                                            Font = Fonts.Monospace(10)
                                        }
                                    }
                                },
                                new Group
                                {
                                    Text = "Options",
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
                Height = 840,
                FixedPanel = SplitterFixedPanel.Panel2,
                Panel1 = new DebuggerGroup
                {
                    Text = "Visualizer",
                    Content = tabletVisualizer = new TabletVisualizer()
                },
                Panel2 = debugger
            };

            var reportBinding = ReportDataBinding.Child(c => (c.ToObject() as IDeviceReport));

            deviceName.TextBinding.Bind(ReportDataBinding.Child(c => c.Tablet.Properties.Name));
            rawTablet.TextBinding.Bind(reportBinding.Child(c => ReportFormatter.GetStringRaw(c)));
            tablet.TextBinding.Bind(reportBinding.Child(c => ReportFormatter.GetStringFormat(c)));
            reportRate.TextBinding.Bind(ReportPeriodBinding.Convert(c => Math.Round(1000.0 / c) + "hz"));
            reportsRecorded.TextBinding.Bind(NumberOfReportsRecordedBinding.Convert(c => c.ToString()));
            tabletVisualizer.ReportDataBinding.Bind(ReportDataBinding);

            Application.Instance.AsyncInvoke(() =>
            {
                App.Driver.AddConnectionHook(ConnectionHook);
            });

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

        private Label deviceName, rawTablet, tablet, reportRate, reportsRecorded;
        private TabletVisualizer tabletVisualizer;
        private CheckBox enableDataRecording;

        private DebugReportData reportData;
        private double reportPeriod;
        private int numReportsRecorded;

        private HPETDeltaStopwatch stopwatch = new HPETDeltaStopwatch(true);
        private TextWriter dataRecordingOutput;

        public DebugReportData ReportData
        {
            set
            {
                this.reportData = value;
                this.OnReportDataChanged();
            }
            get => this.reportData;
        }

        public double ReportPeriod
        {
            set
            {
                this.reportPeriod = value;
                this.OnReportPeriodChanged();
            }
            get => this.reportPeriod;
        }

        public int NumberOfReportsRecorded
        {
            set
            {
                this.numReportsRecorded = value;
                this.OnNumberOfReportsRecordedChanged();
            }
            get => this.numReportsRecorded;
        }

        public event EventHandler<EventArgs> ReportDataChanged;
        public event EventHandler<EventArgs> ReportPeriodChanged;
        public event EventHandler<EventArgs> NumberOfReportsRecordedChanged;

        protected virtual void OnReportDataChanged()
        {
            ReportDataChanged?.Invoke(this, new EventArgs());
            ReportPeriod += (stopwatch.Restart().TotalMilliseconds - ReportPeriod) / 10.0f;
        }

        protected virtual void OnReportPeriodChanged() => ReportPeriodChanged?.Invoke(this, new EventArgs());
        protected virtual void OnNumberOfReportsRecordedChanged() => NumberOfReportsRecordedChanged?.Invoke(this, new EventArgs());

        public BindableBinding<TabletDebugger, DebugReportData> ReportDataBinding
        {
            get
            {
                return new BindableBinding<TabletDebugger, DebugReportData>(
                    this,
                    c => c.ReportData,
                    (c, v) => c.ReportData = v,
                    (c, h) => c.ReportDataChanged += h,
                    (c, h) => c.ReportDataChanged -= h
                );
            }
        }

        public BindableBinding<TabletDebugger, double> ReportPeriodBinding
        {
            get
            {
                return new BindableBinding<TabletDebugger, double>(
                    this,
                    c => c.ReportPeriod,
                    (c, v) => c.ReportPeriod = v,
                    (c, h) => c.ReportPeriodChanged += h,
                    (c, h) => c.ReportPeriodChanged -= h
                );
            }
        }

        public BindableBinding<TabletDebugger, int> NumberOfReportsRecordedBinding
        {
            get
            {
                return new BindableBinding<TabletDebugger, int>(
                    this,
                    c => c.NumberOfReportsRecorded,
                    (c, v) => c.NumberOfReportsRecorded = v,
                    (c, h) => c.NumberOfReportsRecordedChanged += h,
                    (c, h) => c.NumberOfReportsRecordedChanged -= h
                );
            }
        }

        private void ConnectionHook(IDriverDaemon daemon)
        {
            daemon.DeviceReport += HandleReport;
            daemon.TabletsChanged += HandleTabletsChanged;
            App.Driver.Instance.SetTabletDebug(true);
        }

        private void HandleReport(object sender, DebugReportData data) => Application.Instance.AsyncInvoke(() =>
        {
            this.ReportData = data;

            if (data.ToObject() is IDeviceReport deviceReport)
            {
                if (enableDataRecording.Checked ?? false)
                {
                    var output = string.Join(' ', deviceReport.Raw.Select(d => d.ToString("X2")));
                    dataRecordingOutput?.WriteLine(output);
                    NumberOfReportsRecorded++;
                }
            }
        });

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

        private class TabletVisualizer : ScheduledDrawable
        {
            private static readonly Color AccentColor = SystemColors.Highlight;

            public DebugReportData ReportData { set; get; }

            public BindableBinding<TabletVisualizer, DebugReportData> ReportDataBinding
            {
                get
                {
                    return new BindableBinding<TabletVisualizer, DebugReportData>(
                        this,
                        c => c.ReportData,
                        (c, v) => c.ReportData = v
                    );
                }
            }

            protected override void OnNextFrame(PaintEventArgs e)
            {
                if (ReportData?.Tablet is TabletReference tablet)
                {
                    var graphics = e.Graphics;
                    using (graphics.SaveTransformState())
                    {
                        var pxToMM = (float)graphics.DPI / 25.4f;

                        var digitizer = tablet.Properties.Specifications.Digitizer;
                        var clientCenter = new PointF(this.ClientSize.Width, this.ClientSize.Height) / 2;
                        var tabletCenter = new PointF(digitizer.Width, digitizer.Height) / 2 * pxToMM;

                        graphics.TranslateTransform(clientCenter - tabletCenter);

                        DrawBackground(graphics, pxToMM, tablet);
                        DrawPosition(graphics, pxToMM, tablet);
                    }
                }
            }

            protected void DrawBackground(Graphics graphics, float scale, TabletReference tablet)
            {
                var digitizer = ReportData.Tablet.Properties.Specifications.Digitizer;
                var bg = new RectangleF(0, 0, digitizer.Width, digitizer.Height) * scale;

                graphics.FillRectangle(SystemColors.WindowBackground, bg);
                graphics.DrawRectangle(AccentColor, bg);
            }

            protected void DrawPosition(Graphics graphics, float scale, TabletReference tablet)
            {
                var report = ReportData?.ToObject();
                var specifications = ReportData.Tablet.Properties.Specifications;
                var digitizer = specifications.Digitizer;
                var pen = specifications.Pen;

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
