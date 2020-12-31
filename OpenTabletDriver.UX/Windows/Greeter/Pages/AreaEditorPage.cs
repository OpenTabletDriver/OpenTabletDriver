using System;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.UX.Attributes;
using OpenTabletDriver.UX.Controls;
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
                        Text = "Preview",
                        Content = new DemoAreaEditor("mm", true)
                    }
                },
                "You can right click the absolute output mode area editor for more options.",
                "Aligning, resizing, and flipping your area can be done within this context menu.",
                "Other options such as locking aspect ratio, locking input inside of the usable area are also found here.",
            };
        }

        private class DemoAreaEditor : AreaEditor
        {
            public DemoAreaEditor(string unit, bool enableRotation = false)
                : base(unit, enableRotation)
            {
                base.AppendCheckBoxMenuItem(
                    "Lock to usable area",
                    value => base.ChangeLockingState(value),
                    true
                );
            }

            protected override void OnLoadComplete(EventArgs e)
            {
                base.OnLoadComplete(e);

                base.ViewModel.Width = 75;
                base.ViewModel.Height = 75;
                base.ViewModel.X = 75;
                base.ViewModel.Y = 75;
                base.ViewModel.Rotation = 15;
                base.SetBackground(new RectangleF(0, 0, 150, 150));
            }
        }
    }
}