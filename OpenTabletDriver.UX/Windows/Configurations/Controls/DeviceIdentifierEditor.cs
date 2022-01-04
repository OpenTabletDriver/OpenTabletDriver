using System;
using System.Collections.Generic;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Controls.Generic.Dictionary;
using OpenTabletDriver.UX.Controls.Generic.Reflection;
using OpenTabletDriver.UX.Controls.Generic.Text;

namespace OpenTabletDriver.UX.Windows.Configurations.Controls
{
    public class DeviceIdentifierEditor : ModifiableConstructableItemList<DeviceIdentifier>
    {
        protected override Control CreateControl(int index, DirectBinding<DeviceIdentifier> itemBinding)
        {
            var entry = new DeviceIdentifierEntry<DeviceIdentifier>();
            entry.EntryBinding.Bind(itemBinding);

            return new Panel
            {
                Padding = 5,
                Content = new Expander
                {
                    Header = $"Identifier {index + 1}",
                    Content = entry
                }
            };
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
                        new Expander
                        {
                            Header = "Feature Initialization Report",
                            Padding = 5,
                            Content = featureInitReport = new ReportEditor()
                        },
                        new Expander
                        {
                            Header = "Output Initialization Report",
                            Padding = 5,
                            Content = outputInitReport = new ReportEditor()
                        },
                        new Expander
                        {
                            Header = "Device Strings",
                            Padding = 5,
                            Content = deviceStrings = new DeviceStringEditor()
                        },
                        new Expander
                        {
                            Header = "Initialization String Indexes",
                            Padding = 5,
                            Content = initializationStrings = new ByteListEditor()
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

                featureInitReport.ItemSourceBinding.Bind(EntryBinding.Child<IList<byte[]>>(e => e.FeatureInitReport));
                outputInitReport.ItemSourceBinding.Bind(EntryBinding.Child<IList<byte[]>>(e => e.OutputInitReport));
                deviceStrings.ItemSourceBinding.Bind(EntryBinding.Child<IDictionary<byte, string>>(e => e.DeviceStrings));
                initializationStrings.ItemSourceBinding.Bind(EntryBinding.Child<IList<byte>>(e => e.InitializationStrings));
            }

            protected StackLayout layout;

            private HexNumberBox vendorId, productId;
            private UnsignedIntegerNumberBox inputReportLength, outputReportLength;
            private ReportEditor featureInitReport, outputInitReport;
            private TypeDropDown<IReportParser<IDeviceReport>> reportParser;
            private DeviceStringEditor deviceStrings;
            private ByteListEditor initializationStrings;

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

            private class ReportEditor : ModifiableItemList<byte[]>
            {
                protected override void AddNew() => Add(ItemSource.Count, new byte[0]);

                protected override Control CreateControl(int index, DirectBinding<byte[]> itemBinding)
                {
                    MaskedTextBox<byte[]> arrayEditor = new HexByteArrayBox();
                    arrayEditor.ValueBinding.Bind(itemBinding);

                    return new Panel
                    {
                        Padding = new Padding(0, 0, 5, 0),
                        Content = arrayEditor
                    };
                }
            }

            private class DeviceStringEditor : DictionaryEditor<byte, string>
            {
                protected override Control CreateControl(DirectBinding<byte> keyBinding, DirectBinding<string> valueBinding)
                {
                    var keyBox = new IntegerNumberBox();
                    keyBox.ValueBinding.Bind(keyBinding.Convert(
                        b => (int)b,
                        i => (byte)i
                    ));
                    keyBox.TextChanging += (sender, e) => e.Cancel = byte.TryParse(e.NewText, out byte newByte) && ItemSource.Keys.Contains(newByte);

                    var valueBox = new TextBox();
                    valueBox.TextBinding.Bind(valueBinding);

                    return new StackLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Padding = new Padding(0, 0, 5, 0),
                        Spacing = 5,
                        Items =
                        {
                            keyBox,
                            new StackLayoutItem(valueBox, true)
                        }
                    };
                }

                protected override void AddNew() => Add(0, string.Empty);
            }

            private class ByteListEditor : ModifiableConstructableItemList<byte>
            {
                protected override Control CreateControl(int index, DirectBinding<byte> itemBinding)
                {
                    MaskedTextBox<int> intBox = new IntegerNumberBox();
                    intBox.ValueBinding.Bind(
                        itemBinding.Convert<int>(
                            c => (int)c,
                            v => (byte)v
                        )
                    );

                    return new Panel
                    {
                        Padding = new Padding(0, 0, 5, 0),
                        Content = intBox
                    };
                }
            }
        }
    }
}
