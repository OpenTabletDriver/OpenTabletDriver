using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Touch;
using OpenTabletDriver.Plugin.Timing;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Tools;

namespace OpenTabletDriver.UX.Windows.Tablet
{
    public class TabletDebugger : DesktopForm
    {
        const int LARGE_FONTSIZE = 14;
        const int FONTSIZE = LARGE_FONTSIZE - 4;
        const int SPACING = 5;

        public TabletDebugger()
            : base(Application.Instance.MainForm)
        {
            Title = "Tablet Debugger";

            var debugger = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Height = 320,
                Padding = SPACING,
                Spacing = SPACING,
                Items =
                {
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
                                        Text = "Device",
                                        Content = deviceName = new Label
                                        {
                                            Font = Fonts.Monospace(LARGE_FONTSIZE)
                                        }
                                    }
                                },
                                new DebuggerGroup
                                {
                                    Text = "Report Rate",
                                    Width = LARGE_FONTSIZE * 6,
                                    Content = reportRate = new Label
                                    {
                                        Font = Fonts.Monospace(LARGE_FONTSIZE)
                                    }
                                },
                                new StackLayoutItem
                                {
                                    Control = reportsRecordedGroup = new DebuggerGroup
                                    {
                                        Text = "Reports Recorded",
                                        Width = LARGE_FONTSIZE * 10,
                                        Content = reportsRecorded = new Label
                                        {
                                            Font = Fonts.Monospace(LARGE_FONTSIZE)
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
                    },
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = new StackLayout
                        {
                            Orientation = Orientation.Horizontal,
                            VerticalContentAlignment = VerticalAlignment.Stretch,
                            Height = 240,
                            Items =
                            {
                                new StackLayoutItem
                                {
                                    Expand = true,
                                    Control = new DebuggerGroup
                                    {
                                        Text = "Raw Tablet Data",
                                        Width = FONTSIZE * 33,
                                        Content = rawTablet = new Label
                                        {
                                            Font = Fonts.Monospace(FONTSIZE)
                                        }
                                    }
                                },
                                new StackLayoutItem
                                {
                                    Control = new StackLayout
                                    {
                                        Orientation = Orientation.Vertical,
                                        VerticalContentAlignment = VerticalAlignment.Top,
                                        Items =
                                        {
                                            new StackLayoutItem
                                            {
                                                Control = new DebuggerGroup
                                                {
                                                    Text = "Maximum Position",
                                                    Width = FONTSIZE * 33,
                                                    Content = maxReportedPosition = new Label
                                                    {
                                                        Font = Fonts.Monospace(FONTSIZE)
                                                    }
                                                }
                                            },

                                            new StackLayoutItem
                                            {
                                                Expand = true,
                                                Control = new DebuggerGroup
                                                {
                                                    Text = "Tablet Report",
                                                    Width = FONTSIZE * 33,
                                                    Content = tablet = new Label
                                                    {
                                                        Font = Fonts.Monospace(FONTSIZE)
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var splitter = new Splitter
            {
                Orientation = Orientation.Vertical,
                Width = 660,
                Height = 800,
                FixedPanel = SplitterFixedPanel.Panel2,
                Panel1 = new DebuggerGroup
                {
                    Height = 200,
                    Text = "Visualizer",
                    Content = tabletVisualizer = new TabletVisualizer()
                },
                Panel2 = debugger
            };

            this.Content = new Scrollable
            {
                Content = splitter
            };

            this.KeyDown += (_, args) =>
            {
                if (args.Key == Keys.Escape)
                    this.Close();
            };

            var reportBinding = ReportDataBinding.Child(c => (c.ToObject() as IDeviceReport));

            deviceName.TextBinding.Bind(ReportDataBinding.Child(c => c.Tablet.Properties.Name));
            rawTablet.TextBinding.Bind(reportBinding.Child(c => ReportFormatter.GetStringRaw(c)));
            tablet.TextBinding.Bind(reportBinding.Child(c => ReportFormatter.GetStringFormat(c)));
            maxReportedPosition.TextBinding.Bind(MaxPositionBinding.Convert(c => MaxPositionString(c)));
            reportRate.TextBinding.Bind(ReportPeriodBinding.Convert(c => Math.Round(1000.0 / c) + "hz"));
            reportsRecorded.TextBinding.Bind(NumberOfReportsRecordedBinding.Convert(c => c.ToString()));
            tabletVisualizer.ReportDataBinding.Bind(ReportDataBinding);

            var reportRecordedNonZeroBinding = new DelegateBinding<bool>(
                () => NumberOfReportsRecorded > 0,
                addChangeEvent: (e) => NumberOfReportsRecordedChanged += e,
                removeChangeEvent: (e) => NumberOfReportsRecordedChanged -= e
            );

            var visibleChangedBinding = new BindableBinding<DebuggerGroup, bool>(
                reportsRecordedGroup,
                (c) => c.Visible,
                (c, v) => c.Visible = v
            );

            visibleChangedBinding.Bind(reportRecordedNonZeroBinding);
            enableDataRecording.CheckedChanged += (_, _) => OnDataRecordingStateChanged();

            App.Driver.DeviceReport += HandleReport;
            App.Driver.TabletsChanged += HandleTabletsChanged;
            App.Driver.Instance.SetTabletDebug(true);

            string fileName = "tablet-data_" + DateTimeOffset.UtcNow.ToUnixTimeSeconds() + ".txt";

            var outputStream = File.OpenWrite(Path.Join(AppInfo.Current.AppDataDirectory, fileName));
            dataRecordingOutput = new StreamWriter(outputStream);
        }

        protected override async void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            await App.Driver.Instance.SetTabletDebug(false);

            dataRecordingOutput?.Close();
            dataRecordingOutput = null;
        }

        private static string MaxPositionString(Vector2 pos)
        {
            if (pos.X == 0 && pos.Y == 0)
                return "";

            return $"Max Position: [{pos.X},{pos.Y}]";
        }

        private Label deviceName, rawTablet, tablet, reportRate, reportsRecorded, maxReportedPosition;
        private Vector2 maxPosition;
        private TabletVisualizer tabletVisualizer;
        private DebuggerGroup reportsRecordedGroup;
        private CheckBox enableDataRecording;

        private DebugReportData reportData;
        private double reportPeriod;
        private int numReportsRecorded;

        private HPETDeltaStopwatch stopwatch = new HPETDeltaStopwatch();
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

        public Vector2 MaxPositionReported
        {
            set
            {
                maxPosition = value;
                OnMaxPositionReportedChanged();

            }
            get => maxPosition;
        }

        public event EventHandler<EventArgs> ReportDataChanged;
        public event EventHandler<EventArgs> ReportPeriodChanged;
        public event EventHandler<EventArgs> NumberOfReportsRecordedChanged;
        public event EventHandler<EventArgs> MaxPositionReportedChanged;

        protected virtual void OnReportDataChanged()
        {
            ReportDataChanged?.Invoke(this, new EventArgs());
        }

        protected virtual void OnDataRecordingStateChanged()
        {
            if (enableDataRecording.Checked ?? false)
                NumberOfReportsRecorded = 0;
        }

        protected virtual void OnReportPeriodChanged() => ReportPeriodChanged?.Invoke(this, new EventArgs());
        protected virtual void OnNumberOfReportsRecordedChanged() => NumberOfReportsRecordedChanged?.Invoke(this, new EventArgs());
        protected virtual void OnMaxPositionReportedChanged() => MaxPositionReportedChanged?.Invoke(this, new EventArgs());

        public BindableBinding<TabletDebugger, Vector2> MaxPositionBinding
        {
            get
            {
                return new BindableBinding<TabletDebugger, Vector2>(
                    this,
                    c => c.MaxPositionReported,
                    (c, v) => c.MaxPositionReported = v,
                    (c, h) => c.MaxPositionReportedChanged += h,
                    (c, h) => c.MaxPositionReportedChanged -= h
                );
            }
        }

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

        private void HandleReport(object sender, DebugReportData data) => Application.Instance.AsyncInvoke(() =>
        {
            ReportData = data;
            var tabletProperties = data.Tablet.Properties;
            var timeDelta = stopwatch.Restart();
            ReportPeriod += (timeDelta.TotalMilliseconds - ReportPeriod) * 0.01f;

            if (data.ToObject() is ITabletReport tabletReport)
            {

                float x = Math.Max(maxPosition.X, tabletReport.Position.X);
                float y = Math.Max(maxPosition.Y, tabletReport.Position.Y);

                MaxPositionReported = new Vector2(x, y);
            }

            if (data.ToObject() is IDeviceReport deviceReport)
            {
                if (enableDataRecording.Checked ?? false)
                {
                    var output = ReportFormatter.GetStringFormatOneLine(tabletProperties, deviceReport, timeDelta, data.Path);
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
            private List<int> _warnedDigitizers = [];

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
                        var digitizer = tablet.Properties.Specifications.Digitizer;
                        var yScale = (this.ClientSize.Height - SPACING) / digitizer.Height;
                        var xScale = (this.ClientSize.Width - SPACING) / digitizer.Width;
                        var finalScale = Math.Min(yScale, xScale);

                        var clientCenter = new PointF(this.ClientSize.Width, this.ClientSize.Height) / 2;
                        var tabletCenter = new PointF(digitizer.Width, digitizer.Height) / 2 * finalScale;

                        graphics.TranslateTransform(clientCenter - tabletCenter);

                        DrawBackground(graphics, finalScale, tablet);
                        DrawPosition(graphics, finalScale, tablet);
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
                var specifications = ReportData?.Tablet.Properties.Specifications;
                var tabletName = ReportData?.Tablet.Properties.Name;
                var touchDigitizerSpecification = specifications?.Touch;
                var absDigitizerSpecification = specifications?.Digitizer;

                if (report is IAbsolutePositionReport absReport)
                {
                    if (absDigitizerSpecification != null)
                    {
                        var tabletScale = calculateTabletScale(absDigitizerSpecification, scale);
                        var position = new PointF(absReport.Position.X, absReport.Position.Y) * tabletScale;

                        var drawRect = RectangleF.FromCenter(position, new SizeF(SPACING, SPACING));
                        graphics.FillEllipse(AccentColor, drawRect);
                    }
                    else
                    {
                        var absHashName = tabletName + "abs";
                        var absHash = absHashName.GetHashCode();
                        if (!_warnedDigitizers.Contains(absHash))
                        {
                            _warnedDigitizers.Add(absHash);
                            Log.Write("TabletDebugger",
                                "Digitizer undefined in tablet configuration - unable to draw points",
                                LogLevel.Warning);
                        }
                    }
                }

                // touch reports
                if (report is ITouchReport touchReport)
                {
                    if (touchDigitizerSpecification != null)
                    {
                        var tabletScale = calculateTabletScale(touchDigitizerSpecification, scale);

                        foreach (TouchPoint touchPoint in touchReport.Touches.Where((t) => t != null))
                        {
                            var position = new PointF(touchPoint.Position.X, touchPoint.Position.Y) * tabletScale;
                            var drawPen = new Pen(AccentColor, SPACING / 2);
                            var drawRect = RectangleF.FromCenter(position, new SizeF(SPACING * 2, SPACING * 2));
                            graphics.DrawEllipse(drawPen, drawRect);
                        }
                    }
                    else
                    {
                        var touchHashName = tabletName + "touch";
                        var touchHash = touchHashName.GetHashCode();
                        if (!_warnedDigitizers.Contains(touchHash))
                        {
                            _warnedDigitizers.Add(touchHash);
                            Log.Write("TabletDebugger",
                                "Touch undefined in tablet configuration - unable to draw touch points",
                                LogLevel.Warning);
                        }
                    }
                }
            }

            protected SizeF calculateTabletScale(DigitizerSpecifications digitizer, float scale)
            {
                var tabletMm = new SizeF(digitizer.Width, digitizer.Height);
                var tabletPx = new SizeF(digitizer.MaxX, digitizer.MaxY);
                return tabletMm / tabletPx * scale;
            }
        }
    }
}
