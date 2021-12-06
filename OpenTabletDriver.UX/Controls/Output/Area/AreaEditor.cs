using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.UX.Controls.Generic.Text;
using OpenTabletDriver.UX.Controls.Utilities;

namespace OpenTabletDriver.UX.Controls.Output.Area
{
    public class AreaEditor : AreaControl
    {
        public AreaEditor()
        {
            this.Content = new StackLayout
            {
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem
                    {
                        Expand = true,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        Control = new Panel
                        {
                            Padding = new Padding(5),
                            Content = Display = new AreaDisplay()
                        }
                    },
                    new StackLayoutItem
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Control = settingsPanel = new StackLayout
                        {
                            Orientation = Orientation.Horizontal,
                            Spacing = 5,
                            Items =
                            {
                                new StackLayoutItem
                                {
                                    Control = widthGroup = new UnitGroup
                                    {
                                        Text = "Width",
                                        Unit = Unit,
                                        ToolTip = $"Area width in {Unit}",
                                        Orientation = Orientation.Horizontal,
                                        Content = width = new FloatNumberBox()
                                    }
                                },
                                new StackLayoutItem
                                {
                                    Control = heightGroup = new UnitGroup
                                    {
                                        Text = "Height",
                                        Unit = Unit,
                                        ToolTip = $"Area height in {Unit}",
                                        Orientation = Orientation.Horizontal,
                                        Content = height = new FloatNumberBox()
                                    }
                                },
                                new StackLayoutItem
                                {
                                    Control = xGroup = new UnitGroup
                                    {
                                        Text = "X",
                                        Unit = Unit,
                                        ToolTip = $"Area center X offset in {Unit}",
                                        Orientation = Orientation.Horizontal,
                                        Content = x = new FloatNumberBox()
                                    }
                                },
                                new StackLayoutItem
                                {
                                    Control = yGroup = new UnitGroup
                                    {
                                        Text = "Y",
                                        Unit = Unit,
                                        ToolTip = $"Area center Y offset in {Unit}",
                                        Orientation = Orientation.Horizontal,
                                        Content = y = new FloatNumberBox()
                                    }
                                }
                            }
                        }
                    }
                }
            };

            CreateMenu();

            widthGroup.UnitBinding.Bind(UnitBinding);
            heightGroup.UnitBinding.Bind(UnitBinding);
            xGroup.UnitBinding.Bind(UnitBinding);
            yGroup.UnitBinding.Bind(UnitBinding);

            var widthBinding = AreaBinding.Child((AreaSettings s) => s.Width);
            var heightBinding = AreaBinding.Child((AreaSettings s) => s.Height);
            var xBinding = AreaBinding.Child((AreaSettings s) => s.X);
            var yBinding = AreaBinding.Child((AreaSettings s) => s.Y);

            width.ValueBinding.Bind(widthBinding);
            height.ValueBinding.Bind(heightBinding);
            x.ValueBinding.Bind(xBinding);
            y.ValueBinding.Bind(yBinding);

