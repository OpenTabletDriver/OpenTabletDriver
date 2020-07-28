using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;
using HidSharp;

namespace OpenTabletDriver.UX.Windows
{
    public class DeviceListDialog : Dialog<HidDevice>
    {
        public DeviceListDialog()
        {
            Title = "Device List";
            ClientSize = new Size(960 - 100, 730 - 100);
            MinimumSize = new Size(960 - 100, 730 - 100);
            Icon = App.Logo.WithSize(App.Logo.Size);

            Devices = DeviceList.Local.GetHidDevices().ToList();
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

        private List<HidDevice> _devices;
        private List<HidDevice> Devices
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
                        _deviceList.Items.Add(device.GetFriendlyName());
                    }
                    catch (Exception ex)
                    {
                        if (!getDeviceFailed)
                        {
                            MessageBox.Show($"Failed to get a device, one or more HID devices may not be shown.");
                            getDeviceFailed = true;
                        }
                    }
                }
            }
            get => _devices;
        }

        private HidDevice _selected;
        private HidDevice SelectedDevice
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

        private IEnumerable<Control> GeneratePropertyControls() => new List<Control>
        {
            GetControl("Friendly Name",
                SelectedDevice.GetFriendlyName
            ),
            GetControl("Manufacturer",
                () => 
                {
                    try
                    {
                        return SelectedDevice.GetManufacturer();
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
                        return SelectedDevice.GetMaxFeatureReportLength().ToString();
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
                        return SelectedDevice.GetMaxInputReportLength().ToString();
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
                        return SelectedDevice.GetMaxOutputReportLength().ToString();
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
                        return SelectedDevice.GetProductName();
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
                        return SelectedDevice.GetSerialNumber();
                    }
                    catch
                    {
                        return string.Empty;
                    }
                }
            )
        };

        private GroupBox GetControl(string groupName, Func<string> getValue)
        {
            var textBox = new TextBox();
            try
            {
                textBox.TextBinding.Bind(getValue);
            }
            catch
            {
                textBox.Text = $"Failed to obtain '{groupName.ToLower()}'.";
                textBox.TextColor = Colors.Red;
            }
            return new GroupBox
            {
                Text = groupName,
                Padding = App.GroupBoxPadding,
                Content = textBox
            };
        }

        private void Return()
        {
            this.Close(SelectedDevice);
        }
    }
}