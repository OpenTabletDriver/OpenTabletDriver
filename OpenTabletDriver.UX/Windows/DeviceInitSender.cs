using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using HidSharp;
using OpenTabletDriver.Plugin.Devices;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Windows
{
    public class DeviceInitSender : DesktopForm
    {
        public DeviceInitSender()
            : base(Application.Instance.MainForm)
        {
            this.Title = "Device Init Sender";
            this.Icon = App.Logo.WithSize(App.Logo.Size);
            this.ClientSize = new Size(400, 475);

            var sendButton = new Button
            {
                Text = "Send Init",
            };

            sendButton.Click += async (_, _) => await SendInitWithTimeout(initValueText.Text,
                (s) => resultText.Text = s,
                (e) => MessageBox.Show($"Error: {e.Message}", MessageBoxType.Error),
                () => MessageBox.Show(OperationTimedOut)
            );

            var vendorIdCtrl = new Group("VendorID", vendorIdText, Orientation.Horizontal, false);
            var productIdCtrl = new Group("ProductID", productIdText, Orientation.Horizontal, false);
            var interfaceCtrl = new Group("Interface", interfaceText, Orientation.Horizontal, false);
            var initValueCtrl = new Group("Init Value", initValueText, Orientation.Horizontal, false);
            var initTypeCtrl = new Group("Init Type", initTypesButtonList, Orientation.Horizontal, false);
            var initFormatCtrl = new Group("Init Format", initFormatsButtonList, Orientation.Horizontal, false);

            this.Content = new StackLayout
            {
                Padding = 5,
                Spacing = 5,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Items =
                {
                    new Label
                    {
                        Text = "WARNING\nArbitrary use of this tool may cause damage or disrupt usage of your tablet",
                        Font = SystemFonts.Bold(),
                        TextAlignment = TextAlignment.Center,
                        Wrap = WrapMode.Word,
                    },
                    new Group("Connected HIDs", deviceDropDown, Orientation.Horizontal, false),
                    vendorIdCtrl,
                    productIdCtrl,
                    interfaceCtrl,
                    initValueCtrl,
                    initTypeCtrl,
                    initFormatCtrl,
                    new StackLayoutItem(
                        new StackLayout
                        {
                            Orientation = Orientation.Horizontal,
                            Items =
                            {
                                new Panel { Content = sendButton, Padding = new Padding(5, 5) },
                            }
                        },
                        HorizontalAlignment.Center
                    ),
                    new StackLayoutItem(resultText, true)
                }
            };

            this.KeyDown += (_, args) =>
            {
                if (args.Key == Keys.Escape)
                    this.Close();
            };

            deviceDropDown.SelectedItemBinding
                .Convert(x => x?.VendorID.ToString() ?? vendorIdText.Text, _ => null)
                .Bind(vendorIdText.TextBinding);

            deviceDropDown.SelectedItemBinding
                .Convert(x => x?.ProductID.ToString() ?? productIdText.Text, _ => null)
                .Bind(productIdText.TextBinding);

            if (App.Driver.IsConnected)
                SetDeviceDropDownDataStore();

            this.deviceDropDown.ItemTextBinding = Binding.Delegate<SerializedDeviceEndpoint, string>(x =>
            {
                // don't include manufacturer if it's already in the product name (e.g. Razer)
                string name = x.ProductName ?? x.FriendlyName ?? "null";
                string manufacturer = x.Manufacturer ?? "null";
                string title = name.Contains(manufacturer) ? name : $"{manufacturer} {name}";

                if (title.Length >= 32) // truncate if too long, used in GUI
                    title = title[..29] + "...";

                // hex value preferable (but not consistent.. yet?)
                return $"[0x{x.VendorID:x4} 0x{x.ProductID:x4}]{Environment.NewLine}{title}";
            });
        }


        private IEnumerable<SerializedDeviceEndpoint> devicesCache = [];
        private void SetDeviceDropDownDataStore()
        {
            Debug.Assert(App.Driver.IsConnected, "Tried setting device dropdown without an active daemon");

            this.devicesCache = App.Driver.Instance.GetDevices().Result;

            this.deviceDropDown.DataStore =
                this.devicesCache
                    .Where(x => x.CanOpen)
                    .DistinctBy(x => new { x.VendorID, x.ProductID });
        }

        private const int NUMERICBOX_WIDTH = 150;
        private const string DecimalStyle = "Decimal Value";
        private const string OperationTimedOut = "Operation timed-out";
        private const string OperationFailed = "Operation failed";

        private async Task SendInitWithTimeout(string strInitData, Action<string> action, Action<Exception> error, Action timeoutAction)
        {
            if (!App.Driver.IsConnected)
            {
                MessageBox.Show("Unable to send request without an active daemon", MessageBoxType.Error);
                return;
            }

            Int32.TryParse(vendorIdText.Text, out int intVid);
            Int32.TryParse(productIdText.Text, out int intPid);
            byte[] initData = [];
            if (initFormatsButtonList.SelectedValue == InitFormats.Hex)
            {
                initData = Convert.FromHexString(strInitData);
            }
            else if (initFormatsButtonList.SelectedValue == InitFormats.Base64)
            {
                initData = Convert.FromBase64String(strInitData);
            }

            if (deviceDropDown.SelectedItem == null)
            {
                MessageBox.Show("Unable to send init without a device selected", MessageBoxType.Error);
                return;
            }

            var request = SendInit(this.devicesCache, initData, initTypesButtonList.SelectedValue, intVid, intPid, interfaceText.Text);
            var timeout = Task.Delay(TimeSpan.FromSeconds(5));
            var completed = await Task.WhenAny(request, timeout);
            if (completed == timeout)
            {
                timeoutAction();
            }
            else
            {
                try
                {
                    var str = await request;
                    action(str);
                }
                catch (Exception e)
                {
                    error(e);
                }
            }
        }

        private static async Task<string> SendInit(IEnumerable<SerializedDeviceEndpoint> devicesInfo, byte[] initData, InitTypes initType, int intVid, int intPid, string strInterface)
        {
            Debug.Assert(App.Driver.IsConnected, "Sending a request should not be able to be called without an active daemon");

            if (initData.Length == 0)
                throw new ArgumentException("Init length cannot be zero");

            var devicePaths = from d in devicesInfo
                                  where d.CanOpen && d.VendorID == intVid && d.ProductID == intPid && d.DeviceAttributes.TryGetValue("USB_INTERFACE_NUMBER", out var identifierInterface) && identifierInterface == strInterface
                                  select d.DevicePath;

            var devicePath = devicePaths.First();

            var device = DeviceList.Local.GetHidDevices().First(d => d.DevicePath == devicePath);

            if (device.TryOpen(out HidStream hidStream)) {
                try {
                    if (initType == InitTypes.Feature)
                    {
                        hidStream.SetFeature(initData);
                        return "Feature init success";
                    }
                    else if (initType == InitTypes.Output)
                    {
                        hidStream.Write(initData);
                        return "Output init success";
                    }
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }
            }
            else
            {
                throw new ArgumentException("Failed to open device");
            }

            throw new ArgumentException("Invalid init type or init type not selected");
        }

        private readonly DropDown<SerializedDeviceEndpoint> deviceDropDown = new();

        private readonly NumericMaskedTextBox<ushort> vendorIdText = new()
        {
            PlaceholderText = DecimalStyle,
            Width = NUMERICBOX_WIDTH
        };

        private readonly NumericMaskedTextBox<ushort> productIdText = new()
        {
            PlaceholderText = DecimalStyle,
            Width = NUMERICBOX_WIDTH
        };

        private readonly NumericMaskedTextBox<ushort> interfaceText = new()
        {
            PlaceholderText = DecimalStyle,
            Width = NUMERICBOX_WIDTH
        };

        private readonly TextBox initValueText = new()
        {
            Width = NUMERICBOX_WIDTH
        };

        private readonly TextBox resultText = new()
        {
            PlaceholderText = "Result",
            ReadOnly = true
        };

        private readonly EnumRadioButtonList<InitTypes> initTypesButtonList = new()
        {
            SelectedValue = InitTypes.None,
        };

        private enum InitTypes
        {
            None,
            Feature,
            Output,
        }

        private readonly EnumRadioButtonList<InitFormats> initFormatsButtonList = new()
        {
            SelectedValue = InitFormats.Base64,
        };

        private enum InitFormats
        {
            Base64,
            Hex,
        }
    }
}
