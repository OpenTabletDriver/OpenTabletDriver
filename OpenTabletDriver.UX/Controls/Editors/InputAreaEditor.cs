using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Output;
using OpenTabletDriver.UX.Components;
using OpenTabletDriver.UX.Dialogs;

namespace OpenTabletDriver.UX.Controls.Editors
{
    public class InputAreaEditor : AreaEditor
    {
        private readonly App _app;
        private readonly NumericMaskedTextBox<float> _width;
        private readonly NumericMaskedTextBox<float> _height;
        private readonly NumericMaskedTextBox<float> _xPosition;
        private readonly NumericMaskedTextBox<float> _yPosition;
        private readonly NumericMaskedTextBox<float> _rotation;

        public InputAreaEditor(App app, IControlBuilder controlBuilder)
        {
            _app = app;

            _width = TextBoxFor(m => m.Input!.Width);
            _height = TextBoxFor(m => m.Input!.Height);
            _xPosition = TextBoxFor(m => m.Input!.XPosition);
            _yPosition = TextBoxFor(m => m.Input!.YPosition);
            _rotation = TextBoxFor(m => m.Input!.Rotation);

            var areaDisplay = controlBuilder.Build<InputAreaDisplay>();
            _width.ValueChanged += (_, _) => areaDisplay.Invalidate();
            _height.ValueChanged += (_, _) => areaDisplay.Invalidate();
            _xPosition.ValueChanged += (_, _) => areaDisplay.Invalidate();
            _yPosition.ValueChanged += (_, _) => areaDisplay.Invalidate();
            _rotation.ValueChanged += (_, _) => areaDisplay.Invalidate();

            PointF? prevPos = null;
            void AreaMouseHandler(object? _, MouseEventArgs e)
            {
                if ((e.Buttons & MouseButtons.Primary) != 0)
                {
                    if (prevPos == null)
                    {
                        var x = e.Location.X - _xPosition.Value * areaDisplay.Scale;
                        var y = e.Location.Y - _yPosition.Value * areaDisplay.Scale;
                        prevPos = new PointF(x, y);
                        return;
                    }

                    var newPos = (e.Location - prevPos.Value) / areaDisplay.Scale;
                    _xPosition.Value = newPos.X;
                    _yPosition.Value = newPos.Y;
                    return;
                }

                prevPos = null;

                if ((e.Buttons & MouseButtons.Middle) != 0)
                {
                    var newPosition = (e.Location - areaDisplay.ControlOffset) / areaDisplay.Scale;
                    _xPosition.Value = newPosition.X;
                    _yPosition.Value = newPosition.Y;
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
                    new AppCommand("Left", () => _xPosition.Value = _width.Value / 2),
                    new AppCommand("Right", () => _xPosition.Value = areaDisplay.FullBackground.Width - _width.Value / 2),
                    new AppCommand("Top", () => _yPosition.Value = _height.Value / 2),
                    new AppCommand("Bottom", () => _yPosition.Value = areaDisplay.FullBackground.Height - _height.Value / 2),
                    new AppCommand("Center", () =>
                    {
                        _xPosition.Value = areaDisplay.FullBackground.Width / 2;
                        _yPosition.Value = areaDisplay.FullBackground.Height / 2;
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
                        _width.Value = areaDisplay.FullBackground.Width;
                        _height.Value = areaDisplay.FullBackground.Height;
                        _xPosition.Value = _width.Value / 2;
                        _yPosition.Value = _height.Value / 2;
                    }),
                    new AppCommand("Half", () =>
                    {
                        _width.Value = areaDisplay.FullBackground.Width / 2;
                        _height.Value = areaDisplay.FullBackground.Height / 2;
                        _xPosition.Value = areaDisplay.FullBackground.Width / 2;
                        _yPosition.Value = areaDisplay.FullBackground.Height / 2;
                    })
                }
            };

            var flipMenu = new ButtonMenuItem
            {
                Text = "Flip",
                Items =
                {
                    new AppCommand("Horizontal", () => _xPosition.Value = areaDisplay.FullBackground.Width - _xPosition.Value),
                    new AppCommand("Vertical", () => _yPosition.Value = areaDisplay.FullBackground.Height - _yPosition.Value),
                    new AppCommand("Handedness", () =>
                    {
                        _rotation.Value += 180;
                        _rotation.Value %= 360;
                        _xPosition.Value = areaDisplay.FullBackground.Width - _xPosition.Value;
                        _yPosition.Value = areaDisplay.FullBackground.Height - _yPosition.Value;
                    })
                }
            };

            var contextMenu = new ContextMenu
            {
                Items =
                {
                    alignMenu,
                    scaleMenu,
                    flipMenu,
                    new AppCommand("Convert area...", ConvertArea)
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
                    CreateUnitBox("Width", _width, "mm"),
                    CreateUnitBox("Height", _height, "mm"),
                    CreateUnitBox("X", _xPosition, "mm"),
                    CreateUnitBox("Y", _yPosition, "mm"),
                    CreateUnitBox("Rotation", _rotation, "°"),
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

        private void ConvertArea()
        {
            var profile = (Profile)DataContext;
            var tablet = _app.Tablets.First(t => t.Name == profile.Tablet);

            var dialog = _app.ShowDialog<AreaConverterDialog>(ParentWindow, tablet);
            if (dialog.Result is AngledArea area)
            {
                _width.Value = area.Width;
                _height.Value = area.Height;
                _xPosition.Value = area.XPosition;
                _yPosition.Value = area.YPosition;
                _rotation.Value = area.Rotation;
            }
        }
    }
}
