using System;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.UX.Windows
{
    public class TabletDebugger : Form
    {
        public TabletDebugger()
        {
            Title = "Tablet Debugger";

            rawTabCtrl = new GroupBox
            {
                Text = "Raw Tablet Data",
                Padding = App.GroupBoxPadding
            };
            
            tabReportCtrl = new GroupBox
            {
                Text = "Tablet Report",
                Padding = App.GroupBoxPadding
            };

            rawAuxCtrl = new GroupBox
            {
                Text = "Raw Aux Data",
                Padding = App.GroupBoxPadding
            };
            
            auxReportCtrl = new GroupBox
            {
                Text = "Aux Report",
                Padding = App.GroupBoxPadding
            };
            
            var mainLayout = new TableLayout
            {
                Width = 640,
                Height = 480,
                Spacing = new Size(5, 5),
                Padding = new Padding(5),
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

            this.Content = mainLayout;

            lastTime = DateTime.UtcNow;
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

        private GroupBox rawTabCtrl, tabReportCtrl, rawAuxCtrl, auxReportCtrl;
        private float reportRate;
        private DateTime lastTime;

        private void HandleReport(object sender, IDeviceReport report)
        {
            if (report is ITabletReport tabletReport)
            {
                Application.Instance.AsyncInvoke(() => 
                {
                    var now = DateTime.UtcNow;
                    reportRate += (float)(((now - lastTime).TotalMilliseconds - reportRate) / 50);
                    lastTime = now;
                    rawTabCtrl.Content = tabletReport?.StringFormat(true);
                    tabReportCtrl.Content = tabletReport?.StringFormat(false).Replace(", ", Environment.NewLine)
                        + $"\nReportRate:{(uint)(1000 / reportRate)}hz";
                });
            }
            if (report is IAuxReport auxReport)
            {
                Application.Instance.AsyncInvoke(() => 
                {
                    rawAuxCtrl.Content = auxReport?.StringFormat(true);
                    auxReportCtrl.Content = auxReport?.StringFormat(false).Replace(", ", Environment.NewLine);
                });
            }
        }
    }
}