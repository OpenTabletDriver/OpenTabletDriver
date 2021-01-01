using System.Linq;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Windows
{
    public class AreaConverterDialog : Dialog
    {
        public AreaConverterDialog()
        {
            base.Title = "Area Converter";

            App.Driver.Instance.TabletChanged += (sender, newState) => SelectConverterForTablet(newState);

            _ = Refresh();
            converterList.SelectedIndexChanged += (_, _) => RefreshLabel();
        }

        private readonly TypeDropDown<IAreaConverter> converterList = new TypeDropDown<IAreaConverter>();
        private readonly Group[] groups = new Group[4];
        private readonly NumericMaskedTextBox<float>[] numberBoxes = new NumericMaskedTextBox<float>[4];

        private TabletState tabletState;

        protected void RefreshLabel()
        {
            var converter = converterList.ConstructSelectedType();
            foreach (var (group, text) in groups.Zip(converter.Label, (group, text) => (group, text)))
                group.Text = text;
        }

        protected async Task Refresh()
        {
            for (int i = 0; i < groups.Length; i++)
            {
                var numberBox = new NumericMaskedTextBox<float>
                {
                    PlaceholderText = "0"
                };
                var group = new Group
                {
                    Orientation = Orientation.Horizontal,
                    Content = numberBox
                };

                groups[i] = group;
                numberBoxes[i] = numberBox;
            }

            this.Content = new StackLayout
            {
                Orientation = Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Padding = 5,
                Spacing = 5,
                Items =
                {
                    new Group
                    {
                        Text = "Converter",
                        Content = converterList,
                        Orientation = Orientation.Horizontal
                    },
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = new StackLayout
                        {
                            Orientation = Orientation.Horizontal,
                            Spacing = 5,
                            Items =
                            {
                                new StackLayoutItem
                                {
                                    Expand = true,
                                    Control = new StackLayout
                                    {
                                        HorizontalContentAlignment = HorizontalAlignment.Stretch,
                                        Spacing = 5,
                                        Items =
                                        {
                                            groups[0],
                                            groups[1]
                                        }
                                    }
                                },
                                new StackLayoutItem
                                {
                                    Expand = true,
                                    Control = new StackLayout
                                    {
                                        HorizontalContentAlignment = HorizontalAlignment.Stretch,
                                        Spacing = 5,
                                        Items =
                                        {
                                            groups[2],
                                            groups[3]
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new StackLayoutItem(null, true),
                    new StackLayoutItem
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Control = new Button((sender, e) => ConvertArea())
                        {
                            Text = "Apply"
                        }
                    }
                }
            };

            var tablet = await App.Driver.Instance.GetTablet();
            SelectConverterForTablet(tablet);
        }

        protected void ConvertArea()
        {
            var digitizer = tabletState?.Digitizer;
            if (digitizer == null)
            {
                MessageBox.Show("No tablet detected. Unable to convert area.", MessageBoxType.Error);
                return;
            }

            var converter = this.converterList.ConstructSelectedType();
            var convertedArea = converter.Convert(tabletState, numberBoxes[0].Value, numberBoxes[1].Value, numberBoxes[2].Value, numberBoxes[3].Value);

            App.Settings.SetTabletArea(convertedArea);

            (Application.Instance.MainForm as MainForm).Refresh();
            this.Close();
        }

        private void SelectConverterForTablet(TabletState tablet)
        {
            tabletState = tablet;

            var vendorId = tabletState?.Digitizer?.VendorID;
            if (vendorId != null)
            {
                var vendor = (DeviceVendor)vendorId;
                converterList.Select(t => t.Vendor.HasFlag(vendor));
            }
            else
            {
                // Deselect if no tablet is detected
                converterList.SelectedIndex = -1;
            }
        }
    }
}