            Display.AreaBinding.Bind(AreaBinding);
            Display.LockToUsableAreaBinding.Bind(LockToUsableAreaBinding);
            Display.UnitBinding.Bind(UnitBinding);
            Display.AreaBoundsBinding.Bind(AreaBoundsBinding);
            Display.FullAreaBoundsBinding.Bind(FullAreaBoundsBinding);
            Display.InvalidForegroundErrorBinding.Bind(InvalidForegroundErrorBinding);
            Display.InvalidBackgroundErrorBinding.Bind(InvalidBackgroundErrorBinding);
        }

        private BooleanCommand lockToUsableArea = new BooleanCommand
        {
            MenuText = "Lock to usable area"
        };

        private UnitGroup widthGroup, heightGroup, xGroup, yGroup;
        private MaskedTextBox<float> width, height, x, y;

        protected StackLayout settingsPanel;

        public AreaDisplay Display { get; }

        public override IEnumerable<RectangleF> AreaBounds
        {
            set
            {
                this.areaBounds = value;
                this.OnAreaBoundsChanged();
                if (AreaBounds != null)
                {
                    this.FullAreaBounds = new RectangleF
                    {
                        Left = this.AreaBounds.Min(r => r.Left),
                        Top = this.AreaBounds.Min(r => r.Top),
                        Right = this.AreaBounds.Max(r => r.Right),
                        Bottom = this.AreaBounds.Max(r => r.Bottom),
                    };
                }
                else
                {
                    this.FullAreaBounds = RectangleF.Empty;
                }
            }
            get => this.areaBounds;
        }

        public Vector2[] GetAreaCorners()
        {
            var origin = new Vector2(Area.X, Area.Y);
            var matrix = Matrix3x2.CreateTranslation(-origin);
            matrix *= Matrix3x2.CreateRotation((float)(Area.Rotation * Math.PI / 180));
            matrix *= Matrix3x2.CreateTranslation(origin);

            float halfWidth = Area.Width / 2;
            float halfHeight = Area.Height / 2;

            return new Vector2[]
            {
                Vector2.Transform(new Vector2(Area.X - halfWidth, Area.Y - halfHeight), matrix),
                Vector2.Transform(new Vector2(Area.X - halfWidth, Area.Y + halfHeight), matrix),
                Vector2.Transform(new Vector2(Area.X + halfWidth, Area.Y + halfHeight), matrix),
                Vector2.Transform(new Vector2(Area.X + halfWidth, Area.Y - halfHeight), matrix),
            };
        }

        public Vector2 GetAreaCenterOffset()
        {
            var corners = GetAreaCorners();
            var min = new Vector2(
                corners.Min(v => v.X),
                corners.Min(v => v.Y)
            );
            var max = new Vector2(
                corners.Max(v => v.X),
                corners.Max(v => v.Y)
            );
            return (max - min) / 2;
        }

        protected virtual void CreateMenu()
        {
            this.ContextMenu = new ContextMenu
            {
                Items =
                {
                    new ButtonMenuItem
                    {
                        Text = "Align",
                        Items =
                        {
                            new ActionCommand
                            {
                                MenuText = "Left",
                                Action = () => Area.X = GetAreaCenterOffset().X
                            },
                            new ActionCommand
                            {
                                MenuText = "Right",
                                Action = () => Area.X = FullAreaBounds.Width - GetAreaCenterOffset().X
                            },
                            new ActionCommand
                            {
                                MenuText = "Top",
                                Action = () => Area.Y = GetAreaCenterOffset().Y
                            },
                            new ActionCommand
                            {
                                MenuText = "Bottom",
                                Action = () => Area.Y = FullAreaBounds.Height - GetAreaCenterOffset().Y
                            },
                            new ActionCommand
                            {
                                MenuText = "Center",
                                Action = () =>
                                {
                                    Area.X = FullAreaBounds.Center.X;
                                    Area.Y = FullAreaBounds.Center.Y;
                                }
                            }
                        }
                    },
                    new ButtonMenuItem
                    {
                        Text = "Resize",
                        Items =
                        {
                            new ActionCommand
                            {
                                MenuText = "Full area",
                                Action = () =>
                                {
                                    Area.Height = FullAreaBounds.Height;
                                    Area.Width = FullAreaBounds.Width;
                                    Area.Y = FullAreaBounds.Center.Y;
                                    Area.X = FullAreaBounds.Center.X;
                                }
                            },
                            new ActionCommand
                            {
                                MenuText = "Quarter area",
                                Action = () =>
                                {
                                    Area.Height = FullAreaBounds.Height / 2;
                                    Area.Width = FullAreaBounds.Width / 2;
                                }
                            }
                        }
                    },
                    new ButtonMenuItem
                    {
                        Text = "Flip",
                        Items =
                        {
                            new ActionCommand
                            {
                                MenuText = "Horizontal",
                                Action = () => Area.X = FullAreaBounds.Width - Area.X
                            },
                            new ActionCommand
                            {
                                MenuText = "Vertical",
                                Action = () => Area.Y = FullAreaBounds.Height - Area.Y
                            }
                        }
                    },
                    lockToUsableArea
                }
            };

            lockToUsableArea.CheckedBinding.Cast<bool>().Bind(LockToUsableAreaBinding);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            switch (e.Buttons)
            {
                case MouseButtons.Alternate:
                {
                    this.ContextMenu.Show(this);
                    break;
                }
            }
        }

        protected override void OnLockToUsableAreaChanged()
        {
            base.OnLockToUsableAreaChanged();

            if (LockToUsableArea)
                OnAreaChanged();
        }
    }
}
