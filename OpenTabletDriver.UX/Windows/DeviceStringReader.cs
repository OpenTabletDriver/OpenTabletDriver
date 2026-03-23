using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Plugin.Devices;
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
            this.ClientSize = new Size(-1, 320);

            var sendRequestButton = new Button
            {
                Text = "Send Request",
            };

            sendRequestButton.Click += async (_, _) =>
            {
                if (!int.TryParse(vendorIdText.Text, out var vid) ||
                    !int.TryParse(productIdText.Text, out var pid) ||
                    !int.TryParse(stringIndexText.Text, out var index) ||
                    index < 1 || index > 255)
                {
                    deviceStringText.Text = "Invalid input: enter a VendorID, ProductID, and string index between 1 and 255";
                    return;
                }

                try
                {
                    var str = await App.Driver.Instance.RequestDeviceString(vid, pid, index);
                    deviceStringText.Text = !string.IsNullOrEmpty(str)
                        ? System.Text.Json.JsonEncodedText.Encode(str).ToString()
                        : "(no value at this index)";
                }
                catch (Exception ex)
                {
                    deviceStringText.Text = $"Error: {ex.Message}";
                }
            };

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
                    new Group("Connected HIDs", deviceDropDown, Orientation.Horizontal, false),
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

            this.KeyDown += (_, args) =>
            {
                if (args.Key == Keys.Escape)
                    this.Close();
            };

            deviceDropDown.SelectedItemBinding
                .Convert(x => x?.VendorID.ToString())
                .Bind(vendorIdText.TextBinding);

            deviceDropDown.SelectedItemBinding
                .Convert(x => x?.ProductID.ToString())
                .Bind(productIdText.TextBinding);

            this.deviceDropDown.DataStore =
                App.Driver.Instance.GetDevices().Result
                    .Where(x => x.CanOpen)
                    .DistinctBy(x => new { x.VendorID, x.ProductID });

            this.deviceDropDown.ItemTextBinding = Binding.Delegate<SerializedDeviceEndpoint, string>(x =>
            {
                // don't include manufacturer if it's already in the product name (e.g. Razer)
                string name = x.ProductName ?? x.FriendlyName;
                string title = name.Contains(x.Manufacturer) ? name : $"{x.Manufacturer} {name}";

                if (title.Length >= 32) // truncate if too long, used in GUI
                    title = title[..29] + "...";

                // hex value preferable (but not consistent.. yet?)
                return $"[0x{x.VendorID:x4} 0x{x.ProductID:x4}]{Environment.NewLine}{title}";
            });
        }

        private const int NUMERICBOX_WIDTH = 150;
        private const string DecimalStyle = "Decimal Value";
        private const string StringIndex = "Index";

        private async void SendRequestAllStrings(object sender, EventArgs args)
        {
            var validVid = int.TryParse(vendorIdText.Text, out var vid);
            var validPid = int.TryParse(productIdText.Text, out var pid);
            var matchingDeviceFound = (await App.Driver.Instance.GetDevices()).Any(x => x.ProductID == pid && x.VendorID == vid);
            // ensure requested device exists/is found
            if (!validVid || !validPid || !matchingDeviceFound)
            {
                deviceStringText.Text = "Error: Device not found";
                return;
            }

            var strings = (await App.Driver.Instance.RequestDeviceStrings(vid, pid)).ToList();
            var stringDump = new StringBuilder();

            for (int i = 0; i < strings.Count; i++)
                stringDump.AppendLine($"{StringIndex} {i + 1}: {strings[i]}");

            var fileDialog = Extensions.SaveFileDialog(
                "Save string dump to...",
                Eto.EtoEnvironment.GetFolderPath(Eto.EtoSpecialFolder.Documents),
                [new FileFilter("String dump", ".txt")],
                $"string-dump_{vendorIdText.Text}-{productIdText.Text}.txt"
            );

            switch (fileDialog.ShowDialog(this))
            {
                case DialogResult.Ok:
                case DialogResult.Yes:
                    var file = new FileInfo(fileDialog.FileName);
                    if (file.Exists)
                        file.Delete();

                    await using (var fs = file.OpenWrite())
                    await using (var sw = new StreamWriter(fs))
                        await sw.WriteAsync(stringDump);
                    break;
            }
        }

        private readonly DropDown<SerializedDeviceEndpoint> deviceDropDown = new();
        private readonly NumericMaskedTextBox<ushort> vendorIdText, productIdText, stringIndexText;
        private readonly TextBox deviceStringText;
        private readonly Group vendorIdCtrl, productIdCtrl, stringIndexCtrl;
    }
}
