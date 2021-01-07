using System.Threading.Tasks;
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
            converterList.SelectedIndexChanged += (sender, e) => Refresh();
            _ = InitializeAsync();
        }

        private readonly TypeDropDown<IAreaConverter> converterList = new TypeDropDown<IAreaConverter>();
        private Group topGroup, leftGroup, bottomGroup, rightGroup;
        private NumericMaskedTextBox<float> top, left, bottom, right;
        private Button applyButton;
        private TabletState tabletState;

        protected void Refresh()
        {
            var converter = converterList.ConstructSelectedType();
            topGroup.Text = converter.Top;
            leftGroup.Text = converter.Left;
            bottomGroup.Text = converter.Bottom;
            rightGroup.Text = converter.Right;
            applyButton.Enabled = true;
        }

        protected async Task InitializeAsync()
        {
            topGroup = new Group
            {
                Content = top = new NumericMaskedTextBox<float>
                {
                    PlaceholderText = "0"
                }
            };
            leftGroup = new Group
            {
                Content = left = new NumericMaskedTextBox<float>
                {
                    PlaceholderText = "0"
                }
            };
            bottomGroup = new Group
            {
                Content = bottom = new NumericMaskedTextBox<float>
                {
                    PlaceholderText = "0"
                }
            };
            rightGroup = new Group
            {
                Content = right = new NumericMaskedTextBox<float>
                {
                    PlaceholderText = "0"
                }
            };

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
                                            topGroup,
                                            leftGroup
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
                                            bottomGroup,
                                            rightGroup
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new StackLayoutItem
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Control = applyButton = new Button((sender, e) => ConvertArea())
                        {
                            Text = "Apply",
                            Enabled = false
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
            var convertedArea = converter.Convert(tabletState, top.Value, left.Value, bottom.Value, right.Value);

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
                applyButton.Enabled = true;
            }
            else
            {
                // Deselect if no tablet is detected
                converterList.SelectedIndex = -1;
            }
        }
    }
}
