using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.XP_Pen
{
    public class XP_PenDedicatedAuxReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] report)
        {
            return new XP_PenAuxReport(report, 1);
        }
    }
}
