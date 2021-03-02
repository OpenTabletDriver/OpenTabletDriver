using System;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.UX.Attributes;
using OpenTabletDriver.UX.Controls.Area;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Windows.Greeter.Pages
{
    [PageName("Area Editor")]
    public class AreaEditorPage : StylizedPage
    {
        public AreaEditorPage()
        {
            this.Content = new StackedContent
            {
                new StackLayoutItem
                {
                    Expand = true,
                    Control = new Group
                    {
                        Text = "Demo Area Editor",
                        Content = new AreaEditor(new AreaViewModel
                        {
                            Width = 75,
                            Height = 75,
                            X = 75,
                            Y = 75,
                            Rotation = 15,
                            Unit = "mm",
                            EnableRotation = true,
                            Background = new RectangleF[]
                            {
                                new RectangleF(0, 0, 150, 150)
                            }
                        })
                    }
                },
                new StylizedText("This is the area editor.", SystemFonts.Bold(9), new Padding(0, 0, 0, 4)),
                "You can right click the absolute output mode area editor for more options.",
                "Aligning, resizing, and flipping your area can be done within this context menu.",
                "Other options such as locking aspect ratio, locking input inside of the usable area are also found here.",
            };
        }
    }
}
