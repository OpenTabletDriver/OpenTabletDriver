using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Touch;

namespace OpenTabletDriver.Configurations.Parsers.Wacom
{
    public class Wacom64bAuxReportParser : IReportParser<IDeviceReport>
    {
        public virtual IDeviceReport Parse(byte[] data)
        {
            return new WacomTouchReport(data, ref prevTouches);
        }

        private TouchPoint[] prevTouches;
    }
}
