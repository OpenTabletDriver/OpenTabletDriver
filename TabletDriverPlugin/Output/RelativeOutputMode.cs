using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TabletDriverPlugin.Attributes;
using TabletDriverPlugin.Platform.Pointer;
using TabletDriverPlugin.Tablet;

namespace TabletDriverPlugin.Output
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

        public float XSensitivity { set; get; }
        public float YSensitivity { set; get; }
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
            HandleBinding(report);
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
                
                // Normalize (ratio of 1)
                pos.X /= TabletProperties.MaxX;
                pos.Y /= TabletProperties.MaxY;

                // Scale to tablet dimensions (mm)
                pos.X *= TabletProperties.Width;
                pos.Y *= TabletProperties.Height;

                // Sensitivity setting
                pos.X *= XSensitivity;
                pos.Y *= YSensitivity;
                
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