using System;
using System.Collections.Generic;
using System.Numerics;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Controls.Output.Area
{
    public class AreaDisplay : TimedDrawable
    {
        private float areaWidth, areaHeight, areaX, areaY, areaRotation;
        private bool lockToUsableArea;
        private string unit, invalidForegroundError, invalidBackgroundError;
        protected IEnumerable<RectangleF> areaBounds;
        private RectangleF fullAreaBounds;

        public event EventHandler<EventArgs> AreaWidthChanged;
        public event EventHandler<EventArgs> AreaHeightChanged;
        public event EventHandler<EventArgs> AreaXOffsetChanged;
        public event EventHandler<EventArgs> AreaYOffsetChanged;
        public event EventHandler<EventArgs> AreaRotationChanged;
        public event EventHandler<EventArgs> LockToUsableAreaChanged;
        public event EventHandler<EventArgs> UnitChanged;
        public event EventHandler<EventArgs> AreaBoundsChanged;
        public event EventHandler<EventArgs> FullAreaBoundsChanged;
        public event EventHandler<EventArgs> InvalidForegroundErrorChanged;
        public event EventHandler<EventArgs> InvalidBackgroundErrorChanged;

        protected virtual void OnAreaWidthChanged() => AreaWidthChanged?.Invoke(this, new EventArgs());
        protected virtual void OnAreaHeightChanged() => AreaHeightChanged?.Invoke(this, new EventArgs());
        protected virtual void OnAreaXOffsetChanged() => AreaXOffsetChanged?.Invoke(this, new EventArgs());
        protected virtual void OnAreaYOffsetChanged() => AreaYOffsetChanged?.Invoke(this, new EventArgs());
        protected virtual void OnAreaRotationChanged() => AreaRotationChanged?.Invoke(this, new EventArgs());
        protected virtual void OnLockToUsableAreaChanged() => LockToUsableAreaChanged?.Invoke(this, new EventArgs());
        protected virtual void OnUnitChanged() => UnitChanged?.Invoke(this, new EventArgs());
        protected virtual void OnAreaBoundsChanged() => AreaBoundsChanged?.Invoke(this, new EventArgs());
        protected virtual void OnFullAreaBoundsChanged() => FullAreaBoundsChanged?.Invoke(this, new EventArgs());
        protected virtual void OnInvalidForegroundErrorChanged() => InvalidForegroundErrorChanged?.Invoke(this, new EventArgs());
        protected virtual void OnInvalidBackgroundErrorChanged() => InvalidBackgroundErrorChanged?.Invoke(this, new EventArgs());

        public float AreaWidth
        {
            set
            {
                this.areaWidth = value;
                this.OnAreaWidthChanged();
            }
            get => this.areaWidth;
        }

        public float AreaHeight
        {
            set
            {
                this.areaHeight = value;
                this.OnAreaHeightChanged();
            }
            get => this.areaHeight;
        }

        public float AreaXOffset
        {
            set
            {
                this.areaX = value;
                this.OnAreaXOffsetChanged();
            }
            get => this.areaX;
        }

        public float AreaYOffset
        {
            set
            {
                this.areaY = value;
                this.OnAreaYOffsetChanged();
            }
            get => this.areaY;
        }

        public float AreaRotation
        {
            set
            {
                this.areaRotation = value;
                this.OnAreaRotationChanged();
            }
            get => this.areaRotation;
        }

        public bool LockToUsableArea
        {
            set
            {
                this.lockToUsableArea = value;
                this.OnLockToUsableAreaChanged();
            }
            get => this.lockToUsableArea;
        }

        public string Unit
        {
            set
            {
                this.unit = value;
                this.OnUnitChanged();
            }
            get => this.unit;
        }

        public virtual IEnumerable<RectangleF> AreaBounds
        {
            set
            {
                this.areaBounds = value;
                this.OnAreaBoundsChanged();
            }
            get => this.areaBounds;
        }

        public RectangleF FullAreaBounds
        {
            protected set
            {
                this.fullAreaBounds = value;
                this.OnFullAreaBoundsChanged();
            }
            get => this.fullAreaBounds;
        }

        public string InvalidForegroundError
        {
            set
            {
                this.invalidForegroundError = value;
                this.OnInvalidForegroundErrorChanged();
            }
            get => this.invalidForegroundError;
        }

        public string InvalidBackgroundError
        {
            set
            {
                this.invalidBackgroundError = value;
                this.OnInvalidBackgroundErrorChanged();
            }
            get => this.invalidBackgroundError;
        }

        public BindableBinding<AreaDisplay, float> AreaWidthBinding
        {
            get
            {
                return new BindableBinding<AreaDisplay, float>(
                    this,
                    c => c.AreaWidth,
                    (c, v) => c.AreaWidth = v,
                    (c, h) => c.AreaWidthChanged += h,
                    (c, h) => c.AreaWidthChanged -= h
                );
            }
        }

        public BindableBinding<AreaDisplay, float> AreaHeightBinding
        {
            get
            {
                return new BindableBinding<AreaDisplay, float>(
                    this,
                    c => c.AreaHeight,
                    (c, v) => c.AreaHeight = v,
                    (c, h) => c.AreaHeightChanged += h,
                    (c, h) => c.AreaHeightChanged -= h
                );
            }
        }

        public BindableBinding<AreaDisplay, float> AreaXOffsetBinding
        {
            get
            {
                return new BindableBinding<AreaDisplay, float>(
                    this,
                    c => c.AreaXOffset,
                    (c, v) => c.AreaXOffset = v,
                    (c, h) => c.AreaXOffsetChanged += h,
                    (c, h) => c.AreaXOffsetChanged -= h
                );
            }
        }

        public BindableBinding<AreaDisplay, float> AreaYOffsetBinding
        {
            get
            {
                return new BindableBinding<AreaDisplay, float>(
                    this,
                    c => c.AreaYOffset,
                    (c, v) => c.AreaYOffset = v,
                    (c, h) => c.AreaYOffsetChanged += h,
                    (c, h) => c.AreaYOffsetChanged -= h
                );
            }
        }

        public BindableBinding<AreaDisplay, float> AreaRotationBinding
        {
            get
            {
                return new BindableBinding<AreaDisplay, float>(
                    this,
                    c => c.AreaRotation,
                    (c, v) => c.AreaRotation = v,
                    (c, h) => c.AreaRotationChanged += h,
                    (c, h) => c.AreaRotationChanged -= h
                );
            }
        }

        public BindableBinding<AreaDisplay, bool> LockToUsableAreaBinding
        {
            get
            {
                return new BindableBinding<AreaDisplay, bool>(
                    this,
                    c => c.LockToUsableArea,
                    (c, v) => c.LockToUsableArea = v,
                    (c, h) => c.LockToUsableAreaChanged += h,
                    (c, h) => c.LockToUsableAreaChanged -= h
                );
            }
        }

        public BindableBinding<AreaDisplay, string> UnitBinding
        {
            get
            {
                return new BindableBinding<AreaDisplay, string>(
                    this,
                    c => c.Unit,
                    (c, v) => c.Unit = v,
                    (c, h) => c.UnitChanged += h,
                    (c, h) => c.UnitChanged -= h
                );
            }
        }

        public BindableBinding<AreaDisplay, IEnumerable<RectangleF>> AreaBoundsBinding
        {
            get
            {
                return new BindableBinding<AreaDisplay, IEnumerable<RectangleF>>(
                    this,
                    c => c.AreaBounds,
                    (c, v) => c.AreaBounds = v,
                    (c, h) => c.AreaBoundsChanged += h,
                    (c, h) => c.AreaBoundsChanged -= h
                );
            }
        }

        public BindableBinding<AreaDisplay, RectangleF> FullAreaBoundsBinding
        {
            get
            {
                return new BindableBinding<AreaDisplay, RectangleF>(
                    this,
                    c => c.FullAreaBounds,
                    (c, v) => c.FullAreaBounds = v,
                    (c, h) => c.FullAreaBoundsChanged += h,
                    (c, h) => c.FullAreaBoundsChanged -= h
                );
            }
        }

        public BindableBinding<AreaDisplay, string> InvalidForegroundErrorBinding
        {
            get
            {
                return new BindableBinding<AreaDisplay, string>(
                    this,
                    c => c.InvalidForegroundError,
                    (c, v) => c.InvalidForegroundError = v,
                    (c, h) => c.InvalidForegroundErrorChanged += h,
                    (c, h) => c.InvalidForegroundErrorChanged -= h
                );
            }
        }
        
        public BindableBinding<AreaDisplay, string> InvalidBackgroundErrorBinding
        {
            get
            {
                return new BindableBinding<AreaDisplay, string>(
                    this,
                    c => c.InvalidBackgroundError,
                    (c, v) => c.InvalidBackgroundError = v,
                    (c, h) => c.InvalidBackgroundErrorChanged += h,
                    (c, h) => c.InvalidBackgroundErrorChanged -= h
                );
            }
        }

        private static readonly Font Font = SystemFonts.User(8);
        private static readonly Brush TextBrush = new SolidBrush(SystemColors.ControlText);

        private readonly Color AccentColor = SystemColors.Highlight;
        private readonly Color AreaBoundsFillColor = SystemColors.ControlBackground;
        private readonly Color AreaBoundsBorderColor = SystemInterop.CurrentPlatform switch
        {
            PluginPlatform.Windows => new Color(64, 64, 64),
            _                      => SystemColors.Control
        };

        private bool mouseDragging;
        private PointF? mouseOffset;
        private PointF? viewModelOffset;

        private RectangleF ForegroundRect => RectangleF.FromCenter(
            new PointF(AreaXOffset, AreaYOffset),
            new SizeF(AreaWidth, AreaHeight)
        );

        public float PixelScale => CalculateScale(FullAreaBounds);

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            switch (e.Buttons)
            {
                case MouseButtons.Primary:
                {
                    mouseDragging = true;
                    break;
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            switch (e.Buttons)
            {
                case MouseButtons.Primary:
                {
                    mouseDragging = false;
                    break;
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (mouseDragging)
            {
                if (mouseOffset != null)
                {
                    var delta = e.Location - mouseOffset.Value;
                    var newX = viewModelOffset.Value.X + (delta.X / PixelScale);
                    var newY = viewModelOffset.Value.Y + (delta.Y / PixelScale);

                    if (LockToUsableArea)
                    {
                        var bounds = FullAreaBounds;
                        bounds.X = 0;
                        bounds.Y = 0;

                        var rect = RectangleF.FromCenter(PointF.Empty, new SizeF(AreaWidth, AreaHeight));

                        var corners = new PointF[]
                        {
                            PointF.Rotate(rect.TopLeft, AreaRotation),
                            PointF.Rotate(rect.TopRight, AreaRotation),
                            PointF.Rotate(rect.BottomRight, AreaRotation),
                            PointF.Rotate(rect.BottomLeft, AreaRotation)
                        };
                        var pseudoArea = new RectangleF(
                            PointF.Min(corners[0], PointF.Min(corners[1], PointF.Min(corners[2], corners[3]))),
                            PointF.Max(corners[0], PointF.Max(corners[1], PointF.Max(corners[2], corners[3])))
                        );
                        pseudoArea.Center += new PointF(newX, newY);

                        var correction = OutOfBoundsAmount(bounds, pseudoArea);
                        newX -= correction.X;
                        newY -= correction.Y;
                    }

                    AreaXOffset = newX;
                    AreaYOffset = newY;
                }
                else
                {
                    mouseOffset = e.Location;
                    viewModelOffset = new PointF(AreaXOffset, AreaYOffset);
                }
            }
            else if (!mouseDragging && mouseOffset != null)
            {
                mouseOffset = null;
                viewModelOffset = null;
            }
        }

        protected override void OnNextFrame(PaintEventArgs e)
        {
            var graphics = e.Graphics;

            switch (IsValid(ForegroundRect), IsValid(FullAreaBounds))
            {
                case (true, true):
                {
                    using (graphics.SaveTransformState())
                    {
                        var scale = CalculateScale(FullAreaBounds);

                        var clientCenter = new PointF(this.ClientSize.Width, this.ClientSize.Height) / 2;
                        var backgroundCenter = new PointF(FullAreaBounds.Width, FullAreaBounds.Height) / 2 * scale;
                        var offset = clientCenter - backgroundCenter;

                        graphics.TranslateTransform(offset);

                        DrawBackground(graphics, scale);
                        DrawForeground(graphics, scale);
                    }
                    break;
                }
                case (_, false):
                {
                    DrawText(graphics, InvalidBackgroundError);
                    break;
                }
                case (false, _):
                {
                    DrawText(graphics, InvalidForegroundError);
                    break;
                }
            }
        }

        private void DrawBackground(Graphics graphics, float scale)
        {
            using (graphics.SaveTransformState())
            {
                graphics.TranslateTransform(-FullAreaBounds.TopLeft * scale);
                foreach (var rect in AreaBounds)
                {
                    var scaledRect = rect * scale;
                    graphics.FillRectangle(AreaBoundsFillColor, scaledRect);
                    graphics.DrawRectangle(AreaBoundsBorderColor, scaledRect);
                }
            }
        }

        private void DrawForeground(Graphics graphics, float scale)
        {
            using (graphics.SaveTransformState())
            {
                var area = ForegroundRect * scale;

                graphics.TranslateTransform(area.Center);
                graphics.RotateTransform(AreaRotation);
                graphics.TranslateTransform(-area.Center);

                graphics.FillRectangle(AccentColor, area);

                var originEllipse = new RectangleF(0, 0, 1, 1);
                originEllipse.Offset(area.Center - (originEllipse.Size / 2));
                graphics.DrawEllipse(SystemColors.ControlText, originEllipse);

                DrawRatioText(graphics, area);
                DrawWidthText(graphics, area);
                DrawHeightText(graphics, area);
            }
        }

        private void DrawRatioText(Graphics graphics, RectangleF area)
        {
            string ratio = Math.Round(AreaWidth / AreaHeight, 4).ToString();
            SizeF ratioMeasure = graphics.MeasureString(Font, ratio);
            var offsetY = area.Center.Y + (ratioMeasure.Height / 2);
            if (offsetY + ratioMeasure.Height > area.Y + area.Height)
                offsetY = area.Y + area.Height;

            var ratioPos = new PointF(
                area.Center.X - (ratioMeasure.Width / 2),
                offsetY
            );
            graphics.DrawText(Font, TextBrush, ratioPos, ratio);
        }

        private void DrawWidthText(Graphics graphics, RectangleF area)
        {
            var minDist = area.Center.Y - 40;
            string widthText = $"{MathF.Round(AreaWidth, 3)}{Unit}";
            var widthTextSize = graphics.MeasureString(Font, widthText);
            var widthTextPos = new PointF(
                area.MiddleTop.X - (widthTextSize.Width / 2),
                Math.Min(area.MiddleTop.Y, minDist)
            );
            graphics.DrawText(Font, TextBrush, widthTextPos, widthText);
        }

        private void DrawHeightText(Graphics graphics, RectangleF area)
        {
            using (graphics.SaveTransformState())
            {
                var minDist = area.Center.X - 40;
                string heightText = $"{MathF.Round(AreaHeight, 3)}{Unit}";
                var heightSize = graphics.MeasureString(Font, heightText) / 2;
                var heightPos = new PointF(
                    -area.MiddleLeft.Y - heightSize.Width,
                    Math.Min(area.MiddleLeft.X, minDist)
                );
                graphics.RotateTransform(-90);
                graphics.DrawText(Font, TextBrush, heightPos, heightText);
            }
        }

        private void DrawText(Graphics graphics, string errorText)
        {
            var errSize = graphics.MeasureString(Font, errorText);
            var errorOffset = new PointF(errSize.Width, errSize.Height) / 2;
            var clientOffset = new PointF(this.ClientSize.Width, this.ClientSize.Height) / 2;
            var offset = clientOffset - errorOffset;

            graphics.DrawText(Font, TextBrush, offset, errorText);
        }

        private float CalculateScale(RectangleF rect)
        {
            float scaleX = (this.ClientSize.Width - 2) / rect.Width;
            float scaleY = (this.ClientSize.Height - 2) / rect.Height;
            return scaleX > scaleY ? scaleY : scaleX;
        }

        private static bool IsValid(RectangleF rect)
        {
            return rect.Width > 0 && rect.Height > 0;
        }

        private static Vector2 OutOfBoundsAmount(RectangleF bounds, RectangleF rect)
        {
            return new Vector2
            {
                X = Math.Max(rect.Right - bounds.Right - 1, 0) + Math.Min(rect.Left - bounds.Left, 0),
                Y = Math.Max(rect.Bottom - bounds.Bottom - 1, 0) + Math.Min(rect.Top - bounds.Top, 0)
            };
        }
    }
}
