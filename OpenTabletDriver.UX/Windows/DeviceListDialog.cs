using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Devices;
using OpenTabletDriver.Plugin.Devices;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Controls.Generic.Text;

namespace OpenTabletDriver.UX.Windows
{
    public class DeviceListDialog : Dialog<SerializedDeviceEndpoint>
    {
        public DeviceListDialog()
        {
            Title = "Device List";
            ClientSize = new Size(960 - 100, 730 - 100);
            MinimumSize = new Size(960 - 100, 730 - 100);
            Icon = App.Logo.WithSize(App.Logo.Size);

            Content = new Splitter
            {
                Orientation = Orientation.Horizontal,
                Panel1MinimumSize = 200,
                Panel1 = deviceList = new ListBox<SerializedDeviceEndpoint>(),
                Panel2 = new Scrollable
                {
                    Content = new StackLayout
                    {
                        Padding = new Padding(5),
                        Spacing = 5,
                        HorizontalContentAlignment = HorizontalAlignment.Stretch,
                        Items =
                        {
                            new Group
                            {
                                Text = "Friendly Name",
                                Orientation = Orientation.Horizontal,
                                Content = this.friendlyName = new TextBox()
                            },
                            new Group
                            {
                                Text = "Manufacturer",
                                Orientation = Orientation.Horizontal,
                                Content = this.manufacturer = new TextBox()
                            },
                            new Group
                            {
                                Text = "Product Name",
                                Orientation = Orientation.Horizontal,
                                Content = this.productName = new TextBox()
                            },
                            new Group
                            {
                                Text = "Serial Number",
                                Orientation = Orientation.Horizontal,
                                Content = this.serialNumber = new TextBox()
                            },
                            new Group
                            {
                                Text = "Vendor ID",
                                Orientation = Orientation.Horizontal,
                                Content = this.vendorId = new HexNumberBox()
                            },
                            new Group
                            {
                                Text = "Product ID",
                                Orientation = Orientation.Horizontal,
                                Content = this.productId = new HexNumberBox()
                            },
                            new Group
                            {
                                Text = "Input Report Length",
                                Orientation = Orientation.Horizontal,
                                Content = this.inputReportLength = new IntegerNumberBox()
                            },
                            new Group
                            {
                                Text = "Output Report Length",
                                Orientation = Orientation.Horizontal,
                                Content = this.outputReportLength = new IntegerNumberBox()
                            },
                            new Group
                            {
                                Text = "Feature Report Length",
                                Orientation = Orientation.Horizontal,
                                Content = this.featureReportLength = new IntegerNumberBox()
                            },
                            new Group
                            {
                                Text = "Device Path",
                                Orientation = Orientation.Horizontal,
                                Content = this.devicePath = new TextBox()
                            }
                        }
                    }
                }
            };

            deviceList.ItemTextBinding = Binding.Property<IDeviceEndpoint, string>(d => d.FriendlyName);

            var selectedItemBinding = deviceList.SelectedItemBinding;
            this.friendlyName.TextBinding.Bind(selectedItemBinding.Child(c => c.FriendlyName));
            this.manufacturer.TextBinding.Bind(selectedItemBinding.Child(c => c.Manufacturer));
            this.productName.TextBinding.Bind(selectedItemBinding.Child(c => c.ProductName));
            this.serialNumber.TextBinding.Bind(selectedItemBinding.Child(c => c.SerialNumber));
            this.devicePath.TextBinding.Bind(selectedItemBinding.Child(c => c.DevicePath));
            this.vendorId.ValueBinding.Bind(selectedItemBinding.Child(c => c.VendorID));
            this.productId.ValueBinding.Bind(selectedItemBinding.Child(c => c.ProductID));
            this.inputReportLength.ValueBinding.Bind(selectedItemBinding.Child(c => c.InputReportLength));
            this.outputReportLength.ValueBinding.Bind(selectedItemBinding.Child(c => c.OutputReportLength));
            this.featureReportLength.ValueBinding.Bind(selectedItemBinding.Child(c => c.FeatureReportLength));

            var select = new Command { ToolBarText = "Select" };
            select.Executed += (sender, e) => Close(deviceList.SelectedItem);

            ToolBar = new ToolBar
            {
                Items =
                {
                    select
                }
            };
        }

        public async Task InitializeAsync()
        {
            deviceList.Source = (await App.Driver.Instance.GetDevices())
                .Where(d => d.CanOpen)
                .ToList();
        }

        private ListBox<SerializedDeviceEndpoint> deviceList;
        private TextBox friendlyName, manufacturer, productName, serialNumber, devicePath;
        private MaskedTextBox<int> vendorId, productId, inputReportLength, outputReportLength, featureReportLength;
    }
}
