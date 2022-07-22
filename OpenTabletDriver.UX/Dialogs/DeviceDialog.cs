using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Security.Cryptography;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Devices;
using OpenTabletDriver.UX.Components;

namespace OpenTabletDriver.UX.Dialogs
{
    public sealed class DeviceDialog : Dialog<IDeviceEndpoint>
    {
        private readonly IDriverDaemon _daemon;
        private readonly ListBox<IDeviceEndpoint> _list;

        public DeviceDialog(IDriverDaemon daemon)
        {
            _daemon = daemon;

            _list = new ListBox<IDeviceEndpoint>();
            _list.ItemTextBinding = Binding.Property<IDeviceEndpoint, string>(e => $"{e.Manufacturer} {e.FriendlyName ?? e.ProductName}");
            Refresh().Run();

            Title = "Select a device...";
            Width = 600;

            var binding = new BindableBinding<DeviceDialog, object?>(
                this,
                c => c.DataContext,
                (c, v) => c.DataContext = v,
                (d, h) => d.DataContextChanged += h,
                (d, h) => d.DataContextChanged -= h
            );
            binding.Bind(_list.SelectedValueBinding);

            var specs = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Padding = 5,
                Spacing = 5,
                Items =
                {
                    LabelFor(e => e.Manufacturer),
                    LabelFor(e => e.ProductName),
                    HexLabelFor(e => e.VendorID),
                    HexLabelFor(e => e.ProductID),
                    LabelFor(e => e.InputReportLength),
                    LabelFor(e => e.OutputReportLength),
                    LabelFor(e => e.SerialNumber),
                    LabelFor(e => e.DevicePath)
                }
            };

            var splitter = new Splitter
            {
                Orientation = Orientation.Vertical,
                Panel1MinimumSize = 200,
                Panel2MinimumSize = 200,
                Panel1 = new Scrollable
                {
                    Border = BorderType.None,
                    Content = _list
                },
                Panel2 = specs
            };

            Content = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Padding = 5,
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem(splitter, true),
                    new StackLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 5,
                        Items =
                        {
                            new StackLayoutItem(null, true),
                            new Button((_, _) => Close())
                            {
                                Text = "Cancel"
                            },
                            new Button((_, _) => Close(_list.SelectedItem))
                            {
                                Text = "Ok"
                            }
                        }
                    }
                }
            };
        }

        private async Task Refresh()
        {
            var query = from device in await _daemon.GetDevices()
                        orderby device.Manufacturer, device.ProductName, device.ProductID, device.InputReportLength, device.OutputReportLength
                        select device;

            var devices = query.ToImmutableArray();

            _list.Source = devices;
            _list.SelectedIndex = devices.Any() ? 0 : -1;
        }

        private static Control LabelFor(Expression<Func<IDeviceEndpoint, object?>> expression)
        {
            var label = new Label
            {
                TextAlignment = TextAlignment.Right,
                Font = Fonts.Monospace(10),
                Wrap = WrapMode.Character
            };

            label.TextBinding.Convert<object?>(s => s, o => o?.ToString()).BindDataContext(expression);
            return new LabeledGroup(expression.GetFriendlyName(), label);
        }

        private static Control HexLabelFor(Expression<Func<IDeviceEndpoint, int>> expression)
        {
            var label = new Label
            {
                TextAlignment = TextAlignment.Right,
                Font = Fonts.Monospace(10)
            };

            label.TextBinding.Convert(int.Parse, v => $"({v}) 0x{v:x4}").BindDataContext(expression);
            return new LabeledGroup(expression.GetFriendlyName(), label);
        }
    }
}
