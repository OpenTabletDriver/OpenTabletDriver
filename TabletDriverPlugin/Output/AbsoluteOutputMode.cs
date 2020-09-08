using System.Collections.Generic;
using System;
using System.Numerics;
using System.Linq;
using TabletDriverPlugin.Attributes;
using TabletDriverPlugin.Platform.Display;
using TabletDriverPlugin.Platform.Pointer;
using TabletDriverPlugin.Tablet;

namespace TabletDriverPlugin.Output
{
    [PluginIgnore]
    public abstract class AbsoluteOutputMode : BindingHandler, IOutputMode
    {
        private float _halfDisplayWidth, _halfDisplayHeight, _halfTabletWidth, _halfTabletHeight;
        private float _minX, _maxX, _minY, _maxY;
        private Matrix3x2 _transformationMatrix;

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
        
        private TabletProperties _tabletProperties;
        public override TabletProperties TabletProperties
        {
            set
            {
                _tabletProperties = value;
                UpdateCache();
            }
            get => _tabletProperties;
        }

        private Area _displayArea, _tabletArea;
        
        public Area Input
        {
            set
            {
                _tabletArea = value;
                UpdateCache();
            }
            get => _tabletArea;
        }

        public Area Output
        {
            set
            {
                _displayArea = value;
                UpdateCache();
            }
            get => _displayArea;
        }

        public IVirtualScreen VirtualScreen { set; get; }
        public abstract IVirtualTablet VirtualTablet { get; }
        public IVirtualPointer Pointer => VirtualTablet;
        public bool AreaClipping { set; get; }

        internal void UpdateCache()
        {
            _transformationMatrix = CalculateTransformation();
            
            _halfDisplayWidth = Output?.Width / 2 ?? 0;
            _halfDisplayHeight = Output?.Height / 2 ?? 0;
            _halfTabletWidth = Input?.Width / 2 ?? 0;
            _halfTabletHeight = Input?.Height / 2 ?? 0;

            _minX = Output?.Position.X - _halfDisplayWidth ?? 0;
            _maxX = Output?.Position.X + Output?.Width - _halfDisplayWidth ?? 0;
            _minY = Output?.Position.Y - _halfDisplayHeight ?? 0;
            _maxY = Output?.Position.Y + Output?.Height - _halfDisplayHeight ?? 0;
        }

        internal Matrix3x2 CalculateTransformation()
        {
            if (Input is null | Output is null |
                TabletProperties is null | VirtualScreen is null)
                return new Matrix3x2();

            // Convert raw tablet data to millimeters
            var res = Matrix3x2.CreateScale(
                TabletProperties.Width / TabletProperties.MaxX,
                TabletProperties.Height / TabletProperties.MaxY);

            // Translate to the center of input area
            res *= Matrix3x2.CreateTranslation(
                -Input.Position.X, -Input.Position.Y);

            // Apply rotation
            res *= Matrix3x2.CreateRotation(
                (float)(Input.Rotation * System.Math.PI / 180));

            // Scale millimeters to pixels
            res *= Matrix3x2.CreateScale(
                Output.Width / Input.Width, Output.Height / Input.Height);

            // Translate output to virtual screen coordinates
            res *= Matrix3x2.CreateTranslation(
                Output.Position.X, Output.Position.Y);

            return res;
        }

        public virtual void Read(IDeviceReport report)
        {
            if (report is ITabletReport tabletReport)
            {
                if (TabletProperties.ActiveReportID.IsInRange(tabletReport.ReportID))
                {
                    if (VirtualTablet is IPressureHandler pressureHandler)
                        pressureHandler.SetPressure((float)tabletReport.Pressure / (float)TabletProperties.MaxPressure);
                    
                    var pos = Transpose(tabletReport);
                    VirtualTablet.SetPosition(pos);
                }
            }
            HandleBinding(report);
        }

        internal Point Transpose(ITabletReport report)
        {
            var pos = new Point(report.Position.X, report.Position.Y);

            // Pre Filter
            foreach (IFilter filter in _preFilters)
                pos = filter.Filter(pos);

            // Apply transformation
            var posVect = new Vector2(pos.X, pos.Y);
            posVect = Vector2.Transform(posVect, _transformationMatrix);
            pos.X = posVect.X;
            pos.Y = posVect.Y;

            // Clipping to display bounds
            if (AreaClipping)
                pos.Clamp(_minX, _maxX, _minY, _maxY);

            // Post Filter
            foreach (IFilter filter in _postFilters)
                pos = filter.Filter(pos);

            return pos;
        }
    }
}