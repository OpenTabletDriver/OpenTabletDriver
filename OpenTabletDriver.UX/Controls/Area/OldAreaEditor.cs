using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Tools;

namespace OpenTabletDriver.UX.Controls.Area
{
    using static ParseTools;

    public class OldAreaEditor : Panel, IViewModelRoot<AreaViewModel>
    {
        public OldAreaEditor()
        {
            this.ViewModel ??= new AreaViewModel();
            this.ContextMenu = new ContextMenu();
        }

        public AreaViewModel ViewModel
        {
            set => this.DataContext = value;
            get => (AreaViewModel)this.DataContext;
        }

        protected override void OnLoadComplete(EventArgs e)
        {
            base.OnLoadComplete(e);

            AreaDisplay ??= new AreaDisplay
            {
                ViewModel = this.ViewModel
            };

            AreaDisplay.MouseDown += (sender, e) => BeginAreaDrag(e.Buttons);
            AreaDisplay.MouseUp += (sender, e) => EndAreaDrag(e.Buttons);

            widthBox = new TextBox
            {
                Width = 75,
                TextAlignment = TextAlignment.Right
            };
            widthBox.TextBinding.Convert(
                s => ToFloat(s),
                v => $"{v}"
            ).BindDataContext(Binding.Property((AreaViewModel d) =>  d.Width));

            heightBox = new TextBox
            {
                Width = 75,
                TextAlignment = TextAlignment.Right
            };
            heightBox.TextBinding.Convert(
                s => ToFloat(s),
                v => $"{v}"
            ).BindDataContext(Binding.Property((AreaViewModel d) =>  d.Height));

            xOffsetBox = new TextBox
            {
                Width = 75,
                TextAlignment = TextAlignment.Right
            };
            xOffsetBox.TextBinding.Convert(
                s => ToFloat(s),
                v => $"{v}"
            ).BindDataContext(Binding.Property((AreaViewModel d) =>  d.X));

            yOffsetBox = new TextBox
            {
                Width = 75,
                TextAlignment = TextAlignment.Right
            };
            yOffsetBox.TextBinding.Convert(
                s => ToFloat(s),
                v => $"{v}"
            ).BindDataContext(Binding.Property((AreaViewModel d) =>  d.Y));

            rotationBox = new TextBox
            {
                Width = 75,
                TextAlignment = TextAlignment.Right
            };
            rotationBox.TextBinding.Convert(
                s => ToFloat(s),
                v => $"{v}"
            ).BindDataContext(Binding.Property((AreaViewModel d) =>  d.Rotation));

            var stackLayout = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Items =
                {
                    new Group
                    {
                        Text = "Width",
                        Content = AppendUnit(widthBox, ViewModel.Unit),
                        ToolTip = $"Width of the area",
                        Orientation = Orientation.Horizontal
                    },
                    new Group
                    {
                        Text = "Height",
                        Content = AppendUnit(heightBox, ViewModel.Unit),
                        ToolTip = $"Height of the area",
                        Orientation = Orientation.Horizontal
                    },
                    new Group
                    {
                        Text = "X Offset",
                        Content = AppendUnit(xOffsetBox, ViewModel.Unit),
                        ToolTip = $"Center X coordinate of the area",
                        Orientation = Orientation.Horizontal
                    },
                    new Group
                    {
                        Text = "Y Offset",
                        Content = AppendUnit(yOffsetBox, ViewModel.Unit),
                        ToolTip = $"Center Y coordinate of the area",
                        Orientation = Orientation.Horizontal
                    }
                }
            };

            if (ViewModel.EnableRotation)
            {
                stackLayout.Items.Add(
                    new Group
                    {
                        Text = "Rotation",
                        Content = AppendUnit(rotationBox, "°"),
                        ToolTip = $"Rotation of the area about the center",
                        Orientation = Orientation.Horizontal
                    }
                );
            }

            var scrollview = new Scrollable
            {
                Content = stackLayout,
                Border = BorderType.None
            };

