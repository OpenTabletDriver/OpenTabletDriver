using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin.Output
{
    [PluginIgnore]
    public abstract class RelativeOutputMode : BindingHandler, IOutputMode
    {
        private List<IFilter> _filters, _preFilters = new List<IFilter>(), _postFilters = new List<IFilter>();
        public IEnumerable<IFilter> Filters
        {
            set
            {
                _filters = value.ToList();
                _preFilters.Clear();
                _postFilters.Clear();
                foreach (IFilter filter in _filters)
                    if (filter.FilterStage == FilterStage.PreTranspose)
                        _preFilters.Add(filter);
                    else if (filter.FilterStage == FilterStage.PostTranspose)
                        _postFilters.Add(filter);
            }
            get => _filters;
        }

        public abstract IVirtualMouse VirtualMouse { get; }
        public IVirtualPointer Pointer => VirtualMouse;

        private Vector2 _sensitivity;
        private float _rotation;
        private Matrix3x2 _transformationMatrix;
        public Vector2 Sensitivity
        {
            set
            {
                _sensitivity = value;
                UpdateTransformMatrix();
            }
            get { return _sensitivity; }
        }

        public float Rotation
        {
            set
            {
                _rotation = value;
                UpdateTransformMatrix();
            }
            get { return _rotation; }
        }

        private void UpdateTransformMatrix()
        {
            _transformationMatrix = Matrix3x2.CreateRotation(
                (float)(_rotation * System.Math.PI / 180));

            _transformationMatrix *= Matrix3x2.CreateScale(
                _sensitivity.X * ((Digitizer?.Width / Digitizer?.MaxX) ?? 0.01f),
                _sensitivity.Y * ((Digitizer?.Height / Digitizer?.MaxY) ?? 0.01f));
        }

        public TimeSpan ResetTime { set; get; }

        private Vector2? _lastPos;
        private DateTime _lastReceived;

        public virtual void Read(IDeviceReport report)
        {
            if (report is ITabletReport tabletReport)
            {
                if (Digitizer.ActiveReportID.IsInRange(tabletReport.ReportID))
                {
                    if (Transpose(tabletReport) is Vector2 pos)
                    {
                        if (VirtualMouse is IPressureHandler pressureHandler)
                            pressureHandler.SetPressure((float)tabletReport.Pressure / (float)Digitizer.MaxPressure);
                        
                        VirtualMouse.Move(pos.X, pos.Y);
                    }
                }
            }
        }
        
        protected Vector2? Transpose(ITabletReport report)
        {
            var difference = DateTime.Now - _lastReceived;
            _lastReceived = DateTime.Now;

            var pos = report.Position;

            // Pre Filter
            foreach (IFilter filter in _preFilters)
                pos = filter.Filter(pos);

            pos = Vector2.Transform(pos, _transformationMatrix);

            // Post Filter
            foreach (IFilter filter in _postFilters)
                pos = filter.Filter(pos);

            var delta = pos - _lastPos;
            _lastPos = pos;

            return (difference > ResetTime && _lastReceived != default) ? null : delta;
        }
    }
}