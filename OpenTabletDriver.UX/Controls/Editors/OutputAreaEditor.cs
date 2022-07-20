using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.UX.Components;

namespace OpenTabletDriver.UX.Controls.Editors
{
    public class OutputAreaEditor : AreaEditor
    {
        public OutputAreaEditor(IControlBuilder controlBuilder, App app)
        {
            var width = TextBoxFor(m => m.Output!.Width);
            var height = TextBoxFor(m => m.Output!.Height);
            var xPosition = TextBoxFor(m => m.Output!.XPosition);
            var yPosition = TextBoxFor(m => m.Output!.YPosition);

            var areaDisplay = controlBuilder.Build<OutputAreaDisplay>();
            width.ValueChanged += (_, _) => areaDisplay.Invalidate();
            height.ValueChanged += (_, _) => areaDisplay.Invalidate();
            xPosition.ValueChanged += (_, _) => areaDisplay.Invalidate();
            yPosition.ValueChanged += (_, _) => areaDisplay.Invalidate();

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

            var alignMenu = new ButtonMenuItem
            {
                Text = "Align...",
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
                Text = "Scale...",
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
                Text = "Flip...",
                Items =
                {
                    new AppCommand("Horizontal", () => xPosition.Value = areaDisplay.FullBackground.Width - xPosition.Value),
                    new AppCommand("Vertical", () => yPosition.Value = areaDisplay.FullBackground.Height - yPosition.Value)
                }
            };

            var displayMenu = new ButtonMenuItem
            {
                Text = "Display..."
            };

            foreach (var display in app.Displays)
            {
                var text = display.Index == 1 ? "Primary" : $"Display {display.Index}";
                var setDisplayCommand = new AppCommand(text, () =>
                {
                    width.Value = display.Width;
                    height.Value = display.Height;
                    xPosition.Value = display.Position.X + display.Width / 2;
                    yPosition.Value = display.Position.Y + display.Height / 2;
                });
                displayMenu.Items.Add(setDisplayCommand);
            }

            var contextMenu = new ContextMenu
            {
                Items =
                {
                    alignMenu,
                    scaleMenu,
                    flipMenu,
                    displayMenu
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
                    CreateUnitBox("Width", width, "px"),
                    CreateUnitBox("Height", height, "px"),
                    CreateUnitBox("X", xPosition, "px"),
                    CreateUnitBox("Y", yPosition, "px"),
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
                            Text = "Output",
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
