using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Controls.Output.Area;
using OpenTabletDriver.UX.Controls.Utilities;

namespace OpenTabletDriver.UX.Controls.Output
{
    public class AbsoluteModeEditor : Panel
    {
        public AbsoluteModeEditor()
        {
            this.Content = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Items =
                {
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = new Group
                        {
                            Text = "Display",
                            Content = displayAreaEditor = new DisplayAreaEditor(() => HandleAspectRatioLock(displayAreaEditor, EventArgs.Empty))
                            {
                                InvalidForegroundError = "Invalid display area.",
                                Unit = "px"
                            }
                        }
                    },
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = new Group
                        {
                            Text = "Tablet",
                            Content = tabletAreaEditor = new TabletAreaEditor
                            {
                                InvalidForegroundError = "Invalid tablet area.",
                                Unit = "mm"
                            }
                        }
                    }
                }
            };

            tabletAreaEditor.ContextMenu.Items.GetSubmenu("Resize").Items.AddRange(
                [
                    new ActionCommand
                    {
                        MenuText = "Full area",
                        Action = () =>
                        {
                            Vector2 tabletSize = new Vector2(tabletAreaEditor.FullAreaBounds!.Value.Width, tabletAreaEditor.FullAreaBounds!.Value.Height);
                            var displayRatio = displayAreaEditor.Area!.Width / displayAreaEditor.Area!.Height;
                            var largestArea = tabletAreaEditor.LockAspectRatio ? GetLargestRectInRotatedRectRatioLocked(tabletSize, tabletAreaEditor.Area!.Rotation, displayRatio) : GetLargestRectInRotatedRect(tabletSize, tabletAreaEditor.Area!.Rotation);
                            tabletAreaEditor.Area!.Y = tabletAreaEditor.FullAreaBounds!.Value.Center.Y;
                            tabletAreaEditor.Area!.X = tabletAreaEditor.FullAreaBounds!.Value.Center.X;
                            tabletAreaEditor.Area!.Width = largestArea.X;
                            tabletAreaEditor.Area!.Height = largestArea.Y;
                        }
                    },
                    new ActionCommand
                    {
                        MenuText = "Quarter area",
                        Action = () =>
                        {
                            Vector2 tabletSize = new Vector2(tabletAreaEditor.FullAreaBounds!.Value.Width, tabletAreaEditor.FullAreaBounds!.Value.Height);
                            var displayRatio = displayAreaEditor.Area!.Width / displayAreaEditor.Area!.Height;
                            var largestArea = tabletAreaEditor.LockAspectRatio ? GetLargestRectInRotatedRectRatioLocked(tabletSize, tabletAreaEditor.Area!.Rotation, displayRatio) : GetLargestRectInRotatedRect(tabletSize, tabletAreaEditor.Area!.Rotation);
                            tabletAreaEditor.Area!.Y = tabletAreaEditor.FullAreaBounds!.Value.Center.Y;
                            tabletAreaEditor.Area!.X = tabletAreaEditor.FullAreaBounds!.Value.Center.X;
                            tabletAreaEditor.Area!.Width = largestArea.X / 2;
                            tabletAreaEditor.Area!.Height = largestArea.Y / 2;
                        }
                    }
                ]
            );

            displayAreaEditor.AreaBinding.Bind(SettingsBinding.Child(c => c!.Display)!);
            displayAreaEditor.LockToUsableAreaBinding.Bind(App.Current, c => c.Settings.LockUsableAreaDisplay);

            tabletAreaEditor.AreaBinding.Bind(SettingsBinding.Child(c => c!.Tablet)!);
            tabletAreaEditor.LockToUsableAreaBinding.Bind(App.Current, c => c.Settings.LockUsableAreaTablet);

            tabletAreaEditor.LockAspectRatioBinding.Bind(SettingsBinding.Child(c => c!.LockAspectRatio));
            tabletAreaEditor.AreaClippingBinding.Bind(SettingsBinding.Child(c => c!.EnableClipping));
            tabletAreaEditor.IgnoreOutsideAreaBinding.Bind(SettingsBinding.Child(c => c!.EnableAreaLimiting));

            displayWidth = SettingsBinding.Child(c => c!.Display.Width);
            displayHeight = SettingsBinding.Child(c => c!.Display.Height);
            var displayX = SettingsBinding.Child(c => c!.Display.X);
            var displayY = SettingsBinding.Child(c => c!.Display.Y);
            displayWidth.DataValueChanged += HandleDisplayAreaConstraint;
            displayHeight.DataValueChanged += HandleDisplayAreaConstraint;
            displayX.DataValueChanged += HandleDisplayAreaConstraint;
            displayY.DataValueChanged += HandleDisplayAreaConstraint;

            tabletWidth = SettingsBinding.Child(c => c!.Tablet.Width);
            tabletHeight = SettingsBinding.Child(c => c!.Tablet.Height);
            var tabletX = SettingsBinding.Child(c => c!.Tablet.X);
            var tabletY = SettingsBinding.Child(c => c!.Tablet.Y);
            tabletWidth.DataValueChanged += HandleTabletAreaConstraint;
            tabletHeight.DataValueChanged += HandleTabletAreaConstraint;
            tabletX.DataValueChanged += HandleTabletAreaConstraint;
            tabletY.DataValueChanged += HandleTabletAreaConstraint;

            tabletAreaEditor.LockAspectRatioChanged += HookAspectRatioLock;

            tabletAreaEditor.LockToUsableAreaChanged += HandleTabletAreaConstraint;
            displayAreaEditor.LockToUsableAreaChanged += HandleDisplayAreaConstraint;

            HookAspectRatioLock(tabletAreaEditor, EventArgs.Empty);
        }

        internal DisplayAreaEditor displayAreaEditor;
        internal TabletAreaEditor tabletAreaEditor;

        private bool arLockHooked;
        private bool handlingArLock;
        private bool handlingForcedArConstraint;
        private bool handlingSettingsChanging;
        private DirectBinding<float> displayWidth;
        private DirectBinding<float> displayHeight;
        private DirectBinding<float> tabletWidth;
        private DirectBinding<float> tabletHeight;

        private AbsoluteModeSettings? settings;
        public AbsoluteModeSettings? Settings
        {
            set
            {
                this.settings = value;
                this.OnSettingsChanged();
            }
            get => this.settings;
        }

        public event EventHandler<EventArgs>? SettingsChanged;

        protected virtual void OnSettingsChanged()
        {
            handlingSettingsChanging = true;
            SettingsChanged?.Invoke(this, new EventArgs());
            handlingSettingsChanging = false;
        }

        public BindableBinding<AbsoluteModeEditor, AbsoluteModeSettings?> SettingsBinding
        {
            get
            {
                return new BindableBinding<AbsoluteModeEditor, AbsoluteModeSettings?>(
                    this,
                    c => c.Settings,
                    (c, v) => c.Settings = v,
                    (c, h) => c.SettingsChanged += h,
                    (c, h) => c.SettingsChanged -= h
                );
            }
        }

        private void HookAspectRatioLock(object? sender, EventArgs args)
        {
            lock (this)
            {
                if (Settings?.LockAspectRatio ?? false)
                {
                    if (arLockHooked)
                        return;

                    HandleAspectRatioLock(tabletAreaEditor, EventArgs.Empty);

                    displayWidth.DataValueChanged += HandleAspectRatioLock;
                    displayHeight.DataValueChanged += HandleAspectRatioLock;
                    tabletWidth.DataValueChanged += HandleAspectRatioLock;
                    tabletHeight.DataValueChanged += HandleAspectRatioLock;
                    arLockHooked = true;
                }
                else
                {
                    if (!arLockHooked)
                        return;

                    displayWidth.DataValueChanged -= HandleAspectRatioLock;
                    displayHeight.DataValueChanged -= HandleAspectRatioLock;
                    tabletWidth.DataValueChanged -= HandleAspectRatioLock;
                    tabletHeight.DataValueChanged -= HandleAspectRatioLock;
                    arLockHooked = false;
                }
            }
        }

        private void HandleAspectRatioLock(object? sender, EventArgs e)
        {
            if (!tabletAreaEditor.LockAspectRatio) return;
            if (handlingArLock || handlingSettingsChanging || displayAreaEditor.handlingDisplayAreaResize) return;

            // Avoids looping
            handlingArLock = true;

            if (sender == tabletWidth || sender == displayHeight || sender == tabletAreaEditor || sender == displayAreaEditor)
            {
                var fullHeight = tabletAreaEditor.FullAreaBounds!.Value.Height;
                var scaledHeight = displayHeight.DataValue / displayWidth.DataValue * tabletWidth.DataValue;
                if (scaledHeight > fullHeight)
                {
                    tabletHeight.DataValue = fullHeight;
                    tabletWidth.DataValue = displayWidth.DataValue / displayHeight.DataValue * fullHeight;
                }
                else
                {
                    tabletHeight.DataValue = scaledHeight;
                }
            }
            else if (sender == tabletHeight || sender == displayWidth)
            {
                var fullWidth = tabletAreaEditor.FullAreaBounds!.Value.Width;
                var scaledWidth = displayWidth.DataValue / displayHeight.DataValue * tabletHeight.DataValue;
                if (scaledWidth > fullWidth)
                {
                    tabletWidth.DataValue = fullWidth;
                    tabletHeight.DataValue = displayHeight.DataValue / displayWidth.DataValue * fullWidth;
                }
                else
                {
                    tabletWidth.DataValue = scaledWidth;
                }
            }

            handlingArLock = false;
        }

        private void HandleTabletAreaConstraint(object? sender, EventArgs args)
        {
            ForceAreaConstraint(tabletAreaEditor.Display, args);
        }

        private void HandleDisplayAreaConstraint(object? sender, EventArgs args)
        {
            ForceAreaConstraint(displayAreaEditor.Display, args);
        }

        private void ForceAreaConstraint(object? sender, EventArgs args)
        {
            if (sender is not AreaDisplay display) return;

            if (!handlingForcedArConstraint && !handlingSettingsChanging && display.LockToUsableArea && display.Area != null)
            {
                handlingForcedArConstraint = true;
                Debug.Assert(display.FullAreaBounds.HasValue);
                var fullBounds = display.FullAreaBounds.Value;

                if (fullBounds.Width != 0 && fullBounds.Height != 0)
                {
                    if (display.Area.Rotation % 180 == 0)
                    {
                        if (display.Area.Width > fullBounds.Width)
                            display.Area.Width = fullBounds.Width;
                        if (display.Area.Height > fullBounds.Height)
                            display.Area.Height = fullBounds.Height;
                    }

                    var correction = GetOutOfBoundsAmount(display, display.Area.X, display.Area.Y);
                    display.Area.X -= correction.X;
                    display.Area.Y -= correction.Y;
                }

                handlingForcedArConstraint = false;
            }
        }

        private static Vector2 GetOutOfBoundsAmount(AreaDisplay display, float X, float Y)
        {
            Debug.Assert(display.FullAreaBounds.HasValue);
            var bounds = display.FullAreaBounds.Value;
            bounds.X = 0;
            bounds.Y = 0;

            var area = display.Area!;
            var rect = RectangleF.FromCenter(PointF.Empty, new SizeF(area.Width, area.Height));

            var corners = new PointF[]
            {
                    PointF.Rotate(rect.TopLeft, area.Rotation),
                    PointF.Rotate(rect.TopRight, area.Rotation),
                    PointF.Rotate(rect.BottomRight, area.Rotation),
                    PointF.Rotate(rect.BottomLeft, area.Rotation)
            };

            var pseudoArea = new RectangleF(
                PointF.Min(corners[0], PointF.Min(corners[1], PointF.Min(corners[2], corners[3]))),
                PointF.Max(corners[0], PointF.Max(corners[1], PointF.Max(corners[2], corners[3])))
            );

            pseudoArea.Center += new PointF(X, Y);

            return new Vector2
            {
                X = Math.Max(pseudoArea.Right - bounds.Right - 1, 0) + Math.Min(pseudoArea.Left - bounds.Left, 0),
                Y = Math.Max(pseudoArea.Bottom - bounds.Bottom - 1, 0) + Math.Min(pseudoArea.Top - bounds.Top, 0)
            };
        }


        // Converted from python code from stackoverflow:
        // https://stackoverflow.com/questions/16702966/rotate-image-and-crop-out-black-borders/16778797#16778797
        public static Vector2 GetLargestRectInRotatedRect(Vector2 rotatedRectDimensions, float rotationAngleDegrees)
        {
            var width = rotatedRectDimensions.X;
            var height = rotatedRectDimensions.Y;
            var rotationAngleRadians = Math.PI / 180 * rotationAngleDegrees;

            if (width <= 0 || height <= 0)
                return new Vector2(0, 0);

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

        // Due to the aspect ratio being locked, there is a known slope to calculate the largest aspect ratio locked area from (x = ratio * y)
        // Both the negative and positive variants of this slope must be used
        // Then we find the intersections along the top and right side lines of the tablet area (bottom and left would also be equivalent)
        // https://www.desmos.com/calculator/jueq5tuwv7
        // https://www.desmos.com/calculator/fnszuodzlh
        public static Vector2 GetLargestRectInRotatedRectRatioLocked(Vector2 rotatedRectDimensions, float rotationAngleDegrees, float aspectRatio)
        {
            var corners = new RectangleF(0, 0, rotatedRectDimensions.X, rotatedRectDimensions.Y).GetAreaCorners(rotationAngleDegrees);
            var (topLeft, topRight, bottomLeft, bottomRight) = (corners.TopLeft, corners.TopRight, corners.BottomLeft, corners.BottomRight);

            // y = mx + b

            // m: slope
            double mTopBottom = (topRight.Y - topLeft.Y) / (topRight.X - topLeft.X);
            double mRightLeft = (topLeft.Y - bottomLeft.Y) / (topLeft.X - bottomLeft.X);

            // Infinity obviously doesnt work in the calculations, set some really high number to approximate instead
            // This occurs when y = 0
            // For example, at 90 or 270 degrees the right side will be a perfectly straight vertical line
            // When `x = -b` (`x = 0m - b` or `0y = mx + b`) is converted to `y = mx + b` notation it equates to `y = ∞x + b`, instead lets use `y = 9999999x + b`
            if (Double.IsInfinity(mTopBottom) || Math.Abs(mTopBottom) == 0)
                mTopBottom = 9999999;
            if (Double.IsInfinity(mRightLeft) || Math.Abs(mRightLeft) == 0)
                mRightLeft = 9999999;

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
            double rightIntersectionXNeg = (bRight * -mRatio) / (1 - mRightLeft * -mRatio);

            // Solve for Y now that we have X
            // y = mx + b
            // y = mTopBottom * topIntersectionX + bTop
            // y = mRightLeft * rightIntersectionX + bRight
            double topIntersectionYPos = mTopBottom * topIntersectionXPos + bTop;
            double rightIntersectionYPos = mRightLeft * rightIntersectionXPos + bRight;
            double topIntersectionYNeg = mTopBottom * topIntersectionXNeg + bTop;
            // double rightIntersectionYNeg = mRightLeft * rightIntersectionXNeg + bRight;

            var leftTopPoint = new Vector2((float)Math.Abs(topIntersectionXNeg), (float)Math.Abs(topIntersectionYNeg));
            var rightTopPoint = new Vector2((float)Math.Abs(topIntersectionXPos), (float)Math.Abs(topIntersectionYPos));
            // leftSidePoint is redundant, only three points are necessary
            var rightSidePoint = new Vector2((float)Math.Abs(rightIntersectionXPos), (float)Math.Abs(rightIntersectionYPos));

            Vector2[] points = [leftTopPoint, rightTopPoint, rightSidePoint];
            // Discard points that dont fit within the area
            // Get the closest point to the center (measured diagonally, pythagorean this thing) and put it in Vector3.Z, besides special cases of 90 and 270 this will the correct point to choose
            var distances = points.Where(p => aspectRatio <= 1 ? p.Y <= rotatedRectDimensions.X && p.X <= rotatedRectDimensions.Y : p.X <= rotatedRectDimensions.X && p.Y <= rotatedRectDimensions.Y).Select(p => new Vector3(p.X, p.Y, p.X * p.X + p.Y * p.Y));

            var closest = distances.MinBy(p => p.Z);
            return new Vector2(closest.X * 2, closest.Y * 2);
        }
    }
}
