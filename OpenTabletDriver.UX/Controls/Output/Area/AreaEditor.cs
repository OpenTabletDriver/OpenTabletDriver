using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Eto.Drawing;
using Eto.Forms;
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
                            Content = display = new AreaDisplay()
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

            width.ValueBinding.Bind(AreaWidthBinding);
            height.ValueBinding.Bind(AreaHeightBinding);
            x.ValueBinding.Bind(AreaXOffsetBinding);
            y.ValueBinding.Bind(AreaYOffsetBinding);

            display.AreaWidthBinding.Bind(AreaWidthBinding);
            display.AreaHeightBinding.Bind(AreaHeightBinding);
            display.AreaXOffsetBinding.Bind(AreaXOffsetBinding);
            display.AreaYOffsetBinding.Bind(AreaYOffsetBinding);
            display.LockToUsableAreaBinding.Bind(LockToUsableAreaBinding);
            display.UnitBinding.Bind(UnitBinding);
            display.AreaBoundsBinding.Bind(AreaBoundsBinding);
            display.FullAreaBoundsBinding.Bind(FullAreaBoundsBinding);
            display.InvalidForegroundErrorBinding.Bind(InvalidForegroundErrorBinding);
            display.InvalidBackgroundErrorBinding.Bind(InvalidBackgroundErrorBinding);
        }

        private BooleanCommand lockToUsableArea = new BooleanCommand
        {
            MenuText = "Lock to usable area"
        };

        private UnitGroup widthGroup, heightGroup, xGroup, yGroup;
        private MaskedTextBox<float> width, height, x, y;

        protected AreaDisplay display;
        protected StackLayout settingsPanel;

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
            var origin = new Vector2(AreaXOffset, AreaYOffset);
            var matrix = Matrix3x2.CreateTranslation(-origin);
            matrix *= Matrix3x2.CreateRotation((float)(AreaRotation * Math.PI / 180));
            matrix *= Matrix3x2.CreateTranslation(origin);

            float halfWidth = AreaWidth / 2;
            float halfHeight = AreaHeight / 2;

            return new Vector2[]
            {
                Vector2.Transform(new Vector2(AreaXOffset - halfWidth, AreaYOffset - halfHeight), matrix),
                Vector2.Transform(new Vector2(AreaXOffset - halfWidth, AreaYOffset + halfHeight), matrix),
                Vector2.Transform(new Vector2(AreaXOffset + halfWidth, AreaYOffset + halfHeight), matrix),
                Vector2.Transform(new Vector2(AreaXOffset + halfWidth, AreaYOffset - halfHeight), matrix),
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
                                Action = () => AreaXOffset = GetAreaCenterOffset().X
                            },
                            new ActionCommand
                            {
                                MenuText = "Right",
                                Action = () => AreaXOffset = FullAreaBounds.Width - GetAreaCenterOffset().X
                            },
                            new ActionCommand
                            {
                                MenuText = "Top",
                                Action = () => AreaYOffset = GetAreaCenterOffset().Y
                            },
                            new ActionCommand
                            {
                                MenuText = "Bottom",
                                Action = () => AreaYOffset = FullAreaBounds.Height - GetAreaCenterOffset().Y
                            },
                            new ActionCommand
                            {
                                MenuText = "Center",
                                Action = () =>
                                {
                                    AreaXOffset = FullAreaBounds.Center.X;
                                    AreaYOffset = FullAreaBounds.Center.Y;
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
                                    AreaHeight = FullAreaBounds.Height;
                                    AreaWidth = FullAreaBounds.Width;
                                    AreaYOffset = FullAreaBounds.Center.Y;
                                    AreaXOffset = FullAreaBounds.Center.X;
                                }
                            },
                            new ActionCommand
                            {
                                MenuText = "Quarter area",
                                Action = () =>
                                {
                                    AreaHeight = FullAreaBounds.Height / 2;
                                    AreaWidth = FullAreaBounds.Width / 2;
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
                                Action = () => AreaXOffset = FullAreaBounds.Width - AreaXOffset
                            },
                            new ActionCommand
                            {
                                MenuText = "Vertical",
                                Action = () => AreaYOffset = FullAreaBounds.Height - AreaYOffset
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
    }
}
