using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Windows
{
    public class DeviceStringReader : DesktopForm
    {
        public DeviceStringReader()
            : base(Application.Instance.MainForm)
        {
            this.Title = "Device String Reader";
            this.Icon = App.Logo.WithSize(App.Logo.Size);
            this.ClientSize = new Size(300, 250);

            var sendRequestButton = new Button
            {
                Text = "Send Request",
            };

            sendRequestButton.Click += async (_, _) => await SendRequestWithTimeout(stringIndexText.Text,
                (s) => deviceStringText.Text = s,
                (e) => MessageBox.Show($"Error: {e.Message}", MessageBoxType.Error),
                () => MessageBox.Show(OperationTimedOut)
            );

            var sendRequestAllStringsButton = new Button
            {
                Text = "Dump All"
            };

            sendRequestAllStringsButton.Click += SendRequestAllStrings;

            this.vendorIdText = new NumericMaskedTextBox<ushort>
            {
                PlaceholderText = DecimalStyle,
                Width = NUMERICBOX_WIDTH
            };
            this.productIdText = new NumericMaskedTextBox<ushort>
            {
                PlaceholderText = DecimalStyle,
                Width = NUMERICBOX_WIDTH
            };
            this.stringIndexText = new NumericMaskedTextBox<ushort>
            {
                PlaceholderText = "[1..255]",
                Width = NUMERICBOX_WIDTH
            };
            this.deviceStringText = new TextBox
            {
                PlaceholderText = "Device String",
                ReadOnly = true
            };

            this.vendorIdCtrl = new Group("VendorID", vendorIdText, Orientation.Horizontal, false);
            this.productIdCtrl = new Group("ProductID", productIdText, Orientation.Horizontal, false);
            this.stringIndexCtrl = new Group("String Index", stringIndexText, Orientation.Horizontal, false);

            this.Content = new StackLayout
            {
                Padding = 5,
                Spacing = 5,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Items =
                {
                    vendorIdCtrl,
                    productIdCtrl,
                    stringIndexCtrl,
                    new StackLayoutItem(
                        new StackLayout
                        {
                            Orientation = Orientation.Horizontal,
                            Items =
                            {
                                new Panel { Content = sendRequestButton, Padding = new Padding(5, 5) },
                                new Panel { Content = sendRequestAllStringsButton, Padding = new Padding(5, 5) }
                            }
                        },
                        HorizontalAlignment.Center
                    ),
                    new StackLayoutItem(deviceStringText, true)
                }
            };
        }

        private const int NUMERICBOX_WIDTH = 150;
        private const string DecimalStyle = "Decimal Value";
        private const string StringIndex = "Index";
        private const string RequestTabletReplug = "Please replug the tablet, and then press OK to continue";
        private const string DisconnectionIndex = "Device disconnected";
        private const string OperationTimedOut = "Operation timed-out";

        private async void SendRequestAllStrings(object sender, EventArgs args)
        {
            var stringDump = new StringBuilder();

            for (int i = 1; i < 256; i++)
            {
                bool shouldRead = true;
                await SendRequestWithTimeout($"{i}",
                    (str) => stringDump.AppendLine($"{StringIndex} {i}: {str}"),
                    (e) => shouldRead = AskReconnection(stringDump, i),
                    () => stringDump.AppendLine($"{StringIndex} {i}: {{ OTD: {OperationTimedOut} }}")
                );

                // If user pressed "Cancel" return immediately
                if (!shouldRead)
                    return;
            }

            var fileDialog = new SaveFileDialog
            {
                Title = "Save string dump to...",
                Directory = new Uri(Eto.EtoEnvironment.GetFolderPath(Eto.EtoSpecialFolder.Documents)),
                Filters =
                {
                    new FileFilter("String dump", ".txt")
                }
            };

            switch (fileDialog.ShowDialog(this))
            {
                case DialogResult.Ok:
                case DialogResult.Yes:
                    var file = new FileInfo(fileDialog.FileName);
                    if (file.Exists)
                        file.Delete();
                    using (var fs = file.OpenWrite())
                    using (var sw = new StreamWriter(fs))
                        await sw.WriteAsync(stringDump);
                    break;
            }
        }

        private async Task SendRequestWithTimeout(string strIndex, Action<string> action, Action<Exception> error, Action timeoutAction)
        {
            var strVid = vendorIdText.Text;
            var strPid = productIdText.Text;
            var request = SendRequest(strIndex, strVid, strPid);
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

        private async Task<string> SendRequest(string strIndex, string strVid, string strPid)
        {
            if (int.TryParse(strIndex, out var index) && index < 256 && index > 0)
            {
                if (int.TryParse(strVid, out var vid) && int.TryParse(strPid, out var pid))
                    return await App.Driver.Instance.RequestDeviceString(vid, pid, index);
            }
            throw new ArgumentException("Invalid index");
        }

        private bool AskReconnection(StringBuilder stringDump, int i)
        {
            stringDump.AppendLine($"{StringIndex} {i}: {{ OTD: {DisconnectionIndex} }}");
            var result = MessageBox.Show(RequestTabletReplug, MessageBoxButtons.OKCancel);
            return result == DialogResult.Ok;
        }

        private readonly NumericMaskedTextBox<ushort> vendorIdText, productIdText, stringIndexText;
        private readonly TextBox deviceStringText;
        private readonly Group vendorIdCtrl, productIdCtrl, stringIndexCtrl;
    }
}
