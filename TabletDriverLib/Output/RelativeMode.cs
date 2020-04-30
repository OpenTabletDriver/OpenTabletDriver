using System;
using System.Collections.Generic;
using TabletDriverLib.Interop;
using TabletDriverLib.Interop.Cursor;
using TabletDriverPlugin;
using TabletDriverPlugin.Attributes;
using TabletDriverPlugin.Tablet;

namespace TabletDriverLib.Output
{
    [PluginName("Relative Mode")]
    public class RelativeMode : BindingHandler, IRelativeMode
    {
        public float XSensitivity { set; get; }
        public float YSensitivity { set; get; }
        public TimeSpan ResetTime { set; get; } = TimeSpan.FromMilliseconds(100);
        public IEnumerable<IFilter> Filters { set; get; }

        private ICursorHandler CursorHandler { set; get; } = Platform.CursorHandler;
        private ITabletReport _lastReport;
        private DateTime _lastReceived;
        private Point _lastPosition;

        public void Read(IDeviceReport report)
        {
            if (report is ITabletReport tabletReport)
                Position(tabletReport);
        }

        public void Position(ITabletReport report)
        {
            if (report.ReportID < TabletProperties.ActiveReportID)
                return;
            
            var difference = DateTime.Now - _lastReceived;
            if (difference > ResetTime && _lastReceived != DateTime.MinValue)
            {
                _lastReport = null;
                _lastPosition = null;
            }

            if (_lastReport != null)
            {
                var pos = new Point(report.Position.X - _lastReport.Position.X, report.Position.Y - _lastReport.Position.Y);
                
                // Normalize (ratio of 1)
                pos.X /= TabletProperties.MaxX;
                pos.Y /= TabletProperties.MaxY;

                // Scale to tablet dimensions (mm)
                pos.X *= TabletProperties.Width;
                pos.Y *= TabletProperties.Height;

                // Sensitivity setting
                pos.X *= XSensitivity;
                pos.Y *= YSensitivity;
                
                // Translate by cursor position
                pos += GetCursorPosition();

                // Filter
                foreach (var filter in Filters)
                    pos = filter.Filter(pos);

                CursorHandler.SetCursorPosition(pos);
                _lastPosition = pos;
            }
            
            _lastReport = report;
            _lastReceived = DateTime.Now;
        }

        private Point GetCursorPosition()
        {
            if (_lastPosition != null)
                return _lastPosition;
            else
                return CursorHandler.GetCursorPosition();
        }
    }
}