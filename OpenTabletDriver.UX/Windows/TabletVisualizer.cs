using System;
using System.ComponentModel;
using System.Threading;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Windows
{
    public class TabletVisualizer : Form
    {
        public TabletVisualizer()
        {
            this.ClientSize = new Size(600, 450);
        }

        protected override async void OnLoadComplete(EventArgs e)
        {
            base.OnLoadComplete(e);

            this.Content = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Items = 
                {
                    new StackLayoutItem
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Control = new Panel
                        {
                            Padding = new Padding(0, 10),
                            Content = tabletNameLabel = new Label()
                        }
                    },
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = display = new TabletVisualizerDisplay()
                    }
                }
            };

            App.Driver.Instance.TabletChanged += SetTablet;
            App.Driver.Instance.TabletReport += SetReport;
            await App.Driver.Instance.SetTabletDebug(true);

            var tablet = await App.Driver.Instance.GetTablet();
            SetTablet(this, tablet);
        }

        protected override async void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            await App.Driver.Instance.SetTabletDebug(false);
            App.Driver.Instance.TabletChanged -= SetTablet;
            App.Driver.Instance.TabletReport -= SetReport;
        }

        private void SetTablet(object sender, TabletState tablet)
        {
            Application.Instance.AsyncInvoke(() => 
            {
                this.Title = tablet != null ? $"Tablet Visualizer - {tablet.TabletProperties.Name}" : "Tablet Visualizer";
                tabletNameLabel.Text = tablet?.TabletProperties?.Name ?? "No tablet detected.";
                display.SetTablet(tablet);
            });
        }

        private void SetReport(object sender, IDeviceReport report)
        {
            Application.Instance.AsyncInvoke(() => 
            {
                display.SetReport(report);
            });
        }

        private TabletVisualizerDisplay display;
        private Label tabletNameLabel;

        private class TabletVisualizerDisplay : Drawable
        {
            private const int FRAMES_PER_MS = 1000 / 60;

            private static readonly Color AccentColor = SystemColors.Highlight;

            private IDeviceReport report;
            private TabletState tablet;
            private Timer refreshTimer;

            public void SetReport(IDeviceReport report) => this.report = report;
            public void SetTablet(TabletState tablet) => this.tablet = tablet;

            protected override void OnLoadComplete(EventArgs e)
            {
                base.OnLoadComplete(e);

                refreshTimer = new Timer(
                    (s) => Application.Instance.AsyncInvoke(this.Invalidate),
                    null,
                    0,
                    FRAMES_PER_MS
                );

                base.ParentWindow.Closing += (sender, e) =>
                {
                    refreshTimer?.Dispose();
                    refreshTimer = null;
                };
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                if (tablet != null)
                {
                    var graphics = e.Graphics;
                    using (graphics.SaveTransformState())
                    {
                        var pxToMM = (float)graphics.DPI / 25.4f;

                        var center = new PointF(this.ClientSize.Width, this.ClientSize.Height) / 2;
                        var tabletTopLeft = new PointF(tablet.Digitizer.Width, tablet.Digitizer.Height) / 2 * pxToMM;

                        graphics.TranslateTransform(center - tabletTopLeft);

                        DrawBackground(graphics, pxToMM);
                        DrawPosition(graphics, pxToMM);
                    }
                }
            }

            protected void DrawBackground(Graphics graphics, float scale)
            {
                var bg = new RectangleF(0, 0, tablet.Digitizer.Width, tablet.Digitizer.Height) * scale;
                graphics.FillRectangle(SystemColors.ControlBackground, bg);
                graphics.DrawRectangle(AccentColor, bg);
            }

            protected void DrawPosition(Graphics graphics, float scale)
            {
                if (report is ITabletReport tabletReport && tablet.Digitizer.ActiveReportID.IsInRange(tabletReport.ReportID))
                {
                    var tabletMm = new SizeF(tablet.Digitizer.Width, tablet.Digitizer.Height);
                    var tabletPx = new SizeF(tablet.Digitizer.MaxX, tablet.Digitizer.MaxY);
                    var tabletScale = tabletMm / tabletPx * scale;
                    var position = new PointF(tabletReport.Position.X, tabletReport.Position.Y) * tabletScale;

                    var drawRect = RectangleF.FromCenter(position, new SizeF(5, 5));
                    graphics.FillEllipse(AccentColor, drawRect);
                }
            }
        }
    }
}
