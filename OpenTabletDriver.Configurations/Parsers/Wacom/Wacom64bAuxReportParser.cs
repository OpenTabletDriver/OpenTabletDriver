using OpenTabletDriver.Tablet;
using OpenTabletDriver.Tablet.Touch;

namespace OpenTabletDriver.Configurations.Parsers.Wacom
{
    public class Wacom64BAuxReportParser : IReportParser<IDeviceReport>
    {
        public virtual IDeviceReport Parse(byte[] data)
        {
            return new WacomTouchReport(data, ref _prevTouches);
        }

        private TouchPoint?[] _prevTouches;
    }
}
