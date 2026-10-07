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

            var displayRatio = displayAreaEditor.Area!.Width / displayAreaEditor.Area!.Height;

            var tryHeightFirst = sender == tabletWidth || sender == displayHeight || sender == tabletAreaEditor || sender == displayAreaEditor && displayRatio <= 1;
            var tryWidthFirst = sender == tabletHeight || sender == displayWidth || sender == displayAreaEditor && displayRatio > 1;

            if (tryHeightFirst)
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
            else if (tryWidthFirst)
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
        // https://www.desmos.com/calculator/hc53wxwpdc
        // https://www.desmos.com/calculator/yyy1o4n094
        public static Vector2 GetLargestRectInRotatedRectRatioLocked(Vector2 rotatedRectDimensions, float rotationAngleDegrees, float aspectRatio)
        {
            var corners = new RectangleF(0, 0, rotatedRectDimensions.X, rotatedRectDimensions.Y).GetAreaCorners(rotationAngleDegrees);
            var (topLeft, topRight, bottomLeft, bottomRight) = (corners.TopLeft, corners.TopRight, corners.BottomLeft, corners.BottomRight);

            // y = mx + b

            // m: slope
            double mVertical = (topRight.Y - topLeft.Y) / (topRight.X - topLeft.X);
            double mHorizontal = (topLeft.Y - bottomLeft.Y) / (topLeft.X - bottomLeft.X);

            // Infinity obviously doesnt work in the calculations, set some really high number to approximate instead
            // This occurs when y = 0
            // For example, at 90 or 270 degrees the right side will be a perfectly straight vertical line
            // When `x = -b` (`x = 0m - b` or `0y = mx + b`) is converted to `y = mx + b` notation it equates to `y = ∞x + b`, instead lets use `y = 9999999x + b`
            if (Double.IsInfinity(mVertical) || Math.Abs(mVertical) == 0)
                mVertical = 9999999;
            if (Double.IsInfinity(mHorizontal) || Math.Abs(mHorizontal) == 0)
                mHorizontal = 9999999;

            // Aspect ratio slope is reversed from what we want: x = mRatio * y
            // Later it must become: y = x / mRatio
            double mRatio = aspectRatio;

            // b: y intercept
            double bVerticalRight = topRight.Y - mVertical * topRight.X;
            double bHorizontalTop = topLeft.Y - mHorizontal * topLeft.X;

            // Get intersection on X axis of ratio and side by setting them equal
            // x / mRatio = mVertical * x + bVerticalRight -> x = (bVerticalRight * mRatio) / (1 - mVertical * mRatio)
            // x / mRatio = mHorizontal * x + bHorizontalTop -> x = (bHorizontalTop * mRatio) / (1 - mHorizontal * mRatio)
            double rightSideIntersectionXPos = (bVerticalRight * mRatio) / (1 - mVertical * mRatio);
            double rightTopIntersectionXPos = (bHorizontalTop * mRatio) / (1 - mHorizontal * mRatio);
            double rightBottomIntersectionXNeg = (bVerticalRight * -mRatio) / (1 - mVertical * -mRatio);
            double leftTopIntersectionXNeg = (bHorizontalTop * -mRatio) / (1 - mHorizontal * -mRatio);

            // Solve for Y now that we have X
            // y = mx + b
            // y = mVertical * topIntersectionX + bVerticalRight
            // y = mHorizontal * rightIntersectionX + bHorizontalTop
            double rightSideIntersectionYPos = mVertical * rightSideIntersectionXPos + bVerticalRight;
            double rightTopIntersectionYPos = mHorizontal * rightTopIntersectionXPos + bHorizontalTop;
            double rightBottomIntersectionYNeg = mVertical * rightBottomIntersectionXNeg + bVerticalRight;
            double leftTopIntersectionYNeg = mHorizontal * leftTopIntersectionXNeg + bHorizontalTop;

            var rightBottomPoint = new Vector2((float)Math.Abs(rightBottomIntersectionXNeg), (float)Math.Abs(rightBottomIntersectionYNeg));
            var rightSidePoint = new Vector2((float)Math.Abs(rightSideIntersectionXPos), (float)Math.Abs(rightSideIntersectionYPos));
            var leftTopPoint = new Vector2((float)Math.Abs(leftTopIntersectionXNeg), (float)Math.Abs(leftTopIntersectionYNeg));
            var rightTopPoint = new Vector2((float)Math.Abs(rightTopIntersectionXPos), (float)Math.Abs(rightTopIntersectionYPos));

            Vector2[] points = [rightBottomPoint, rightSidePoint, leftTopPoint, rightTopPoint];
            // Discard points that dont fit within the area
            var filteredPoints = points.Where(p => aspectRatio <= 1 ? p.Y <= rotatedRectDimensions.X && p.X <= rotatedRectDimensions.Y : p.X <= rotatedRectDimensions.X && p.Y <= rotatedRectDimensions.Y);

            // Fix special case when all points end up broken from 90 or 270 degree rotation and unrecoverable infinite slope
            if (filteredPoints.Count() == 0)
            {
                Vector2[] specialPoints = [
                    // x = cornerPoint
                    // y = x * mRatio
                    new Vector2(Math.Abs(topRight.X), (float)Math.Abs(topRight.X / mRatio)),
                    // x = y * mRatio
                    // y = cornerPoint
                    new Vector2((float)Math.Abs(topRight.Y * mRatio), Math.Abs(topRight.Y)),
                ];
                filteredPoints = specialPoints;
            }

            // Get the closest point to the center (measured diagonally, pythagorean this thing) and put it in Vector3.Z, besides special cases of 90 and 270 this will the correct point to choose
            var distances = filteredPoints.Select(p => new Vector3(p.X, p.Y, p.X * p.X + p.Y * p.Y));

            var closest = distances.MinBy(p => p.Z);
            return new Vector2(closest.X * 2, closest.Y * 2);
        }
    }
}
