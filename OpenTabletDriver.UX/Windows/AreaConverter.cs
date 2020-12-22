using System;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Tablet;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Windows
{
    class AreaConverter : Form
    {
        public AreaConverter()
        {
            Title = "Area Converter";

            typeSelector = new RadioButtonList
            {
                Orientation = Orientation.Vertical,
                Items =
                {
                    new ListItem
                    {
                        Key = "wacom",
                        Text = "Wacom/Veikk (raw coordinates)"
                    },
                    new ListItem
                    {
                        Key = "huion",
                        Text = "Huion/Gaomon (normalized)"
                    },
                    new ListItem
                    {
                        Key = "xppen",
                        Text = "XP-Pen (wierd units)"
                    }
                }
            };

            var boxWidth = 150;

            topBox = new InputBox("Top",
                () => top.ToString(),
                (o) => float.TryParse(o, out top),
                textboxWidth: boxWidth
            );
            bottomBox = new InputBox("Bottom",
                () => top.ToString(),
                (o) => float.TryParse(o, out bottom),
                textboxWidth: boxWidth
            );
            leftBox = new InputBox("Left",
                () => top.ToString(),
                (o) => float.TryParse(o, out left),
                textboxWidth: boxWidth
            );
            rightBox = new InputBox("Right",
                () => top.ToString(),
                (o) => float.TryParse(o, out right),
                textboxWidth: boxWidth
            );

            var table = new TableLayout
            {
                Rows =
                {
                    new TableRow
                    {
                        Cells =
                        {
                            topBox,
                            bottomBox
                        },
                        ScaleHeight = true
                    },
                    new TableRow
                    {
                        Cells =
                        {
                            leftBox,
                            rightBox
                        },
                        ScaleHeight = true
                    },
                }
            };

            Content = new StackLayout
            {
                Orientation = Orientation.Vertical,
                Items =
                {
                    new Group
                    {
                        Text = "Type",
                        Content = typeSelector,
                        ToolTip = $"Type of the area to convert",
                        Orientation = Orientation.Horizontal
                    },
                    table,
                    new Button(SetArea)
                    {
                        Text = "Apply"
                    }
                }
            };

            SetDefaultType();
        }

        private RadioButtonList typeSelector;
        private float top, bottom, left, right;
        private InputBox topBox, bottomBox, leftBox, rightBox;
        private async void SetArea(object sender, EventArgs e)
        {
            float x, y, offsetX, offsetY;
            var digitizer = (await App.Driver.Instance.GetTablet())?.Digitizer;
            if (digitizer is null)
            {
                MessageBox.Show("No tablet detected!", MessageBoxType.Warning);
                this.Close();
                return;
            }
            float lpmm = digitizer.MaxX / digitizer.Width;

            switch (typeSelector.SelectedKey)
            {
            case "wacom":
                x = (right - left) / lpmm;
                y = (bottom - top) / lpmm;
                offsetX = (x / 2) + (left / lpmm);
                offsetY = (y / 2) + (top / lpmm);
                break;
            case "huion":
                x = (right - left) * digitizer.Width;
                y = (bottom - top) * digitizer.Height;
                offsetX = (x / 2) + (left * digitizer.Width);
                offsetY = (y / 2) + (top * digitizer.Height);
                break;
            case "xppen":
                var xppenUnit = 3.937f;
                x = (right - left) / xppenUnit;
                y = (bottom - top) / xppenUnit;
                offsetX = (x / 2) + (left / xppenUnit);
                offsetY = (y / 2) + (top / xppenUnit);
                break;
            default:
                MessageBox.Show("No area type selected!", MessageBoxType.Warning);
                return;
            }

            App.Settings.LockAspectRatio = false;
            App.Settings.TabletWidth = x;
            App.Settings.TabletHeight = y;
            App.Settings.TabletX = offsetX;
            App.Settings.TabletY = offsetY;
        }

        private async void SetDefaultType()
        {
            var vid = (await App.Driver.Instance.GetTablet())?.Digitizer?.VendorID;
            if (vid is null)
            {
                MessageBox.Show("No tablet detected!", MessageBoxType.Warning);
                this.Close();
                return;
            }
            typeSelector.SelectedKey = vid switch
            {
                1386 => "wacom",    // Wacom
                12267 => "wacom",   // Veikk
                9580 => "huion",    // Huion and Gaomon
                10429 => "xppen",    // XP-Pen
                _=> "",
            };
        }
    }
}
