using System.Collections.Generic;
using System.Linq;
using TabletDriverLib.Interop;
using TabletDriverPlugin;
using TabletDriverPlugin.Attributes;
using TabletDriverPlugin.Platform.Pointer;
using TabletDriverPlugin.Tablet;

namespace TabletDriverLib.Output
{
    [PluginName("Absolute Mode")]
    public class AbsoluteMode : BindingHandler, IAbsoluteMode
    {
        public void Read(IDeviceReport report)
        {
            if (report is ITabletReport tabletReport)
                Position(tabletReport);
        }
        
        private ICursorHandler CursorHandler { set; get; } = Platform.CursorHandler;
        private Area _displayArea, _tabletArea;
        private TabletProperties _tabletProperties;

        public Area Output
        {
            set
            {
                _displayArea = value;
                UpdateCache();
            }
            get => _displayArea;
        }

        public Area Input
        {
            set
            {
                _tabletArea = value;
                UpdateCache();
            }
            get => _tabletArea;
        }

        public override TabletProperties TabletProperties
        {
            set
            {
                _tabletProperties = value;
                UpdateCache();
            }
            get => _tabletProperties;
        }

        private IEnumerable<IFilter> _filters, _preFilters, _postFilters;
        public IEnumerable<IFilter> Filters
        {
            set
            {
                _filters = value;
                _preFilters = value.Where(f => f.FilterStage == FilterStage.PreTranspose);
                _postFilters = value.Where(f => f.FilterStage == FilterStage.PostTranspose);
            }
            get => _filters;
        }

        public bool AreaClipping { set; get; }
        
        private void UpdateCache()
        {
            _rotationMatrix = Input?.GetRotationMatrix();
            
            _halfDisplayWidth = Output?.Width / 2 ?? 0;
            _halfDisplayHeight = Output?.Height / 2 ?? 0;
            _halfTabletWidth = Input?.Width / 2 ?? 0;
            _halfTabletHeight = Input?.Height / 2 ?? 0;

            _minX = Output?.Position.X - _halfDisplayWidth ?? 0;
            _maxX = Output?.Position.X + Output?.Width - _halfDisplayWidth ?? 0;
            _minY = Output?.Position.Y - _halfDisplayHeight ?? 0;
            _maxY = Output?.Position.Y + Output?.Height - _halfDisplayHeight ?? 0;
        }

        private float[] _rotationMatrix;
        private float _halfDisplayWidth, _halfDisplayHeight, _halfTabletWidth, _halfTabletHeight;
        private float _minX, _maxX, _minY, _maxY;

        public void Position(ITabletReport report)
        {
            if (TabletProperties.ActiveReportID != 0 && report.ReportID <= TabletProperties.ActiveReportID)
                return;
            
            var pos = new Point(report.Position.X, report.Position.Y);

            // Pre Filter
            foreach (IFilter filter in _preFilters)
                pos = filter.Filter(pos);

            // Normalize (ratio of 1)
            pos.X /= TabletProperties.MaxX;
            pos.Y /= TabletProperties.MaxY;

            // Scale to tablet dimensions (mm)
            pos.X *= TabletProperties.Width;
            pos.Y *= TabletProperties.Height;

            // Adjust area to set origin to 0,0
            pos -= Input.Position;

            // Rotation
            if (Input.Rotation != 0f)
            {
                var tempCopy = new Point(pos.X, pos.Y);
                pos.X = (tempCopy.X * _rotationMatrix[0]) + (tempCopy.Y * _rotationMatrix[1]);
                pos.Y = (tempCopy.X * _rotationMatrix[2]) + (tempCopy.Y * _rotationMatrix[3]);
            }

            // Move area back
            pos.X += _halfTabletWidth;
            pos.Y += _halfTabletHeight;

            // Scale to tablet area (ratio of 1)
            pos.X /= Input.Width;
            pos.Y /= Input.Height;

            // Scale to display area
            pos.X *= Output.Width;
            pos.Y *= Output.Height;

            // Adjust display offset by center
            pos.X += Output.Position.X - _halfDisplayWidth;
            pos.Y += Output.Position.Y - _halfDisplayHeight;

            // Clipping to display bounds
            if (AreaClipping)
            {
                if (pos.X < _minX)
                    pos.X = _minX;
                if (pos.X > _maxX)
                    pos.X = _maxX;
                if (pos.Y < _minY)
                    pos.Y = _minY;
                if (pos.Y > _maxY)
                    pos.Y = _maxY;
            }

            // Post Filter
            foreach (IFilter filter in _postFilters)
                pos = filter.Filter(pos);

            // Setting cursor position
            CursorHandler.SetCursorPosition(pos);
        }
    }
}