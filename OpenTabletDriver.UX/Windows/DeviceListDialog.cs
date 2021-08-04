using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Devices;
using OpenTabletDriver.Plugin.Devices;
using OpenTabletDriver.UX.Controls.Generic;

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

            InitializeAsync();
        }

        private List<SerializedDeviceEndpoint> _devices;
        private List<SerializedDeviceEndpoint> Devices
        {
            set 
            {
                _devices = value;
                _deviceList.Items.Clear();
                bool getDeviceFailed = false;
                foreach (var device in _devices)
                {
                    try
                    {
                        _deviceList.Items.Add(device.FriendlyName);
                    }
                    catch (Exception ex)
                    {
                        if (!getDeviceFailed)
                        {
                            MessageBox.Show($"Failed to get a device, one or more HID devices may not be shown." + Environment.NewLine
                                + $"{ex.GetType().Name}: {ex.Message}");
                            getDeviceFailed = true;
                        }
                    }
                }
            }
            get => _devices;
        }

        private SerializedDeviceEndpoint _selected;
        private SerializedDeviceEndpoint SelectedDevice
        {
            set
            {
                _selected = value;
                _devicePropertyList.Items.Clear();
                foreach (var prop in GeneratePropertyControls())
                {
                    var item = new StackLayoutItem(prop, HorizontalAlignment.Stretch);
                    _devicePropertyList.Items.Add(item);
                }
            }
            get => _selected;
        }

        private ListBox _deviceList = new ListBox();
        
        private StackLayout _devicePropertyList = new StackLayout
        {
            Padding = new Padding(5),
            Spacing = 5
        };

        private async void InitializeAsync()
        {
            var devices = from device in await App.Driver.Instance.GetDevices()
                          where device.CanOpen
                          select device;

            Devices = devices.ToList();

            _deviceList.SelectedIndexChanged += (sender, e) =>
            {
                if (_deviceList.SelectedIndex >= 0)
                    SelectedDevice = Devices[_deviceList.SelectedIndex];
            };

            Content = new Splitter
            {
                Orientation = Orientation.Horizontal,
                Panel1MinimumSize = 200,
                Panel1 = _deviceList,
                Panel2 = new Scrollable
                {
                    Content = _devicePropertyList
                }
            };

            var selectDevice = new Command { ToolBarText = "Select" };
            selectDevice.Executed += (sender, e) => Return();

            ToolBar = new ToolBar
            {
                Items =
                {
                    selectDevice
                }
            };
        }

        private IEnumerable<Control> GeneratePropertyControls() => new List<Control>
        {
            GetControl("Friendly Name",
                () => SelectedDevice.FriendlyName
            ),
            GetControl("Manufacturer",
                () => 
                {
                    try
                    {
                        return SelectedDevice.Manufacturer;
                    }
                    catch
                    {
                        return string.Empty;
                    }
                }
            ),
            GetControl("Vendor ID",
                () => SelectedDevice.VendorID.ToString()
            ),
            GetControl("Product ID",
                () => SelectedDevice.ProductID.ToString()
            ),
            GetControl("Max Feature Report Length",
                () => 
                {
                    try
                    {
                        return SelectedDevice.FeatureReportLength.ToString();
                    }
                    catch
                    {
                        return string.Empty;
                    }
                }
            ),
            GetControl("Max Input Report Length",
                () =>
                {
                    try
                    {
                        return SelectedDevice.InputReportLength.ToString();
                    }
                    catch
                    {
                        return string.Empty;
                    }
                }
            ),
            GetControl("Max Output Report Length",
                () => 
                {
                    try
                    {
                        return SelectedDevice.OutputReportLength.ToString();
                    }
                    catch
                    {
                        return string.Empty;
                    }
                }
            ),
            GetControl("Product Name",
                () => 
                {
                    try
                    {
                        return SelectedDevice.ProductName;
                    }
                    catch
                    {
                        return string.Empty;
                    }
                }
            ),
            GetControl("Serial Number",
                () => 
                {
                    try
                    {
                        return SelectedDevice.SerialNumber;
                    }
                    catch
                    {
                        return string.Empty;
                    }
                }
            )
        };

        private Control GetControl(string groupName, Func<string> getValue)
        {
            var textBox = new TextBox
            {
                Width = 400
            };
            try
            {
                textBox.TextBinding.Bind(getValue);
            }
            catch
            {
                textBox.Text = $"Failed to obtain '{groupName.ToLower()}'.";
                textBox.TextColor = Colors.Red;
            }
            return new Group(groupName, textBox, Orientation.Horizontal, false);
        }

        private void Return()
        {
            this.Close(SelectedDevice);
        }
    }
}