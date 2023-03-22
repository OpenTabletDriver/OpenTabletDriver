using System;
using OpenTabletDriver.Tablet;
using OpenTabletDriver.Tablet.Touch;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.IntuosV2
{
    public class IntuosV2ReportParser : IReportParser<IDeviceReport>
    {
        public virtual IDeviceReport Parse(byte[] data)
        {
            return data[0] switch
            {
                0x10 => new IntuosV2Report(data),
                0x11 => new IntuosV2AuxReport(data),
                0x21 => new IntuosV2TouchReport(data, ref _prevTouches),
                0xD2 => new IntuosV2TouchReport(data, ref _prevTouches),
                _ => new DeviceReport(data)
            };
        }

        private TouchPoint?[] _prevTouches = Array.Empty<TouchPoint?>();
    }
}
