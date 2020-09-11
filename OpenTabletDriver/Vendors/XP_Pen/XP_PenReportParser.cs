using OpenTabletDriver.Tablet;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Vendors.XP_Pen
{
    public class XP_PenReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] data)
        {
            var isAuxReport = ((data[1] & (1 << 5)) != 0) & ((data[1] & (1 << 6)) != 0);
            if (isAuxReport)
                return new XP_PenAuxReport(data);
            else
                return new TabletReport(data);
        }
    }
}