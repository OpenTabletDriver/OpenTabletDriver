using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Plugin;
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
            this.ClientSize = new Size(400, 450);

            var sendButton = new Button
            {
                Text = "Send Init",
            };

            sendButton.Click += async (_, _) => await SendInitWithTimeout(initValueText.Text,
                (s) => MessageBox.Show($"Success: {s}", MessageBoxType.Information),
                (e) => MessageBox.Show($"Error: {e.Message}", MessageBoxType.Error),
                () => MessageBox.Show(OperationTimedOut)
            );

            var vendorIdCtrl = new Group("VendorID", vendorIdText, Orientation.Horizontal, false);
            var productIdCtrl = new Group("ProductID", productIdText, Orientation.Horizontal, false);
            var interfaceCtrl = new Group("Interface", interfaceDropdown, Orientation.Horizontal, false);
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

            deviceDropDown.SelectedValueChanged += (sender, e) => SetInterfaceDropDownDataStore();
            SetInterfaceDropDownDataStore();

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

        private void SetInterfaceDropDownDataStore()
        {
            this.interfaceDropdown.SelectedItem = null;

            if (this.deviceDropDown.SelectedItem == null)
            {
                // Sane max default of 10 interfaces, tablets should not be using this many interfaces
                this.interfaceDropdown.DataStore = Enumerable.Range(0, 10).Select(x => x.ToString());
                return;
            }

            var interfaces =
                this.devicesCache
                    .Where(d => d.VendorID == this.deviceDropDown.SelectedItem?.VendorID && d.ProductID == this.deviceDropDown.SelectedItem?.ProductID)
                    .Select(d =>
                    {
                        d.DeviceAttributes.TryGetValue("USB_INTERFACE_NUMBER", out var usbInterface);
                        return usbInterface ?? "0";
                    })
                    .Distinct();
            this.interfaceDropdown.DataStore = interfaces;
        }

        private const int NUMERICBOX_WIDTH = 150;
        private const string DecimalStyle = "Decimal Value";
        private const string OperationTimedOut = "Operation timed-out";

        private async Task SendInitWithTimeout(string strInitData, Action<string> action, Action<Exception> error, Action timeoutAction)
        {
            if (!App.Driver.IsConnected)
            {
                MessageBox.Show("Unable to send init without an active daemon", MessageBoxType.Error);
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

            if (interfaceDropdown.SelectedItem == null)
            {
                MessageBox.Show("Error: Unable to send init without an interface selected", MessageBoxType.Error);
                return;
            }

            var init = SendInit(initData, initTypesButtonList.SelectedValue, intVid, intPid, interfaceDropdown.SelectedItem);
            var timeout = Task.Delay(TimeSpan.FromSeconds(5));
            var completed = await Task.WhenAny(init, timeout);
            if (completed == timeout)
            {
                timeoutAction();
            }
            else
            {
                try
                {
                    var str = await init;
                    action(str);
                }
                catch (Exception e)
                {
                    error(e);
                }
            }
        }

        private static async Task<string> SendInit(byte[] initData, InitTypes initType, int intVid, int intPid, string strInterface)
        {
            Debug.Assert(App.Driver.IsConnected, "Sending an init should not be able to be called without an active daemon");

            if (initData.Length == 0)
                throw new ArgumentException("Init length cannot be zero");

            if (initType == InitTypes.Feature)
            {
                await App.Driver.Instance.SendFeatureInit(intVid, intPid, strInterface, initData);
                Log.Debug("DeviceInitSender", "Set device feature: " + BitConverter.ToString(initData));
                return "Successfully sent feature init";
            }
            else if (initType == InitTypes.Output)
            {
                await App.Driver.Instance.SendOutputInit(intVid, intPid, strInterface, initData);
                Log.Debug("DeviceInitSender", "Set device output: " + BitConverter.ToString(initData));
                return "Successfully sent output init";
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

        private readonly DropDown<string> interfaceDropdown = new()
        {
            Width = NUMERICBOX_WIDTH
        };

        private readonly TextBox initValueText = new()
        {
            Width = NUMERICBOX_WIDTH
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
