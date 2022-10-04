using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.UX.Components;
using OpenTabletDriver.Devices;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.UX.Windows
{
    public class LegacyTabletConnectionWindow : DesktopForm
    {
        private readonly IDriverDaemon _daemon;
        private readonly App _app;

        public LegacyTabletConnectionWindow(IDriverDaemon daemon, App app)
        {
            _daemon = daemon;
            _app = app;

            Title = "Connect legacy tablet...";

            //Icon = App.Logo.WithSize(App.Logo.Size);
            ClientSize = new Size(300, 250);

            var connectButton = new Button
            {
                Text = "Connect",
            };

            connectButton.Click += async (_, _) =>
            {
                if (await _daemon.ConnectLegacyTablet(new Uri(devicePathText.Text), (TabletConfiguration)tablet.SelectedValue, reconnectBox.Checked.Value))
                {
                    Close();
                }
            };

            //App.Driver.Disconnected += (sender, args) => Close();

            devicePathText = new ComboBox();

            devicePathText.DataStore = _daemon.GetLegacyPorts().Result;

            tablet = new DropDown();

            tablet.DataStore = _daemon.GetSupportedTablets().Result.Where(x => x.Attributes.ContainsKey("isLegacy"));;

            // Orientation.Vertical
            devicePathGroup = new GroupBox
            {
                Text = "Device path",
                Content = devicePathText
            };

            // Orientation.Vertical
            tabletGroup = new GroupBox
            {
                Text = "Tablet",
                Content = tablet
            };

            reconnectBox = new CheckBox
            {
                Text = "Remember tablet"
            };

            Content = new StackLayout
            {
                Padding = 5,
                Spacing = 5,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Items =
                {
                    portTypeGroup,
                    devicePathGroup,
                    tabletGroup,
                    reconnectBox,
                    connectButton
                }
            };
        }

        private readonly ComboBox devicePathText;
        private readonly CheckBox reconnectBox;
        private readonly Button connectButton;

        private readonly GroupBox devicePathGroup, tabletGroup, portTypeGroup;

        private readonly DropDown tablet;
    }
}
