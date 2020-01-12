using TabletDriverPlugin.Tablet;

namespace TabletDriverLib.Tablet
{
    public class AuxReportParser : IDeviceReportParser
    {
        public IDeviceReport Parse(byte[] data)
        {
            return new AuxReport(data);
        }
    }
}