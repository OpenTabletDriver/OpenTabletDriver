using TabletDriverPlugin.Tablet;

namespace TabletDriverLib.Tablet
{
    public class AuxReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] data)
        {
            return new AuxReport(data);
        }
    }
}