            Content = new StackLayout
            {
                Orientation = Orientation.Vertical,
                Items =
                {
                    new StackLayoutItem
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        Expand = true,
                        Control = new Panel
                        {
                            Padding = new Padding(5),
                            Content = AreaDisplay
                        }
                    },
                    new StackLayoutItem(scrollview, HorizontalAlignment.Center)
                }
            };

            this.ContextMenu.Items.GetSubmenu("Align").Items.AddRange(
                new MenuItem[]
                {
                    CreateMenuItem("Left", () => ViewModel.X = GetAreaCenterOffset().X),
                    CreateMenuItem("Right", () => ViewModel.X = ViewModel.FullBackground.Width - GetAreaCenterOffset().X),
                    CreateMenuItem("Top", () => ViewModel.Y = GetAreaCenterOffset().Y),
                    CreateMenuItem("Bottom", ()  => ViewModel.Y = ViewModel.FullBackground.Height - GetAreaCenterOffset().Y),
                    CreateMenuItem("Center",
                        () =>
                        {
                            ViewModel.X = ViewModel.FullBackground.Center.X;
                            ViewModel.Y = ViewModel.FullBackground.Center.Y;
                        }
                    )
                }
            );

            this.ContextMenu.Items.GetSubmenu("Resize").Items.AddRange(
                new MenuItem[]
                {
                    CreateMenuItem(
                        "Full area",
                        () =>
                        {
                            ViewModel.Height = ViewModel.FullBackground.Height;
                            ViewModel.Width = ViewModel.FullBackground.Width;
                            ViewModel.Y = ViewModel.FullBackground.Center.Y;
                            ViewModel.X = ViewModel.FullBackground.Center.X;
                        }
                    ),
                    CreateMenuItem(
                        "Quarter area",
                        () =>
                        {
                            ViewModel.Height = ViewModel.FullBackground.Height / 2;
                            ViewModel.Width = ViewModel.FullBackground.Width / 2;
                        }
                    )
                }
            );

            this.ContextMenu.Items.GetSubmenu("Flip").Items.AddRange(
                new MenuItem[]
                {
                    CreateMenuItem("Horizontal", () => ViewModel.X = ViewModel.FullBackground.Width - ViewModel.X),
                    CreateMenuItem("Vertical", () => ViewModel.Y = ViewModel.FullBackground.Height - ViewModel.Y),
                }
            );

            if (ViewModel.EnableRotation)
            {
                this.ContextMenu.Items.GetSubmenu("Flip").Items.Add(
                    CreateMenuItem("Handedness",
                        () =>
                        {
                            ViewModel.Rotation += 180;
                            ViewModel.Rotation %= 360;
                            ViewModel.X = ViewModel.FullBackground.Width - ViewModel.X;
                            ViewModel.Y = ViewModel.FullBackground.Height - ViewModel.Y;
                        }
                    )
                );
            }

            AppendMenuItemSeparator();

