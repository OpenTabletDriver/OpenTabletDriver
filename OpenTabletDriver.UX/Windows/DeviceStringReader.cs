using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            this.ClientSize = new Size(400, 410);

            var sendRequestButton = new Button
            {
                Text = "Send Request",
            };

            sendRequestButton.Click += async (_, _) => await SendRequestWithTimeout(stringIndexText.Text,
                (s) => deviceStringText.Text = System.Text.Json.JsonEncodedText.Encode(s).ToString(),
                (e) => MessageBox.Show($"Error: {e.Message}", MessageBoxType.Error),
                () => MessageBox.Show(OperationTimedOut)
            );

            var sendRequestAllStringsButton = new Button
            {
                Text = "Dump All"
            };

            sendRequestAllStringsButton.Click += SendRequestAllStrings;

            var vendorIdCtrl = new Group("VendorID", vendorIdText, Orientation.Horizontal, false);
            var productIdCtrl = new Group("ProductID", productIdText, Orientation.Horizontal, false);
            var stringIndexCtrl = new Group("String Index", stringIndexText, Orientation.Horizontal, false);

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
                    stringIndexCtrl,
                    requireReconnect,
                    requestDangerous,
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

        private void SetDeviceDropDownDataStore()
        {
            Debug.Assert(App.Driver.IsConnected, "Tried setting device dropdown without an active daemon");

            this.deviceDropDown.DataStore =
                App.Driver.Instance.GetDevices().Result
                    .Where(x => x.CanOpen)
                    .DistinctBy(x => new { x.VendorID, x.ProductID });
        }

        private const int NUMERICBOX_WIDTH = 150;
        private const string DecimalStyle = "Decimal Value";
        private const string StringIndex = "Index";
        private const string RequestTabletReplug = "Please replug the tablet, and then press OK to continue";
        private const string DisconnectionIndex = "Device disconnected";
        private const string OperationTimedOut = "Operation timed-out";
        private const string OperationFailed = "Operation failed";
        private static Dictionary<int, string> GetDangerousStrings(int vendorId) => vendorId switch
        {
            // XP-Pen, UGEE, and XenceLabs
            10429 => new()
            {
                [200] = "Common DFU String"
            },
            _ => [],
        };

        private async void SendRequestAllStrings(object? sender, EventArgs args)
        {
            if (!App.Driver.IsConnected)
            {
                MessageBox.Show("Unable to send request without an active daemon", MessageBoxType.Error);
                return;
            }

            var validVid = int.TryParse(vendorIdText.Text, out var vid);
            var validPid = int.TryParse(productIdText.Text, out var pid);
            var matchingDeviceFound = (await App.Driver.Instance.GetDevices()).Any(x => x.ProductID == pid && x.VendorID == vid);
            // ensure requested device exists/is found
            if (!validVid || !validPid || !matchingDeviceFound)
            {
                MessageBox.Show($"Error: Device not found", MessageBoxType.Error);
                return;
            }

            var requestDangerousStrings = requestDangerous.Checked.HasValue && (bool)requestDangerous.Checked;
            var dangerousStrings = requestDangerousStrings ? [] : GetDangerousStrings(vid);

            var stringDump = new StringBuilder();

            for (int i = 1; i < 256; i++)
            {
                if (dangerousStrings.ContainsKey(i))
                {
                    stringDump.AppendLine($"{StringIndex} {i}: {{ OTD: {dangerousStrings[i]} }}");
                    continue;
                }

                bool shouldRead = true;
                await SendRequestWithTimeout($"{i}",
                    (str) => stringDump.AppendLine($"{StringIndex} {i}: {str}"),
                    (e) =>
                    {
                        if (requireReconnect.Checked.HasValue && (bool)requireReconnect.Checked)
                        {
                            shouldRead = AskReconnection(stringDump, i);
                        }
                        else
                        {
                            stringDump.AppendLine($"{StringIndex} {i}: {{ OTD: {OperationFailed} }}");
                        }
                    },
                    () => stringDump.AppendLine($"{StringIndex} {i}: {{ OTD: {OperationTimedOut} }}")
                );

                // If user pressed "Cancel" return immediately
                if (!shouldRead)
                    return;
            }

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

        private async Task SendRequestWithTimeout(string strIndex, Action<string> action, Action<Exception> error, Action timeoutAction)
        {
            if (!App.Driver.IsConnected)
            {
                MessageBox.Show("Unable to send request without an active daemon", MessageBoxType.Error);
                return;
            }

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

        private static async Task<string> SendRequest(string strIndex, string strVid, string strPid)
        {
            Debug.Assert(App.Driver.IsConnected, "Sending a request should not be able to be called without an active daemon");
            if (int.TryParse(strIndex, out var index) && index < 256 && index > 0)
            {
                if (int.TryParse(strVid, out var vid) && int.TryParse(strPid, out var pid))
                    return await App.Driver.Instance.RequestDeviceString(vid, pid, index);
            }
            throw new ArgumentException("Invalid index");
        }

        private static bool AskReconnection(StringBuilder stringDump, int i)
        {
            stringDump.AppendLine($"{StringIndex} {i}: {{ OTD: {DisconnectionIndex} }}");
            var result = MessageBox.Show(RequestTabletReplug, MessageBoxButtons.OKCancel);
            return result == DialogResult.Ok;
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

        private readonly NumericMaskedTextBox<ushort> stringIndexText = new()
        {
            PlaceholderText = "[1..255]",
            Width = NUMERICBOX_WIDTH
        };

        private readonly TextBox deviceStringText = new()
        {
            PlaceholderText = "Device String",
            ReadOnly = true
        };

        private readonly CheckBox requireReconnect = new()
        {
            Text = "Require reconnect on fail",
            Checked = false,
            ToolTip = "Pauses string dump with a pop-up box if any string dump errors occur",
        };

        private readonly CheckBox requestDangerous = new()
        {
            Text = "Dump potentially dangerous strings",
            Checked = false,
            ToolTip = "Requests all strings in a string dump even if they are known to likely damage or disrupt usage of the tablet",
        };
    }
}
