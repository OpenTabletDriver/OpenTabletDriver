using System;
using System.Collections.Generic;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Profiles;

namespace OpenTabletDriver.UX.Controls.Output.Area
{
    public abstract class AreaControl : Panel
    {
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
                field = value;
                this.OnAreaChanged();
            }
            get;
        }

        public bool LockToUsableArea
        {
            set
            {
                field = value;
                this.OnLockToUsableAreaChanged();
            }
            get;
        }

        public string Unit
        {
            set
            {
                field = value;
                this.OnUnitChanged();
            }
            get;
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
                field = value;
                this.OnInvalidForegroundErrorChanged();
            }
            get;
        }

        public string InvalidBackgroundError
        {
            set
            {
                field = value;
                this.OnInvalidBackgroundErrorChanged();
            }
            get;
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