            this.MouseDown += (sender, e) =>
            {
                if (e.Buttons.HasFlag(MouseButtons.Alternate))
                    this.ContextMenu.Show(this);
            };
        }

        public void SetBackground(params RectangleF[] bgs) => SetBackground(bgs as IEnumerable<RectangleF>);
        public void SetBackground(IEnumerable<RectangleF> bgs) => ViewModel.Background = bgs;

        public Command AppendMenuItem(string menuText, Action handler)
        {
            var item = CreateMenuItem(menuText, handler);
            this.ContextMenu.Items.Add(item);
            return item;
        }

        public CheckCommand AppendCheckBoxMenuItem(string menuText, Action<bool> handler, bool defaultValue = false)
        {
            var command = CreateCheckBoxMenuItem(menuText, handler, defaultValue);
            this.ContextMenu.Items.Add(command);
            return command;
        }

        public void AppendMenuItemSeparator()
        {
            this.ContextMenu.Items.AddSeparator();
        }

        private Control AppendUnit(Control control, string unit)
        {
            return new StackView
            {
                Orientation = Orientation.Horizontal,
                VerticalContentAlignment = VerticalAlignment.Center,
                Items =
                {
                    new StackLayoutItem
                    {
                        Control = control,
                        Expand = true
                    },
                    new StackLayoutItem
                    {
                        Control = new Label
                        {
                            Text = unit,
                            VerticalAlignment = VerticalAlignment.Center,
                        }
                    }
                }
            };
        }

        private Command CreateMenuItem(string menuText, Action handler)
        {
            var command = new Command { MenuText = menuText };
            command.Executed += (sender, e) => handler();
            return command;
        }

        private CheckCommand CreateCheckBoxMenuItem(string menuText, Action<bool> handler, bool defaultValue = false)
        {
            var command = new CheckCommand { MenuText = menuText };
            command.Executed += (sender, e) => handler(command.Checked);
            command.Checked = defaultValue;
            if (defaultValue)
                command.Execute();
            return command;
        }

        private Vector2 GetAreaCenterOffset(out Vector2 min, out Vector2 max)
        {
            var origin = new Vector2(ViewModel.X, ViewModel.Y);
            var matrix = Matrix3x2.CreateTranslation(-origin);
            matrix *= Matrix3x2.CreateRotation((float)(ViewModel.Rotation * Math.PI / 180));
            matrix *= Matrix3x2.CreateTranslation(origin);

            float halfWidth = ViewModel.Width / 2;
            float halfHeight = ViewModel.Height / 2;

            var corners = new Vector2[]
            {
                Vector2.Transform(new Vector2(ViewModel.X - halfWidth, ViewModel.Y - halfHeight), matrix),
                Vector2.Transform(new Vector2(ViewModel.X - halfWidth, ViewModel.Y + halfHeight), matrix),
                Vector2.Transform(new Vector2(ViewModel.X + halfWidth, ViewModel.Y + halfHeight), matrix),
                Vector2.Transform(new Vector2(ViewModel.X + halfWidth, ViewModel.Y - halfHeight), matrix),
            };

            min = new Vector2(
                corners.Min(v => v.X),
                corners.Min(v => v.Y)
            );
            max = new Vector2(
                corners.Max(v => v.X),
                corners.Max(v => v.Y)
            );
            return (max - min) / 2;
        }

        private Vector2 GetAreaCenterOffset() => GetAreaCenterOffset(out _, out _);

        protected void BeginAreaDrag(MouseButtons buttons)
        {
            if (buttons.HasFlag(MouseButtons.Primary) && AreaValid)
                this.MouseMove += MoveArea;
        }

        protected void EndAreaDrag(MouseButtons buttons)
        {
            if (buttons.HasFlag(MouseButtons.Primary))
            {
                this.MouseMove -= MoveArea;
                this.lastMouseLocation = null;
            }
        }

        protected void MoveArea(object sender, MouseEventArgs e)
        {
            if (lastMouseLocation is PointF lastPos)
            {
                var delta = lastPos - e.Location;
                var scale = AreaDisplay.PixelScale;
                var newX = ViewModel.X - (delta.X / scale);
                var newY = ViewModel.Y - (delta.Y / scale);

                if (ViewModel.LockToUsableArea)
                {
                    var center = GetAreaCenterOffset(out var min, out var max);
                    if (min.X < 0)
                        ViewModel.X = center.X;
                    else if (max.X > ViewModel.FullBackground.Width)
                        ViewModel.X = ViewModel.FullBackground.Width - center.X;
                    if (min.Y < 0)
                        ViewModel.Y = center.Y;
                    else if (max.Y > ViewModel.FullBackground.Height)
                        ViewModel.Y = ViewModel.FullBackground.Height - center.Y;
                }
                else
                {
                    ViewModel.X = newX;
                    ViewModel.Y = newY;
                }
            }
            this.lastMouseLocation = e.Location;
        }

        public AreaDisplay AreaDisplay { protected set; get; }
        public bool AreaValid => ViewModel.Width != 0 && ViewModel.Height != 0 && ViewModel.FullBackground != RectangleF.Empty;

        private PointF? lastMouseLocation;
        private TextBox widthBox, heightBox, xOffsetBox, yOffsetBox, rotationBox;
    }
}
