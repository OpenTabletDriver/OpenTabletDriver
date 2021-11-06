using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Forms;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Controls.Generic.Reflection;
using OpenTabletDriver.UX.Controls.Generic.Text;

namespace OpenTabletDriver.UX.Windows
{
    public class AreaConverterDialog : ChildDialog
    {
        public AreaConverterDialog()
            : base(Application.Instance.MainForm)
        {
            base.Title = "Convert Area...";

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

            converterList.SelectedIndexChanged += (sender, e) => OnSelectionChanged();

            Application.Instance.AsyncInvoke(async () =>
            {
                var tablets = await App.Driver.Instance.GetTablets();
                var targetProfile = App.Current.Settings.Profiles.FirstOrDefault(p => p.AbsoluteModeSettings.Tablet == this.DataContext);
                var tablet = tablets.FirstOrDefault(t => t.Properties.Name == targetProfile.Tablet);
                Select(tablet);
            });
        }

        private readonly TypeDropDown<IAreaConverter> converterList = new TypeDropDown<IAreaConverter>();
        private Group topGroup, leftGroup, bottomGroup, rightGroup;
        private FloatNumberBox top, left, bottom, right;
        private Button applyButton;
        private TabletReference selectedTablet;

        protected void OnSelectionChanged()
        {
            // Simpler than binding with MVVM since .ctor() is being called.
            if (converterList.ConstructSelectedType() is IAreaConverter converter)
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

        protected void ConvertArea()
        {
            var converter = this.converterList.ConstructSelectedType();
            var convertedArea = converter.Convert(selectedTablet, top.Value, left.Value, bottom.Value, right.Value);

            (this.DataContext as AreaSettings).Area = convertedArea;
            this.Close();
        }

        private void Select(TabletReference tablet)
        {
            if (tablet.Identifiers?.FirstOrDefault()?.VendorID is int vendorId)
            {
                var vendor = (DeviceVendor)vendorId;
                converterList.Select(t => t.Vendor.HasFlag(vendor));
                applyButton.Enabled = true;
                selectedTablet = tablet;
            }
            else
            {
                // Deselect if no tablet is detected
                converterList.SelectedIndex = -1;
                selectedTablet = null;

                MessageBox.Show("No tablet detected.", MessageBoxType.Error);
                this.Close();
            }
        }
    }
}
