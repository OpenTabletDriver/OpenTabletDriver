using OpenTabletDriver.Tablet;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Vendors.XP_Pen
{
    public class XP_PenReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] report)
        {
            if (report[1].IsBitSet(4))
                return new XP_PenAuxReport(report);
            
            if (report.Length >= 10)
                return new XP_PenTabletReport(report);
            else
                return new TabletReport(report);
        }
    }
}