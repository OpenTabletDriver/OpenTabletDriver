using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Vendors.Wacom
{
    public class Wacom64bAuxReportParser : IReportParser<IDeviceReport>
    {
        public virtual IDeviceReport Parse(byte[] data)
        {
            return data[2] switch
            {
                0x80 => new AuxReport(data),
                _ => new WacomTouchReport(data)
            };
        }
    }
}
