using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Forms;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Controls.Generic.Text;

namespace OpenTabletDriver.UX.Windows
{
    public class AreaConverterDialog : Dialog
    {
        public AreaConverterDialog()
        {
            base.Title = "Area Converter";

            App.Driver.Instance.TabletsChanged += (sender, newStates) => Application.Instance.AsyncInvoke(() => SelectConverterForTablets(newStates));
            converterList.SelectedIndexChanged += (sender, e) => OnSelectionChanged();
        }

        private readonly TypeDropDown<IAreaConverter> converterList = new TypeDropDown<IAreaConverter>();
        private Group topGroup, leftGroup, bottomGroup, rightGroup;
        private FloatNumberBox top, left, bottom, right;
        private Button applyButton;
        private TabletReference tabletState;

        protected void OnSelectionChanged()
        {
            var converter = converterList.ConstructSelectedType();
            if (converter != null)
            {
                topGroup.Text = converter.Top;
                leftGroup.Text = converter.Left;
                bottomGroup.Text = converter.Bottom;
                rightGroup.Text = converter.Right;
                applyButton.Enabled = true;
            }
            else
            {
                topGroup.Text = string.Empty;
                leftGroup.Text = string.Empty;
                bottomGroup.Text = string.Empty;
                rightGroup.Text = string.Empty;
                applyButton.Enabled = false;
            }
        }

        protected override async void OnLoadComplete(EventArgs e)
        {
            base.OnLoadComplete(e);

            topGroup = new Group
            {
                Content = top = new FloatNumberBox()
                {
                    PlaceholderText = "0"
                }
            };
            leftGroup = new Group
            {
                Content = left = new FloatNumberBox()
                {
                    PlaceholderText = "0"
                }
            };
            bottomGroup = new Group
            {
                Content = bottom = new FloatNumberBox()
                {
                    PlaceholderText = "0"
                }
            };
            rightGroup = new Group
            {
                Content = right = new FloatNumberBox()
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

            var tablets = await App.Driver.Instance.GetTablets();
            SelectConverterForTablets(tablets);
        }

        protected void ConvertArea()
        {
            if (tabletState == null)
            {
                MessageBox.Show("No tablet detected. Unable to convert area.", MessageBoxType.Error);
                return;
            }

            var converter = this.converterList.ConstructSelectedType();
            var convertedArea = converter.Convert(tabletState, top.Value, left.Value, bottom.Value, right.Value);

            (this.DataContext as Profile).AbsoluteModeSettings.Tablet.Area = convertedArea;
            this.Close();
        }

        private void SelectConverterForTablets(IEnumerable<TabletReference> tablets)
        {
            var profile = (this.DataContext as Profile);
            tabletState = tablets.FirstOrDefault(t => t.Properties.Name == profile.Tablet);

            if (tabletState.Identifiers?.FirstOrDefault()?.VendorID is int vendorId)
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
