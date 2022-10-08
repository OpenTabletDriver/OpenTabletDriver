using System.ComponentModel;
using System.Linq.Expressions;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.UX.Components;

namespace OpenTabletDriver.UX.Windows
{
    public sealed class TabletDebugger : DesktopForm
    {
        private readonly RpcClient<IDriverDaemon> _rpc;

        public TabletDebugger(IControlBuilder controlBuilder, RpcClient<IDriverDaemon> rpc, IDriverDaemon daemon)
        {
            _rpc = rpc;

            Title = "Tablet Debugger";
            MinimumSize = new Size(800, 600);

            Menu = new MenuBar
            {
                QuitItem = new AppCommand("Close", Close, Keys.Escape)
            };

            var viewer = controlBuilder.Build<TabletDisplay>();

            var deviceLabel = LabelFor(d => d.DeviceName, "None");
            var reportTypeLabel = LabelFor(d => d.ReportType);
            var rawLabel = LabelFor(d => d.Raw);
            var propertiesLabel = LabelFor(d => d.Formatted);

            var reportRateLabel = new Label { Text = "0hz" };
            var reportPeriod = 0.0;
            var sw = new HPETDeltaStopwatch();

            daemon.SetTabletDebug(true).Run();
            daemon.DeviceReport += (_, data) => Application.Instance.AsyncInvoke(() =>
            {
                viewer.Draw(data);
                DataContext = data;

                reportPeriod += (sw.Restart().TotalMilliseconds - reportPeriod) / 10.0;
                reportRateLabel.Text = $"{Math.Round(1000.0 / reportPeriod)}hz";
            });

            var left = new StackLayout
            {
                Padding = 5,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Items =
                {
                    new StackLayout
                    {
                        Orientation = Orientation.Horizontal,
                        VerticalContentAlignment = VerticalAlignment.Stretch,
                        Spacing = 5,
                        Items =
                        {
                            new StackLayoutItem(CreateGroupBox("Device", deviceLabel), true),
                            CreateGroupBox("Report Rate", reportRateLabel)
                        }
                    },
                    new StackLayoutItem(CreateGroupBox("Raw", rawLabel), true)
                }
            };

            var right = new StackLayout
            {
                Padding = 5,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Items =
                {
                    CreateGroupBox("Report Type", reportTypeLabel),
                    new StackLayoutItem(CreateGroupBox("Properties", propertiesLabel), true)
                }
            };

            Content = new Splitter
            {
                Orientation = Orientation.Vertical,
                Panel1MinimumSize = 250,
                Panel1 = viewer,
                Panel2 = new Splitter
                {
                    Orientation = Orientation.Horizontal,
                    Panel1MinimumSize = 250,
                    Panel2MinimumSize = 250,
                    Panel1 = left,
                    Panel2 = right
                }
            };
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (_rpc.IsConnected)
                _rpc.Instance!.SetTabletDebug(false).Run();
        }

        /// <summary>
        /// Creates a label for an expression.
        /// </summary>
        /// <param name="expression">The expression to bind to.</param>
        /// <param name="monospace">Whether the label font is monospaced.</param>
        /// <param name="defaultText">The fallback data</param>
        /// <returns></returns>
        private static Label LabelFor(
            Expression<Func<DebugReportData, string>>? expression,
            string? defaultText = null,
            bool monospace = true)
        {
            var label = new Label
            {
                Text = defaultText,
                Font = monospace ? Fonts.Monospace(10) : default,
                Wrap = WrapMode.Word
            };
            label.TextBinding.BindDataContext(expression);
            return label;
        }

        /// <summary>
        /// Creates a <see cref="CreateGroupBox"/> for a control.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        private static GroupBox CreateGroupBox(string text, Control? content)
        {
            return new GroupBox
            {
                Text = text,
                Content = content
            };
        }
    }
}
