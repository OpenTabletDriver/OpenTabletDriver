using System;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Tablet;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Windows
{
    public class TabletDebugger : Form
    {
        public TabletDebugger()
        {
            Title = "Tablet Debugger";

            rawTabCtrl = new Group
            {
                Text = "Raw Tablet Data",
                Content = rawTabText = new Label
                {
                    Font = new Font(FontFamilies.Monospace, textSize)
                }
            };
            
            tabReportCtrl = new Group
            {
                Text = "Tablet Report",
                Content = tabReportText = new Label
                {
                    Font = new Font(FontFamilies.Monospace, textSize)
                }
            };

            rawAuxCtrl = new Group
            {
                Text = "Raw Aux Data",
                Content = rawAuxText = new Label
                {
                    Font = new Font(FontFamilies.Monospace, textSize)
                }
            };
            
            auxReportCtrl = new Group
            {
                Text = "Aux Report",
                Content = auxReportText = new Label
                {
                    Font = new Font(FontFamilies.Monospace, textSize)
                }
            };

            reportRateCtrl = new Group
            {
                Text = "Report Rate",
                Content = reportRateText = new Label
                {
                    Font = new Font(FontFamilies.Monospace, textSize)
                }
            };

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
                            new TableCell(rawTabCtrl, true),
                            new TableCell(tabReportCtrl, true)
                        },
                        ScaleHeight = true
                    },
                    new TableRow
                    {
                        Cells =
                        {
                            new TableCell(rawAuxCtrl, true),
                            new TableCell(auxReportCtrl, true)
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
                    new StackLayoutItem(reportRateCtrl, HorizontalAlignment.Stretch)
                }
            };

            InitializeAsync();
        }

        private void InitializeAsync()
        {
            App.Driver.Instance.TabletReport += HandleReport;
            App.Driver.Instance.AuxReport += HandleReport;
            App.Driver.Instance.SetTabletDebug(true);
            this.Closing += (sender, e) =>
            {
                App.Driver.Instance.TabletReport -= HandleReport;
                App.Driver.Instance.AuxReport -= HandleReport;
                App.Driver.Instance.SetTabletDebug(false);
            };
        }

        private Group rawTabCtrl, tabReportCtrl, rawAuxCtrl, auxReportCtrl, reportRateCtrl;
        private Label rawTabText, tabReportText, rawAuxText, auxReportText, reportRateText;
        private float textSize = 10;
        private float reportRate;
        private DateTime lastTime = DateTime.UtcNow;

        private void HandleReport(object sender, IDeviceReport report)
        {
            if (report is ITabletReport tabletReport)
            {
                Application.Instance.AsyncInvoke(() => 
                {
                    var now = DateTime.UtcNow;
                    reportRate += (float)(((now - lastTime).TotalMilliseconds - reportRate) / 50);
                    lastTime = now;
                    rawTabText.Text = tabletReport?.StringFormat(true);
                    tabReportText.Text = tabletReport?.StringFormat(false).Replace(", ", Environment.NewLine);
                    reportRateText.Text = $"{(uint)(1000 / reportRate)}hz";
                });
            }
            if (report is IAuxReport auxReport)
            {
                Application.Instance.AsyncInvoke(() => 
                {
                    rawAuxText.Text = auxReport?.StringFormat(true);
                    auxReportText.Text = auxReport?.StringFormat(false).Replace(", ", Environment.NewLine);
                });
            }
        }
    }
}