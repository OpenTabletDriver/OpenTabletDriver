using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.UX.Components;

namespace OpenTabletDriver.UX.Controls.Editors
{
    public class InputAreaEditor : AreaEditor
    {
        public InputAreaEditor(IControlBuilder controlBuilder)
        {
            var width = TextBoxFor(m => m.Input!.Width);
            var height = TextBoxFor(m => m.Input!.Height);
            var xPosition = TextBoxFor(m => m.Input!.XPosition);
            var yPosition = TextBoxFor(m => m.Input!.YPosition);
            var rotation = TextBoxFor(m => m.Input!.Rotation);

            var areaDisplay = controlBuilder.Build<InputAreaDisplay>();
            width.ValueChanged += (_, _) => areaDisplay.Invalidate();
            height.ValueChanged += (_, _) => areaDisplay.Invalidate();
            xPosition.ValueChanged += (_, _) => areaDisplay.Invalidate();
            yPosition.ValueChanged += (_, _) => areaDisplay.Invalidate();
            rotation.ValueChanged += (_, _) => areaDisplay.Invalidate();

            PointF? prevPos = null;
            void AreaMouseHandler(object? _, MouseEventArgs e)
            {
                if ((e.Buttons & MouseButtons.Primary) != 0)
                {
                    if (prevPos == null)
                    {
                        var x = e.Location.X - xPosition.Value * areaDisplay.Scale;
                        var y = e.Location.Y - yPosition.Value * areaDisplay.Scale;
                        prevPos = new PointF(x, y);
                        return;
                    }

                    var newPos = (e.Location - prevPos.Value) / areaDisplay.Scale;
                    xPosition.Value = newPos.X;
                    yPosition.Value = newPos.Y;
                    return;
                }

                prevPos = null;

                if ((e.Buttons & MouseButtons.Middle) != 0)
                {
                    var newPosition = (e.Location - areaDisplay.ControlOffset) / areaDisplay.Scale;
                    xPosition.Value = newPosition.X;
                    yPosition.Value = newPosition.Y;
                }
            }

            areaDisplay.MouseMove += AreaMouseHandler;
            areaDisplay.MouseDown += AreaMouseHandler;

            // TODO: Fix rotation handling, doesn't move to actual edges
            var alignMenu = new ButtonMenuItem
            {
                Text = "Align",
                Items =
                {
                    new AppCommand("Left", () => xPosition.Value = width.Value / 2),
                    new AppCommand("Right", () => xPosition.Value = areaDisplay.FullBackground.Width - width.Value / 2),
                    new AppCommand("Top", () => yPosition.Value = height.Value / 2),
                    new AppCommand("Bottom", () => yPosition.Value = areaDisplay.FullBackground.Height - height.Value / 2),
                    new AppCommand("Center", () =>
                    {
                        xPosition.Value = areaDisplay.FullBackground.Width / 2;
                        yPosition.Value = areaDisplay.FullBackground.Height / 2;
                    })
                }
            };

            var scaleMenu = new ButtonMenuItem
            {
                Text = "Scale",
                Items =
                {
                    new AppCommand("Full", () =>
                    {
                        width.Value = areaDisplay.FullBackground.Width;
                        height.Value = areaDisplay.FullBackground.Height;
                        xPosition.Value = width.Value / 2;
                        yPosition.Value = height.Value / 2;
                    }),
                    new AppCommand("Half", () =>
                    {
                        width.Value = areaDisplay.FullBackground.Width / 2;
                        height.Value = areaDisplay.FullBackground.Height / 2;
                        xPosition.Value = areaDisplay.FullBackground.Width / 2;
                        yPosition.Value = areaDisplay.FullBackground.Height / 2;
                    })
                }
            };

            var flipMenu = new ButtonMenuItem
            {
                Text = "Flip",
                Items =
                {
                    new AppCommand("Horizontal", () => xPosition.Value = areaDisplay.FullBackground.Width - xPosition.Value),
                    new AppCommand("Vertical", () => yPosition.Value = areaDisplay.FullBackground.Height - yPosition.Value),
                    new AppCommand("Handedness", () =>
                    {
                        rotation.Value += 180;
                        rotation.Value %= 360;
                        xPosition.Value = areaDisplay.FullBackground.Width - xPosition.Value;
                        yPosition.Value = areaDisplay.FullBackground.Height - yPosition.Value;
                    })
                }
            };

            var contextMenu = new ContextMenu
            {
                Items =
                {
                    alignMenu,
                    scaleMenu,
                    flipMenu
                }
            };

            areaDisplay.MouseDown += (_, e) =>
            {
                if ((e.Buttons & MouseButtons.Alternate) != 0)
                    contextMenu.Show();
            };

            var editorControls = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                VerticalContentAlignment = VerticalAlignment.Center,
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem(null, true),
                    CreateUnitBox("Width", width, "mm"),
                    CreateUnitBox("Height", height, "mm"),
                    CreateUnitBox("X", xPosition, "mm"),
                    CreateUnitBox("Y", yPosition, "mm"),
                    CreateUnitBox("Rotation", rotation, "°"),
                    new StackLayoutItem(null, true)
                }
            };

            Content = new StackLayout
            {
                Orientation = Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem
                    {
                        Control = new GroupBox
                        {
                            Text = "Input",
                            Padding = 5,
                            Content = areaDisplay
                        },
                        Expand = true
                    },
                    new Scrollable
                    {
                        Border = BorderType.None,
                        Content = editorControls
                    }
                }
            };
        }
    }
}
