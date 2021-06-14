using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Vendors.Veikk
{
    public class VeikkReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] report)
        {
            return new VeikkTabletReport(report);
        }
    }
}