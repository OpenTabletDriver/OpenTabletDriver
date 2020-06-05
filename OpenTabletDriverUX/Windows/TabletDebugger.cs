using System;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriverUX.Debugging;
using TabletDriverLib.Tablet;
using TabletDriverPlugin.Tablet;

namespace OpenTabletDriverUX.Windows
{
    public class TabletDebugger : Form
    {
        private TabletDebugger()
        {
            Title = "Tablet Debugger";
        }

        public TabletDebugger(string tabletPipeName, string auxPipeName) : this()
        {
            rawTabCtrl = new GroupBox
            {
                Text = "Raw Tablet Data",
                Padding = new Padding(5)
            };
            
            tabReportCtrl = new GroupBox
            {
                Text = "Tablet Report",
                Padding = new Padding(5)
            };

            rawAuxCtrl = new GroupBox
            {
                Text = "Raw Aux Data",
                Padding = new Padding(5)
            };
            
            auxReportCtrl = new GroupBox
            {
                Text = "Aux Report",
                Padding = new Padding(5)
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

            InitializeAsync(tabletPipeName, auxPipeName);
        }


        private async void InitializeAsync(string tabletPipeName, string auxPipeName)
        {
            await App.DriverDaemon.InvokeAsync(d => d.SetTabletDebug(true));

            var tabletReader = new PipeReader<DebugTabletReport>(tabletPipeName);
            tabletReader.Report += HandleReport;
            
            var auxReader = new PipeReader<DebugAuxReport>(auxPipeName);
            auxReader.Report += HandleReport;

            this.Closing += async (sender, e) => 
            {
                // For whatever reason, this sometimes hangs the entire GUI application.
                await App.DriverDaemon.InvokeAsync(d => d.SetTabletDebug(false));
                tabletReader.Dispose();
                auxReader.Dispose();
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