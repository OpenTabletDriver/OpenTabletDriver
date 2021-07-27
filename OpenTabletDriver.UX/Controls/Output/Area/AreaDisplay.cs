using System;
using System.Collections.Generic;
using System.Numerics;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Controls.Output.Area
{
    public class AreaDisplay : ScheduledDrawable
    {
        private AreaSettings area;
        private bool lockToUsableArea;
        private string unit, invalidForegroundError, invalidBackgroundError;
        protected IEnumerable<RectangleF> areaBounds;
        private RectangleF fullAreaBounds;

        public event EventHandler<EventArgs> AreaChanged;
        public event EventHandler<EventArgs> LockToUsableAreaChanged;
        public event EventHandler<EventArgs> UnitChanged;
        public event EventHandler<EventArgs> AreaBoundsChanged;
        public event EventHandler<EventArgs> FullAreaBoundsChanged;
        public event EventHandler<EventArgs> InvalidForegroundErrorChanged;
        public event EventHandler<EventArgs> InvalidBackgroundErrorChanged;

        protected virtual void OnAreaChanged() => AreaChanged?.Invoke(this, new EventArgs());
        protected virtual void OnLockToUsableAreaChanged() => LockToUsableAreaChanged?.Invoke(this, new EventArgs());
        protected virtual void OnUnitChanged() => UnitChanged?.Invoke(this, new EventArgs());
        protected virtual void OnAreaBoundsChanged() => AreaBoundsChanged?.Invoke(this, new EventArgs());
        protected virtual void OnFullAreaBoundsChanged() => FullAreaBoundsChanged?.Invoke(this, new EventArgs());
        protected virtual void OnInvalidForegroundErrorChanged() => InvalidForegroundErrorChanged?.Invoke(this, new EventArgs());
        protected virtual void OnInvalidBackgroundErrorChanged() => InvalidBackgroundErrorChanged?.Invoke(this, new EventArgs());

        public AreaSettings Area
        {
            set
            {
                this.area = value;
                this.OnAreaChanged();
            }
            get => this.area;
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

        public BindableBinding<AreaDisplay, AreaSettings> AreaBinding
        {
            get
            {
                return new BindableBinding<AreaDisplay, AreaSettings>(
                    this,
                    c => c.Area,
                    (c, v) => c.Area = v,
                    (c, h) => c.AreaChanged += h,
                    (c, h) => c.AreaChanged -= h
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

        private RectangleF ForegroundRect => Area == null ? RectangleF.Empty : RectangleF.FromCenter(
            new PointF(Area.X, Area.Y),
            new SizeF(Area.Width, Area.Height)
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

                        var rect = RectangleF.FromCenter(PointF.Empty, new SizeF(Area.Width, Area.Height));

                        var corners = new PointF[]
                        {
                            PointF.Rotate(rect.TopLeft, Area.Rotation),
                            PointF.Rotate(rect.TopRight, Area.Rotation),
                            PointF.Rotate(rect.BottomRight, Area.Rotation),
                            PointF.Rotate(rect.BottomLeft, Area.Rotation)
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

                    Area.X = newX;
                    Area.Y = newY;
                }
                else
                {
                    mouseOffset = e.Location;
                    viewModelOffset = new PointF(Area.X, Area.Y);
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
                graphics.RotateTransform(Area.Rotation);
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
            string ratio = Math.Round(Area.Width / Area.Height, 4).ToString();
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
            string widthText = $"{MathF.Round(Area.Width, 3)}{Unit}";
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
                string heightText = $"{MathF.Round(Area.Height, 3)}{Unit}";
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
