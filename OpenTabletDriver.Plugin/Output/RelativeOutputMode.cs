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
        private Vector2 _reportScaleMultiplier;
        public Vector2 Sensitivity
        {
            set
            {
                _sensitivity = value;
                _reportScaleMultiplier = _sensitivity;

                // Normalize (ratio of 1)
                _reportScaleMultiplier /= new Vector2(TabletProperties.MaxX, TabletProperties.MaxY);

                // Scale to tablet dimensions (mm)
                _reportScaleMultiplier *= new Vector2(TabletProperties.Width, TabletProperties.Height);
            }
            get { return _sensitivity; }
        }

        public TimeSpan ResetTime { set; get; }

        private ITabletReport _lastReport;
        private DateTime _lastReceived;

        public virtual void Read(IDeviceReport report)
        {
            if (report is ITabletReport tabletReport)
            {
                if (TabletProperties.ActiveReportID.IsInRange(tabletReport.ReportID))
                {
                    if (Transpose(tabletReport) is Vector2 pos)
                    {
                        if (VirtualMouse is IPressureHandler pressureHandler)
                            pressureHandler.SetPressure((float)tabletReport.Pressure / (float)TabletProperties.MaxPressure);
                        
                        VirtualMouse.Move(pos.X, pos.Y);
                    }
                }
            }
        }
        
        protected Vector2? Transpose(ITabletReport report)
        {
            var difference = DateTime.Now - _lastReceived;
            if (difference > ResetTime && _lastReceived != default)
            {
                _lastReport = null;
            }

            if (_lastReport != null)
            {
                var pos = new Vector2(report.Position.X - _lastReport?.Position.X ?? 0, report.Position.Y - _lastReport?.Position.Y ?? 0);

                // Pre Filter
                foreach (IFilter filter in _preFilters)
                    pos = filter.Filter(pos);

                pos *= _reportScaleMultiplier;

                // Post Filter
                foreach (IFilter filter in _postFilters)
                    pos = filter.Filter(pos);

                _lastReport = report;
                return pos;
            }
            
            _lastReport = report;
            _lastReceived = DateTime.Now;
            
            return null;
        }
    }
}