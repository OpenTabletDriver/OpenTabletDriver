using System;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.DTU
{
    public class DTUReportParser : IReportParser<IDeviceReport>
    {
        public virtual IDeviceReport Parse(byte[] data)
        {
            return data[0] switch
            {
                0x11 => new DTUReport(data),
                0x15 => new DTUAuxReport(data),
                _ => new DeviceReport(data)
            };
        }
    }
}
