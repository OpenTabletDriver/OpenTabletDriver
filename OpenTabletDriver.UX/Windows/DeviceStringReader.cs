using System;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.UX.Controls.Generic;

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

            this.vendorIdText = new NumericMaskedTextBox<int>
            {
                PlaceholderText = "Decimal Representation"
            };
            this.productIdText = new NumericMaskedTextBox<int>
            {
                PlaceholderText = "Decimal Representation"
            };
            this.stringIndexText = new NumericMaskedTextBox<int>
            {
                PlaceholderText = "[1..255]"
            };
            this.deviceStringText = new TextBox
            {
                PlaceholderText = "Device String",
                ReadOnly = true
            };

            this.vendorIdCtrl = new Group("VendorID", vendorIdText);
            this.productIdCtrl = new Group("ProductID", productIdText);
            this.stringIndexCtrl = new Group("String Index", stringIndexText);

            var deviceInfoInput = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem(vendorIdCtrl, true),
                    new StackLayoutItem(productIdCtrl, true)
                }
            };

            this.Content = new StackLayout
            {
                Padding = 5,
                Spacing = 5,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Items =
                {
                    new StackLayoutItem(deviceInfoInput),
                    new StackLayoutItem(stringIndexCtrl),
                    new StackLayoutItem(sendRequestButton),
                    new StackLayoutItem(),
                    new StackLayoutItem(deviceStringText, true)
                }
            };
        }

        private async void SendRequestWithTimeout(object sender, EventArgs args)
        {
            var strIndex = stringIndexText.Text;
            var strVid = vendorIdText.Text;
            var strPid = productIdText.Text;
            var request = Task.Run(() => SendRequest(strIndex, strVid, strPid));
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

        private Task<string> SendRequest(string strIndex, string strVid, string strPid)
        {
            if (int.TryParse(strIndex, out var index))
            {
                if (int.TryParse(strVid, out var vid) &&
                    int.TryParse(strPid, out var pid))
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
        private readonly NumericMaskedTextBox<int> vendorIdText, productIdText, stringIndexText;
        private readonly TextBox deviceStringText;
        private readonly Group vendorIdCtrl, productIdCtrl, stringIndexCtrl;
    }
}