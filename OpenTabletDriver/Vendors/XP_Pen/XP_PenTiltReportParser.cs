using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Vendors.XP_Pen
{
    public class XP_PenTiltReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] data)
        {
            return (data[1] & 0xc0) == 0xc0 ? new XP_PenAuxReport(data) : new XP_PenTiltTabletReport(data);
        }
    }
}
