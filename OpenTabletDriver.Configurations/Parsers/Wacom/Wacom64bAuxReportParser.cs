using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Parsers.Wacom
{
    public class Wacom64bAuxReportParser : IReportParser<IDeviceReport>
    {
        public virtual IDeviceReport Parse(byte[] data)
        {
            return new WacomTouchReport(data);
        }
    }
}
