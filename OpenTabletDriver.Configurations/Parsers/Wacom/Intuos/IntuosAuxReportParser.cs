using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Configurations.Parsers.Wacom.IntuosV2;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.Intuos
{
    public class IntuosAuxReportParser : IReportParser<IDeviceReport>
    {
        public virtual IDeviceReport Parse(byte[] data)
        {
            return data[0] switch
            {
                0x02 => new IntuosV2AuxReport(data),
                _ => new DeviceReport(data),
            };
        }
    }
}
