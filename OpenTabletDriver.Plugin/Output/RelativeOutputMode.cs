using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin.Output
{
    [PluginIgnore]
    public abstract class RelativeOutputMode : IOutputMode
    {
        public RelativeOutputMode()
        {
            stopwatch.Start();
        }
        
        private IList<IFilter> filters, preFilters, postFilters;
        private Vector2? lastPos;
        private Stopwatch stopwatch = new Stopwatch();
        private TimeSpan lastElapsed = default;
        private bool skipReport = false;
        private Matrix3x2 transformationMatrix;

        public IList<IFilter> Filters
        {
            set
            {
                this.filters = value;
                if (Info.Driver.InterpolatorActive)
                    this.preFilters = Filters.Where(t => t.FilterStage == FilterStage.PreTranspose).ToList();
                else
                    this.preFilters = Filters.Where(t => t.FilterStage == FilterStage.PreTranspose || t.FilterStage == FilterStage.PreInterpolate).ToList();
                this.postFilters = filters.Where(t => t.FilterStage == FilterStage.PostTranspose).ToList();
            }
            get => this.filters;
        }

        public abstract IRelativePointer Pointer { get; }

        private TabletState tablet;
        public TabletState Tablet
        {
            set
            {
                this.tablet = value;
                UpdateTransformMatrix();
            }
            get => this.tablet;
        }

        private Vector2 sensitivity;
        public Vector2 Sensitivity
        {
            set
            {
                this.sensitivity = value;
                UpdateTransformMatrix();
            }
            get => this.sensitivity;
        }

        private float rotation;
        public float Rotation
        {
            set
            {
                this.rotation = value;
                UpdateTransformMatrix();
            }
            get => this.rotation;
        }

        private void UpdateTransformMatrix()
        {
            this.skipReport = true;     // Prevents cursor from jumping on sensitivity change

            this.transformationMatrix = Matrix3x2.CreateRotation(
                (float)(-Rotation * System.Math.PI / 180));

            this.transformationMatrix *= Matrix3x2.CreateScale(
                sensitivity.X * ((Tablet?.Digitizer?.Width / Tablet?.Digitizer?.MaxX) ?? 0.01f),
                sensitivity.Y * ((Tablet?.Digitizer?.Height / Tablet?.Digitizer?.MaxY) ?? 0.01f));
        }

        public TimeSpan ResetTime { set; get; }

        public virtual void Read(IDeviceReport report)
        {
            if (report is ITabletReport tabletReport)
            {
                if (Tablet.Digitizer.ActiveReportID.IsInRange(tabletReport.ReportID))
                {
                    if (Transpose(tabletReport) is Vector2 position)
                        Pointer.Translate(position);
                }
            }
        }

        public Vector2? Transpose(ITabletReport report)
        {
            var currElapsed = stopwatch.Elapsed;
            var deltaTime = currElapsed - lastElapsed;
            lastElapsed = currElapsed;

            var pos = report.Position;

            // Pre Filter
            foreach (IFilter filter in this.preFilters)
                pos = filter.Filter(pos);

            pos = Vector2.Transform(pos, this.transformationMatrix);

            // Post Filter
            foreach (IFilter filter in this.postFilters)
                pos = filter.Filter(pos);

            var delta = pos - this.lastPos;
            this.lastPos = pos;

            if (skipReport)
            {
                skipReport = false;
                return null;
            }
            return (deltaTime > ResetTime) ? null : delta;
        }
    }
}
