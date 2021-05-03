using System;
using System.Collections.Generic;
using Eto.Drawing;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Output.Area
{
    public abstract class AreaControl : Panel
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

        public BindableBinding<AreaControl, float> AreaWidthBinding
        {
            get
            {
                return new BindableBinding<AreaControl, float>(
                    this,
                    c => c.AreaWidth,
                    (c, v) => c.AreaWidth = v,
                    (c, h) => c.AreaWidthChanged += h,
                    (c, h) => c.AreaWidthChanged -= h
                );
            }
        }

        public BindableBinding<AreaControl, float> AreaHeightBinding
        {
            get
            {
                return new BindableBinding<AreaControl, float>(
                    this,
                    c => c.AreaHeight,
                    (c, v) => c.AreaHeight = v,
                    (c, h) => c.AreaHeightChanged += h,
                    (c, h) => c.AreaHeightChanged -= h
                );
            }
        }

        public BindableBinding<AreaControl, float> AreaXOffsetBinding
        {
            get
            {
                return new BindableBinding<AreaControl, float>(
                    this,
                    c => c.AreaXOffset,
                    (c, v) => c.AreaXOffset = v,
                    (c, h) => c.AreaXOffsetChanged += h,
                    (c, h) => c.AreaXOffsetChanged -= h
                );
            }
        }

        public BindableBinding<AreaControl, float> AreaYOffsetBinding
        {
            get
            {
                return new BindableBinding<AreaControl, float>(
                    this,
                    c => c.AreaYOffset,
                    (c, v) => c.AreaYOffset = v,
                    (c, h) => c.AreaYOffsetChanged += h,
                    (c, h) => c.AreaYOffsetChanged -= h
                );
            }
        }

        public BindableBinding<AreaControl, float> AreaRotationBinding
        {
            get
            {
                return new BindableBinding<AreaControl, float>(
                    this,
                    c => c.AreaRotation,
                    (c, v) => c.AreaRotation = v,
                    (c, h) => c.AreaRotationChanged += h,
                    (c, h) => c.AreaRotationChanged -= h
                );
            }
        }

        public BindableBinding<AreaControl, bool> LockToUsableAreaBinding
        {
            get
            {
                return new BindableBinding<AreaControl, bool>(
                    this,
                    c => c.LockToUsableArea,
                    (c, v) => c.LockToUsableArea = v,
                    (c, h) => c.LockToUsableAreaChanged += h,
                    (c, h) => c.LockToUsableAreaChanged -= h
                );
            }
        }

        public BindableBinding<AreaControl, string> UnitBinding
        {
            get
            {
                return new BindableBinding<AreaControl, string>(
                    this,
                    c => c.Unit,
                    (c, v) => c.Unit = v,
                    (c, h) => c.UnitChanged += h,
                    (c, h) => c.UnitChanged -= h
                );
            }
        }

        public BindableBinding<AreaControl, IEnumerable<RectangleF>> AreaBoundsBinding
        {
            get
            {
                return new BindableBinding<AreaControl, IEnumerable<RectangleF>>(
                    this,
                    c => c.AreaBounds,
                    (c, v) => c.AreaBounds = v,
                    (c, h) => c.AreaBoundsChanged += h,
                    (c, h) => c.AreaBoundsChanged -= h
                );
            }
        }

        public BindableBinding<AreaControl, RectangleF> FullAreaBoundsBinding
        {
            get
            {
                return new BindableBinding<AreaControl, RectangleF>(
                    this,
                    c => c.FullAreaBounds,
                    (c, v) => c.FullAreaBounds = v,
                    (c, h) => c.FullAreaBoundsChanged += h,
                    (c, h) => c.FullAreaBoundsChanged -= h
                );
            }
        }

        public BindableBinding<AreaControl, string> InvalidForegroundErrorBinding
        {
            get
            {
                return new BindableBinding<AreaControl, string>(
                    this,
                    c => c.InvalidForegroundError,
                    (c, v) => c.InvalidForegroundError = v,
                    (c, h) => c.InvalidForegroundErrorChanged += h,
                    (c, h) => c.InvalidForegroundErrorChanged -= h
                );
            }
        }
        
        public BindableBinding<AreaControl, string> InvalidBackgroundErrorBinding
        {
            get
            {
                return new BindableBinding<AreaControl, string>(
                    this,
                    c => c.InvalidBackgroundError,
                    (c, v) => c.InvalidBackgroundError = v,
                    (c, h) => c.InvalidBackgroundErrorChanged += h,
                    (c, h) => c.InvalidBackgroundErrorChanged -= h
                );
            }
        }
    }
}