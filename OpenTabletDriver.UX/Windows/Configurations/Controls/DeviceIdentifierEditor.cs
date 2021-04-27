using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Forms;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Controls.Generic.Text;

namespace OpenTabletDriver.UX.Windows.Configurations.Controls
{
    public class DeviceIdentifierEditor : CustomItemList<DeviceIdentifier>
    {
        protected override Control CreateControl(int index, DirectBinding<DeviceIdentifier> itemBinding)
        {
            var entry = new DeviceIdentifierEntry<DeviceIdentifier>();
            entry.EntryBinding.Bind(itemBinding);

            return entry;
        }

        private class DeviceIdentifierEntry<T> : Panel where T : DeviceIdentifier
        {
            public DeviceIdentifierEntry()
            {
                this.Content = layout = new StackLayout
                {
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Padding = 5,
                    Spacing = 5,
                    Items =
                    {
                        new Group
                        {
                            Text = "Vendor ID",
                            Orientation = Orientation.Horizontal,
                            Content = vendorId = new HexNumberBox()
                        },
                        new Group
                        {
                            Text = "Product ID",
                            Orientation = Orientation.Horizontal,
                            Content = productId = new HexNumberBox()
                        },
                        new Group
                        {
                            Text = "Input Report Length",
                            Orientation = Orientation.Horizontal,
                            Content = inputReportLength = new UnsignedIntegerNumberBox()
                        },
                        new Group
                        {
                            Text = "Output Report Length",
                            Orientation = Orientation.Horizontal,
                            Content = outputReportLength = new UnsignedIntegerNumberBox()
                        },
                        new Group
                        {
                            Text = "Report Parser",
                            Orientation = Orientation.Horizontal,
                            Content = reportParser = new TypeDropDown<IReportParser<IDeviceReport>>()
                        },
                        new Group
                        {
                            Text = "Feature Initialization Report",
                            Orientation = Orientation.Horizontal,
                            Content = featureInitReport = new ReportEditor()
                        },
                        new Group
                        {
                            Text = "Output Initialization Report",
                            Orientation = Orientation.Horizontal,
                            Content = outputInitReport = new ReportEditor()
                        },
                        new Group
                        {
                            Text = "Device Strings",
                            Orientation = Orientation.Horizontal,
                            Content = deviceStrings = new DeviceStringEditor()
                        },
                        new Group
                        {
                            Text = "Initialization String Indexes",
                            Orientation = Orientation.Horizontal,
                            Content = initializationStrings = new IntegerArrayBox()
                        }
                    }
                };

                vendorId.ValueBinding.Bind(EntryBinding.Child(e => e.VendorID));
                productId.ValueBinding.Bind(EntryBinding.Child(e => e.ProductID));

                inputReportLength.ValueBinding.Bind(
                    EntryBinding.Child(e => e.InputReportLength).Convert<uint>(
                        c => c ?? 0,
                        v => v
                    )
                );

                outputReportLength.ValueBinding.Bind(
                    EntryBinding.Child(e => e.OutputReportLength).Convert<uint>(
                        c => c ?? 0,
                        v => v
                    )
                );

                reportParser.SelectedKeyBinding.Bind(EntryBinding.Child(e => e.ReportParser));

                featureInitReport.ItemSourceBinding.Bind(EntryBinding.Child(e => (IList<byte[]>)e.FeatureInitReport));
                outputInitReport.ItemSourceBinding.Bind(EntryBinding.Child(e => (IList<byte[]>)e.OutputInitReport));

                deviceStrings.ItemSourceBinding.Bind(
                    EntryBinding.Child(e => e.DeviceStrings).Convert<IList<KeyValuePair<byte, string>>>(
                        s => s?.ToList(),
                        c => new Dictionary<byte, string>(c)
                    )
                );

                initializationStrings.ValueBinding.Bind(
                    EntryBinding.Child(e => e.InitializationStrings).Convert<int[]>(
                        o => o?.ConvertAll<int>(c => (int)c).ToArray(),
                        k => k.ToList().ConvertAll<byte>(c => (byte)c)
                    )
                );
            }

            protected StackLayout layout;

            private MaskedTextBox<int> vendorId, productId;
            private MaskedTextBox<uint> inputReportLength, outputReportLength;
            private ReportEditor featureInitReport, outputInitReport;
            private TypeDropDown<IReportParser<IDeviceReport>> reportParser;
            private DeviceStringEditor deviceStrings;
            private MaskedTextBox<int[]> initializationStrings;

            private T entry;
            public T Entry
            {
                set
                {
                    this.entry = value;
                    this.OnEntryChanged();
                }
                get => this.entry;
            }

            public event EventHandler<EventArgs> EntryChanged;

            protected virtual void OnEntryChanged() => EntryChanged?.Invoke(this, new EventArgs());

            public BindableBinding<DeviceIdentifierEntry<T>, T> EntryBinding
            {
                get
                {
                    return new BindableBinding<DeviceIdentifierEntry<T>, T>(
                        this,
                        c => c.Entry,
                        (c, v) => c.Entry = v,
                        (c, h) => c.EntryChanged += h,
                        (c, h) => c.EntryChanged -= h
                    );
                }
            }

            private class ReportEditor : CustomItemList<byte[]>
            {
                protected override Control CreateControl(int index, DirectBinding<byte[]> itemBinding)
                {
                    MaskedTextBox<byte[]> arrayEditor = new HexByteArrayBox();
                    arrayEditor.ValueBinding.Bind(itemBinding);
                    return arrayEditor;
                }
            }

            private class DeviceStringEditor : CustomItemList<KeyValuePair<byte, string>>
            {
                protected override Control CreateControl(int index, DirectBinding<KeyValuePair<byte, string>> itemBinding)
                {
                    MaskedTextBox<int> keyBox = new IntegerNumberBox();
                    keyBox.ValueBinding.Bind(
                        itemBinding.Child(i => i.Key).Convert<int>(
                            c => (int)c,
                            v => (byte)v
                        )
                    );

                    TextBox valueBox = new TextBox();
                    valueBox.TextBinding.Bind(itemBinding.Child(i => i.Value));

                    return new StackLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 5,
                        Items =
                        {
                            keyBox,
                            new StackLayoutItem(valueBox, true)
                        }
                    };
                }
            }
        }
    }
}