using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Controls.Generic.Text;
using OpenTabletDriver.UX.Controls.Utilities;
using System;

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

            var widthBinding = AreaBinding.Child((AreaSettings? s) => s!.Width);
            var heightBinding = AreaBinding.Child((AreaSettings? s) => s!.Height);
            var xBinding = AreaBinding.Child((AreaSettings? s) => s!.X);
            var yBinding = AreaBinding.Child((AreaSettings? s) => s!.Y);

            width.ValueBinding.Bind(widthBinding);
            height.ValueBinding.Bind(heightBinding);
            x.ValueBinding.Bind(xBinding);
            y.ValueBinding.Bind(yBinding);

            width.ValueChanged += (_, _) => Display.Invalidate();
            height.ValueChanged += (_, _) => Display.Invalidate();
            x.ValueChanged += (_, _) => Display.Invalidate();
            y.ValueChanged += (_, _) => Display.Invalidate();

            Display.AreaBinding.Bind(AreaBinding);
            Display.LockToUsableAreaBinding.Bind(LockToUsableAreaBinding);
            Display.UnitBinding.Bind(UnitBinding);
            Display.AreaBoundsBinding.Bind(AreaBoundsBinding);
            Display.FullAreaBoundsBinding.Bind(FullAreaBoundsBinding);
            Display.InvalidForegroundErrorBinding.Bind(InvalidForegroundErrorBinding);
        }

        private BooleanCommand lockToUsableArea = new BooleanCommand
        {
            MenuText = "Lock to usable area"
        };

        private UnitGroup widthGroup, heightGroup, xGroup, yGroup;
        private MaskedTextBox<float> width, height, x, y;

        protected StackLayout settingsPanel;

        public AreaDisplay Display { get; }

        public override IEnumerable<RectangleF>? AreaBounds
        {
            set
            {
                this.areaBounds = value?.ToArray();
                this.OnAreaBoundsChanged();
                if (areaBounds != null)
                {
                    this.FullAreaBounds = new RectangleF
                    {
                        Left = this.areaBounds.Min(r => r.Left),
                        Top = this.areaBounds.Min(r => r.Top),
                        Right = this.areaBounds.Max(r => r.Right),
                        Bottom = this.areaBounds.Max(r => r.Bottom),
                    };
                }
                else
                {
                    this.FullAreaBounds = RectangleF.Empty;
                }

                this.Invalidate();
            }
            get => this.areaBounds;
        }

        public Vector2 GetAreaCenterOffset()
        {
            Debug.Assert(Area != null, "Tried to get area corners but Area is null");
            var corners = this.Area.GetRectangleF().GetAreaCorners(Area.Rotation);
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
                                Action = () => Area!.X = GetAreaCenterOffset().X
                            },
                            new ActionCommand
                            {
                                MenuText = "Right",
                                Action = () => Area!.X = FullAreaBounds!.Value.Width - GetAreaCenterOffset().X
                            },
                            new ActionCommand
                            {
                                MenuText = "Top",
                                Action = () => Area!.Y = GetAreaCenterOffset().Y
                            },
                            new ActionCommand
                            {
                                MenuText = "Bottom",
                                Action = () => Area!.Y = FullAreaBounds!.Value.Height - GetAreaCenterOffset().Y
                            },
                            new ActionCommand
                            {
                                MenuText = "Center",
                                Action = () =>
                                {
                                    Area!.X = FullAreaBounds!.Value.Center.X;
                                    Area!.Y = FullAreaBounds!.Value.Center.Y;
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
                                    var maxRotated = GetLargestRectInRotatedRectRatioLocked(new Vector2(FullAreaBounds!.Value.Width, FullAreaBounds!.Value.Height), Area!.Rotation, 1.7777777777f);
                                    Area!.Y = FullAreaBounds!.Value.Center.Y;
                                    Area!.X = FullAreaBounds!.Value.Center.X;
                                    Area!.Width = maxRotated.X;
                                    Area!.Height = maxRotated.Y;
                                }
                            },
                            new ActionCommand
                            {
                                MenuText = "Quarter area",
                                Action = () =>
                                {
                                    Area!.Y = FullAreaBounds!.Value.Center.Y;
                                    Area!.X = FullAreaBounds!.Value.Center.X;
                                    Area!.Height = FullAreaBounds!.Value.Height / 2;
                                    Area!.Width = FullAreaBounds!.Value.Width / 2;
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
                                Action = () => Area!.X = FullAreaBounds!.Value.Width - Area.X
                            },
                            new ActionCommand
                            {
                                MenuText = "Vertical",
                                Action = () => Area!.Y = FullAreaBounds!.Value.Height - Area.Y
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

        // Converted from python code from stackoverflow:
        // https://stackoverflow.com/questions/16702966/rotate-image-and-crop-out-black-borders/16778797#16778797
        public static Vector2 GetLargestRectInRotatedRect(Vector2 rotatedRectDimensions, float rotationAngleDegrees)
        {
            var width = rotatedRectDimensions.X;
            var height = rotatedRectDimensions.Y;
            var rotationAngleRadians = Math.PI / 180 * rotationAngleDegrees;

            if (width <= 0 || height <= 0)
                return new Vector2(0,0);

            bool widthIsLonger = width >= height;
            var (long_side, short_side) = widthIsLonger ? (width, height) : (height, width);
            var (sinA, cosA) = (Math.Abs(Math.Sin(rotationAngleRadians)), Math.Abs(Math.Cos(rotationAngleRadians)));
            if (short_side <= 2.0 * sinA * cosA * long_side || Math.Abs(sinA - cosA) < 1E-10)
            {
                var x = 0.5 * short_side;
                return widthIsLonger ? new Vector2((float)(x / sinA), (float)(x / cosA)) : new Vector2((float)(x / cosA), (float)(x / sinA));
            }
            else
            {
                var cos2A = cosA * cosA - sinA * sinA;
                return new Vector2((float)((width * cosA - height * sinA) / cos2A), (float)((height * cosA - width * sinA) / cos2A));
            }
        }

        // https://www.desmos.com/calculator/jueq5tuwv7
        // https://www.desmos.com/calculator/fnszuodzlh
        public static Vector2 GetLargestRectInRotatedRectRatioLocked(Vector2 rotatedRectDimensions, float rotationAngleDegrees, float aspectRatio)
        {
            // Special cases for 0, 90, 180, 270, 360
            // These could also be handled by adding another intercept but this is easier
            if (rotationAngleDegrees % 180 < 0.01)
                return new Vector2(rotatedRectDimensions.X, rotatedRectDimensions.X / aspectRatio);
            if (rotationAngleDegrees % 90 < 0.01)
                return new Vector2(rotatedRectDimensions.Y, rotatedRectDimensions.Y / aspectRatio);

            var corners = new RectangleF(0, 0, rotatedRectDimensions.X, rotatedRectDimensions.Y).GetAreaCorners(rotationAngleDegrees);
            var (topLeft, topRight, bottomLeft, bottomRight) = (corners[0], corners[1], corners[2], corners[3]);

            // y = mx + b

            // m: slope
            double mTopBottom = (topRight.Y - topLeft.Y) / (topRight.X - topLeft.X);
            double mRightLeft = (topLeft.Y - bottomLeft.Y) / (topLeft.X - bottomLeft.X);
            if (Double.IsInfinity(mTopBottom))
                mTopBottom = 0;
            if (Double.IsInfinity(mRightLeft))
                mRightLeft = 0;

            // Aspect ratio slope is reversed from what we want: x = mRatio * y
            // Later it must become: y = x / mRatio
            double mRatio = aspectRatio;

            // b: x intercept
            double bTop = topRight.Y - mTopBottom * topRight.X;
            double bRight = topRight.Y - mRightLeft * topRight.X;

            // Get intersection on X axis of ratio and side by setting them equal
            // x / mRatio = mTopBottom * x + bTop -> x = (bTop * mRatio) / (1 - mTopBottom * mRatio)
            // x / mRatio = mRightLeft * x + bRight -> x = (bRight * mRatio) / (1 - mRightLeft * mRatio)
            double topIntersectionXPos = (bTop * mRatio) / (1 - mTopBottom * mRatio);
            double rightIntersectionXPos = (bRight * mRatio) / (1 - mRightLeft * mRatio);
            double topIntersectionXNeg = (bTop * -mRatio) / (1 - mTopBottom * -mRatio);
            // var rightIntersectionXNeg = (bRight * -mRatio) / (1 - mRightLeft * -mRatio);

            // Solve for Y now that we have X
            // y = mx + b
            // y = mTopBottom * topIntersectionX + bTop
            // y = mRightLeft * rightIntersectionX + bRight
            double topIntersectionYPos = mTopBottom * topIntersectionXPos + bTop;
            double rightIntersectionYPos = mRightLeft * rightIntersectionXPos + bRight;
            double topIntersectionYNeg = mTopBottom * topIntersectionXNeg + bTop;
            // var rightIntersectionYNeg = mRightLeft * rightIntersectionXNeg + bRight;

            var leftTopPoint = new Vector2((float)Math.Abs(topIntersectionXNeg), (float)Math.Abs(topIntersectionYNeg));
            var rightTopPoint = new Vector2((float)Math.Abs(topIntersectionXPos), (float)Math.Abs(topIntersectionYPos));
            // leftSidePoint is redundant, only three points are necessary
            var rightSidePoint = new Vector2((float)Math.Abs(rightIntersectionXPos), (float)Math.Abs(rightIntersectionYPos));

            Vector2[] points = [leftTopPoint, rightTopPoint, rightSidePoint];
            // Discard points that dont fit within the area
            // Get the closest point to the center (measured diagonally, pythagorean this thing) and put it in Vector3.Z, besides special cases of 90 and 270 this will the correct point to choose
            var distances = points.Where(p => p.X <= rotatedRectDimensions.X && p.Y <= rotatedRectDimensions.Y).Select(p => new Vector3(p.X, p.Y, p.X * p.X + p.Y * p.Y));

            var closest = distances.MinBy(p => p.Z);
            return new Vector2(closest.X * 2, closest.Y * 2);
        }
    }
}
