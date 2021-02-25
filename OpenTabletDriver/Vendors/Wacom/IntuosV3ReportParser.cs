using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Vendors.Wacom
{
    public class IntuosV3ReportParser : IReportParser<IDeviceReport>
    {
        public virtual IDeviceReport Parse(byte[] data)
        {
            return data[0] switch
            {
                0x10 => new IntuosV3Report(data),
                0x11 => new IntuosV3AuxReport(data),
                _ => null
            };
        }
    }
}