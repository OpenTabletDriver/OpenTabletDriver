using System;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriverUX.Debugging;
using TabletDriverLib.Tablet;
using TabletDriverPlugin.Tablet;

namespace OpenTabletDriverUX.Windows
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

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            var result = await App.DriverDaemon.InvokeAsync(d => d.SetTabletDebug(true));
            var guids = result.ToList();

            if (guids.Count > 0)
            {
                var tabletReader = new PipeReader<DebugTabletReport>(guids[0].ToString());
                tabletReader.Report += HandleReport;
                this.Closing += (sender, e) => tabletReader.Dispose();
            }
            
            if (guids.Count > 1)
            {
                var auxReader = new PipeReader<DebugAuxReport>(guids[1].ToString());
                auxReader.Report += HandleReport;
                this.Closing += (sender, e) => auxReader.Dispose();
            }

            this.Closing += async (sender, e) => 
            {
                // For whatever reason, this sometimes hangs the entire GUI application.
                await App.DriverDaemon.InvokeAsync(d => d.SetTabletDebug(false));
            };
        }

        private GroupBox rawTabCtrl, tabReportCtrl, rawAuxCtrl, auxReportCtrl;

        private async void HandleReport(object sender, IDeviceReport report)
        {
            if (report is ITabletReport tabletReport)
            {
                await Application.Instance.InvokeAsync(() => 
                {
                    rawTabCtrl.Content = tabletReport.StringFormat(true);
                    tabReportCtrl.Content = tabletReport.StringFormat(false).Replace(", ", Environment.NewLine);
                });
            }
            if (report is IAuxReport auxReport)
            {
                await Application.Instance.InvokeAsync(() => 
                {
                    rawAuxCtrl.Content = auxReport.StringFormat(true);
                    auxReportCtrl.Content = auxReport.StringFormat(false).Replace(", ", Environment.NewLine);
                });
            }
        }
    }
}