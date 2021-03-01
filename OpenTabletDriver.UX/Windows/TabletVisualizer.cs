using System;
using System.ComponentModel;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.RPC;
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

            App.Driver.Instance.DeviceReport += SetReport;
            await App.Driver.Instance.SetTabletDebug(true);

            var tablet = await App.Driver.Instance.GetTablet();
            SetTablet(this, tablet);
        }

        protected override async void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (!e.Cancel)
            {
                await App.Driver.Instance.SetTabletDebug(false);
                App.Driver.Instance.TabletChanged -= SetTablet;
                App.Driver.Instance.DeviceReport -= SetReport;
            }
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

        private void SetReport(object sender, RpcData data)
        {
            Application.Instance.AsyncInvoke(() => 
            {
                display.SetData(data);
            });
        }

        private TabletVisualizerDisplay display;
        private Label tabletNameLabel;

        private class TabletVisualizerDisplay : TimedDrawable
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

                        var clientCenter = new PointF(this.ClientSize.Width, this.ClientSize.Height) / 2;
                        var tabletCenter = new PointF(tablet.Digitizer.Width, tablet.Digitizer.Height) / 2 * pxToMM;

                        graphics.TranslateTransform(clientCenter - tabletCenter);

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
                var report = data?.GetData(AppInfo.PluginManager);
                
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
