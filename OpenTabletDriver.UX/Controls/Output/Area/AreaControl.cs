using System;
using System.Collections.Generic;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Profiles;

namespace OpenTabletDriver.UX.Controls.Output.Area
{
    public abstract class AreaControl : Panel
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

        public BindableBinding<AreaControl, AreaSettings> AreaBinding
        {
            get
            {
                return new BindableBinding<AreaControl, AreaSettings>(
                    this,
                    c => c.Area,
                    (c, v) => c.Area = v,
                    (c, h) => c.AreaChanged += h,
                    (c, h) => c.AreaChanged -= h
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
