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
            base.Size = new Size(450, 150);
            base.MinimumSize = base.Size;

            App.Driver.Instance.TabletChanged += (sender, newState) => SelectConverterForTablet(newState);

            _ = Refresh();
        }

        private TypeComboBox<IAreaConverter> converterList = new TypeComboBox<IAreaConverter>();
        private NumericMaskedTextBox<float> top = new NumericMaskedTextBox<float>(),
            bottom = new NumericMaskedTextBox<float>(),
            left = new NumericMaskedTextBox<float>(),
            right = new NumericMaskedTextBox<float>();

        private TabletState tabletState;

        protected async Task Refresh()
        {
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
                                            new Group("Top", top, Orientation.Horizontal),
                                            new Group("Left", left, Orientation.Horizontal)
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
                                            new Group("Bottom", bottom, Orientation.Horizontal),
                                            new Group("Right", right, Orientation.Horizontal)
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

            float conversionFactor = digitizer.MaxX / digitizer.Width;

            var converter = this.converterList.ConstructSelectedType();
            var convertedArea = converter.Convert(tabletState, left.Value, top.Value, right.Value, bottom.Value);

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
