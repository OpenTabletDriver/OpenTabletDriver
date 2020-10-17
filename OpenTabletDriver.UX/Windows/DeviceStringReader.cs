using System;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;

namespace OpenTabletDriver.UX.Windows
{
    public class DeviceStringReader : Form
    {
        public DeviceStringReader()
        {
            this.Title = "Device String Reader";
            this.Icon = App.Logo.WithSize(App.Logo.Size);
            this.Size = new Size(600, -1);

            this.sendRequestButton = new Button
            {
                Text = "Send Request"
            };
            sendRequestButton.Click += SendRequestWithTimeout;

            this.vendorIdText = new TextBox
            {
                PlaceholderText = "Decimal Representation"
            };
            this.productIdText = new TextBox
            {
                PlaceholderText = "Decimal Representation"
            };
            this.stringIndexText = new TextBox
            {
                PlaceholderText = "[1..255]"
            };
            this.deviceStringText = new TextBox
            {
                PlaceholderText = "Device String",
                ReadOnly = true
            };

            static void restrictToNumbers(object sender, TextChangingEventArgs args)
            {
                if (!int.TryParse(args.NewText, out int result))
                {
                    args.Cancel = true;
                }
            }

            this.vendorIdCtrl = new GroupBox
            {
                Text = "VendorID",
                Padding = App.GroupBoxPadding,
                Content = vendorIdText
            };

            vendorIdText.TextChanging += restrictToNumbers;

            this.productIdCtrl = new GroupBox
            {
                Text = "ProductID",
                Padding = App.GroupBoxPadding,
                Content = productIdText
            };

            productIdText.TextChanging += restrictToNumbers;

            this.stringIndexCtrl = new GroupBox
            {
                Text = "String Index",
                Padding = App.GroupBoxPadding,
                Content = stringIndexText
            };

            stringIndexText.TextChanging += restrictToNumbers;

            var deviceInfoInput = new TableLayout
            {
                Spacing = new Size(5, 5),
                Rows =
                {
                    new TableRow
                    {
                        Cells =
                        {
                            new TableCell(vendorIdCtrl, true),
                            new TableCell(productIdCtrl, true)
                        }
                    }
                }
            };

            this.Content = new StackLayout
            {
                Padding = 5,
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem(deviceInfoInput, HorizontalAlignment.Stretch, true),
                    new StackLayoutItem(stringIndexCtrl, HorizontalAlignment.Stretch, true),
                    new StackLayoutItem(sendRequestButton, HorizontalAlignment.Stretch, true),
                    new StackLayoutItem(),
                    new StackLayoutItem(deviceStringText, HorizontalAlignment.Stretch, true)
                }
            };
        }

        private async void SendRequestWithTimeout(object sender, EventArgs args)
        {
            var request = Task.Run(SendRequest);
            var timeout = Task.Delay(TimeSpan.FromSeconds(5));
            var completed = await Task.WhenAny(request, timeout);
            if (completed == timeout)
            {
                MessageBox.Show("Operation timed-out", MessageBoxType.Error);
                return;
            }
            else
            {
                try
                {
                    deviceStringText.Text = await request;
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Error: {e.Message}", MessageBoxType.Error);
                }
            }
        }

        private Task<string> SendRequest()
        {
            if (int.TryParse(stringIndexText.Text, out var index))
            {
                if (int.TryParse(vendorIdText.Text, out var vid) &&
                    int.TryParse(productIdText.Text, out var pid))
                {
                    return App.Driver.Instance.RequestDeviceString(vid, pid, index);
                }
                else
                {
                    return App.Driver.Instance.RequestDeviceString(index);
                }
            }
            return Task.FromResult("");
        }

        private readonly Button sendRequestButton;
        private readonly TextBox vendorIdText, productIdText, stringIndexText, deviceStringText;
        private readonly GroupBox vendorIdCtrl, productIdCtrl, stringIndexCtrl;
    }
}